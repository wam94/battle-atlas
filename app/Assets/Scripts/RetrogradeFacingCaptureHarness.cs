using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace BattleAtlas
{
    // Retrograde-facing slice evidence harness. Inert unless the player is
    // launched with -retrofacingshots; then it captures the requested shot
    // SET (-retrofacingset collapse|pickett — matching the phase file the
    // run loads via -battleFile) to -retrofacingOut <dir>, and quits. Same
    // DayExpansion2/3CaptureHarness mold (windowed Development standalone;
    // the owner's editor untouched).
    //
    // Each set is one retreating unit, two timestamps: an "engaged" shot
    // (facing established while still in the fight — identical before/
    // after the retrograde-facing fix, since that leg was never touched)
    // and a "retreat" shot on the SAME unit's next keyframe — pre-fix the
    // symbol's chevron spins to match the direction of travel (facing
    // away from the enemy); post-fix it holds the engaged facing (facing
    // the enemy) while the unit's position moves back. Run once against a
    // pristine origin/main battle file and once against this worktree's
    // fixed one (see scripts/retrograde-facing-captures.sh) to get the
    // before/after pair.
    public class RetrogradeFacingCaptureHarness : MonoBehaviour
    {
        struct Shot
        {
            public string Name;
            public float T;
            public Vector3 Pivot;
            public float Yaw, Pitch, Dist;

            public Shot(string name, float t, Vector3 pivot, float yaw,
                float pitch, float dist)
            {
                Name = name; T = t; Pivot = pivot;
                Yaw = yaw; Pitch = pitch; Dist = dist;
            }
        }

        // July 1 afternoon: von Gilsa's brigade (us-vongilsa), Barlow's
        // Knoll -> the break -> through town. Both shots sit on the
        // t=6300->9600 leg, whose START formation is "line" — the symbol
        // wears its facing chevron throughout (the chevron only renders
        // for ordered formations, and UnitTrack carries the segment's
        // start formation). "engaged" is the leg's start keyframe (the
        // knoll line's north face, facing 350 — identical before/after
        // by construction, the control shot); "retreat" is MID-LEG at
        // t=8500 (u~0.67), the brigade ~250 m back off the knoll:
        // pre-fix, Mathf.LerpAngle is mid-sweep toward the old keyframe
        // value 185, so the chevron has visibly spun to ~240 (pointing
        // away from the enemy); post-fix both bracketing keyframes read
        // 350, so the chevron still points north AT the knoll while the
        // line backs away.
        // The -figures shots sit in the Soldiers LOD tier (dist <
        // BattleDirector.SoldiersInDist): the line's individual figures
        // show the facing directly. The -symbol shots back off past
        // RegimentsOutDist into the Block tier, where the monolithic
        // draped map symbol renders WITH its facing chevron
        // (SymbolMeshBuilder facingSpine) — the chevron is the mark the
        // owner ruling is about, so both tiers are evidenced.
        static readonly Shot[] CollapseShots =
        {
            new Shot("vongilsa-engaged-t6300", 6300f,
                new Vector3(5350f, 0f, 8480f), 0f, 55f, 450f),
            new Shot("vongilsa-retreat-t8500", 8500f,
                new Vector3(5217f, 0f, 8227f), 0f, 55f, 450f),
            new Shot("vongilsa-engaged-symbol-t6300", 6300f,
                new Vector3(5350f, 0f, 8480f), 0f, 60f, 4600f),
            new Shot("vongilsa-retreat-symbol-t8500", 8500f,
                new Vector3(5217f, 0f, 8227f), 0f, 60f, 4600f),
        };

        // July 3, the Pickett episode's supporting advance: Wilcox's
        // brigade (csa-wilcox — NOT one of the 13 Angle-cast pinned
        // units; the pinned brigade markers Garnett/Kemper/Armistead/Fry
        // still show the pre-fix spin, deferred to the owner per the
        // film-safety tripwire, see docs/reconstruction/audit/
        // retrograde-facing.md §4). Both shots sit on the t=10380->10500
        // retreat leg back toward Seminary Ridge after the 16th
        // Vermont's flank attack; start formation "line", chevron
        // rendered. "engaged" is the leg's start keyframe (facing 85,
        // toward the Union line — identical before/after, the control
        // shot); "recross" is MID-LEG at t=10450 (u~0.58): pre-fix the
        // facing is mid-sweep toward the old keyframe value 260 (chevron
        // spun to ~187, pointing south/away); post-fix it holds 85, the
        // chevron still pointing east AT the McGilvery line while the
        // brigade retires west.
        static readonly Shot[] PickettShots =
        {
            new Shot("wilcox-engaged-t10380", 10380f,
                new Vector3(4060f, 0f, 4290f), 0f, 55f, 450f),
            new Shot("wilcox-recross-t10450", 10450f,
                new Vector3(3909f, 0f, 4313f), 0f, 55f, 450f),
            new Shot("wilcox-engaged-symbol-t10380", 10380f,
                new Vector3(4060f, 0f, 4290f), 0f, 60f, 4600f),
            new Shot("wilcox-recross-symbol-t10450", 10450f,
                new Vector3(3909f, 0f, 4313f), 0f, 60f, 4600f),
        };

        string outDir;
        string set;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-retrofacingshots") < 0)
                return;
            var go = new GameObject("RetrogradeFacingCaptureHarness");
            DontDestroyOnLoad(go);
            go.AddComponent<RetrogradeFacingCaptureHarness>();
        }

        static string ArgValue(string name, string fallback)
        {
            var args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, name);
            return (i >= 0 && i + 1 < args.Length) ? args[i + 1] : fallback;
        }

        void Start()
        {
            Application.runInBackground = true;
            outDir = ArgValue("-retrofacingOut",
                Path.Combine(Application.persistentDataPath, "retrofacing-shots"));
            set = ArgValue("-retrofacingset", "collapse");
            Directory.CreateDirectory(outDir);
            StartCoroutine(Run());
        }

        IEnumerator Capture(string name)
        {
            yield return null;
            yield return null;
            ScreenCapture.CaptureScreenshot(Path.Combine(outDir, name));
            yield return null;
            yield return null;
            yield return new WaitForSecondsRealtime(0.3f);
        }

        IEnumerator Run()
        {
            var clock = FindFirstObjectByType<BattleClock>();
            var orbit = FindFirstObjectByType<OrbitCameraController>();
            if (clock == null || orbit == null)
            {
                Debug.LogError("retrofacing shots: scene has no clock/orbit camera");
                Quit(1);
                yield break;
            }
            yield return new WaitForSecondsRealtime(3f); // warm up rendering
            clock.Playing = false;

            Shot[] shots = set == "pickett" ? PickettShots : CollapseShots;
            foreach (Shot s in shots)
            {
                clock.CurrentTime = s.T;
                orbit.followPivot = null;
                orbit.pivot = s.Pivot;
                orbit.yawDeg = s.Yaw;
                orbit.pitchDeg = s.Pitch;
                orbit.distance = s.Dist;
                yield return new WaitForSecondsRealtime(1.0f);
                yield return Capture($"retrofacing-{set}-{s.Name}.png");
            }

            Debug.Log($"retrofacing shots ({set}) written to {outDir}");
            Quit(0);
        }

        void Quit(int code)
        {
#if UNITY_EDITOR
            Debug.Log($"RetrogradeFacingCaptureHarness done (code {code}); not quitting in editor.");
#else
            Application.Quit(code);
#endif
        }
    }
}
