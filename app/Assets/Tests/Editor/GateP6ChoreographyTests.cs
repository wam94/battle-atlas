using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using BattleAtlas;
using UnityEditor;
using UnityEngine;

namespace BattleAtlas.Tests
{
    // Gate P6 choreography: deterministic, scrub-invariant, and the gate
    // checklist properties (formation, reload legibility, fence crossing,
    // falls by direction, persistent bodies) hold by construction.
    public class GateP6ChoreographyTests
    {
        const float Eps = 1e-4f;

        [Test]
        public void Resolve_IsDeterministic_AcrossCallsAndOrder()
        {
            // sample a grid forward, then replay the SAME grid in reverse
            // order — a pure function must not care (scrub invariance)
            var grid = new List<(int slot, float t)>();
            for (int slot = 0; slot < GateP6Choreography.SoldierCount; slot += 7)
                for (float t = 0f; t <= 60f; t += 1.7f)
                    grid.Add((slot, t));
            var forward = grid.Select(g =>
                GateP6Choreography.Resolve(g.slot, g.t)).ToList();
            for (int i = grid.Count - 1; i >= 0; i--)
            {
                var again = GateP6Choreography.Resolve(grid[i].slot, grid[i].t);
                Assert.AreEqual(forward[i].clip, again.clip,
                    $"slot {grid[i].slot} t {grid[i].t}");
                Assert.AreEqual(forward[i].clipTime, again.clipTime, Eps);
                Assert.AreEqual(forward[i].posLocal.x, again.posLocal.x, Eps);
                Assert.AreEqual(forward[i].posLocal.y, again.posLocal.y, Eps);
            }
        }

        [Test]
        public void Bodies_PersistAndFreeze_AfterFall()
        {
            // slot 11 falls at 41.5 (crumple); afterwards it stays dead,
            // its position never changes, and its clip time clamps
            var atFall = GateP6Choreography.Resolve(11, 42f);
            Assert.IsTrue(atFall.dead);
            Assert.AreEqual(GateP6Choreography.FallCrumple, atFall.clip);
            var later = GateP6Choreography.Resolve(11, 59.9f);
            Assert.IsTrue(later.dead);
            Assert.AreEqual(atFall.posLocal, later.posLocal,
                "a body must not drift");
            Assert.LessOrEqual(later.clipTime, GateP6Choreography.FallCrumpleDur);
        }

        [Test]
        public void Falls_SelectClip_ByIncomingDirection()
        {
            // frontal musketry throws men back or crumples them; the
            // oblique canister slots use the sideways knockdown
            Assert.AreEqual(GateP6Choreography.FallSide,
                GateP6Choreography.Resolve(19, 59f).clip);
            Assert.AreEqual(GateP6Choreography.FallBack,
                GateP6Choreography.Resolve(2, 59f).clip);
            Assert.AreEqual(GateP6Choreography.FallCrumple,
                GateP6Choreography.Resolve(47, 59f).clip);
        }

        [Test]
        public void Falls_VaryPerSoldier_Deterministically()
        {
            // Gate P6 review: simultaneous falls must not run in lockstep.
            // Slots 2 and 55 both use Fall_Shot_Front_Back; the shared-hash
            // jitter must give them different playback rates and facings,
            // and the SAME values on every call (scrub invariance).
            float FirstFall(int slot)
            {
                for (float t = 0f; t <= 60f; t += 1f / 96f)
                    if (GateP6Choreography.Resolve(slot, t).dead) return t;
                Assert.Fail($"slot {slot} never falls");
                return -1f;
            }
            float f2 = FirstFall(2), f55 = FirstFall(55);
            var a = GateP6Choreography.Resolve(2, f2 + 0.5f);
            var b = GateP6Choreography.Resolve(55, f55 + 0.5f);
            // rate jitter: same elapsed time, different clip time
            Assert.Greater(Mathf.Abs(a.clipTime - b.clipTime), 0.02f,
                "fall playback rates must differ per soldier");
            // yaw jitter: facing varies around the by-direction default
            Assert.Greater(Mathf.Abs(a.facingDeg - b.facingDeg), 0.5f,
                "fall facings must differ per soldier");
            Assert.Less(Mathf.Abs(a.facingDeg - 90f),
                GateP6Choreography.FallYawJitterDeg + 0.01f,
                "yaw jitter must stay small enough to keep the "
                + "by-incoming-direction differentiation legible");
            // deterministic: identical on re-resolve
            var again = GateP6Choreography.Resolve(2, f2 + 0.5f);
            Assert.AreEqual(a.clipTime, again.clipTime);
            Assert.AreEqual(a.facingDeg, again.facingDeg);
        }

        [Test]
        public void EveryLineSoldier_CrossesTheFence_OrDiesFirst()
        {
            for (int slot = 0; slot < 64; slot++)
            {
                bool sawCross = false, dead = false;
                for (float t = 0f; t <= 60f; t += 1f / 30f)
                {
                    var p = GateP6Choreography.Resolve(slot, t);
                    if (p.clip == GateP6Choreography.Cross) sawCross = true;
                    if (p.dead) { dead = true; break; }
                }
                Assert.IsTrue(sawCross || dead,
                    $"slot {slot} neither crossed nor fell");
            }
        }

        [Test]
        public void CrossingSoldier_ResumesBeyondTheFence()
        {
            // slot 0 (front rank, first file): before crossing it is west of
            // the fence; after, east of it — and marching again
            var before = GateP6Choreography.Resolve(0, 1f);
            Assert.Less(before.posLocal.x, GateP6Choreography.FenceX);
            Assert.AreEqual(GateP6Choreography.March, before.clip);
            var after = GateP6Choreography.Resolve(0, 59f);
            Assert.Greater(after.posLocal.x, GateP6Choreography.FenceX);
        }

        [Test]
        public void Skirmisher_CompletesTwoFullReloads()
        {
            float reloadSeen = 0f;
            int reloads = 0;
            bool inReload = false;
            for (float t = 0f; t <= 60f; t += 1f / 30f)
            {
                var p = GateP6Choreography.Resolve(84, t);
                if (p.clip == GateP6Choreography.Reload)
                {
                    if (!inReload) { inReload = true; reloadSeen = 0f; }
                    reloadSeen = Mathf.Max(reloadSeen, p.clipTime);
                }
                else if (inReload)
                {
                    inReload = false;
                    if (reloadSeen > GateP6Choreography.ReloadDur - 0.5f) reloads++;
                }
            }
            Assert.GreaterOrEqual(reloads, 2,
                "the hero skirmisher must show two complete reload cycles");
        }

        [Test]
        public void UsaLine_ShowsStaggeredReloadStages_Simultaneously()
        {
            // at one instant the 20-man wall line must show several distinct
            // reload phases (the plan's fire/reload rhythm at unit scale)
            var stages = new HashSet<int>();
            for (int slot = 64; slot < 84; slot++)
            {
                var p = GateP6Choreography.Resolve(slot, 30f);
                if (p.clip == GateP6Choreography.Reload)
                    stages.Add((int)(p.clipTime / 2.5f));
            }
            Assert.GreaterOrEqual(stages.Count, 4,
                "wall line should exhibit at least four distinct reload phases");
        }

        [Test]
        public void FormationHoldsAsALine_BeforeTheFence()
        {
            // at t=8 the front rank (slots 0..31, minus interrupted men)
            // should stand within a narrow x band — a line, not a scatter
            var xs = new List<float>();
            for (int slot = 0; slot < 32; slot++)
            {
                var p = GateP6Choreography.Resolve(slot, 8f);
                if (p.clip == GateP6Choreography.March) xs.Add(p.posLocal.x);
            }
            Assert.GreaterOrEqual(xs.Count, 28);
            // slot 5 took a nonfatal hit at t=18 and lags ~1.5 m — one
            // straggler is the historical read, not a formation failure
            Assert.Less(xs.Max() - xs.Min(), 2.0f,
                "front rank must dress as a line (one hit straggler allowed)");
        }

        [Test]
        public void NoSoldier_EverReferencesAnUnknownClip()
        {
            var known = new HashSet<string>
            {
                GateP6Choreography.March, GateP6Choreography.StandReady,
                GateP6Choreography.Aim, GateP6Choreography.Fire,
                GateP6Choreography.Reload, GateP6Choreography.Cross,
                GateP6Choreography.Hit, GateP6Choreography.FallBack,
                GateP6Choreography.FallCrumple, GateP6Choreography.FallSide,
                GateP6Choreography.Retreat, GateP6Choreography.Waver,
            };
            for (int slot = 0; slot < GateP6Choreography.SoldierCount; slot++)
                for (float t = 0f; t <= 60f; t += 0.5f)
                    Assert.Contains(GateP6Choreography.Resolve(slot, t).clip, known.ToList(),
                        $"slot {slot} t {t}");
        }

        [Test]
        public void RetreatingSoldiers_MoveWest_AfterTurning()
        {
            var atTurn = GateP6Choreography.Resolve(13, 49.2f);
            Assert.AreEqual(GateP6Choreography.Retreat, atTurn.clip);
            var later = GateP6Choreography.Resolve(13, 56f);
            Assert.Less(later.posLocal.x, atTurn.posLocal.x,
            	"a retreating soldier must give ground westward");
        }
    }

    // The kit FBX contract: every clip the choreography references exists
    // in every hero variant, with the prop bones the reload depends on.
    public class GateP6KitAssetTests
    {
        static readonly string[] Variants =
            { "csa_a", "csa_b", "csa_c", "union_a", "union_b", "union_c" };

        static IEnumerable<Object> Assets(string variant) =>
            AssetDatabase.LoadAllAssetsAtPath(
                $"Assets/ProjectOwned/Characters/Kit/{variant}.fbx");

        // Blender FBX takes import as "<rig>|<action>"
        static string ClipName(AnimationClip c) =>
            c.name.Contains("|")
                ? c.name.Substring(c.name.LastIndexOf('|') + 1)
                : c.name;

        [Test]
        public void EveryVariant_ContainsEveryChoreographyClip()
        {
            string[] required =
            {
                GateP6Choreography.March, GateP6Choreography.StandReady,
                GateP6Choreography.Aim, GateP6Choreography.Fire,
                GateP6Choreography.Reload, GateP6Choreography.Cross,
                GateP6Choreography.Hit, GateP6Choreography.FallBack,
                GateP6Choreography.FallCrumple, GateP6Choreography.FallSide,
                GateP6Choreography.Retreat, GateP6Choreography.Waver,
            };
            foreach (var v in Variants)
            {
                var clips = Assets(v).OfType<AnimationClip>()
                    .Select(ClipName).ToHashSet();
                Assert.IsNotEmpty(clips, $"{v}.fbx not imported");
                foreach (var r in required)
                    Assert.IsTrue(clips.Contains(r), $"{v}.fbx missing clip {r}");
            }
        }

        [Test]
        public void ReloadClip_IsTheAuthoredTwentySeconds()
        {
            foreach (var v in Variants)
            {
                var reload = Assets(v).OfType<AnimationClip>()
                    .First(c => ClipName(c) == GateP6Choreography.Reload);
                Assert.AreEqual(20f, reload.length, 0.10f,
                    $"{v} reload duration drifted from the authored stages");
            }
        }

        [Test]
        public void HeroVariants_CarryPropBones_ForMusketAndRamrod()
        {
            foreach (var v in Variants)
            {
                var root = (GameObject)AssetDatabase.LoadMainAssetAtPath(
                    $"Assets/ProjectOwned/Characters/Kit/{v}.fbx");
                var names = root.GetComponentsInChildren<Transform>(true)
                    .Select(tr => tr.name).ToHashSet();
                Assert.IsTrue(names.Contains("prop_musket"), $"{v}: no prop_musket");
                Assert.IsTrue(names.Contains("prop_ramrod"), $"{v}: no prop_ramrod");
            }
        }

        [Test]
        public void HeroBudget_StaysInsideSpikeEnvelope()
        {
            // §7.4 hero tier: keep each variant's total skinned tris sane
            foreach (var v in Variants)
            {
                long tris = 0;
                foreach (var mesh in Assets(v).OfType<Mesh>())
                    tris += mesh.triangles.Length / 3;
                Assert.Less(tris, 120000, $"{v} exceeds the hero-tier budget");
            }
        }

        [Test]
        public void MidTier_PoseMeshes_ExistForBothFactions()
        {
            foreach (var f in new[] { "csa_a_mid", "union_a_mid" })
            {
                var meshes = AssetDatabase.LoadAllAssetsAtPath(
                        $"Assets/ProjectOwned/Characters/Kit/{f}.fbx")
                    .OfType<GameObject>()
                    .SelectMany(g => g.GetComponentsInChildren<MeshFilter>(true))
                    .Select(mf => mf.sharedMesh).Where(m => m != null).ToList();
                Assert.GreaterOrEqual(meshes.Count, 7,
                    $"{f} should bake at least the 7 mid poses");
                foreach (var m in meshes)
                    Assert.Less(m.triangles.Length / 3, 20000,
                        $"{f}/{m.name} too heavy for RenderMeshInstanced crowds");
            }
        }
    }
}
