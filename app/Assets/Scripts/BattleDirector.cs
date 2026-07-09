using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BattleAtlas
{
    // Loads the battle JSON and renders every unit each frame from the
    // clock's current time. Three LOD tiers by camera distance: a draped
    // map-symbol ribbon per unit (UnitSymbol grammar + SymbolMeshBuilder
    // geometry — shape encodes arm, border encodes echelon, thickness
    // encodes strength, fill encodes provenance) at the Block tier beyond
    // RegimentsInDist and at the Regiments tier in the middle band — where
    // a brigade with a regiments roster partitions into per-regiment
    // ribbons via RegimentSlots — and instanced soldier ranks inside
    // SoldiersInDist (with a hysteresis band at each boundary to avoid
    // flicker). Units without a roster — or in scattered/routed formation,
    // where an ordered partition would be a lie — keep the monolithic
    // symbol at the middle tier. Decomposed brigades (units with a
    // `parent`) form a family: the tier is evaluated ONCE from the
    // parent's center and the whole family swaps atomically — parent
    // symbol far, children (own tracks) near — per battle-format.md
    // "Parent / children". Symbol meshes are persistent per unit and
    // rebuilt only when SymbolNeedsRebuild says a viewer could tell —
    // most of the 190 units are static most of the battle.
    public class BattleDirector : MonoBehaviour
    {
        public TextAsset battleJson;
        public Terrain terrain;
        public BattleClock clock;
        // a real material ASSET (not a runtime instance): asset references keep
        // the shader in device builds, where runtime-created materials render
        // magenta because the shader was stripped
        public Material unitMaterial;
        // material for instanced soldier figures; falls back to unitMaterial
        // when left unset in the inspector (e.g. older scenes). For the
        // two-tone uniforms, wire Assets/Battle/SoldierFigure.mat here — the
        // vertex-color bands need its SoldierVertexTint shader (URP Lit
        // ignores vertex color, so the fallback renders monochrome figures).
        public Material soldierMaterial;
        // material for the instanced flag layer (Assets/Battle/Flag.mat —
        // its shader carries the deterministic vertex wave). Unset: flags
        // are skipped with a warning, everything else renders as before.
        public Material flagMaterial;
        // material for the draped unit map-symbol ribbons (Assets/Battle/
        // UnitSymbol.mat — its shader carries the provenance hatch and the
        // echelon border band). Unset: warn once and fall back to
        // unitMaterial — symbols keep rendering, minus the cartographic
        // styling (the flagMaterial pattern).
        public Material symbolMaterial;
        // transparent materials for the obscuration field (Assets/Battle/
        // Smoke.mat and Dust.mat, created + wired by the BattleAtlas/Setup
        // Obscuration menu item). Unset: ObscurationField warns once and
        // renders no obscuration, everything else renders as before.
        public Material smokeMaterial;
        public Material dustMaterial;

        // activity salience: units neither moving nor named by a live battle
        // event fall this far toward terrain gray, so the corridor's actors
        // carry the frame over the attested-static ring. 0.55 keeps the side
        // hue readable — the ring recedes, it doesn't vanish.
        public const float InactiveDesat = 0.55f;
        public static readonly Color FieldGray = new Color(0.45f, 0.43f, 0.39f);
        // echelon border weights the UnitSymbol shader reads from the MPB:
        // > 1 draws the brigade DOUBLE line (ink at both strip edges),
        // <= 1 draws a single centered line covering that fraction of the
        // 2 m border strip — regiment thin, battery baseline / park outline
        // full-width. Public: the MPB truth-table test pins them.
        public const float BrigadeBorderWeight = 1.25f;
        public const float RegimentBorderWeight = 0.4f;
        public const float FullBorderWeight = 1f;
        // selection highlight: added onto the echelon's border weight (a
        // selected regiment thickens, a selected brigade/battery crosses
        // into the heavier double line) — selection is deliberate
        // attention, same rule as the full-saturation fill below
        public const float SelectedBorderBoost = 0.5f;
        // click raymarch reach: past the far corner of the 8.5 km field
        // from any orbit the camera controller allows
        const float PickMaxDistM = 30000f;
        // LOD hysteresis: soldiers resolve in below SoldiersInDist and hold
        // until SoldiersOutDist; regiment-grain symbols resolve in below
        // RegimentsInDist and hold until RegimentsOutDist. The band at each
        // boundary (150m / 400m) prevents flicker when the camera hovers there.
        const float SoldiersInDist = 1500f;
        const float SoldiersOutDist = 1650f;
        const float RegimentsInDist = 4000f;
        const float RegimentsOutDist = 4400f;
        // regiment rosters realistically run <= 10; sized with headroom so the
        // per-unit mesh/spec buffers never reallocate (over-long rosters clamp)
        const int MaxRegiments = 16;
        // pose bias input (UnitFormationRenderer.PoseFor): a unit is
        // "moving" when its track position changes across a 1s window
        // around now — sampled symmetrically so scrub direction can't flip
        // the answer at the same t
        const float MovingSampleHalfWindow = 0.5f;
        const float MovingEpsilonM = 0.05f;
        // flag pivot above the unit-center ground: clears the draped symbol
        // and the figures, so the flag reads at every tier
        const float FlagPoleHeight = 10f;
        // label anchor above the unit-center ground (display meters, like
        // the symbol lift): under the flag, over the ribbon
        const float LabelLiftM = 2f;
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int FillStyleId = Shader.PropertyToID("_FillStyle");
        static readonly int BorderWeightId = Shader.PropertyToID("_BorderWeight");
        // battle-clock seconds for the Flag shader's vertex wave — a global
        // driven from BattleClock, NOT _Time, so time-scrubbing replays the
        // identical wave (determinism even in the cloth)
        static readonly int BattleWaveTimeId = Shader.PropertyToID("_BattleWaveTime");
        // corners first (order is load-bearing for tests), then edge midpoints —
        // a denser ring catches ground rising inside the footprint, not just at
        // its extremes
        static readonly (float dx, float dz)[] CornerOffsets =
        {
            (-0.5f, -0.5f), (0.5f, -0.5f), (-0.5f, 0.5f), (0.5f, 0.5f),
            (0f, -0.5f), (0f, 0.5f), (-0.5f, 0f), (0.5f, 0f),
        };

        public enum LodTier { Soldiers, Regiments, Block }

        // arm-of-service buckets — see KindOf for the id convention
        public enum UnitKind { Infantry, Artillery, Cavalry }

        // one live window of a battle event attached to a unit. IsActiveAt
        // scans the per-unit list Start builds once — a unit's windows are
        // few, and nothing rebuilds or rescans battle.events per frame.
        public readonly struct EventWindow
        {
            public readonly float T0;
            public readonly float T1;
            public EventWindow(float t0, float t1) { T0 = t0; T1 = t1; }
        }

        // one roster-partition ribbon at the Regiments tier: where the
        // regiment-grain symbol sits (world XZ), the slot frontage the
        // RegimentSlots partition attests, and the DISPLAY-INFERRED
        // strength share (parent strength / roster count — invented at
        // display grain, which is why roster ribbons render hatched) with
        // its resulting thickness. Pure data so the partition is testable
        // without a scene.
        public readonly struct RosterSymbolSpec
        {
            public readonly Vector2 CenterXZ;
            public readonly float Frontage;
            public readonly float StrengthShare;
            public readonly float DisplayDepth;

            public RosterSymbolSpec(
                Vector2 centerXZ, float frontage, float strengthShare, float displayDepth)
            {
                CenterXZ = centerXZ;
                Frontage = frontage;
                StrengthShare = strengthShare;
                DisplayDepth = displayDepth;
            }
        }

        // per-unit runtime state: the persistent symbol mesh(es) plus
        // everything needed to switch representation for soldier ranks at
        // closer range. A small class beats a growing tuple once it carries
        // latches and a renderer.
        class UnitEntry
        {
            public UnitTrack Track;
            public UnitFormationRenderer FormationRenderer;
            public LodTier Tier;
            // family links, built once in Start from UnitDto.parent (the
            // loader already guaranteed validity and depth 1): a child holds
            // its parent, a parent holds its children, everyone else null
            public UnitEntry Parent;
            public List<UnitEntry> Children;
            // middle-tier roster state, null/0 when the unit has no roster
            public int RegimentCount;
            public MaterialPropertyBlock ColorBlock;
            public bool IsUnion; // flag color bucket
            // symbol grammar state, fixed at Start (kind never changes):
            // KindOf's shape class and the SideColor the MPB shows while
            // the unit is active (kind SHADES retired with the slabs —
            // shape does that job now)
            public UnitSymbol.SymbolKind Kind;
            public UnitSymbol.Echelon Echelon;
            public Color ActiveColor;
            // label state, fixed at Start: what the label says (unit name,
            // id fallback), this unit's fixed candidate slot, and — for
            // roster brigades — the first of its roster-name slots (fixed
            // slots keep the declutter's sticky bonus keyed to the same
            // unit across frames) and the roster names themselves
            public string DisplayName;
            public int LabelSlot;
            public int RosterLabelBase;
            public List<string> RosterNames;
            // the persistent draped symbol: one Mesh per unit (MarkDynamic,
            // created lazily on first render at a symbol tier), rebuilt from
            // the Director's scratch buffers only when SymbolNeedsRebuild
            // fires; the two counts remember which submeshes are non-empty
            // (a park has no body tris, a skirmish line no border)
            public Mesh SymbolMesh;
            public int BodyIndexCount;
            public int BorderIndexCount;
            // roster partition at the Regiments tier: one persistent mesh
            // per roster slot (lazy, like SymbolMesh) drawn with the
            // regiment-echelon MPB below
            public Mesh[] RosterMeshes;
            public int[] RosterBodyCounts;
            public int[] RosterBorderCounts;
            public MaterialPropertyBlock RosterBlock;
            // rebuild bookkeeping: the state/tier the last build consumed
            // (valid once HasPrev) and which representation that build fed —
            // a rebuild invalidates the other one, so a formation or tier
            // flip that swaps representations rebuilds exactly once
            public UnitState PrevState;
            public LodTier PrevTier;
            public bool PrevSelected;
            public bool HasPrev;
            public bool SymbolBuilt;
            public bool RosterBuilt;
            // provenance latch: the fill style the MPB currently shows —
            // rewritten ONLY when the bracketing keyframe's confidence
            // changes the style, never per frame
            public UnitSymbol.FillStyle FillStyle;
            // activity salience: the Start-built event windows (null = no
            // authored event names this unit) and the latch — the MPB color
            // is rewritten ONLY when the active state flips, never per frame
            public List<EventWindow> EventWindows;
            public bool Active;
        }

        readonly List<UnitEntry> units = new();
        // selection (Task 6 wires the click picking; null = none). The
        // label pass already honors it — a selected label always survives
        // the declutter and is the only one shown at the Soldiers tier.
        UnitEntry selectedEntry;
        // label candidate slots, one FIXED slot per unit plus one per
        // roster ribbon, sized in Start and refilled every Update: +inf
        // priority = no candidate this frame. UnitLabelField reads these
        // in LateUpdate — director-owned so the render pass fills them in
        // place with zero allocation.
        float[] labelPriorities;
        Vector3[] labelPositions;
        string[] labelTexts;
        Color[] labelColors;
        // pick footprints, refilled alongside the render pass: one oriented
        // rectangle per rendered SYMBOL (a roster partition registers its
        // brigade once — roster ribbons have no track or citation of their
        // own; the Soldiers figure tier registers nothing, symbols are what
        // the picker picks). Preallocated at unit count in Start.
        Vector2[] pickCenters;
        float[] pickFacings;
        float[] pickFrontages;
        float[] pickDepths;
        UnitEntry[] pickEntries;
        int pickCount;
        // scratch for RegimentSlots + the roster specs, reused across units
        // within one Update
        readonly (Vector2 center, Vector2 size)[] slotsBuffer =
            new (Vector2, Vector2)[MaxRegiments];
        readonly RosterSymbolSpec[] rosterSpecsBuffer = new RosterSymbolSpec[MaxRegiments];
        // symbol-geometry scratch, one set at the audited worst case —
        // builds are sequential within Update, so every rebuild shares it
        readonly Vector3[] symbolVerts = new Vector3[SymbolMeshBuilder.MaxSymbolVerts];
        readonly Vector2[] symbolUvs = new Vector2[SymbolMeshBuilder.MaxSymbolVerts];
        readonly int[] symbolTris = new int[SymbolMeshBuilder.MaxSymbolIndices];
        Mesh[] soldierPoseMeshes;
        Mesh flagMesh;
        // one flag per unit at every tier, ONE RenderMeshInstanced per side
        // across all units (side color rides the MPB, so union and
        // confederate flags are the two class-homogeneous batches);
        // preallocated at unit count in Start, refilled each Update
        Matrix4x4[] unionFlagMatrices;
        Matrix4x4[] csaFlagMatrices;
        int unionFlagCount;
        int csaFlagCount;
        MaterialPropertyBlock unionFlagBlock;
        MaterialPropertyBlock csaFlagBlock;
        bool warnedNoFlagMaterial;
        Camera lodCamera;
        // cached once in Start: avoids allocating a closure per unit per
        // frame. groundYBase is refreshed once per Update (not per unit)
        // since GroundY reads it on every call.
        System.Func<float, float, float> groundYFunc;
        float groundYBase;

        public static Color SideColor(string side)
        {
            switch (side)
            {
                case "union": return new Color(0.23f, 0.35f, 0.61f);       // deep blue
                case "confederate": return new Color(0.63f, 0.31f, 0.31f); // muted red
                default:
                    // gray keeps rendering but a typo'd side in authored data
                    // should be loud, not invisible
                    Debug.LogWarning($"unknown unit side '{side}', rendering gray");
                    return Color.gray;
            }
        }

        // Arm of service from the unit id. The LOCKED id-prefix convention's
        // ONE decoder lives in UnitSymbol.KindOf (which additionally splits
        // the `us-arty-` park out of Artillery for the symbol grammar);
        // this delegates and collapses that split back into three buckets
        // for callers that don't draw the park distinction.
        public static UnitKind KindOf(string unitId)
        {
            switch (UnitSymbol.KindOf(unitId))
            {
                case UnitSymbol.SymbolKind.Artillery:
                case UnitSymbol.SymbolKind.ArtilleryPark:
                    return UnitKind.Artillery;
                case UnitSymbol.SymbolKind.Cavalry:
                    return UnitKind.Cavalry;
                default:
                    return UnitKind.Infantry;
            }
        }

        // The echelon's border weight, written once onto the per-unit MPB
        // (echelon never changes at runtime). The battery baseline and the
        // park outline draw the full strip — their echelon mark is the
        // symbol class itself (gun-dot grammar, hollow outline), not a
        // border pattern.
        public static float SymbolBorderWeight(UnitSymbol.Echelon echelon)
        {
            switch (echelon)
            {
                case UnitSymbol.Echelon.Brigade: return BrigadeBorderWeight;
                case UnitSymbol.Echelon.Regiment: return RegimentBorderWeight;
                default: return FullBorderWeight;
            }
        }

        // The symbol rebuild predicate: SymbolMeshBuilder.SymbolDirty (the
        // shared thickness/pose/formation/tier/selection gate) PLUS the
        // artillery correction — a battery encodes strength as its GUN-DOT
        // COUNT, not thickness, and its clamped MinDepth never moves, so a
        // strength fall must dirty on the dot step instead. Pure so the
        // truth table is testable without a scene.
        public static bool SymbolNeedsRebuild(
            UnitState prev, UnitState cur, float frontage,
            UnitSymbol.SymbolKind kind, LodTier prevTier, LodTier tier,
            bool prevSelected, bool selected)
        {
            if (SymbolMeshBuilder.SymbolDirty(prev, cur, frontage,
                    prevTier, tier, prevSelected, selected))
                return true;
            return kind == UnitSymbol.SymbolKind.Artillery
                && UnitSymbol.GunDotCount(prev.strength)
                    != UnitSymbol.GunDotCount(cur.strength);
        }

        // Selection-blind arity, kept for callers (and pinned tests) that
        // predate the Task 6 picking: both sides unselected.
        public static bool SymbolNeedsRebuild(
            UnitState prev, UnitState cur, float frontage,
            UnitSymbol.SymbolKind kind, LodTier prevTier, LodTier tier)
        {
            return SymbolNeedsRebuild(
                prev, cur, frontage, kind, prevTier, tier, false, false);
        }

        // The Regiments-tier roster partition as pure data: RegimentSlots
        // carves the attested frontage, the parent strength splits evenly
        // (DISPLAY-INFERRED — the format attests no per-regiment strengths,
        // which is why the roster ribbons render hatched), and each share
        // takes its thickness from the slot's own frontage. Slot centers
        // rotate into world XZ by the unit facing.
        public static int RosterSymbolSpecs(
            UnitState s, float frontage, float depth, int regimentCount,
            (Vector2 center, Vector2 size)[] slotsBuffer, RosterSymbolSpec[] results)
        {
            FormationLayout.RegimentSlots(
                s.formation, regimentCount, frontage, depth, slotsBuffer);
            var rot = Quaternion.Euler(0f, s.facingDeg, 0f);
            float share = s.strength / regimentCount;
            for (int i = 0; i < regimentCount; i++)
            {
                var (center, size) = slotsBuffer[i];
                // local (x=right-of-line, y=forward) -> world via facing
                Vector3 world = rot * new Vector3(center.x, 0f, center.y);
                results[i] = new RosterSymbolSpec(
                    new Vector2(s.posXZ.x + world.x, s.posXZ.y + world.z),
                    size.x, share, UnitSymbol.DisplayDepth(share, size.x));
            }
            return regimentCount;
        }

        // Pose-bias and salience input: is the track position changing
        // around t? Sampled symmetrically (MovingSampleHalfWindow each way)
        // so scrub direction can't flip the answer at the same t; StateAt
        // clamps at the track ends, so the window degrades to one-sided
        // there instead of misreading.
        public static bool IsMovingAt(UnitTrack track, float t)
        {
            return Vector2.Distance(
                track.StateAt(t - MovingSampleHalfWindow).posXZ,
                track.StateAt(t + MovingSampleHalfWindow).posXZ)
                > MovingEpsilonM;
        }

        // Activity salience: a unit is "active" at t when it is moving (the
        // same symmetric-window test as the soldier pose bias) or when any
        // battle event naming it is live — t0 <= t <= t1, INCLUSIVE, an
        // event owns its boundary instants. Deterministic in t, so scrubbing
        // replays identical salience; the windows list is Start-built.
        public static bool IsActiveAt(UnitTrack track, List<EventWindow> eventWindows, float t)
        {
            if (IsMovingAt(track, t))
                return true;
            if (eventWindows == null)
                return false;
            for (int i = 0; i < eventWindows.Count; i++)
                if (eventWindows[i].T0 <= t && t <= eventWindows[i].T1)
                    return true;
            return false;
        }

        // Inactive units keep their side color but fall toward terrain
        // gray, so the ring of attested-static brigades stops drowning the
        // corridor's action. The symbol border keeps the side hue (the
        // shader darkens whatever _BaseColor shows, so the ring never
        // vanishes); figures (Soldiers tier) and flags NEVER take this:
        // close zoom is deliberate attention, and the flag is the unit's
        // identity.
        public static Color InactiveColor(Color activeColor) =>
            Color.Lerp(activeColor, FieldGray, InactiveDesat);

        // Fills the buffer with world-XZ sample points under a unit
        // footprint: center, corners, and edge midpoints, rotated by
        // facing. The slab renderer's min/max ground consumer retired with
        // the blocks; the ring stays as the footprint sampler for tests
        // and the Task 6 picking math.
        public static void FootprintSamplePoints(
            Vector2 centerXZ, float facingDeg, float frontage, float depth, Vector2[] buffer)
        {
            var rot = Quaternion.Euler(0f, facingDeg, 0f);
            buffer[0] = centerXZ;
            for (int i = 0; i < CornerOffsets.Length; i++)
            {
                Vector3 off = rot * new Vector3(
                    CornerOffsets[i].dx * frontage, 0f, CornerOffsets[i].dz * depth);
                buffer[i + 1] = new Vector2(centerXZ.x + off.x, centerXZ.y + off.z);
            }
        }

        // The family suppression contract (battle-format.md "Parent /
        // children"): at Block tier the parent's symbol IS the family; at
        // the nearer tiers the children are, and the parent hides.
        // Parentless, childless units render at every tier — the pre-family
        // behavior. Pure static so the truth table is testable without a
        // scene.
        public static bool RendersAtTier(bool isChild, bool hasChildren, LodTier tier)
        {
            if (hasChildren) return tier == LodTier.Block;
            if (isChild) return tier != LodTier.Block;
            return true;
        }

        // Three tiers with a sticky band at each boundary: a tier only
        // switches once the camera clears the far edge of its band. For a
        // family, dist is the PARENT's center distance and the latch lives on
        // the parent — evaluated once, so a family never half-swaps.
        public static LodTier EvaluateTier(float dist, LodTier current)
        {
            if (dist < SoldiersInDist
                || (current == LodTier.Soldiers && dist < SoldiersOutDist))
                return LodTier.Soldiers;
            if (dist < RegimentsInDist
                || (current == LodTier.Regiments && dist < RegimentsOutDist))
                return LodTier.Regiments;
            return LodTier.Block;
        }

        // Middle-tier roster ribbons only for units with a roster holding an
        // ordered formation; scattered/routed (and roster-less units) fall
        // through to the monolithic symbol — honesty over uniformity.
        public static bool RendersRosterSymbols(
            int regimentCount, string formation, LodTier tier)
        {
            return tier == LodTier.Regiments && regimentCount > 0
                && (formation == "line" || formation == "column");
        }

        // The label pass (UnitLabelField.LateUpdate) reads the director's
        // per-frame candidate buffers through these — internal wiring for
        // the same assembly, not API. Valid once Start has run.
        internal int LabelSlotCount => labelPriorities.Length;
        internal float[] LabelPriorities => labelPriorities;
        internal Vector3[] LabelPositions => labelPositions;
        internal string[] LabelTexts => labelTexts;
        internal Color[] LabelColors => labelColors;

        // World-space terrain height at (x, z), offset by groundYBase (the
        // terrain object's Y, refreshed once per Update). Instance method so
        // groundYFunc can be built once in Start instead of allocating a
        // closure per unit per frame in Update.
        float GroundY(float x, float z) =>
            terrain.SampleHeight(new Vector3(x, 0f, z)) + groundYBase;

        void Start()
        {
            if (terrain == null)
            {
                // terrain re-imports replace the scene object and orphan the
                // serialized reference; fall back rather than NRE every frame
                terrain = Terrain.activeTerrain;
                Debug.LogWarning("BattleDirector.terrain was unset; using Terrain.activeTerrain");
            }
            if (soldierMaterial == null)
            {
                soldierMaterial = unitMaterial;
            }
            // asset reference keeps the shader in device builds; the
            // instancing flag itself is not stripped, so it's safe to set here
            soldierMaterial.enableInstancing = true;
            if (flagMaterial != null) flagMaterial.enableInstancing = true;
            if (symbolMaterial == null)
            {
                Debug.LogWarning(
                    "BattleDirector.symbolMaterial is unset; wire Assets/Battle/UnitSymbol.mat " +
                    "for the cartographic symbol styling — falling back to unitMaterial");
                symbolMaterial = unitMaterial;
            }
            // pose meshes indexed by UnitFormationRenderer.Pose* — shared
            // across every unit's renderer
            soldierPoseMeshes = new Mesh[UnitFormationRenderer.PoseCount];
            soldierPoseMeshes[UnitFormationRenderer.PoseStanding] = InstancedMeshes.BuildSoldier();
            soldierPoseMeshes[UnitFormationRenderer.PoseAdvancing] = InstancedMeshes.BuildSoldierAdvancing();
            soldierPoseMeshes[UnitFormationRenderer.PoseKneeling] = InstancedMeshes.BuildSoldierKneeling();
            flagMesh = InstancedMeshes.BuildFlag();
            unionFlagBlock = new MaterialPropertyBlock();
            unionFlagBlock.SetColor(BaseColorId, SideColor("union"));
            csaFlagBlock = new MaterialPropertyBlock();
            csaFlagBlock.SetColor(BaseColorId, SideColor("confederate"));
            lodCamera = Camera.main;
            // built once: a fresh (x, z) => ... lambda per unit per frame
            // would allocate a closure every Update
            groundYFunc = GroundY;

            BattleDto battle = BattleLoader.Parse(battleJson.text);
            clock.EndTime = battle.endTime;
            clock.StartTime = battle.startTime;
            var entriesById = new Dictionary<string, UnitEntry>(battle.units.Count);
            int labelSlots = 0;
            foreach (UnitDto u in battle.units)
            {
                // the symbol's resting color is the plain side color — kind
                // reads as SHAPE now, not as a shade of one box; rewritten
                // only when the activity latch flips in Update
                UnitSymbol.SymbolKind kind = UnitSymbol.KindOf(u.id);
                Color activeColor = SideColor(u.side);
                var block = new MaterialPropertyBlock();
                block.SetColor(BaseColorId, activeColor);
                // documented-solid until the first confidence latch check;
                // matches FillStyle below the same way Active matches the
                // color set here
                block.SetFloat(FillStyleId, (float)UnitSymbol.FillStyle.Solid);

                var formationRenderer = new UnitFormationRenderer(
                    u.id, u.frontage_m, u.depth_m, soldierPoseMeshes, soldierMaterial,
                    SideColor(u.side));
                // clamp defensively: the schema doesn't cap roster length, and
                // the per-unit mesh/spec buffers must never grow at render
                // time. Loud, like unknown sides: authored data the renderer
                // refuses to fully show must never vanish silently.
                if (u.regiments != null && u.regiments.Count > MaxRegiments)
                    Debug.LogWarning(
                        $"unit '{u.id}' has {u.regiments.Count} regiments; rendering only the first {MaxRegiments}");
                int regimentCount = u.regiments == null
                    ? 0 : Mathf.Min(u.regiments.Count, MaxRegiments);
                var entry = new UnitEntry
                {
                    Track = new UnitTrack(u),
                    FormationRenderer = formationRenderer,
                    Tier = LodTier.Block,
                    RegimentCount = regimentCount,
                    RosterMeshes = regimentCount > 0 ? new Mesh[MaxRegiments] : null,
                    RosterBodyCounts = regimentCount > 0 ? new int[MaxRegiments] : null,
                    RosterBorderCounts = regimentCount > 0 ? new int[MaxRegiments] : null,
                    ColorBlock = block,
                    IsUnion = u.side == "union",
                    Kind = kind,
                    ActiveColor = activeColor,
                    FillStyle = UnitSymbol.FillStyle.Solid,
                    // latch starts true to match the color set above; the
                    // first Update flips genuinely idle units to FieldGray
                    Active = true,
                    // fixed label slots: the unit's own, then one per
                    // roster ribbon — a name the data attests, id if not
                    DisplayName = string.IsNullOrEmpty(u.name) ? u.id : u.name,
                    LabelSlot = labelSlots,
                    RosterLabelBase = regimentCount > 0 ? labelSlots + 1 : -1,
                    RosterNames = regimentCount > 0 ? u.regiments : null,
                };
                labelSlots += 1 + regimentCount;
                units.Add(entry);
                entriesById[u.id] = entry;
            }
            // label candidate buffers at the fixed slot capacity; +inf
            // priority = absent, re-stamped at the top of every Update
            labelPriorities = new float[labelSlots];
            labelPositions = new Vector3[labelSlots];
            labelTexts = new string[labelSlots];
            labelColors = new Color[labelSlots];
            // second pass: link families. Built once here so Update never
            // searches or allocates; Parse already threw on unknown parents,
            // grandparents, and roster-carrying parents.
            for (int i = 0; i < battle.units.Count; i++)
            {
                string parentId = battle.units[i].parent;
                if (string.IsNullOrEmpty(parentId))
                    continue;
                UnitEntry parent = entriesById[parentId];
                if (parent.Children == null)
                    parent.Children = new List<UnitEntry>();
                parent.Children.Add(units[i]);
                units[i].Parent = parent;
            }
            // third pass, after the family links exist: echelon border
            // weights (EchelonOf needs hasChildren/hasParent, which the
            // second pass just built). Echelon never changes at runtime, so
            // this is the ONE write; roster-carrying units also get their
            // regiment-echelon MPB here — always hatched, because the
            // partition's strength split is display-inferred.
            foreach (UnitEntry entry in units)
            {
                UnitSymbol.Echelon echelon = UnitSymbol.EchelonOf(
                    entry.RegimentCount > 0, entry.Children != null,
                    entry.Parent != null, entry.Kind);
                entry.Echelon = echelon; // label priority + tier rule input
                entry.ColorBlock.SetFloat(BorderWeightId, SymbolBorderWeight(echelon));
                if (entry.RegimentCount > 0)
                {
                    var rosterBlock = new MaterialPropertyBlock();
                    rosterBlock.SetColor(BaseColorId, entry.ActiveColor);
                    rosterBlock.SetFloat(FillStyleId,
                        (float)UnitSymbol.FillStyle.Hatched);
                    rosterBlock.SetFloat(BorderWeightId,
                        SymbolBorderWeight(UnitSymbol.Echelon.Regiment));
                    entry.RosterBlock = rosterBlock;
                }
            }
            // one slot per unit — every unit flies a flag every frame
            unionFlagMatrices = new Matrix4x4[units.Count];
            csaFlagMatrices = new Matrix4x4[units.Count];
            // pick footprints: at most one per unit per frame
            pickCenters = new Vector2[units.Count];
            pickFacings = new float[units.Count];
            pickFrontages = new float[units.Count];
            pickDepths = new float[units.Count];
            pickEntries = new UnitEntry[units.Count];

            // activity salience: bucket each unit-attached event's window
            // onto its unit ONCE, so IsActiveAt scans a short per-unit list
            // instead of all of battle.events per frame. Segment-form events
            // (empty unitId) light no unit — they have no symbol to raise.
            // Parse already guaranteed every non-empty unitId resolves.
            if (battle.events != null)
            {
                foreach (EventDto ev in battle.events)
                {
                    if (string.IsNullOrEmpty(ev.unitId))
                        continue;
                    UnitEntry target = entriesById[ev.unitId];
                    if (target.EventWindows == null)
                        target.EventWindows = new List<EventWindow>();
                    target.EventWindows.Add(new EventWindow(ev.t0, ev.t1));
                }
            }

            // obscuration rides its own component but the SAME parsed battle:
            // authored events resolve emitter positions from each event's own
            // unit's track, and dust derives only from CHILDLESS units — a
            // decomposed brigade's parent track is the far-tier record of the
            // same men, so deriving from both would double-dust the family
            var tracksById = new Dictionary<string, UnitTrack>(units.Count);
            var childlessTracks = new List<UnitTrack>();
            // authored file order, not dictionary order: emitter order feeds
            // ObscurationField's deterministic overflow clamp
            foreach (UnitDto u in battle.units)
            {
                UnitEntry entry = entriesById[u.id];
                tracksById[u.id] = entry.Track;
                if (entry.Children == null)
                    childlessTracks.Add(entry.Track);
            }
            gameObject.AddComponent<ObscurationField>().Init(
                battle, tracksById, childlessTracks, clock, terrain,
                smokeMaterial, dustMaterial);
            // and the soundscape: same parsed battle, same attach-level rule
            // (emitters sound from their own track at t, whatever tier the
            // LOD ladder currently draws)
            gameObject.AddComponent<AcousticField>().Init(
                battle, tracksById, clock, terrain);
            // and the label layer: reads the candidate slots this director
            // fills each Update; its TMP pool is created in ITS Start, so
            // headless EditMode drives of this component never touch TMP
            gameObject.AddComponent<UnitLabelField>().Init(this, lodCamera);
        }

        void OnDestroy()
        {
            // the per-unit symbol meshes are runtime objects, not assets —
            // release them with the director (mode-appropriate, like the
            // marker colliders before them)
            foreach (UnitEntry entry in units)
            {
                DestroyMesh(entry.SymbolMesh);
                if (entry.RosterMeshes == null)
                    continue;
                foreach (Mesh m in entry.RosterMeshes)
                    DestroyMesh(m);
            }
        }

        static void DestroyMesh(Mesh mesh)
        {
            if (mesh == null) return;
            if (Application.isPlaying) Destroy(mesh);
            else DestroyImmediate(mesh);
        }

        void Update()
        {
            float baseY = terrain.transform.position.y;
            groundYBase = baseY;
            // the Flag shader's wave runs on battle time, set once per frame
            Shader.SetGlobalFloat(BattleWaveTimeId, clock.CurrentTime);
            unionFlagCount = 0;
            csaFlagCount = 0;
            // every label slot starts absent; rendering members re-stamp
            // theirs below (fixed slots — see the field comment)
            for (int i = 0; i < labelPriorities.Length; i++)
                labelPriorities[i] = float.PositiveInfinity;
            pickCount = 0;
            foreach (UnitEntry entry in units)
            {
                if (entry.Parent != null)
                    continue; // children render on their family's pass below
                UnitState s = entry.Track.StateAt(clock.CurrentTime);

                // one representative height sample (unit center) is enough to
                // judge camera distance
                float centerY = baseY + terrain.SampleHeight(new Vector3(s.posXZ.x, 0f, s.posXZ.y));
                Vector3 worldPos = new Vector3(s.posXZ.x, centerY, s.posXZ.y);

                // no camera (editor edge case): treat everything as far so the
                // familiar symbol path keeps working
                float dist = lodCamera != null
                    ? Vector3.Distance(lodCamera.transform.position, worldPos)
                    : float.MaxValue;
                // the hysteresis latch lives on this entry — for a family,
                // that's the parent: one center, one tier, atomic swap
                entry.Tier = EvaluateTier(dist, entry.Tier);

                RenderMember(entry, s, entry.Tier, baseY);
                if (entry.Children == null)
                    continue;
                for (int i = 0; i < entry.Children.Count; i++)
                {
                    UnitEntry child = entry.Children[i];
                    RenderMember(child, child.Track.StateAt(clock.CurrentTime),
                        entry.Tier, baseY);
                }
            }
            RenderFlags();
            // after the render pass: the pick buffer now holds exactly this
            // frame's rendered symbol footprints
            HandleSelectionClick();
        }

        // Left-click pick (plan Task 6): raymarch the cursor ray to the
        // displayed terrain, then point-in-oriented-footprint over the
        // rendered symbols — smallest area wins, so a regiment beats its
        // overlapping brigade ribbon. Empty ground (or sky) clears. No
        // physics, no colliders — the same pure math the tests pin. Clicks
        // over the HUD strip or a live IMGUI control never pick; at the
        // Soldiers tier there is no symbol footprint to hit, so close-zoom
        // clicks on figures fall through to the ground (selection is a
        // symbol-tier gesture, and the current selection persists).
        void HandleSelectionClick()
        {
            if (lodCamera == null || !Input.GetMouseButtonDown(0))
                return;
            if (GUIUtility.hotControl != 0)
                return; // the scrubber (or another IMGUI control) owns this press
            if (TimelineHud.IsTouchOverHud(
                    Input.mousePosition, TimelineHud.CurrentHudHeightPx))
                return;
            Ray ray = lodCamera.ScreenPointToRay(Input.mousePosition);
            if (!UnitPicker.RaycastTerrain(ray.origin, ray.direction,
                    groundYFunc, PickMaxDistM, out Vector3 ground))
            {
                if (SelectedHasFootprint()) SetSelected(null);
                return;
            }
            int picked = UnitPicker.PickUnit(
                new Vector2(ground.x, ground.z), pickCount,
                pickCenters, pickFacings, pickFrontages, pickDepths);
            if (picked >= 0)
            {
                SetSelected(pickEntries[picked]);
                return;
            }
            // a miss clears only when the selected unit is itself on the
            // board as a symbol. At the Soldiers tier its family registers
            // no footprint, so every close-zoom click would read as a
            // "miss" — clearing there would drop the citation line the
            // moment you zoomed in to look at the unit it describes
            // (review finding: enforce the persistence promised above).
            if (SelectedHasFootprint()) SetSelected(null);
        }

        // Did the currently selected unit register a pick footprint this
        // frame (i.e., is it on the board as a symbol rather than figures)?
        // Linear scan of <= unit-count entries, and only on click frames.
        bool SelectedHasFootprint()
        {
            if (selectedEntry == null) return false;
            for (int i = 0; i < pickCount; i++)
                if (pickEntries[i] == selectedEntry) return true;
            return false;
        }

        // Selection state: rewrite the two entries' MPBs on the transition
        // only (the latch discipline — never per frame). The selected
        // symbol reads at full saturation regardless of salience and wears
        // the boosted border; its next SymbolStale check also fires (the
        // selection input of the dirty predicate), keeping mesh-state
        // bookkeeping honest.
        void SetSelected(UnitEntry next)
        {
            if (next == selectedEntry)
                return;
            UnitEntry prev = selectedEntry;
            selectedEntry = next;
            if (prev != null) RefreshSelectionMpb(prev);
            if (next != null) RefreshSelectionMpb(next);
        }

        void RefreshSelectionMpb(UnitEntry entry)
        {
            bool selected = entry == selectedEntry;
            Color color = entry.Active || selected
                ? entry.ActiveColor : InactiveColor(entry.ActiveColor);
            float boost = selected ? SelectedBorderBoost : 0f;
            entry.ColorBlock.SetColor(BaseColorId, color);
            entry.ColorBlock.SetFloat(BorderWeightId,
                SymbolBorderWeight(entry.Echelon) + boost);
            if (entry.RosterBlock != null)
            {
                // the roster ribbons highlight with their brigade — one
                // selection, two blocks (same shape as the salience latch)
                entry.RosterBlock.SetColor(BaseColorId, color);
                entry.RosterBlock.SetFloat(BorderWeightId,
                    SymbolBorderWeight(UnitSymbol.Echelon.Regiment) + boost);
            }
        }

        // The TimelineHud citation line's data feed: the selected unit's
        // track (name, keyframes, StateAt) and its echelon. False = nothing
        // selected, draw no line.
        public bool TryGetSelected(out UnitTrack track, out UnitSymbol.Echelon echelon)
        {
            track = selectedEntry?.Track;
            echelon = selectedEntry != null
                ? selectedEntry.Echelon : UnitSymbol.Echelon.Brigade;
            return selectedEntry != null;
        }

        // Two RenderMeshInstanced calls total — one per side across ALL
        // units (the class-homogeneous MPB pattern, like FenceField). The
        // flag material must be an ASSET (the magenta/stripping lesson);
        // when unset, say so once and keep everything else rendering.
        void RenderFlags()
        {
            if (flagMaterial == null)
            {
                if (!warnedNoFlagMaterial)
                {
                    Debug.LogWarning(
                        "BattleDirector.flagMaterial is unset; wire Assets/Battle/Flag.mat " +
                        "to fly unit flags");
                    warnedNoFlagMaterial = true;
                }
                return;
            }
            var rp = new RenderParams(flagMaterial)
            {
                // a waving flag's shadow would flicker across the whole
                // symbol at strategic zoom; flags never cast (the B8 audit)
                shadowCastingMode = ShadowCastingMode.Off,
            };
            if (unionFlagCount > 0)
            {
                rp.matProps = unionFlagBlock;
                Graphics.RenderMeshInstanced(
                    rp, flagMesh, 0, unionFlagMatrices, unionFlagCount);
            }
            if (csaFlagCount > 0)
            {
                rp.matProps = csaFlagBlock;
                Graphics.RenderMeshInstanced(
                    rp, flagMesh, 0, csaFlagMatrices, csaFlagCount);
            }
        }

        // Renders one unit at the given tier — or skips it when the family
        // contract says another tier's representation owns the family right
        // now (symbols are drawn per frame, so a suppressed unit simply
        // isn't submitted; there is no marker to hide). For parentless,
        // childless units RendersAtTier is always true, so this is exactly
        // the pre-family tier dispatch.
        void RenderMember(UnitEntry entry, UnitState s, LodTier tier, float baseY)
        {
            if (!RendersAtTier(entry.Parent != null, entry.Children != null, tier))
                return;

            // a rendering member flies its flag — brigade colors at the block
            // tier resolve into regiment flags exactly when the tracks do;
            // hidden family representations never double-fly
            float flagY = baseY + terrain.SampleHeight(new Vector3(s.posXZ.x, 0f, s.posXZ.y));
            var flagMatrix = Matrix4x4.TRS(
                new Vector3(s.posXZ.x, flagY + FlagPoleHeight, s.posXZ.y),
                Quaternion.Euler(0f, s.facingDeg, 0f), Vector3.one);
            if (entry.IsUnion) unionFlagMatrices[unionFlagCount++] = flagMatrix;
            else csaFlagMatrices[csaFlagCount++] = flagMatrix;

            // activity salience: latch per entry, MPBs rewritten only on a
            // transition — 190 symbols never see a per-frame SetColor
            // (RenderParams reads the block live at draw time, so no re-set
            // is needed). The flag blocks above and the figure color are
            // untouched: flags keep full side color (identity), figures
            // never desaturate. Suppressed family members skip this (early
            // return above) and reconcile through the same latch check when
            // they reappear.
            bool active = IsActiveAt(entry.Track, entry.EventWindows, clock.CurrentTime);
            if (active != entry.Active)
            {
                entry.Active = active;
                // a selected symbol holds full saturation whatever the
                // salience says — selection is deliberate attention
                Color color = active || entry == selectedEntry
                    ? entry.ActiveColor : InactiveColor(entry.ActiveColor);
                entry.ColorBlock.SetColor(BaseColorId, color);
                // the roster ribbons dim with their brigade — one latch,
                // two blocks
                entry.RosterBlock?.SetColor(BaseColorId, color);
            }

            // label candidate (plan D3): the unit's own slot, gated by the
            // tier rule. A roster partition labels through its roster-name
            // slots instead (RenderRosterSymbols) — the brigade line only
            // keeps its own name while selected. Inactive labels take the
            // same desaturated tint as their symbol.
            bool roster = RendersRosterSymbols(entry.RegimentCount, s.formation, tier);
            bool selected = entry == selectedEntry;
            if ((!roster || selected)
                && LabelLayout.LabelsAtTier(tier, entry.Echelon, active, selected))
            {
                int slot = entry.LabelSlot;
                labelPriorities[slot] = LabelLayout.Priority(
                    entry.Echelon, active, selected, entry.Track.Unit.id);
                labelPositions[slot] = new Vector3(
                    s.posXZ.x, flagY + LabelLiftM, s.posXZ.y);
                labelTexts[slot] = entry.DisplayName;
                labelColors[slot] = active || selected
                    ? entry.ActiveColor : InactiveColor(entry.ActiveColor);
            }

            if (tier == LodTier.Soldiers)
            {
                // pose bias input: the same symmetric moving window the
                // salience latch uses (IsMovingAt), shared so the two can
                // never disagree about "moving" at the same t
                entry.FormationRenderer.Render(
                    s, IsMovingAt(entry.Track, clock.CurrentTime), groundYFunc);
                return;
            }

            // provenance: the bracketing START keyframe's confidence drives
            // the fill style (StyleOf carries the 2026-07-09 two-state
            // ruling); latched like the color — most units never flip. The
            // roster MPB stays permanently Hatched: its strength split is
            // display-inferred whatever the parent's keyframes attest.
            UnitSymbol.FillStyle style = UnitSymbol.StyleOf(s.confidence);
            if (style != entry.FillStyle)
            {
                entry.FillStyle = style;
                entry.ColorBlock.SetFloat(FillStyleId, (float)style);
            }

            if (roster)
                RenderRosterSymbols(entry, s, tier);
            else
                RenderSymbol(entry, s, tier);
        }

        // Has anything a viewer could see changed since the last build? The
        // first render (no prev yet) always builds.
        bool SymbolStale(UnitEntry entry, UnitState s, LodTier tier)
        {
            return !entry.HasPrev || SymbolNeedsRebuild(
                entry.PrevState, s, entry.Track.Unit.frontage_m,
                entry.Kind, entry.PrevTier, tier,
                entry.PrevSelected, entry == selectedEntry);
        }

        // Records what a rebuild consumed; the caller marks which
        // representation it fed.
        void CommitBuild(UnitEntry entry, UnitState s, LodTier tier)
        {
            entry.PrevState = s;
            entry.PrevTier = tier;
            entry.PrevSelected = entry == selectedEntry;
            entry.HasPrev = true;
        }

        // Registers this frame's rendered symbol footprint for the click
        // picker (the cavalry shear and roster partition are picked by
        // their bounding rectangle — footprint grain, deliberately).
        void RegisterPickFootprint(
            UnitEntry entry, Vector2 centerXZ, float facingDeg,
            float frontage, float depth)
        {
            pickCenters[pickCount] = centerXZ;
            pickFacings[pickCount] = facingDeg;
            pickFrontages[pickCount] = frontage;
            pickDepths[pickCount] = depth;
            pickEntries[pickCount] = entry;
            pickCount++;
        }

        // Writes one BuildRibbon result from the shared scratch into a
        // persistent mesh: two submeshes on one vertex stream — body (fill)
        // and border band — so both draw with the same material and MPB.
        // Normals stay untouched: the symbol shader is unlit map ink.
        void ApplyScratchToMesh(Mesh mesh, SymbolMeshBuilder.SymbolCounts counts)
        {
            mesh.Clear();
            mesh.SetVertices(symbolVerts, 0, counts.VertexCount);
            mesh.SetUVs(0, symbolUvs, 0, counts.VertexCount);
            mesh.subMeshCount = 2;
            mesh.SetTriangles(symbolTris, 0, counts.BodyIndexCount, 0, false);
            mesh.SetTriangles(symbolTris, counts.BodyIndexCount,
                counts.BorderIndexCount, 1, false);
            // vertices are world-space (the ribbon drapes the terrain in
            // place, drawn with an identity matrix), so the recalculated
            // bounds ARE the world-space culling bounds
            mesh.RecalculateBounds();
        }

        // Block tier (and the middle-tier fallback): the unit's monolithic
        // draped symbol — one persistent mesh, rebuilt only when stale.
        void RenderSymbol(UnitEntry entry, UnitState s, LodTier tier)
        {
            if (!entry.SymbolBuilt || SymbolStale(entry, s, tier))
            {
                if (entry.SymbolMesh == null)
                {
                    entry.SymbolMesh = new Mesh { name = $"symbol {entry.Track.Unit.id}" };
                    entry.SymbolMesh.MarkDynamic();
                }
                float frontage = entry.Track.Unit.frontage_m;
                SymbolMeshBuilder.SymbolCounts counts = SymbolMeshBuilder.BuildRibbon(
                    entry.Track.Unit.id, s.posXZ, s.facingDeg, frontage,
                    UnitSymbol.DisplayDepth(s.strength, frontage),
                    entry.Kind, s.formation, UnitSymbol.GunDotCount(s.strength),
                    groundYFunc, SymbolMeshBuilder.DefaultLiftM,
                    symbolVerts, symbolUvs, symbolTris);
                ApplyScratchToMesh(entry.SymbolMesh, counts);
                entry.BodyIndexCount = counts.BodyIndexCount;
                entry.BorderIndexCount = counts.BorderIndexCount;
                CommitBuild(entry, s, tier);
                entry.SymbolBuilt = true;
                entry.RosterBuilt = false;
            }
            RegisterPickFootprint(entry, s.posXZ, s.facingDeg,
                entry.Track.Unit.frontage_m,
                UnitPicker.PickDepth(entry.Kind, UnitSymbol.DisplayDepth(
                    s.strength, entry.Track.Unit.frontage_m)));
            var rp = new RenderParams(symbolMaterial)
            {
                matProps = entry.ColorBlock,
                // map ink casts no shadow — a symbol's shadow on the relief
                // would read as height it doesn't have
                shadowCastingMode = ShadowCastingMode.Off,
            };
            // skip empty submeshes: a park has no body, a skirmish line no
            // border — two RenderMesh calls at most per symbol
            if (entry.BodyIndexCount > 0)
                Graphics.RenderMesh(rp, entry.SymbolMesh, 0, Matrix4x4.identity);
            if (entry.BorderIndexCount > 0)
                Graphics.RenderMesh(rp, entry.SymbolMesh, 1, Matrix4x4.identity);
        }

        // Middle tier for roster-carrying brigades: the RegimentSlots
        // partition as regiment-echelon ribbons, one persistent mesh per
        // slot, all sharing the roster MPB (regiment border, hatched
        // display-inferred fill, the brigade's salience color).
        void RenderRosterSymbols(UnitEntry entry, UnitState s, LodTier tier)
        {
            int count = entry.RegimentCount;
            // specs every frame (cheap pure math into shared scratch): the
            // roster LABEL anchors need current slot centers even on the
            // many frames where no ribbon rebuild fires
            RosterSymbolSpecs(s, entry.Track.Unit.frontage_m,
                entry.Track.Unit.depth_m, count, slotsBuffer, rosterSpecsBuffer);
            // roster-name labels, one fixed slot per ribbon (always at the
            // Regiments tier here, where every member is a candidate);
            // regiment priority, the roster name as the FNV tie-break key
            for (int i = 0; i < count; i++)
            {
                RosterSymbolSpec spec = rosterSpecsBuffer[i];
                int slot = entry.RosterLabelBase + i;
                labelPriorities[slot] = LabelLayout.Priority(
                    UnitSymbol.Echelon.Regiment, entry.Active, false,
                    entry.RosterNames[i]);
                labelPositions[slot] = new Vector3(
                    spec.CenterXZ.x,
                    GroundY(spec.CenterXZ.x, spec.CenterXZ.y) + LabelLiftM,
                    spec.CenterXZ.y);
                labelTexts[slot] = entry.RosterNames[i];
                labelColors[slot] = entry.Active
                    ? entry.ActiveColor : InactiveColor(entry.ActiveColor);
            }
            if (!entry.RosterBuilt || SymbolStale(entry, s, tier))
            {
                for (int i = 0; i < count; i++)
                {
                    if (entry.RosterMeshes[i] == null)
                    {
                        entry.RosterMeshes[i] = new Mesh
                        {
                            name = $"symbol {entry.Track.Unit.id} roster {i}",
                        };
                        entry.RosterMeshes[i].MarkDynamic();
                    }
                    RosterSymbolSpec spec = rosterSpecsBuffer[i];
                    SymbolMeshBuilder.SymbolCounts counts = SymbolMeshBuilder.BuildRibbon(
                        entry.Track.Unit.id, spec.CenterXZ, s.facingDeg, spec.Frontage,
                        spec.DisplayDepth, entry.Kind, s.formation,
                        UnitSymbol.GunDotCount(spec.StrengthShare),
                        groundYFunc, SymbolMeshBuilder.DefaultLiftM,
                        symbolVerts, symbolUvs, symbolTris);
                    ApplyScratchToMesh(entry.RosterMeshes[i], counts);
                    entry.RosterBodyCounts[i] = counts.BodyIndexCount;
                    entry.RosterBorderCounts[i] = counts.BorderIndexCount;
                }
                CommitBuild(entry, s, tier);
                entry.RosterBuilt = true;
                entry.SymbolBuilt = false;
            }
            // the partition picks as ONE brigade rectangle — roster ribbons
            // have no tracks or citations of their own to select
            RegisterPickFootprint(entry, s.posXZ, s.facingDeg,
                entry.Track.Unit.frontage_m,
                UnitSymbol.DisplayDepth(s.strength, entry.Track.Unit.frontage_m));
            var rp = new RenderParams(symbolMaterial)
            {
                matProps = entry.RosterBlock,
                shadowCastingMode = ShadowCastingMode.Off,
            };
            for (int i = 0; i < count; i++)
            {
                if (entry.RosterBodyCounts[i] > 0)
                    Graphics.RenderMesh(rp, entry.RosterMeshes[i], 0, Matrix4x4.identity);
                if (entry.RosterBorderCounts[i] > 0)
                    Graphics.RenderMesh(rp, entry.RosterMeshes[i], 1, Matrix4x4.identity);
            }
        }
    }
}
