using UnityEngine;

namespace BattleAtlas
{
    // Phase-2 placeholder scrubber: immediate-mode GUI in the bottom strip.
    // The real timeline UI ("The Window" design) replaces this in a later phase.
    public class TimelineHud : MonoBehaviour
    {
        public BattleClock clock;

        const float HudHeightPts = 96f;  // logical points; scaled by dpi
        static readonly float[] Speeds = { 1f, 60f, 300f };

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
