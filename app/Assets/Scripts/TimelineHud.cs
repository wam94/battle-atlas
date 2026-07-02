using UnityEngine;

namespace BattleAtlas
{
    // Phase-2 placeholder scrubber: immediate-mode GUI in the bottom strip.
    // The real timeline UI ("The Window" design) replaces this in a later phase.
    public class TimelineHud : MonoBehaviour
    {
        public BattleClock clock;
        // sun rig on the Directional Light; the chip toggles its
        // presentation mode (see SunDirector for the labeling doctrine)
        public SunDirector sun;
        // contour swap on the terrain (added by LandcoverImporter); found
        // at Start since re-imports replace the terrain GameObject
        public ReliefContourToggle contours;

        const float HudHeightPts = 96f;  // logical points; scaled by dpi
        static readonly float[] Speeds = { 1f, 60f, 300f };
        const float ChipWidth = 200f;

        // Set every frame so OrbitCameraController can ignore touches over the HUD.
        public static float CurrentHudHeightPx { get; private set; }

        public static float HudScale(float dpi) =>
            Mathf.Max(1f, dpi / 160f);

        // touchPos uses Input.Touch coords: origin bottom-left, y up.
        public static bool IsTouchOverHud(Vector2 touchPos, float hudHeightPx) =>
            touchPos.y <= hudHeightPx;

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
