using System;
using System.Collections.Generic;
using System.Linq;
using BattleAtlas;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace BattleAtlas.EditorTools
{
    // Phase 8 living tactical scene (plan §12 Phase 8): the compiled
    // reconstruction (angle.bundle.json), the Phase 6 character kit, and
    // the Phase 7 sourced environment come together.
    //
    // Everything on screen at battle time t is a PURE function of
    // (bundle, seed, environment, t): the resolver picks every soldier's
    // clip and pose, the casualty schedule picks who falls when, the VFX
    // math places every smoke puff, and the trampling field re-splats the
    // corridor — so scrubbing to any t reconstructs the identical scene.
    public class AngleActionScene
    {
        public AngleEnvironmentStage.StageResult env;
        public AngleActionContext ctx;
        public Camera camera;
        public Terrain terrain;
        public float cropX0, cropZ0;

        // per-unit clip dictionaries: [side][variant] -> clips
        Dictionary<string, Dictionary<string, AnimationClip>> clipsByVariant;

        // skinned figure pools: key "csa_a" / "union_b_near" etc.
        readonly Dictionary<string, List<GameObject>> pools = new();
        readonly Dictionary<string, int> poolCursor = new();
        readonly Dictionary<GameObject, Renderer[]> musketRenderers = new();
        readonly Dictionary<GameObject, Transform> torsoBones = new();

        // mid-tier pose library: side -> pose name -> template
        class MidPose
        {
            public Mesh mesh;
            public Material[] mats;
            public Quaternion rot;
            public Vector3 scale;
        }
        Dictionary<string, Dictionary<string, MidPose>> midPoses;
        readonly List<GameObject> midPool = new();
        int midCursor;

        // far impostors + dressing + smoke: rebuilt combined meshes
        GameObject farRoot;
        readonly Dictionary<string, (GameObject go, Mesh mesh)> builtMeshes = new();

        // VFX
        public List<SmokeEvent> smokeEvents;     // sorted by t
        float maxSmokeLife;
        Material[] smokeMats;                     // [tex*24 + alpha*3 + shade]
        Material flashMat;
        Material bloodMat, musketDropMat, woundMat;
        Material[] impostorMats;                  // csa/usa x stand/fallen
        Mesh musketDropMesh;

        // cannon
        public struct CannonVfx
        {
            public float t;
            public Vector2 pos;      // macro
            public float dirDeg;
            public string gunGoName; // recoil target (null = unstaged gun)
        }
        public List<CannonVfx> cannonShots;
        readonly Dictionary<string, (Transform tr, Vector3 basePos, Vector2 fwd)> recoilGuns = new();

        // trampling
        float[] trampleFirstT;
        int trampleRes;
        float[,,] baseAlpha;
        int lastTrampleSecond = int.MinValue;

        // atmosphere
        UnityEngine.Rendering.HighDefinition.Fog fog;
        Transform sun;

        // state buffers
        public SoldierState[][] states;   // [unitIndex][slot]
        public CrowdTier[] tiers;

        public const string KitDir = "Assets/ProjectOwned/Characters/Kit";

        float Ground(Vector2 macro) =>
            terrain.transform.position.y + terrain.SampleHeight(
                new Vector3(macro.x - cropX0, 0f, macro.y - cropZ0));

        Vector3 World(Vector2 macro, float aboveGround) => new Vector3(
            macro.x - cropX0,
            Ground(macro) + aboveGround,
            macro.y - cropZ0);

        // ------------------------------------------------------------------
        public static AngleActionScene StageAll()
        {
            var scene = new AngleActionScene();
            scene.env = AngleEnvironmentStage.StageAll();
            scene.terrain = scene.env.terrain;
            scene.camera = scene.env.camera;
            scene.cropX0 = scene.env.env.crop.x0;
            scene.cropZ0 = scene.env.env.crop.z0;

            var bundle = AngleBundleLoader.Load();
            // battle seed: tied to the compiled input (plan §6.4 — victim
            // selection deterministic from battle seed + unit + slot)
            string seed = bundle.checksum;
            scene.ctx = AngleActionContext.Compile(
                bundle, seed, ObstaclePolylines(scene.env.env));

            scene.ValidateCasualtyReconciliation();
            scene.LoadKit();
            scene.CompileVfx();
            scene.BuildMaterials();
            scene.CompileTrampling();
            scene.FindAtmosphere();

            scene.states = new SoldierState[scene.ctx.units.Count][];
            for (int u = 0; u < scene.ctx.units.Count; u++)
                scene.states[u] = new SoldierState[scene.ctx.units[u].slotCount];

            scene.farRoot = new GameObject("P8 Crowd + VFX");
            return scene;
        }

        // Traced obstacle polylines (macro meters) from the sourced
        // environment bake: the road fences and the stone wall.
        static Dictionary<string, List<Vector2>> ObstaclePolylines(AngleEnvironment env)
        {
            var result = new Dictionary<string, List<Vector2>>();
            foreach (var fence in env.fences)
                result[fence.featureId] = AngleEnvironmentData.Points(fence.polylineFlat);
            result[env.wall.featureId] = AngleEnvironmentData.Points(env.wall.polylineFlat);
            return result;
        }

        // Render-time validation (plan §12 Phase 8): the schedule must
        // reconcile with the compiled per-second strength before we stage
        // a single figure.
        void ValidateCasualtyReconciliation()
        {
            foreach (var ur in ctx.units)
            {
                var u = ur.unit;
                int expected = 0;
                foreach (var p in u.casualtyProfiles) expected += p.count;
                int fallen = 0;
                foreach (var e in ur.casualties)
                    if (!float.IsInfinity(e.fallT)) fallen++;
                if (fallen != expected)
                    throw new InvalidOperationException(
                        $"{u.unitId}: schedule {fallen} != profiles {expected}");
                for (int s = 0; s < u.perSecond.strength.Count; s += 30)
                {
                    float t = u.perSecond.t0 + s;
                    int alive = CasualtySchedule.AliveCount(ur.casualties, t);
                    if (Mathf.Abs(alive - u.perSecond.strength[s]) > 1.001f)
                        throw new InvalidOperationException(
                            $"{u.unitId} t={t}: alive {alive} vs compiled " +
                            $"{u.perSecond.strength[s]}");
                }
            }
            Debug.Log("AngleActionScene: casualty schedules reconcile with " +
                      "compiled strengths (exact totals, <=1 mid-window)");
        }

        // ------------------------------------------------------------------
        void LoadKit()
        {
            clipsByVariant = new Dictionary<string, Dictionary<string, AnimationClip>>();
            foreach (var v in new[] { "csa_a", "csa_b", "csa_c",
                                      "union_a", "union_b", "union_c" })
                clipsByVariant[v] = GateP6Render.LoadClips(v);

            // mid-tier pose library from the *_mid FBX bakes
            midPoses = new Dictionary<string, Dictionary<string, MidPose>>();
            foreach (var side in new[] { "csa", "union" })
            {
                string path = $"{KitDir}/{side}_a_mid.fbx";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                    throw new InvalidOperationException($"{path} not imported");
                var template = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                GateP6Render.RemapMaterials(template);
                var lib = new Dictionary<string, MidPose>();
                foreach (var mf in template.GetComponentsInChildren<MeshFilter>())
                {
                    var mr = mf.GetComponent<MeshRenderer>();
                    if (mr == null) continue;
                    lib[mf.gameObject.name] = new MidPose
                    {
                        mesh = mf.sharedMesh,
                        mats = mr.sharedMaterials,
                        rot = mf.transform.rotation,
                        scale = mf.transform.lossyScale,
                    };
                }
                foreach (var pose in new[] { "pose_march_a", "pose_march_b",
                    "pose_aim", "pose_fire", "pose_reload_rod",
                    "pose_fallen_back", "pose_fallen_side" })
                    if (!lib.ContainsKey(pose))
                        throw new InvalidOperationException(
                            $"{path}: missing baked pose {pose}");
                midPoses[side] = lib;
                UnityEngine.Object.DestroyImmediate(template);
            }
        }

        static readonly string[] Variants = { "a", "b", "c" };

        string PoolKey(bool usa, byte variant, CrowdTier tier)
        {
            string side = usa ? "union" : "csa";
            string v = Variants[variant % 3];
            // only variant a has a near LOD for csa/union? no: all have _near
            return tier == CrowdTier.Near
                ? $"{side}_{v}_near" : $"{side}_{v}";
        }

        GameObject RentFigure(string key)
        {
            if (!pools.TryGetValue(key, out var pool))
            {
                pool = new List<GameObject>();
                pools[key] = pool;
                poolCursor[key] = 0;
            }
            int cursor = poolCursor[key];
            if (cursor < pool.Count)
            {
                poolCursor[key] = cursor + 1;
                var existing = pool[cursor];
                if (!existing.activeSelf) existing.SetActive(true);
                return existing;
            }
            string path = $"{KitDir}/{key}.fbx";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
                throw new InvalidOperationException($"{path} not imported");
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.name = $"fig_{key}_{pool.Count}";
            var animator = go.GetComponent<Animator>();
            if (animator != null) animator.enabled = false;
            GateP6Render.RemapMaterials(go);
            GateP6Render.ForcePerRenderSkinning(go);
            var muskets = go.GetComponentsInChildren<Renderer>(true)
                .Where(r => r.gameObject.name.ToLowerInvariant().Contains("musket"))
                .ToArray();
            musketRenderers[go] = muskets;
            var torso = go.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(tr => tr.name == "spine_02");
            torsoBones[go] = torso;
            pool.Add(go);
            poolCursor[key] = pool.Count;
            return go;
        }

        GameObject RentMid()
        {
            if (midCursor < midPool.Count)
            {
                var existing = midPool[midCursor++];
                if (!existing.activeSelf) existing.SetActive(true);
                return existing;
            }
            var go = new GameObject($"mid_{midPool.Count}");
            go.transform.SetParent(farRoot.transform, false);
            go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mr.shadowCastingMode = ShadowCastingMode.On;
            midPool.Add(go);
            midCursor = midPool.Count;
            return go;
        }

        // ------------------------------------------------------------------
        void CompileVfx()
        {
            smokeEvents = new List<SmokeEvent>();
            cannonShots = new List<CannonVfx>();
            int seedIndex = 0;
            var discharges = new List<float>();

            foreach (var ur in ctx.units)
            {
                var u = ur.unit;
                if (ur.isArtillery)
                {
                    CompileBatteryVfx(ur, ref seedIndex);
                    // strike dust where this battery's shots land is
                    // compiled on the TARGET units below
                    continue;
                }

                // musket smoke at every resolved discharge
                foreach (var seg in u.segments)
                {
                    bool fires = FireCycles.IsFireAction(seg.action) ||
                                 seg.action == "breach";
                    if (!fires) continue;
                    for (int slot = 0; slot < ur.slotCount; slot++)
                    {
                        float offset = FireCycles.Offset(
                            ctx.seed, u.unitId, seg, slot, ur.slotCount);
                        discharges.Clear();
                        FireCycles.DischargeTimes(
                            seg, offset, ur.casualties[slot].fallT,
                            seg.t0, seg.t1, discharges);
                        foreach (float ft in discharges)
                        {
                            // the discharge only happened if the resolver
                            // says the man is actually firing then (breach
                            // slots fire only after crossing the wall)
                            var st = SoldierActionResolver.Resolve(
                                ctx, ur.unitIndex, slot, ft);
                            if (st.clip != ClipId.Fire || st.Fallen) continue;
                            float r = st.facingDeg * Mathf.Deg2Rad;
                            var fwd = new Vector2(Mathf.Sin(r), Mathf.Cos(r));
                            smokeEvents.Add(new SmokeEvent
                            {
                                t = ft,
                                pos = new Vector2(st.posX, st.posZ) + fwd * 0.9f,
                                heightM = 1.45f,
                                dirDeg = st.facingDeg,
                                kind = SmokeKind.Musket,
                                seedIndex = seedIndex++,
                            });
                        }
                    }
                }

                // incoming canister/shell strikes throw earth
                foreach (var strike in ur.strikes)
                {
                    smokeEvents.Add(new SmokeEvent
                    {
                        t = strike.t,
                        pos = strike.pos,
                        heightM = 0.2f,
                        dirDeg = 0f,
                        kind = SmokeKind.StrikeDust,
                        seedIndex = seedIndex++,
                    });
                }

                BlackPowderVfx.CompileMarchDust(
                    u, ur.slotCount, ctx.seed, smokeEvents, ref seedIndex);
            }

            smokeEvents.Sort((a, b) =>
            {
                int c = a.t.CompareTo(b.t);
                return c != 0 ? c : a.seedIndex.CompareTo(b.seedIndex);
            });
            cannonShots.Sort((a, b) => a.t.CompareTo(b.t));
            maxSmokeLife = BlackPowderVfx.CannonLife;
            Debug.Log($"AngleActionScene: {smokeEvents.Count} smoke/dust events, " +
                      $"{cannonShots.Count} cannon discharges compiled");
        }

        void CompileBatteryVfx(UnitRuntime ur, ref int seedIndex)
        {
            var u = ur.unit;
            // Cushing's staged pieces fire from their traced positions;
            // Cowan/Arnold fire from drill-spacing offsets on their line.
            var gunPositions = new List<(Vector2 pos, string goName)>();
            if (u.unitId == "us-btty-cushing")
            {
                // names disambiguated by env order (two staged pieces share
                // "gun_wall_intact"); FindAtmosphere renames to match
                int gi = 0;
                foreach (var gun in env.env.battery.guns)
                {
                    if (gun.state == "intact")
                        gunPositions.Add((new Vector2(gun.x, gun.z),
                            $"gun_{gun.at}_{gun.state}#{gi}"));
                    gi++;
                }
            }
            else
            {
                for (int g = 0; g < FormationRoster.GunsPerBattery; g++)
                    gunPositions.Add((default, null)); // computed per shot
            }

            var shots = FireCycles.CompileCannon(ctx.seed, u, gunPositions.Count);
            foreach (var shot in shots)
            {
                Vector2 pos;
                string goName = gunPositions[shot.gun].goName;
                float facing = u.FacingAt(shot.t);
                if (goName != null)
                    pos = gunPositions[shot.gun].pos;
                else
                    pos = FormationRoster.ToWorld(
                        u.PositionAt(shot.t), facing,
                        FormationRoster.GunOffset(shot.gun));
                float r = facing * Mathf.Deg2Rad;
                var fwd = new Vector2(Mathf.Sin(r), Mathf.Cos(r));
                cannonShots.Add(new CannonVfx
                {
                    t = shot.t,
                    pos = pos + fwd * 1.4f,
                    dirDeg = facing,
                    gunGoName = goName,
                });
                smokeEvents.Add(new SmokeEvent
                {
                    t = shot.t,
                    pos = pos + fwd * 1.8f,
                    heightM = 1.1f,
                    dirDeg = facing,
                    kind = SmokeKind.Cannon,
                    seedIndex = seedIndex++,
                });
            }
            // NOTE: ResolveArtillery syncs crew braces against
            // FireCycles.CompileCannon(seed, unit, GunsPerBattery); for
            // Cushing only the intact pieces (gun 0..1) fire here, so
            // crews of the disabled crest pieces stand to their silent
            // guns — historically right for 15:20.
        }

        // ------------------------------------------------------------------
        void BuildMaterials()
        {
            // deterministic procedural smoke sprites
            var texes = new Texture2D[3];
            for (int v = 0; v < 3; v++) texes[v] = SmokeSprite(v);

            // buckets: tex(3) x alpha(8) x shade(3)
            smokeMats = new Material[3 * 8 * 3];
            for (int tv = 0; tv < 3; tv++)
            {
                for (int a = 0; a < 8; a++)
                {
                    for (int sh = 0; sh < 3; sh++)
                    {
                        float alpha = (a + 0.5f) / 8f;
                        float shade = (sh + 0.5f) / 3f;
                        // shade blends thrown-earth brown into powder white
                        Color c = Color.Lerp(
                            new Color(0.42f, 0.36f, 0.28f),
                            new Color(0.94f, 0.94f, 0.92f), shade);
                        c.a = alpha;
                        var m = TransparentLit(c);
                        m.SetTexture("_BaseColorMap", texes[tv]);
                        m.name = $"p8_smoke_{tv}_{a}_{sh}";
                        smokeMats[(tv * 8 + a) * 3 + sh] = m;
                    }
                }
            }

            flashMat = new Material(Shader.Find("HDRP/Unlit"));
            flashMat.SetColor("_UnlitColor", new Color(1f, 0.86f, 0.5f, 0.9f));
            flashMat.SetFloat("_SurfaceType", 1f);
            flashMat.SetFloat("_BlendMode", 1f);   // additive
            HDMaterial.ValidateMaterial(flashMat);

            bloodMat = TransparentLit(new Color(0.26f, 0.03f, 0.03f, 0.82f));
            bloodMat.name = "p8_blood";
            woundMat = TransparentLit(new Color(0.22f, 0.02f, 0.02f, 0.88f));
            woundMat.name = "p8_wound";

            musketDropMat = new Material(Shader.Find("HDRP/Lit"));
            musketDropMat.SetColor("_BaseColor", new Color(0.23f, 0.17f, 0.12f));
            musketDropMat.SetFloat("_Smoothness", 0.3f);

            impostorMats = new Material[4];
            impostorMats[0] = Opaque(new Color(0.45f, 0.41f, 0.34f)); // csa stand
            impostorMats[1] = Opaque(new Color(0.15f, 0.17f, 0.26f)); // usa stand
            impostorMats[2] = Opaque(new Color(0.33f, 0.30f, 0.25f)); // csa fallen
            impostorMats[3] = Opaque(new Color(0.12f, 0.13f, 0.19f)); // usa fallen

            musketDropMesh = BuildDroppedMusketMesh();
        }

        static Material Opaque(Color c)
        {
            var m = new Material(Shader.Find("HDRP/Lit"));
            m.SetColor("_BaseColor", c);
            m.SetFloat("_Smoothness", 0.08f);
            return m;
        }

        static Material TransparentLit(Color c)
        {
            var m = new Material(Shader.Find("HDRP/Lit"));
            m.SetColor("_BaseColor", c);
            m.SetFloat("_Smoothness", 0.02f);
            m.SetFloat("_SurfaceType", 1f);     // transparent
            m.SetFloat("_BlendMode", 0f);       // alpha
            m.SetFloat("_ZWrite", 0f);
            m.SetFloat("_EnableBlendModePreserveSpecularLighting", 0f);
            m.renderQueue = (int)RenderQueue.Transparent;
            HDMaterial.ValidateMaterial(m);
            return m;
        }

        // radial-falloff FBM smoke sprite, deterministic from the shared hash
        static Texture2D SmokeSprite(int variant)
        {
            const int n = 256;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, true)
            {
                name = $"p8_smoke_sprite_{variant}",
                wrapMode = TextureWrapMode.Clamp,
            };
            var px = new Color32[n * n];
            string key = "p8-smoke-tex" + variant;
            // coarse value-noise lattice
            const int lat = 9;
            var lattice = new float[lat * lat];
            for (int i = 0; i < lattice.Length; i++)
                lattice[i] = AngleEnvironmentLayout.Hash01(key, i);
            float Noise(float u, float v)
            {
                float fu = u * (lat - 1), fv = v * (lat - 1);
                int iu = Mathf.Min((int)fu, lat - 2);
                int iv = Mathf.Min((int)fv, lat - 2);
                float du = fu - iu, dv = fv - iv;
                float a = Mathf.Lerp(lattice[iv * lat + iu],
                    lattice[iv * lat + iu + 1], du);
                float b = Mathf.Lerp(lattice[(iv + 1) * lat + iu],
                    lattice[(iv + 1) * lat + iu + 1], du);
                return Mathf.Lerp(a, b, dv);
            }
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    float u = x / (float)(n - 1), v = y / (float)(n - 1);
                    float dx = u - 0.5f, dy = v - 0.5f;
                    float rr = Mathf.Sqrt(dx * dx + dy * dy) * 2f;
                    float falloff = Mathf.Clamp01(1f - rr);
                    falloff = falloff * falloff * (3f - 2f * falloff);
                    float nse = 0.6f * Noise(u, v)
                        + 0.3f * Noise(u * 2.7f % 1f, v * 2.7f % 1f)
                        + 0.1f * Noise(u * 6.1f % 1f, v * 6.1f % 1f);
                    float a = falloff * (0.45f + 0.55f * nse);
                    px[y * n + x] = new Color32(255, 255, 255,
                        (byte)Mathf.Clamp(Mathf.RoundToInt(a * 255f), 0, 255));
                }
            }
            tex.SetPixels32(px);
            tex.Apply(true);
            return tex;
        }

        static Mesh BuildDroppedMusketMesh()
        {
            // 1.42 m musket lying flat: stock box + barrel box
            var verts = new List<Vector3>();
            var tris = new List<int>();
            void Box(Vector3 c, Vector3 half)
            {
                int b = verts.Count;
                for (int i = 0; i < 8; i++)
                    verts.Add(c + new Vector3(
                        ((i & 1) * 2 - 1) * half.x,
                        (((i >> 1) & 1) * 2 - 1) * half.y,
                        (((i >> 2) & 1) * 2 - 1) * half.z));
                int[,] f = { {0,1,3,2},{5,4,6,7},{4,0,2,6},{1,5,7,3},{2,3,7,6},{4,5,1,0} };
                for (int q = 0; q < 6; q++)
                {
                    tris.Add(b + f[q, 0]); tris.Add(b + f[q, 1]); tris.Add(b + f[q, 2]);
                    tris.Add(b + f[q, 0]); tris.Add(b + f[q, 2]); tris.Add(b + f[q, 3]);
                }
            }
            Box(new Vector3(0f, 0.03f, -0.35f), new Vector3(0.025f, 0.03f, 0.36f));
            Box(new Vector3(0f, 0.035f, 0.35f), new Vector3(0.012f, 0.012f, 0.37f));
            var m = new Mesh { name = "p8_dropped_musket" };
            m.SetVertices(verts);
            m.SetTriangles(tris, 0);
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }

        // ------------------------------------------------------------------
        void CompileTrampling()
        {
            trampleRes = 256;
            trampleFirstT = TramplingField.Compile(
                ctx.units, cropX0, cropZ0,
                env.env.crop.x1 - env.env.crop.x0, trampleRes);
            var td = terrain.terrainData;
            baseAlpha = td.GetAlphamaps(0, 0,
                td.alphamapWidth, td.alphamapHeight);
            Debug.Log($"AngleActionScene: trampling grid {trampleRes}, " +
                      $"{TramplingField.TrampledCount(trampleFirstT, 9000f)} " +
                      "cells trampled by t=9000");
        }

        void FindAtmosphere()
        {
            foreach (var vol in UnityEngine.Object.FindObjectsByType<Volume>(
                FindObjectsSortMode.None))
            {
                if (vol.sharedProfile != null &&
                    vol.sharedProfile.TryGet(out UnityEngine.Rendering.HighDefinition.Fog f))
                { fog = f; break; }
            }
            var sunGo = GameObject.Find("Sun (ephemeris 15:20 LMT)");
            if (sunGo == null)
                throw new InvalidOperationException("environment sun not staged");
            sun = sunGo.transform;
            sunGo.name = "Sun (ephemeris, battle clock)";

            // recoil targets: Cushing's intact staged pieces. The stage
            // creates gun children in env.battery.guns order; rename with
            // the env index so the two "gun_wall_intact" pieces resolve.
            var battery = GameObject.Find("Cushing's Battery (ED-16)");
            if (battery != null)
            {
                int gi = 0;
                foreach (Transform child in battery.transform)
                {
                    if (!child.name.StartsWith("gun_")) continue;
                    child.name = $"{child.name}#{gi}";
                    if (child.name.Contains("intact"))
                    {
                        // muzzle bearing from the SOURCED gun yaw (prop
                        // transforms carry FBX axis compensation)
                        float yaw = env.env.battery.guns[gi].yawDeg
                            * Mathf.Deg2Rad;
                        recoilGuns[child.name] = (child, child.position,
                            new Vector2(Mathf.Sin(yaw), Mathf.Cos(yaw)));
                    }
                    gi++;
                }
            }
        }

        // ==================================================================
        // Pose the ENTIRE scene at battle time t. Pure: same t -> same scene.
        // ==================================================================
        public void Pose(float t)
        {
            // 1) resolve all logical states
            for (int u = 0; u < ctx.units.Count; u++)
            {
                var ur = ctx.units[u];
                for (int s = 0; s < ur.slotCount; s++)
                    states[u][s] = SoldierActionResolver.Resolve(ctx, u, s, t);
            }

            // 2) tiers from the camera
            var camPos3 = camera.transform.position;
            var camMacro = new Vector2(camPos3.x + cropX0, camPos3.z + cropZ0);
            tiers = CrowdTiers.Assign(camMacro, ctx.units,
                (u, s) => new Vector2(states[u][s].posX, states[u][s].posZ));

            // 3) figures
            PoseFigures(t, camMacro);

            // 4) VFX + dressing + atmosphere + terrain
            PoseSmoke(t);
            PoseCannon(t);
            PoseDressing(t, camMacro);
            PoseSun(t);
            PoseTrampling(t);
        }

        void PoseFigures(float t, Vector2 camMacro)
        {
            foreach (var key in poolCursor.Keys.ToList()) poolCursor[key] = 0;
            midCursor = 0;

            var farStand = new List<(Vector2 pos, float facing, bool usa)>();
            var farFallen = new List<(Vector2 pos, float facing, bool usa)>();

            int flat = 0;
            for (int u = 0; u < ctx.units.Count; u++)
            {
                var ur = ctx.units[u];
                bool usa = ur.unit.side == "union";
                string side = usa ? "union" : "csa";
                for (int s = 0; s < ur.slotCount; s++, flat++)
                {
                    var st = states[u][s];
                    var tier = tiers[flat];
                    var pos = new Vector2(st.posX, st.posZ);
                    switch (tier)
                    {
                        case CrowdTier.Hero:
                        case CrowdTier.Near:
                        {
                            var go = RentFigure(PoolKey(usa, st.variant, tier));
                            go.transform.position = World(pos, 0f);
                            go.transform.rotation =
                                Quaternion.Euler(0f, st.facingDeg, 0f);
                            bool noMusket = (st.equip & 0x80) != 0;
                            foreach (var r in musketRenderers[go])
                                if (r.enabled == noMusket) r.enabled = !noMusket;
                            string variantName =
                                $"{side}_{Variants[st.variant % 3]}";
                            if (!clipsByVariant[variantName].TryGetValue(
                                KitClips.Name(st.clip), out var clip))
                                throw new InvalidOperationException(
                                    $"missing animation '{KitClips.Name(st.clip)}' " +
                                    $"in kit variant {variantName}");
                            clip.SampleAnimation(go,
                                Mathf.Min(st.clipTime, clip.length));
                            break;
                        }
                        case CrowdTier.Mid:
                        {
                            var go = RentMid();
                            string pose = CrowdTiers.MidPose(st.clip, st.clipTime, s, t);
                            var lib = midPoses[side];
                            var mp = lib[pose];
                            go.GetComponent<MeshFilter>().sharedMesh = mp.mesh;
                            var mr = go.GetComponent<MeshRenderer>();
                            if (mr.sharedMaterials != mp.mats)
                                mr.sharedMaterials = mp.mats;
                            go.transform.position = World(pos, 0f);
                            go.transform.rotation =
                                Quaternion.Euler(0f, st.facingDeg, 0f) * mp.rot;
                            break;
                        }
                        default:
                        {
                            if (st.Fallen)
                                farFallen.Add((pos, st.facingDeg, usa));
                            else
                                farStand.Add((pos, st.facingDeg, usa));
                            break;
                        }
                    }
                }
            }

            // park unused pooled figures
            foreach (var kv in pools)
            {
                int used = poolCursor[kv.Key];
                for (int i = used; i < kv.Value.Count; i++)
                    if (kv.Value[i].activeSelf) kv.Value[i].SetActive(false);
            }
            for (int i = midCursor; i < midPool.Count; i++)
                if (midPool[i].activeSelf) midPool[i].SetActive(false);

            BuildImpostors(farStand, farFallen);
        }

        void BuildImpostors(
            List<(Vector2 pos, float facing, bool usa)> stand,
            List<(Vector2 pos, float facing, bool usa)> fallen)
        {
            var verts = new List<Vector3>[4];
            var tris = new List<int>[4];
            for (int i = 0; i < 4; i++) { verts[i] = new(); tris[i] = new(); }

            foreach (var (pos, facing, usa) in stand)
            {
                int b = usa ? 1 : 0;
                float g = Ground(pos);
                var c = new Vector3(pos.x - cropX0, g, pos.y - cropZ0);
                // two crossed quads, 0.55 m wide, 1.75 m tall
                float r = facing * Mathf.Deg2Rad;
                var d1 = new Vector3(Mathf.Cos(r), 0f, -Mathf.Sin(r)) * 0.275f;
                var d2 = new Vector3(Mathf.Sin(r), 0f, Mathf.Cos(r)) * 0.275f;
                AddQuad(verts[b], tris[b], c - d1, c + d1, 1.75f);
                AddQuad(verts[b], tris[b], c - d2, c + d2, 1.75f);
            }
            foreach (var (pos, facing, usa) in fallen)
            {
                int b = usa ? 3 : 2;
                float g = Ground(pos) + 0.06f;
                float r = facing * Mathf.Deg2Rad;
                var fwd = new Vector3(Mathf.Sin(r), 0f, Mathf.Cos(r));
                var right = new Vector3(fwd.z, 0f, -fwd.x);
                var c = new Vector3(pos.x - cropX0, g, pos.y - cropZ0);
                int i0 = verts[b].Count;
                verts[b].Add(c - right * 0.28f - fwd * 0.9f);
                verts[b].Add(c + right * 0.28f - fwd * 0.9f);
                verts[b].Add(c + right * 0.28f + fwd * 0.9f);
                verts[b].Add(c - right * 0.28f + fwd * 0.9f);
                tris[b].Add(i0); tris[b].Add(i0 + 2); tris[b].Add(i0 + 1);
                tris[b].Add(i0); tris[b].Add(i0 + 3); tris[b].Add(i0 + 2);
            }
            string[] names = { "far_csa", "far_usa", "far_csa_fallen", "far_usa_fallen" };
            for (int i = 0; i < 4; i++)
                SetBuiltMesh(names[i], verts[i], null, tris[i], impostorMats[i],
                    castShadows: i < 2);
        }

        static void AddQuad(List<Vector3> verts, List<int> tris,
            Vector3 a, Vector3 b, float height)
        {
            int i0 = verts.Count;
            verts.Add(a);
            verts.Add(b);
            verts.Add(b + Vector3.up * height);
            verts.Add(a + Vector3.up * height);
            tris.Add(i0); tris.Add(i0 + 2); tris.Add(i0 + 1);
            tris.Add(i0); tris.Add(i0 + 3); tris.Add(i0 + 2);
            // back face
            tris.Add(i0); tris.Add(i0 + 1); tris.Add(i0 + 2);
            tris.Add(i0); tris.Add(i0 + 2); tris.Add(i0 + 3);
        }

        void SetBuiltMesh(string name, List<Vector3> verts, List<Vector2> uvs,
            List<int> tris, Material mat, bool castShadows = false)
        {
            if (!builtMeshes.TryGetValue(name, out var built))
            {
                var go = new GameObject(name);
                go.transform.SetParent(farRoot.transform, false);
                var mesh = new Mesh
                {
                    name = name,
                    indexFormat = IndexFormat.UInt32,
                };
                mesh.MarkDynamic();
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                var mr = go.AddComponent<MeshRenderer>();
                mr.sharedMaterial = mat;
                mr.shadowCastingMode = castShadows
                    ? ShadowCastingMode.On : ShadowCastingMode.Off;
                built = (go, mesh);
                builtMeshes[name] = built;
            }
            built.mesh.Clear();
            built.mesh.SetVertices(verts);
            if (uvs != null) built.mesh.SetUVs(0, uvs);
            built.mesh.SetTriangles(tris, 0);
            if (uvs == null) built.mesh.RecalculateNormals();
            built.mesh.RecalculateBounds();
            built.go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        // ------------------------------------------------------------------
        readonly List<PuffInstance> puffBuf = new();

        void PoseSmoke(float t)
        {
            puffBuf.Clear();
            // active window via binary search on the sorted event list
            int lo = LowerBound(smokeEvents, t - maxSmokeLife);
            float musketCannonAlpha = 0f;
            var camTr = camera.transform;

            var bucketVerts = new Dictionary<int, List<Vector3>>();
            var bucketUvs = new Dictionary<int, List<Vector2>>();
            var bucketDepth = new Dictionary<int, List<(float depth, int quad)>>();

            for (int i = lo; i < smokeEvents.Count; i++)
            {
                var e = smokeEvents[i];
                if (e.t > t) break;
                int before = puffBuf.Count;
                BlackPowderVfx.Emit(e, t, ctx.seed, puffBuf);
                if (e.kind == SmokeKind.Musket || e.kind == SmokeKind.Cannon)
                    for (int p = before; p < puffBuf.Count; p++)
                        musketCannonAlpha += puffBuf[p].alpha;
            }

            // fog responds to accumulated powder smoke (§9.1 visibility)
            if (fog != null)
                fog.meanFreePath.value =
                    BlackPowderVfx.FogMeanFreePath(musketCannonAlpha);

            // bucket the puffs and build camera-facing quads
            foreach (var p in puffBuf)
            {
                int aBucket = Mathf.Clamp((int)(p.alpha * 8f), 0, 7);
                if (aBucket == 0 && p.alpha < 0.03f) continue;
                int shBucket = Mathf.Clamp((int)(p.shade * 3f), 0, 2);
                int key = (p.texVariant * 8 + aBucket) * 3 + shBucket;
                if (!bucketVerts.TryGetValue(key, out var verts))
                {
                    verts = new List<Vector3>();
                    bucketVerts[key] = verts;
                    bucketUvs[key] = new List<Vector2>();
                    bucketDepth[key] = new List<(float, int)>();
                }
                var uvs = bucketUvs[key];
                var center = World(p.pos, p.heightM);
                float depth = (center - camTr.position).sqrMagnitude;
                bucketDepth[key].Add((depth, verts.Count / 4));

                var right = camTr.right;
                var up = camTr.up;
                float roll = p.rollDeg * Mathf.Deg2Rad;
                var r2 = right * Mathf.Cos(roll) + up * Mathf.Sin(roll);
                var u2 = up * Mathf.Cos(roll) - right * Mathf.Sin(roll);
                r2 *= p.radius;
                u2 *= p.radius;
                verts.Add(center - r2 - u2);
                verts.Add(center + r2 - u2);
                verts.Add(center + r2 + u2);
                verts.Add(center - r2 + u2);
                uvs.Add(new Vector2(0f, 0f));
                uvs.Add(new Vector2(1f, 0f));
                uvs.Add(new Vector2(1f, 1f));
                uvs.Add(new Vector2(0f, 1f));
            }

            // emit sorted (back to front) triangles per bucket
            var used = new HashSet<string>();
            foreach (var kv in bucketVerts)
            {
                int key = kv.Key;
                var order = bucketDepth[key];
                order.Sort((a, b) => b.depth.CompareTo(a.depth));
                var tris = new List<int>(order.Count * 6);
                foreach (var (_, quad) in order)
                {
                    int i0 = quad * 4;
                    tris.Add(i0); tris.Add(i0 + 2); tris.Add(i0 + 1);
                    tris.Add(i0); tris.Add(i0 + 3); tris.Add(i0 + 2);
                }
                string name = $"smoke_{key}";
                used.Add(name);
                SetBuiltMesh(name, kv.Value, bucketUvs[key], tris, smokeMats[key]);
            }
            // clear stale smoke buckets
            foreach (var kv in builtMeshes)
                if (kv.Key.StartsWith("smoke_") && !used.Contains(kv.Key))
                    kv.Value.mesh.Clear();

            PoseFlashes(t);
        }

        void PoseFlashes(float t)
        {
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();
            var camTr = camera.transform;
            void Flash(Vector2 pos, float h, float size)
            {
                var c = World(pos, h);
                var r = camTr.right * size;
                var u = camTr.up * size;
                int i0 = verts.Count;
                verts.Add(c - r - u); verts.Add(c + r - u);
                verts.Add(c + r + u); verts.Add(c - r + u);
                for (int k = 0; k < 4; k++) uvs.Add(Vector2.zero);
                tris.Add(i0); tris.Add(i0 + 2); tris.Add(i0 + 1);
                tris.Add(i0); tris.Add(i0 + 3); tris.Add(i0 + 2);
            }
            int lo = LowerBound(smokeEvents, t - BlackPowderVfx.FlashDur);
            for (int i = lo; i < smokeEvents.Count; i++)
            {
                var e = smokeEvents[i];
                if (e.t > t) break;
                if (e.kind != SmokeKind.Musket) continue;
                if (BlackPowderVfx.FlashActive(e.t, t))
                    Flash(e.pos, e.heightM, 0.22f);
            }
            foreach (var shot in cannonShots)
            {
                if (shot.t > t) break;
                if (BlackPowderVfx.FlashActive(shot.t, t))
                    Flash(shot.pos, 1.1f, 1.1f);
            }
            SetBuiltMesh("muzzle_flashes", verts, uvs, tris, flashMat);
        }

        void PoseCannon(float t)
        {
            // deterministic recoil-and-return on the staged intact pieces
            foreach (var kv in recoilGuns)
            {
                float recoil = 0f;
                foreach (var shot in cannonShots)
                {
                    if (shot.t > t) break;
                    if (shot.gunGoName != kv.Key) continue;
                    float age = t - shot.t;
                    if (age < 0.18f) recoil = 0.85f * (age / 0.18f);
                    else if (age < 3f)
                        recoil = 0.85f * (1f - (age - 0.18f) / 2.82f);
                }
                var (tr, basePos, fwd) = kv.Value;
                tr.position = basePos -
                    new Vector3(fwd.x, 0f, fwd.y) * recoil;
            }
        }

        static int LowerBound(List<SmokeEvent> events, float t)
        {
            int lo = 0, hi = events.Count;
            while (lo < hi)
            {
                int mid = (lo + hi) / 2;
                if (events[mid].t < t) lo = mid + 1;
                else hi = mid;
            }
            return lo;
        }

        // ------------------------------------------------------------------
        void PoseDressing(float t, Vector2 camMacro)
        {
            var bloodVerts = new List<Vector3>();
            var bloodUvs = new List<Vector2>();
            var bloodTris = new List<int>();
            var dropVerts = new List<Vector3>();
            var dropTris = new List<int>();
            var woundVerts = new List<Vector3>();
            var woundUvs = new List<Vector2>();
            var woundTris = new List<int>();
            var camTr = camera.transform;

            var dropMeshVerts = new List<Vector3>();
            musketDropMesh.GetVertices(dropMeshVerts);
            var dropMeshTris = musketDropMesh.triangles;

            int flat = 0;
            for (int u = 0; u < ctx.units.Count; u++)
            {
                var ur = ctx.units[u];
                string unitKey = ctx.seed + "|" + ur.unit.unitId;
                for (int s = 0; s < ur.slotCount; s++, flat++)
                {
                    var st = states[u][s];
                    if (!st.Fallen) continue;
                    var pos = new Vector2(st.posX, st.posZ);
                    if ((pos - camMacro).magnitude > CrowdTiers.MidRangeM)
                        continue;   // dressing only where figures resolve

                    var cas = ur.casualties[s];
                    if (CasualtyDressing.Pool(cas, pos, t, unitKey, s, out var pool))
                    {
                        float g = Ground(pool.pos) + 0.02f;
                        var c = new Vector3(
                            pool.pos.x - cropX0, g, pool.pos.y - cropZ0);
                        int i0 = bloodVerts.Count;
                        float rr = pool.radius;
                        bloodVerts.Add(c + new Vector3(-rr, 0f, -rr));
                        bloodVerts.Add(c + new Vector3(rr, 0f, -rr));
                        bloodVerts.Add(c + new Vector3(rr, 0f, rr));
                        bloodVerts.Add(c + new Vector3(-rr, 0f, rr));
                        bloodUvs.Add(new Vector2(0f, 0f));
                        bloodUvs.Add(new Vector2(1f, 0f));
                        bloodUvs.Add(new Vector2(1f, 1f));
                        bloodUvs.Add(new Vector2(0f, 1f));
                        bloodTris.Add(i0); bloodTris.Add(i0 + 2); bloodTris.Add(i0 + 1);
                        bloodTris.Add(i0); bloodTris.Add(i0 + 3); bloodTris.Add(i0 + 2);
                    }

                    ClipId fallClip = st.clip == ClipId.ProneCrawl
                        ? ClipId.FallSide : st.clip;
                    if (CasualtyDressing.Dropped(cas, ur.isArtillery, pos,
                        KitClips.Duration(fallClip), t, unitKey, s, out var item))
                    {
                        float g = Ground(item.pos);
                        var rot = Quaternion.Euler(0f, item.yawDeg, 0f);
                        var c = new Vector3(
                            item.pos.x - cropX0, g, item.pos.y - cropZ0);
                        int i0 = dropVerts.Count;
                        foreach (var v in dropMeshVerts)
                            dropVerts.Add(c + rot * v);
                        foreach (int idx in dropMeshTris)
                            dropTris.Add(i0 + idx);
                    }

                    // hero-tier wound patch (billboard on the torso)
                    if (tiers[flat] == CrowdTier.Hero &&
                        st.status != SoldierState.StatusFalling)
                    {
                        float size = CasualtyDressing.WoundPatchSize(
                            (CasualtySchedule.WoundCategory)st.wound);
                        if (size > 0f)
                        {
                            var c = World(pos, 0.35f);
                            var r = camTr.right * size;
                            var uv = camTr.up * size;
                            int i0 = woundVerts.Count;
                            woundVerts.Add(c - r - uv);
                            woundVerts.Add(c + r - uv);
                            woundVerts.Add(c + r + uv);
                            woundVerts.Add(c - r + uv);
                            woundUvs.Add(new Vector2(0f, 0f));
                            woundUvs.Add(new Vector2(1f, 0f));
                            woundUvs.Add(new Vector2(1f, 1f));
                            woundUvs.Add(new Vector2(0f, 1f));
                            woundTris.Add(i0); woundTris.Add(i0 + 2); woundTris.Add(i0 + 1);
                            woundTris.Add(i0); woundTris.Add(i0 + 3); woundTris.Add(i0 + 2);
                        }
                    }
                }
            }

            SetBuiltMesh("blood_pools", bloodVerts, bloodUvs, bloodTris, bloodMat);
            SetBuiltMesh("dropped_muskets", dropVerts, null, dropTris,
                musketDropMat, castShadows: true);
            SetBuiltMesh("wound_patches", woundVerts, woundUvs, woundTris, woundMat);
        }

        // Wound patches ride the torso bone once the body settles: refine
        // hero patches against the sampled skeleton (called after figures
        // are posed; deterministic because the sampled pose is).
        // NOTE: billboard placement at 0.35 m reads correctly for prone
        // bodies; skinned-decal precision is out of Phase 8 scope and
        // documented in violence-and-representation.md.

        void PoseSun(float t)
        {
            float secondsSinceMidnight =
                ctx.bundle.clock.startTimeSecondsSinceMidnight + t;
            var (elev, azim) = SunEphemeris.SunAngles(secondsSinceMidnight);
            sun.rotation = SunEphemeris.LightRotation(elev, azim);
        }

        void PoseTrampling(float t)
        {
            int second = Mathf.FloorToInt(t);
            if (second == lastTrampleSecond) return;
            lastTrampleSecond = second;

            var td = terrain.terrainData;
            int res = td.alphamapWidth;
            var maps = (float[,,])baseAlpha.Clone();
            var w = new float[AngleEnvironmentLayout.LayerCount];
            int trampled = 0;
            for (int y = 0; y < res; y++)          // alphamap row 0 = south
            {
                int tz = y * trampleRes / res;
                for (int x = 0; x < res; x++)
                {
                    int tx = x * trampleRes / res;
                    float firstT = trampleFirstT[tz * trampleRes + tx];
                    if (firstT > t) continue;
                    trampled++;
                    // ease the corridor in over a minute of traffic
                    float k = Mathf.Clamp01((t - firstT) / 60f) * 0.85f;
                    float noise = AngleEnvironmentLayout.Hash01(
                        "p8-trample", y * res + x);
                    AngleEnvironmentLayout.ClassWeights(5, noise, w);
                    for (int l = 0; l < w.Length; l++)
                        maps[y, x, l] = Mathf.Lerp(maps[y, x, l], w[l], k);
                }
            }
            td.SetAlphamaps(0, 0, maps);
        }

        // ==================================================================
        // Gate P8 logical-state digest: SHA-256 over every resolved soldier
        // state at battle time t (the bitwise-identical proof).
        // ==================================================================
        public string LogicalStateDigest(float t)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var buf = new List<byte>(ctx.TotalSlots * 21);
            for (int u = 0; u < ctx.units.Count; u++)
            {
                var ur = ctx.units[u];
                for (int s = 0; s < ur.slotCount; s++)
                {
                    var st = SoldierActionResolver.Resolve(ctx, u, s, t);
                    buf.AddRange(BitConverter.GetBytes(st.posX));
                    buf.AddRange(BitConverter.GetBytes(st.posZ));
                    buf.AddRange(BitConverter.GetBytes(st.facingDeg));
                    buf.AddRange(BitConverter.GetBytes(st.clipTime));
                    buf.Add((byte)st.clip);
                    buf.Add(st.status);
                    buf.Add(st.cause);
                    buf.Add(st.wound);
                    buf.Add(st.variant);
                    buf.Add(st.equip);
                }
            }
            return BitConverter.ToString(sha.ComputeHash(buf.ToArray()))
                .Replace("-", "").ToLowerInvariant();
        }

        // Casualty reconciliation numbers for the gate report.
        public string CasualtyReport()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("[");
            bool first = true;
            foreach (var ur in ctx.units)
            {
                int profileTotal = 0;
                foreach (var p in ur.unit.casualtyProfiles)
                    profileTotal += p.count;
                int scheduled = 0;
                foreach (var e in ur.casualties)
                    if (!float.IsInfinity(e.fallT)) scheduled++;
                int aliveEnd = CasualtySchedule.AliveCount(
                    ur.casualties, ctx.bundle.slice.t1);
                if (!first) sb.Append(",");
                first = false;
                sb.Append("\n    {\"unitId\": \"").Append(ur.unit.unitId)
                  .Append("\", \"startStrength\": ").Append(ur.slotCount)
                  .Append(", \"profileTotal\": ").Append(profileTotal)
                  .Append(", \"scheduled\": ").Append(scheduled)
                  .Append(", \"aliveAtEnd\": ").Append(aliveEnd)
                  .Append(", \"compiledStrengthAtEnd\": ")
                  .Append(ur.unit.perSecond.strength[
                      ur.unit.perSecond.strength.Count - 1])
                  .Append("}");
            }
            sb.Append("\n  ]");
            return sb.ToString();
        }
    }
}
