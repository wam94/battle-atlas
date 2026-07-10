using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleAtlas
{
    // ------------------------------------------------------------------
    // Phase 8: the deterministic action resolver (plan §7.5).
    //
    // NOT combat AI. A pure function:
    //
    //   SoldierState Resolve(segment, battleTime, unitId, slotId,
    //                        casualtyProfiles, engagementEvents, battleSeed)
    //
    // realized as Resolve(ctx, unitIndex, slotId, t) over an
    // AngleActionContext compiled once from the bundle + seed (the compile
    // itself is pure: same bundle + seed + obstacle geometry -> identical
    // context). No state is kept between calls; no Random, no Time.*;
    // scrubbing in any order reconstructs identical states (§13 tests).
    // ------------------------------------------------------------------

    // Logical soldier state — the thing Gate P8 hashes for the
    // bitwise-identical proof. Every field is a pure function of
    // (bundle, seed, unitId, slotId, t).
    public struct SoldierState
    {
        public float posX, posZ;      // macro battlefield meters
        public float facingDeg;
        public ClipId clip;
        public float clipTime;
        public byte status;           // SoldierStatus
        public byte cause;            // CasualtySchedule.Cause when status>0
        public byte wound;            // CasualtySchedule.WoundCategory
        public byte variant;          // kit variant index 0..2
        public byte equip;            // equipment-variation byte (bit7: no musket)

        public const byte StatusAlive = 0;
        public const byte StatusFalling = 1;
        public const byte StatusDead = 2;
        public const byte StatusCrawling = 3;

        public bool Fallen => status != StatusAlive;
    }

    // Incoming-fire window for a target unit (compiled engagement event).
    public struct IncomingFire
    {
        public float t0, t1;
        public byte kind;             // CasualtySchedule.Cause of the source
        public float bearingDeg;      // world bearing the fire ARRIVES FROM
    }

    // A projectile strike near a unit (canister/shell ground impact).
    public struct StrikeEvent
    {
        public float t;
        public Vector2 pos;           // macro meters
    }

    public class UnitRuntime
    {
        public AngleBundleUnit unit;
        public int unitIndex;
        public int slotCount;
        public bool isArtillery;
        public CasualtySchedule.Entry[] casualties;
        public List<FireCycles.CannonShot> cannonShots;  // artillery only
        public List<StrikeEvent> strikes = new();        // incoming, by time
        public List<IncomingFire> incoming = new();
        // per slot: sorted obstacle-crossing start times (empty = never
        // crosses a traced line). A slot may cross several fences in one
        // road corridor and re-cross during the repulse.
        public float[][] slotCrossings;

        // cumulative centroid arc length at 1 Hz (meters walked along the
        // compiled track since slice start) — locomotion stride phase is a
        // pure function of this distance, so stride rate always matches
        // ground speed (P8 locomotion review fix).
        public float[] arc;

        // FORMATION-FRAME facing at 1 Hz: the compiled facing with
        // half-turn steps unwrapped (each inter-second delta reduced to
        // (-90°, 90°] mod 180°). Placement must use this, never the raw
        // facing: rotating slot offsets through a compiled about-face
        // (e.g. Garnett's fall_back at t=8700 steps 78.7°->264.0°)
        // point-reflects the formation about its centroid and teleports a
        // flank man ~134 m in one second. In drill terms an about-face
        // leaves every man standing where he stood — the frame changes
        // handedness, the men do not run around the colors. Placing with
        // the unwrapped frame yaw realizes exactly that (front rank
        // becomes rear rank relative to the new facing), and only the
        // residual wheel (5.3° in the Garnett case) moves anybody.
        // Body facing (SoldierState.facingDeg) still uses the raw
        // compiled facing + MotionBearing. Found by the Phase 9 hero
        // camera comfort bound; fixes figures and camera alike.
        public float[] frameFacing;

        // precomputed hash keys (avoid per-call string churn)
        public string keyVariant, keyStep, keyYaw, keyWaver, keyHit,
            keyRetreat, keyFall, keyCrawl;
    }

    public class AngleActionContext
    {
        public AngleBundle bundle;
        public string seed;
        public List<UnitRuntime> units = new();

        // §12 Phase 8 gate: "no parent/child double-count". The bundle
        // carries Webb's brigade AND its wall regiments as separate units
        // on overlapping ground; the scene stages the regiments and
        // excludes the parent roll-up from figures, casualties, and VFX.
        public static readonly Dictionary<string, string[]> ParentChildren =
            new()
            {
                { "us-webb", new[] { "us-69pa", "us-71pa", "us-72pa" } },
            };

        public UnitRuntime Unit(string unitId)
        {
            foreach (var u in units)
                if (u.unit.unitId == unitId) return u;
            throw new InvalidOperationException($"context has no unit '{unitId}'");
        }

        public int TotalSlots
        {
            get
            {
                int n = 0;
                foreach (var u in units) n += u.slotCount;
                return n;
            }
        }

        // Obstacle crossing lookup: featureId -> x(z) table (the traced
        // fences and wall run roughly north-south, so a z-binned x table
        // resolves "which side of the obstacle is this point on").
        public class CrossingTable
        {
            public float zMin, zStep;
            public float[] xAtZ;

            public float XAt(float z)
            {
                if (xAtZ.Length == 0) return float.NaN;
                int i = Mathf.Clamp(
                    Mathf.RoundToInt((z - zMin) / zStep), 0, xAtZ.Length - 1);
                return xAtZ[i];
            }

            public static CrossingTable Build(List<Vector2> polyline, float step = 2f)
            {
                float zMin = float.MaxValue, zMax = float.MinValue;
                foreach (var p in polyline)
                {
                    zMin = Mathf.Min(zMin, p.y);
                    zMax = Mathf.Max(zMax, p.y);
                }
                int n = Mathf.Max(1, Mathf.CeilToInt((zMax - zMin) / step) + 1);
                var xs = new float[n];
                for (int i = 0; i < n; i++)
                {
                    float z = zMin + i * step;
                    // nearest polyline point/segment at this z
                    float best = float.MaxValue, bx = polyline[0].x;
                    for (int s = 0; s < polyline.Count - 1; s++)
                    {
                        Vector2 a = polyline[s], b = polyline[s + 1];
                        float dz = b.y - a.y;
                        float tt = Mathf.Abs(dz) < 1e-4f
                            ? 0f : Mathf.Clamp01((z - a.y) / dz);
                        Vector2 q = Vector2.Lerp(a, b, tt);
                        float d = Mathf.Abs(q.y - z);
                        if (d < best) { best = d; bx = q.x; }
                    }
                    xs[i] = bx;
                }
                return new CrossingTable { zMin = zMin, zStep = step, xAtZ = xs };
            }
        }

        public Dictionary<string, CrossingTable> crossings = new();

        // ---------------- compile ----------------

        public static AngleActionContext Compile(
            AngleBundle bundle, string seed,
            IDictionary<string, List<Vector2>> obstaclePolylines)
        {
            var ctx = new AngleActionContext { bundle = bundle, seed = seed };
            if (obstaclePolylines != null)
                foreach (var kv in obstaclePolylines)
                    ctx.crossings[kv.Key] = CrossingTable.Build(kv.Value);

            var excluded = new HashSet<string>(ParentChildren.Keys);
            foreach (var u in bundle.units)
            {
                if (excluded.Contains(u.unitId)) continue;
                var ur = new UnitRuntime
                {
                    unit = u,
                    unitIndex = ctx.units.Count,
                    slotCount = u.startStrength,
                    isArtillery = u.arm == "artillery",
                    casualties = CasualtySchedule.Compile(u, seed),
                    keyVariant = seed + "|" + u.unitId + "|variant",
                    keyStep = seed + "|" + u.unitId + "|step",
                    keyYaw = seed + "|" + u.unitId + "|yaw",
                    keyWaver = seed + "|" + u.unitId + "|waver",
                    keyHit = seed + "|" + u.unitId + "|hit",
                    keyRetreat = seed + "|" + u.unitId + "|retreat",
                    keyFall = seed + "|" + u.unitId + "|fall",
                    keyCrawl = seed + "|" + u.unitId + "|crawl",
                };
                if (ur.isArtillery)
                    ur.cannonShots = FireCycles.CompileCannon(
                        seed, u, FormationRoster.GunsPerBattery);
                var ps = u.perSecond;
                ur.arc = new float[ps.x.Count];
                for (int i = 1; i < ps.x.Count; i++)
                {
                    float dx = ps.x[i] - ps.x[i - 1];
                    float dz = ps.z[i] - ps.z[i - 1];
                    ur.arc[i] = ur.arc[i - 1] + Mathf.Sqrt(dx * dx + dz * dz);
                }
                ur.frameFacing = new float[ps.facingDeg.Count];
                ur.frameFacing[0] = ps.facingDeg[0];
                for (int i = 1; i < ps.facingDeg.Count; i++)
                {
                    float d = Mathf.DeltaAngle(
                        ps.facingDeg[i - 1], ps.facingDeg[i]);
                    while (d > 90f) d -= 180f;
                    while (d <= -90f) d += 180f;
                    ur.frameFacing[i] = ur.frameFacing[i - 1] + d;
                }
                SmoothFormationFrame(ur);
                ctx.units.Add(ur);
            }

            ctx.CompileEngagements();
            foreach (var ur in ctx.units) ctx.CompileCrossings(ur);
            return ctx;
        }

        // P10 must-fix (the Gate P9 "teleport" class): the compiled
        // per-second facing table can STEP at segment boundaries — e.g.
        // Armistead's advance->breach boundary steps 82.8° -> 99.5° in the
        // single second t=8639..8640, and every CSA brigade carries a
        // 17-20°/s wheel at some boundary. Placement rotates slot offsets
        // by this frame, so a one-second step wheels the whole line at
        // once: a flank man on a ~370 m frontage sweeps at up to ~65 m/s
        // (measured; the P9 evidence's "flank translation" limitation is
        // the same defect seen through the ±2.6 s camera smoother). Men
        // cannot outrun the frame. Spread each unit's placement frame with
        // a symmetric triangular kernel sized from ITS OWN worst step and
        // widest offset so no wheel moves any man faster than
        // MaxWheelFlankSpeedMps. Body facing (SoldierState.facingDeg)
        // still reads the raw compiled table — a man may snap his own
        // shoulders in a second; the LINE may not. Symmetric, precomputed,
        // pure -> scrub-invariant. Units without fast wheels (all Union
        // units here) are untouched.
        public const float MaxWheelFlankSpeedMps = 3.0f;

        static void SmoothFormationFrame(UnitRuntime ur)
        {
            var ff = ur.frameFacing;
            if (ff.Length < 3) return;
            // widest plan offset a slot can carry in any formation
            float radius = ur.isArtillery
                ? FormationRoster.GunsPerBattery * FormationRoster.GunSpacingM / 2f
                : FormationRoster.Frontage(ur.slotCount) / 2f;
            float need = 0f;
            for (int i = 1; i < ff.Length; i++)
                need = Mathf.Max(need, Mathf.Abs(ff[i] - ff[i - 1])
                    * Mathf.Deg2Rad * radius / MaxWheelFlankSpeedMps);
            // triangular kernel of half-width W turns a step S into a ramp
            // with peak slope S/(W+1) per second
            int w = Mathf.CeilToInt(Mathf.Min(need, 30f));
            if (w <= 1) return;
            var smoothed = new float[ff.Length];
            for (int i = 0; i < ff.Length; i++)
            {
                float sum = 0f, wSum = 0f;
                for (int k = -w; k <= w; k++)
                {
                    int j = Mathf.Clamp(i + k, 0, ff.Length - 1);
                    float weight = w + 1 - Mathf.Abs(k);
                    sum += ff[j] * weight;
                    wSum += weight;
                }
                smoothed[i] = sum / wSum;
            }
            ur.frameFacing = smoothed;
        }

        void CompileEngagements()
        {
            // incoming-fire windows (fall directions, reactions)
            foreach (var target in units)
            {
                foreach (var source in units)
                {
                    if (source.unit.side == target.unit.side) continue;
                    foreach (var seg in source.unit.segments)
                    {
                        if (!FireCycles.IsFireAction(seg.action)) continue;
                        float mid = (seg.t0 + seg.t1) / 2f;
                        Vector2 from = source.unit.PositionAt(mid);
                        Vector2 at = target.unit.PositionAt(mid);
                        Vector2 d = from - at;
                        if (d.magnitude > 700f) continue;
                        target.incoming.Add(new IncomingFire
                        {
                            t0 = seg.t0,
                            t1 = seg.t1,
                            kind = (byte)(source.isArtillery
                                ? CasualtySchedule.Cause.Canister
                                : CasualtySchedule.Cause.Musketry),
                            bearingDeg = Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg,
                        });
                    }
                }
            }

            // canister/shell strikes: every cannon shot lands near the
            // nearest enemy infantry unit (deterministic scatter)
            foreach (var battery in units)
            {
                if (!battery.isArtillery || battery.cannonShots == null) continue;
                string key = seed + "|" + battery.unit.unitId + "|strike";
                for (int i = 0; i < battery.cannonShots.Count; i++)
                {
                    var shot = battery.cannonShots[i];
                    UnitRuntime targetUr = null;
                    float best = float.MaxValue;
                    Vector2 gunPos = battery.unit.PositionAt(shot.t);
                    foreach (var cand in units)
                    {
                        if (cand.unit.side == battery.unit.side) continue;
                        if (cand.isArtillery) continue;
                        float d = (cand.unit.PositionAt(shot.t) - gunPos).magnitude;
                        if (d < best) { best = d; targetUr = cand; }
                    }
                    if (targetUr == null || best > 650f) continue;
                    Vector2 c = targetUr.unit.PositionAt(shot.t + 0.4f);
                    float frontage = FormationRoster.Frontage(targetUr.slotCount);
                    var impact = new Vector2(
                        c.x + (frontage / 3f) *
                            (2f * AngleEnvironmentLayout.Hash01(key, i * 7 + 1) - 1f),
                        c.y + 12f *
                            (2f * AngleEnvironmentLayout.Hash01(key, i * 7 + 2) - 1f));
                    targetUr.strikes.Add(new StrikeEvent
                    {
                        t = shot.t + 0.4f,
                        pos = impact,
                    });
                }
            }
            foreach (var ur in units)
                ur.strikes.Sort((a, b) => a.t.CompareTo(b.t));
        }

        // Per-slot obstacle-crossing times: a slot crosses when its OWN
        // resolved path crosses a traced obstacle line, detected in a
        // widened window around each obstacle-declaring EPISODE.
        //
        // P9 fix (found by the hero viewpoint): consecutive crossing
        // segments (Garnett's two road-fence segments, 8120..8180 and
        // 8280..8340) are one corridor EPISODE for a 348 m-wide line —
        // the far flank trails the centroid so much that it reaches the
        // traced fences ~50 s after the LAST crossing segment ends, and
        // the old per-segment ±30 s windows (clamped at the neighboring
        // segment) missed it entirely: flank men walked THROUGH the
        // rails. Episodes merge crossing segments separated by <120 s,
        // pool their obstacle tables, and pad the episode window by
        // 120 s each side (still clamped against the neighboring
        // episode), so every man crosses where and WHEN his own path
        // meets the traced line.
        void CompileCrossings(UnitRuntime ur)
        {
            const float WindowPad = 120f;
            const float EpisodeGap = 120f;
            const float SampleStep = 0.5f;

            ur.slotCrossings = new float[ur.slotCount][];
            var perSlot = new List<float>[ur.slotCount];
            for (int s = 0; s < ur.slotCount; s++) perSlot[s] = new List<float>();

            var segs = ur.unit.segments;
            var crossingSegs = new List<int>();
            for (int si = 0; si < segs.Count; si++)
            {
                var seg = segs[si];
                bool crossingAction =
                    seg.action == "cross_obstacle" || seg.action == "breach" ||
                    seg.action == "fall_back";
                if (crossingAction && seg.obstacleIds != null &&
                    seg.obstacleIds.Count > 0)
                    crossingSegs.Add(si);
            }

            // group consecutive crossing segments into episodes
            var episodes = new List<(float t0, float t1, List<CrossingTable> tables)>();
            foreach (int si in crossingSegs)
            {
                var seg = segs[si];
                var tables = new List<CrossingTable>();
                foreach (var oid in seg.obstacleIds)
                    if (crossings.TryGetValue(oid, out var tab)) tables.Add(tab);
                if (tables.Count == 0) continue;
                if (episodes.Count > 0 &&
                    seg.t0 - episodes[^1].t1 < EpisodeGap)
                {
                    var last = episodes[^1];
                    foreach (var tab in tables)
                        if (!last.tables.Contains(tab)) last.tables.Add(tab);
                    episodes[^1] = (last.t0, Mathf.Max(last.t1, seg.t1), last.tables);
                }
                else
                {
                    episodes.Add((seg.t0, seg.t1, tables));
                }
            }

            for (int ci = 0; ci < episodes.Count; ci++)
            {
                var (et0, et1, tables) = episodes[ci];

                float lo = et0 - WindowPad;
                float hi = et1 + WindowPad;
                if (ci > 0) lo = Mathf.Max(lo, episodes[ci - 1].t1);
                if (ci < episodes.Count - 1)
                    hi = Mathf.Min(hi, episodes[ci + 1].t0);
                lo = Mathf.Max(lo, bundle.slice.t0);
                hi = Mathf.Min(hi, bundle.slice.t1);

                for (int slot = 0; slot < ur.slotCount; slot++)
                {
                    float prevSide = float.NaN;
                    CrossingTable prevTab = null;
                    for (float t = lo; t <= hi; t += SampleStep)
                    {
                        Vector2 p = SoldierActionResolver.BasePosition(
                            this, ur, slot, t,
                            segs[SoldierActionResolver.SegIndexAt(ur, t)]);
                        CrossingTable near = null;
                        float bestDx = float.MaxValue;
                        foreach (var tab in tables)
                        {
                            float dx = p.x - tab.XAt(p.y);
                            if (Mathf.Abs(dx) < Mathf.Abs(bestDx))
                            { bestDx = dx; near = tab; }
                        }
                        if (near == prevTab && !float.IsNaN(prevSide) &&
                            Mathf.Sign(bestDx) != Mathf.Sign(prevSide) &&
                            Mathf.Abs(bestDx) < 6f)
                        {
                            float crossT = t - SampleStep / 2f;
                            perSlot[slot].Add(crossT);
                            // a man crosses one rail line at a time
                            t = crossT + SoldierActionResolver.CrossDur + 1f;
                            prevSide = float.NaN;
                            prevTab = null;
                            continue;
                        }
                        prevSide = bestDx;
                        prevTab = near;
                    }
                }
            }
            for (int s = 0; s < ur.slotCount; s++)
            {
                perSlot[s].Sort();
                ur.slotCrossings[s] = perSlot[s].ToArray();
            }
        }
    }

    public static class SoldierActionResolver
    {
        public const float CrossDur = 4f;          // Cross_RailFence length
        public const float CrossTravelM = 1.3f;    // clip root motion, fwd
        public const float CatchupDur = 3f;
        public const float FlinchRadiusM = 8f;
        public const float BraceRadiusM = 18f;
        public const float CrawlDelay = 5f;        // fall end -> first crawl

        // P8 locomotion review fix ("floating from position to position"):
        // below this ground speed a man is STANDING (whatever the segment
        // action nominally is); at or above it he MUST play a locomotion
        // clip whose stride phase is keyed to track distance.
        public const float MoveThresholdMps = 0.25f;
        const float SpeedProbeDt = 0.6f;

        // ------------------------------------------------------------------
        public static SoldierState Resolve(
            AngleActionContext ctx, int unitIndex, int slot, float t)
        {
            if (unitIndex < 0 || unitIndex >= ctx.units.Count)
                throw new ArgumentOutOfRangeException(nameof(unitIndex));
            var ur = ctx.units[unitIndex];
            if (slot < 0 || slot >= ur.slotCount)
                throw new ArgumentOutOfRangeException(
                    nameof(slot), $"{ur.unit.unitId}: slot {slot} outside roster " +
                                  $"0..{ur.slotCount - 1}");
            t = Mathf.Clamp(t, ctx.bundle.slice.t0, ctx.bundle.slice.t1);

            var cas = ur.casualties[slot];
            var s = new SoldierState
            {
                variant = (byte)(AngleEnvironmentLayout.Hash01(ur.keyVariant, slot) * 2.999f),
                equip = (byte)((int)(AngleEnvironmentLayout.Hash01(
                    ur.keyVariant, slot * 31 + 7) * 127f) |
                    (ur.isArtillery ? 0x80 : 0)),
                wound = (byte)CasualtySchedule.Wound(cas.cause),
            };

            if (t >= cas.fallT)
            {
                ResolveFallen(ctx, ur, slot, t, cas, ref s);
                return s;
            }
            ResolveAlive(ctx, ur, slot, t, ref s);
            return s;
        }

        // Plan-signature facade (§7.5) for a single already-known segment;
        // unitId string lookup. Kept for tests/documentation symmetry.
        public static SoldierState Resolve(
            AngleActionContext ctx, string unitId, int slot, float battleTime)
        {
            var ur = ctx.Unit(unitId);
            return Resolve(ctx, ur.unitIndex, slot, battleTime);
        }

        // ------------------------------------------------------------------
        // Position pipeline (shared with crossing precompute): centroid +
        // blended formation offset. PURE in t.
        internal static Vector2 BasePosition(
            AngleActionContext ctx, UnitRuntime ur, int slot, float t,
            AngleBundleSegment seg)
        {
            Vector2 centroid = ur.unit.PositionAt(t);
            // placement uses the UNWRAPPED formation-frame facing (see
            // UnitRuntime.frameFacing): an about-face must not point-
            // reflect the formation
            float facing = FrameFacingAt(ur, t);
            Vector2 offset = ur.isArtillery
                ? FormationRoster.ArtilleryOffset(
                    ur.unit.unitId, slot, ur.slotCount)
                : FormationRoster.BlendedOffset(
                    ur.unit.unitId, slot, ur.slotCount,
                    seg.formationFrom, seg.formationTo, seg.Progress(t));
            return FormationRoster.ToWorld(centroid, facing, offset);
        }

        // Unwrapped formation-frame facing (linear lerp on the compiled
        // 1 Hz frame table; continuous by construction). Pure in t.
        internal static float FrameFacingAt(UnitRuntime ur, float t)
        {
            int n = ur.frameFacing.Length;
            float ft = Mathf.Clamp(t - ur.unit.perSecond.t0, 0f, n - 1);
            int i = Mathf.Min((int)ft, n - 2);
            return Mathf.Lerp(ur.frameFacing[i], ur.frameFacing[i + 1], ft - i);
        }

        internal static int SegIndexAt(UnitRuntime ur, float t)
        {
            var segs = ur.unit.segments;
            for (int i = segs.Count - 1; i >= 0; i--)
                if (t >= segs[i].t0) return i;
            return 0;
        }

        // Position including crossing choreography: hold at the obstacle
        // for the clip (its root motion carries the body over), then blend
        // back onto the formation position.
        static Vector2 PositionAt(
            AngleActionContext ctx, UnitRuntime ur, int slot, float t,
            int segIdx, out bool crossingNow, out bool hasCrossed)
        {
            var seg = ur.unit.segments[segIdx];
            crossingNow = false;
            hasCrossed = false;

            Vector2 basePos = BasePosition(ctx, ur, slot, t, seg);
            var times = ur.slotCrossings != null ? ur.slotCrossings[slot] : null;
            if (times == null || times.Length == 0) return basePos;

            // latest crossing that started at or before t
            float crossStart = float.NegativeInfinity;
            foreach (float c in times)
            {
                if (c > t) break;
                crossStart = c;
            }
            if (float.IsNegativeInfinity(crossStart)) return basePos;
            hasCrossed = t >= crossStart + CrossDur;

            float facing = FrameFacingAt(ur, crossStart);
            float r = facing * Mathf.Deg2Rad;
            var fwd = new Vector2(Mathf.Sin(r), Mathf.Cos(r));
            int holdSegIdx = SegIndexAt(ur, crossStart);
            Vector2 hold = BasePosition(
                ctx, ur, slot, crossStart, ur.unit.segments[holdSegIdx]);
            if (!hasCrossed)
            {
                crossingNow = true;
                return hold;   // clip root motion crosses the rail
            }
            float catchup = Mathf.Clamp01(
                (t - crossStart - CrossDur) / CatchupDur);
            if (catchup >= 1f) return basePos;
            return Vector2.Lerp(hold + fwd * CrossTravelM, basePos, catchup);
        }

        // The active crossing start (for clip phase), if crossingNow.
        static float ActiveCrossStart(UnitRuntime ur, int slot, float t)
        {
            var times = ur.slotCrossings[slot];
            float crossStart = float.NegativeInfinity;
            foreach (float c in times)
            {
                if (c > t) break;
                crossStart = c;
            }
            return crossStart;
        }

        // Meters the unit centroid has walked since slice start (1 Hz
        // compiled table, linear between samples). Pure in t.
        static float ArcAt(UnitRuntime ur, float t)
        {
            int n = ur.arc.Length;
            float ft = Mathf.Clamp(t - ur.unit.perSecond.t0, 0f, n - 1);
            int i = Mathf.Min((int)ft, n - 2);
            return Mathf.Lerp(ur.arc[i], ur.arc[i + 1], ft - i);
        }

        // The slot's own ground speed through the FULL position pipeline
        // (formation blend + crossing hold + catch-up), so a man gliding
        // for any reason is detected. Pure in t.
        static float SlotSpeed(
            AngleActionContext ctx, UnitRuntime ur, int slot, float t)
        {
            float ta = Mathf.Max(t - SpeedProbeDt, ctx.bundle.slice.t0);
            float tb = Mathf.Min(t + SpeedProbeDt, ctx.bundle.slice.t1);
            if (tb - ta < 1e-3f) return 0f;
            Vector2 a = PositionAt(ctx, ur, slot, ta,
                SegIndexAt(ur, ta), out _, out _);
            Vector2 b = PositionAt(ctx, ur, slot, tb,
                SegIndexAt(ur, tb), out _, out _);
            return (b - a).magnitude / (tb - ta);
        }

        // Stride phase from track DISTANCE (plus a per-slot desync of up
        // to one full cycle) — feet advance exactly as fast as the ground
        // passes underneath, however slow the compiled track is.
        static float GaitTime(UnitRuntime ur, int slot, ClipId gait, float t)
        {
            float jitterM = KitClips.MetersPerCycle(gait) *
                AngleEnvironmentLayout.Hash01(ur.keyStep, slot);
            return KitClips.DistancePhase(gait, ArcAt(ur, t) + jitterM);
        }

        // Bearing of unit motion (fall-back/rout facing).
        static float MotionBearing(UnitRuntime ur, float t)
        {
            Vector2 a = ur.unit.PositionAt(t - 1f);
            Vector2 b = ur.unit.PositionAt(t + 1f);
            Vector2 d = b - a;
            if (d.sqrMagnitude < 1e-6f) return ur.unit.FacingAt(t);
            return Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg;
        }

        // ------------------------------------------------------------------
        static void ResolveAlive(
            AngleActionContext ctx, UnitRuntime ur, int slot, float t,
            ref SoldierState s)
        {
            int segIdx = SegIndexAt(ur, t);
            var seg = ur.unit.segments[segIdx];
            Vector2 pos = PositionAt(ctx, ur, slot, t, segIdx,
                out bool crossingNow, out bool hasCrossed);
            s.posX = pos.x;
            s.posZ = pos.y;
            s.status = SoldierState.StatusAlive;

            float unitFacing = ur.unit.FacingAt(t);
            float yawJit = 8f *
                (AngleEnvironmentLayout.Hash01(ur.keyYaw, slot) - 0.5f);
            s.facingDeg = unitFacing + yawJit;

            float stepJit = 0.35f * AngleEnvironmentLayout.Hash01(ur.keyStep, slot);

            if (crossingNow)
            {
                float crossStart = ActiveCrossStart(ur, slot, t);
                s.clip = ClipId.Cross;
                s.clipTime = KitClips.Phase(ClipId.Cross, t - crossStart);
                return;
            }

            if (ur.isArtillery)
            {
                ResolveArtillery(ctx, ur, slot, t, seg, ref s);
                return;
            }

            // strike reactions interrupt anything except firing itself
            bool reacting = StrikeReaction(ur, slot, t, pos, out var reactClip,
                out float reactTime);

            // P8 locomotion review fix: whether this man's RESOLVED position
            // is actually moving decides walk-vs-stand, and stride phase is
            // keyed to track distance — never to the segment label alone.
            float speed = SlotSpeed(ctx, ur, slot, t);
            bool moving = speed >= MoveThresholdMps;

            switch (seg.action)
            {
                case "hold":
                case "halt":
                    if (reacting) { Set(ref s, reactClip, reactTime); return; }
                    if (moving) { SetGait(ref s, ur, slot, GaitClip(seg), t); return; }
                    Set(ref s, ClipId.StandReady,
                        KitClips.Phase(ClipId.StandReady, t + stepJit * 5f));
                    return;

                case "dress_line":
                {
                    if (moving) { SetGait(ref s, ur, slot, GaitClip(seg), t); return; }
                    float start = seg.t0 + 1.5f *
                        AngleEnvironmentLayout.Hash01(ur.keyStep, slot * 3 + 1);
                    if (t < start + KitClips.Duration(ClipId.HaltDress))
                        Set(ref s, ClipId.HaltDress,
                            KitClips.Phase(ClipId.HaltDress, t - start));
                    else
                        Set(ref s, ClipId.StandReady,
                            KitClips.Phase(ClipId.StandReady, t + stepJit * 5f));
                    return;
                }

                case "advance":
                case "oblique":
                case "close_gap":
                case "cross_obstacle":
                {
                    if (reacting) { Set(ref s, reactClip, reactTime); return; }
                    if (!moving)
                    {
                        Set(ref s, ClipId.StandReady,
                            KitClips.Phase(ClipId.StandReady, t + stepJit * 5f));
                        return;
                    }
                    SetGait(ref s, ur, slot, GaitClip(seg), t);
                    return;
                }

                case "double_quick":
                    if (moving) SetGait(ref s, ur, slot, ClipId.DoubleQuick, t);
                    else Set(ref s, ClipId.StandReady,
                        KitClips.Phase(ClipId.StandReady, t + stepJit * 5f));
                    return;

                case "fire_by_rank":
                case "fire_independent":
                {
                    // a unit whose track is moving cannot work the piece:
                    // locomotion wins (fire cycles resume where it halts)
                    if (moving) { SetGait(ref s, ur, slot, GaitClip(seg), t); return; }
                    float offset = FireCycles.Offset(
                        ctx.seed, ur.unit.unitId, seg, slot, ur.slotCount);
                    var (phase, phaseTime) = FireCycles.PhaseAt(seg, offset, t);
                    switch (phase)
                    {
                        case FireCycles.FirePhase.Aiming:
                            Set(ref s, ClipId.Aim, phaseTime); return;
                        case FireCycles.FirePhase.Firing:
                            Set(ref s, ClipId.Fire, phaseTime); return;
                        case FireCycles.FirePhase.Reloading:
                            Set(ref s, ClipId.Reload, phaseTime); return;
                        default:
                            if (reacting) { Set(ref s, reactClip, reactTime); return; }
                            Set(ref s, ClipId.StandReady,
                                KitClips.Phase(ClipId.StandReady, t + stepJit * 5f));
                            return;
                    }
                }

                case "take_canister":
                {
                    if (reacting) { Set(ref s, reactClip, reactTime); return; }
                    if (moving) { SetGait(ref s, ur, slot, GaitClip(seg), t); return; }
                    // stalled under the storm: a hash minority wavers, the
                    // rest stand at the ready
                    float w = AngleEnvironmentLayout.Hash01(ur.keyWaver, slot);
                    if (w < 0.30f && seg.Progress(t) > 0.35f)
                        Set(ref s, ClipId.Waver,
                            KitClips.Phase(ClipId.Waver, t + stepJit * 3f));
                    else
                        Set(ref s, ClipId.StandReady,
                            KitClips.Phase(ClipId.StandReady, t + stepJit * 5f));
                    return;
                }

                case "waver":
                {
                    if (reacting) { Set(ref s, reactClip, reactTime); return; }
                    if (moving)
                    {
                        // wavering men drifting with the line pick their
                        // way, they do not march in step
                        SetGait(ref s, ur, slot, ClipId.RouteStep, t);
                        return;
                    }
                    float k = AngleEnvironmentLayout.Hash01(ur.keyWaver, slot * 7 + 3);
                    if (k < 0.2f)
                        Set(ref s, ClipId.KneelReady, KitClips.Duration(ClipId.KneelReady) - 1f / 48f);
                    else
                        Set(ref s, ClipId.Waver,
                            KitClips.Phase(ClipId.Waver, t + stepJit * 3f));
                    return;
                }

                case "breach":
                {
                    // beyond the wall the survivors surge for the guns while
                    // the track carries them; they fight where it stalls
                    if (hasCrossed && !moving)
                    {
                        float offset = FireCycles.Offset(
                            ctx.seed, ur.unit.unitId, seg, slot, ur.slotCount);
                        var (phase, phaseTime) = FireCycles.PhaseAt(seg, offset, t);
                        switch (phase)
                        {
                            case FireCycles.FirePhase.Aiming:
                                Set(ref s, ClipId.Aim, phaseTime); return;
                            case FireCycles.FirePhase.Firing:
                                Set(ref s, ClipId.Fire, phaseTime); return;
                            case FireCycles.FirePhase.Reloading:
                                Set(ref s, ClipId.Reload, phaseTime); return;
                            default:
                                Set(ref s, ClipId.StandReady,
                                    KitClips.Phase(ClipId.StandReady, t)); return;
                        }
                    }
                    if (moving) SetGait(ref s, ur, slot, ClipId.DoubleQuick, t);
                    else Set(ref s, ClipId.StandReady,
                        KitClips.Phase(ClipId.StandReady, t + stepJit * 5f));
                    return;
                }

                case "fall_back":
                case "rout":
                {
                    float turnT = seg.t0 + 6f *
                        AngleEnvironmentLayout.Hash01(ur.keyRetreat, slot);
                    if (t < turnT)
                    {
                        Set(ref s, ClipId.Waver,
                            KitClips.Phase(ClipId.Waver, t + stepJit * 3f));
                        return;
                    }
                    s.facingDeg = MotionBearing(ur, t) + yawJit;
                    if (t < turnT + 1f)
                    {
                        Set(ref s, ClipId.TurnRetreat, t - turnT);
                        return;
                    }
                    var gait = seg.formationTo == "routed"
                        ? ClipId.RoutedRun : ClipId.RouteStep;
                    if (moving) SetGait(ref s, ur, slot, gait, t);
                    else Set(ref s, ClipId.Waver,
                        KitClips.Phase(ClipId.Waver, t + stepJit * 3f));
                    return;
                }

                default:
                    throw new InvalidOperationException(
                        $"{ur.unit.unitId}: segment {seg.id} has unsupported " +
                        $"action '{seg.action}'");
            }
        }

        static void SetGait(
            ref SoldierState s, UnitRuntime ur, int slot, ClipId gait, float t)
        {
            s.clip = gait;
            s.clipTime = GaitTime(ur, slot, gait, t);
        }

        static ClipId GaitClip(AngleBundleSegment seg)
        {
            bool disordered = seg.formationTo == "line_disordered" ||
                              seg.formationFrom == "line_disordered" ||
                              seg.formationTo == "scattered";
            if (seg.paceProfile == "surge") return ClipId.DoubleQuick;
            return disordered ? ClipId.RouteStep : ClipId.March;
        }

        static void Set(ref SoldierState s, ClipId clip, float time)
        {
            s.clip = clip;
            s.clipTime = time;
        }

        // Nearby canister/shell strike -> flinch (close) or brace (near).
        static bool StrikeReaction(
            UnitRuntime ur, int slot, float t, Vector2 pos,
            out ClipId clip, out float clipTime)
        {
            clip = ClipId.StandReady;
            clipTime = 0f;
            var strikes = ur.strikes;
            // strikes sorted by t; scan the recent window
            for (int i = strikes.Count - 1; i >= 0; i--)
            {
                float age = t - strikes[i].t;
                if (age < 0f) continue;
                if (age > 2.2f) break;
                float d = (strikes[i].pos - pos).magnitude;
                if (d <= FlinchRadiusM &&
                    age <= KitClips.Duration(ClipId.Flinch))
                {
                    clip = ClipId.Flinch;
                    clipTime = age;
                    return true;
                }
                if (d <= BraceRadiusM &&
                    age <= KitClips.Duration(ClipId.Brace))
                {
                    clip = ClipId.Brace;
                    clipTime = age;
                    return true;
                }
            }
            return false;
        }

        // Artillery crew: gun crews brace at their gun's discharges;
        // everyone else stands. §9.1 "crew response placeholder".
        static void ResolveArtillery(
            AngleActionContext ctx, UnitRuntime ur, int slot, float t,
            AngleBundleSegment seg, ref SoldierState s)
        {
            int crewSlots = FormationRoster.GunsPerBattery * FormationRoster.CrewPerGun;
            if (slot < crewSlots && ur.cannonShots != null)
            {
                int gun = slot % FormationRoster.GunsPerBattery;
                foreach (var shot in ur.cannonShots)
                {
                    if (shot.gun != gun) continue;
                    float age = t - shot.t;
                    if (age >= -0.5f && age < KitClips.Duration(ClipId.Brace))
                    {
                        Set(ref s, ClipId.Brace,
                            Mathf.Max(0f, age));
                        return;
                    }
                    if (shot.t > t + 0.5f) break;
                }
            }
            float k = AngleEnvironmentLayout.Hash01(ur.keyStep, slot * 11 + 5);
            if (k < 0.25f && slot < crewSlots)
                Set(ref s, ClipId.KneelReady,
                    KitClips.Duration(ClipId.KneelReady) - 1f / 48f);
            else
                Set(ref s, ClipId.StandReady,
                    KitClips.Phase(ClipId.StandReady, t + k * 7f));
        }

        // ------------------------------------------------------------------
        static void ResolveFallen(
            AngleActionContext ctx, UnitRuntime ur, int slot, float t,
            CasualtySchedule.Entry cas, ref SoldierState s)
        {
            float fallT = cas.fallT;
            int segIdx = SegIndexAt(ur, fallT);
            Vector2 pos = PositionAt(ctx, ur, slot, fallT, segIdx,
                out _, out _);
            s.posX = pos.x;
            s.posZ = pos.y;
            s.cause = (byte)cas.cause;

            // fall clip from cause + incoming direction at the moment of
            // the hit (§7.3 clip 14: "multiple fatal falls by incoming
            // direction")
            float unitFacing = ur.unit.FacingAt(fallT);
            float bearing = IncomingBearing(ur, fallT, cas.cause, unitFacing);
            float rel = Mathf.DeltaAngle(unitFacing, bearing);
            ClipId fall;
            if (Mathf.Abs(rel) > 55f)
                fall = ClipId.FallSide;
            else
                fall = AngleEnvironmentLayout.Hash01(ur.keyFall, slot) < 0.5f
                    ? ClipId.FallBack : ClipId.FallCrumple;

            // deterministic pose variation: rate +-10%, yaw +-9 deg (the
            // P6 review-fix conventions)
            float rate = 1f + 0.10f *
                (2f * AngleEnvironmentLayout.Hash01(ur.keyFall, slot * 5 + 1) - 1f);
            s.facingDeg = unitFacing + 9f *
                (2f * AngleEnvironmentLayout.Hash01(ur.keyFall, slot * 5 + 2) - 1f);

            float dur = KitClips.Duration(fall);
            float since = (t - fallT) * rate;
            if (since < dur)
            {
                s.status = SoldierState.StatusFalling;
                s.clip = fall;
                s.clipTime = Mathf.Min(since, dur - 1f / 48f);
                return;
            }

            // §6.4: bodies persist. A hash subset of the fallen are the
            // wounded who drag themselves (sober, slow) — the rest hold
            // the fall clip's final frame to the end of the slice.
            if (cas.woundedCrawl && t >= fallT + dur / rate + CrawlDelay)
            {
                s.status = SoldierState.StatusCrawling;
                s.clip = ClipId.ProneCrawl;
                s.clipTime = KitClips.Phase(
                    ClipId.ProneCrawl, t - fallT - dur / rate - CrawlDelay);
                return;
            }
            s.status = SoldierState.StatusDead;
            s.clip = fall;
            s.clipTime = dur - 1f / 48f;
        }

        static float IncomingBearing(
            UnitRuntime ur, float t, CasualtySchedule.Cause cause,
            float unitFacing)
        {
            bool wantArtillery = cause == CasualtySchedule.Cause.Canister ||
                                 cause == CasualtySchedule.Cause.Shell;
            float bearing = float.NaN;
            foreach (var inc in ur.incoming)
            {
                if (t < inc.t0 || t > inc.t1) continue;
                bool srcArtillery =
                    inc.kind == (byte)CasualtySchedule.Cause.Canister;
                if (srcArtillery == wantArtillery || float.IsNaN(bearing))
                    bearing = inc.bearingDeg;
                if (srcArtillery == wantArtillery) break;
            }
            // no active source (e.g. losses during the approach under
            // long-range fire): incoming reads as frontal
            return float.IsNaN(bearing) ? unitFacing : bearing;
        }
    }
}
