using UnityEngine;

namespace BattleAtlas
{
    public static class ClockMath
    {
        // Returns the new battle time and whether playback continues
        // (playback auto-pauses when the clock reaches the end of the battle).
        public static (float t, bool playing) Advance(
            float t, float dt, float speed, float endTime)
        {
            float next = t + dt * speed;
            if (next >= endTime) return (endTime, false);
            return (next, true);
        }

        public static string FormatTime(float seconds)
        {
            int s = Mathf.FloorToInt(seconds);
            return $"{s / 3600:D2}:{s / 60 % 60:D2}:{s % 60:D2}";
        }

        // startTime is seconds since local midnight of the battle day; shows
        // the wall clock the participants lived ("14:32:10"), not elapsed time
        public static string FormatClockTime(float startTime, float t)
        {
            int s = Mathf.FloorToInt(startTime + t);
            return $"{s / 3600 % 24:D2}:{s / 60 % 60:D2}:{s % 60:D2}";
        }
    }

    // The battle's single source of time. Everything renders FROM this clock;
    // nothing advances it but Update (or the scrubber writing CurrentTime).
    public class BattleClock : MonoBehaviour
    {
        public float CurrentTime;
        public float StartTime; // seconds since local midnight of the battle day
        public float EndTime = 3600f;
        public bool Playing;
        public float Speed = 60f; // battle-seconds per real second

        void Update()
        {
            if (!Playing) return;
            (CurrentTime, Playing) =
                ClockMath.Advance(CurrentTime, Time.deltaTime, Speed, EndTime);
        }
    }
}
