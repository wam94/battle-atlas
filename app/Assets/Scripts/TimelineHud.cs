using System.Collections.Generic;
using UnityEngine;

namespace BattleAtlas
{
    // Phase-2 placeholder scrubber: immediate-mode GUI in the bottom strip.
    // The real timeline UI ("The Window" design) replaces this in a later
    // phase — including the one selected-unit citation line seeded here
    // (atlas-cartography plan, Task 6): one line added in IMGUI is one
    // line deleted there, and it is the SEED of the Phase 11 source
    // drawer, scoped deliberately small.
    public class TimelineHud : MonoBehaviour
    {
        public BattleClock clock;
        // sun rig on the Directional Light; the chip toggles its
        // presentation mode (see SunDirector for the labeling doctrine)
        public SunDirector sun;
        // contour swap on the terrain (added by LandcoverImporter); found
        // at Start since re-imports replace the terrain GameObject
        public ReliefContourToggle contours;
        // selection feed for the citation line; found at Start like the
        // chips' targets
        public BattleDirector director;

        const float HudHeightPts = 96f;  // logical points; scaled by dpi
        static readonly float[] Speeds = { 1f, 60f, 300f };
        const float ChipWidth = 200f;

        // the no-faking gate's display text: where the record is silent,
        // the line says so — it never invents a source
        public const string NoReliableRecord = "no reliable record";

        // Set every frame so OrbitCameraController can ignore touches over the HUD.
        public static float CurrentHudHeightPx { get; private set; }

        public static float HudScale(float dpi) =>
            Mathf.Max(1f, dpi / 160f);

        // touchPos uses Input.Touch coords: origin bottom-left, y up.
        public static bool IsTouchOverHud(Vector2 touchPos, float hudHeightPx) =>
            touchPos.y <= hudHeightPx;

        // The citation carried by the bracketing START keyframe at t —
        // exactly UnitTrack's formation/confidence carry rule (a segment
        // cites its start; the clamps cite the track's ends). Linear scan:
        // this runs once per frame for ONE selected unit, not per unit.
        public static string CitationAt(List<KeyframeDto> keyframes, float t)
        {
            KeyframeDto bracketing = keyframes[0];
            for (int i = 1; i < keyframes.Count; i++)
            {
                if (keyframes[i].t > t) break;
                bracketing = keyframes[i];
            }
            return bracketing.citation;
        }

        // The selected unit's provenance line: name · echelon ·
        // strength-at-t rounded · confidence · citation (or the honest
        // absence). Pure so the two formats the tests pin can't drift.
        public static string SelectedUnitLine(
            string name, UnitSymbol.Echelon echelon, float strength,
            string confidence, string citation)
        {
            string echelonWord;
            switch (echelon)
            {
                case UnitSymbol.Echelon.Regiment: echelonWord = "regiment"; break;
                case UnitSymbol.Echelon.Battery: echelonWord = "battery"; break;
                case UnitSymbol.Echelon.Park: echelonWord = "park"; break;
                default: echelonWord = "brigade"; break;
            }
            string cite = string.IsNullOrEmpty(citation) ? NoReliableRecord : citation;
            return $"{name} · {echelonWord} · {Mathf.RoundToInt(strength)} men" +
                $" · {confidence} · {cite}";
        }

        void Awake()
        {
            CurrentHudHeightPx = HudHeightPts * HudScale(Screen.dpi);
        }

        void Start()
        {
            // tolerate a lost scene reference (same fallback doctrine as
            // BattleDirector.terrain) so the chips survive scene surgery
            if (sun == null) sun = FindFirstObjectByType<SunDirector>();
            if (contours == null) contours = FindFirstObjectByType<ReliefContourToggle>();
            if (director == null) director = FindFirstObjectByType<BattleDirector>();
        }

        void OnGUI()
        {
            float s = HudScale(Screen.dpi);
            CurrentHudHeightPx = HudHeightPts * s;
            float h = HudHeightPts; // we draw in scaled space below
            GUI.matrix = Matrix4x4.Scale(new Vector3(s, s, 1f));
            float w = Screen.width / s;
            float top = Screen.height / s - h;

            GUI.Box(new Rect(0, top, w, h), GUIContent.none);

            // play/pause
            string label = clock.Playing ? "❚❚" : "►";
            if (GUI.Button(new Rect(8, top + 8, 48, 36), label))
            {
                if (!clock.Playing && clock.CurrentTime >= clock.EndTime)
                    clock.CurrentTime = 0f; // replay from the start
                clock.Playing = !clock.Playing;
            }

            // speed cycle
            if (GUI.Button(new Rect(64, top + 8, 64, 36), $"{clock.Speed:0}×"))
            {
                int i = System.Array.IndexOf(Speeds, clock.Speed);
                clock.Speed = Speeds[(i + 1) % Speeds.Length];
            }

            // Reading-light chip: the ephemeris sun is the record, the NW
            // raking light is presentation — the label says so on the chip
            // itself (the same labeled-not-smuggled doctrine as the 2.5x
            // vertical exaggeration)
            if (sun != null)
                sun.ReadingLight = GUI.Toggle(
                    new Rect(w - ChipWidth - 8, top + 8, ChipWidth, 36),
                    sun.ReadingLight, "Reading light (presentation)", GUI.skin.button);

            // Contour chip: swaps the terrain albedos for the pipeline's
            // relief_contours variant — both are derivatives of the DEM,
            // honest in either position (see ReliefContourToggle)
            if (contours != null)
                contours.SetContours(GUI.Toggle(
                    new Rect(w - ChipWidth - 8 - 92, top + 8, 84, 36),
                    contours.Contours, "Contours", GUI.skin.button));

            // time label: wall clock when the battle has a real-day anchor
            string timeLabel = clock.StartTime > 0f
                ? ClockMath.FormatClockTime(clock.StartTime, clock.CurrentTime)
                : ClockMath.FormatTime(clock.CurrentTime);
            GUI.Label(new Rect(140, top + 14, 100, 24), timeLabel);

            // selected-unit citation line, in the slack between the time
            // label and the chips (per-frame string build is IMGUI-normal
            // here — the time label above does the same)
            if (director != null
                && director.TryGetSelected(out UnitTrack track, out UnitSymbol.Echelon echelon))
            {
                string name = string.IsNullOrEmpty(track.Unit.name)
                    ? track.Unit.id : track.Unit.name;
                UnitState state = track.StateAt(clock.CurrentTime);
                GUI.Label(new Rect(248, top + 14, w - ChipWidth - 356, 24),
                    SelectedUnitLine(name, echelon, state.strength, state.confidence,
                        CitationAt(track.Unit.keyframes, clock.CurrentTime)));
            }

            // scrubber
            float scrubbed = GUI.HorizontalSlider(
                new Rect(8, top + 56, w - 16, 24), clock.CurrentTime, 0f, clock.EndTime);
            if (!Mathf.Approximately(scrubbed, clock.CurrentTime))
            {
                clock.CurrentTime = scrubbed;
                clock.Playing = false; // grabbing the scrubber pauses playback
            }
        }
    }
}
