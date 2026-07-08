using System;

namespace BattleAtlas
{
    // Pure battle-clock <-> video-time mapping for pre-rendered Soldier View
    // media (Angle V2 plan section 10.1):
    //
    //     videoTime       = battleClockTime - viewpoint.t0
    //     battleClockTime = viewpoint.t0 + videoTime
    //
    // The battle clock is the single source of historical time; video time is
    // always derived. All functions are static and deterministic so the
    // EditMode suite can pin the contract exactly.
    public static class SoldierViewMath
    {
        public static double BattleToVideo(double battleTime, double t0)
            => battleTime - t0;

        public static double VideoToBattle(double videoTime, double t0)
            => t0 + videoTime;

        // A viewpoint is enterable while the clock is inside [t0, t1).
        // t1 is exclusive: the final video frame starts one frame-duration
        // before t1, so battleTime == t1 has no frame to show.
        public static bool WithinWindow(double battleTime, double t0, double t1)
            => battleTime >= t0 && battleTime < t1;

        // Clamp a seek target so it always lands on a decodable frame:
        // [t0, t1 - frameDuration].
        public static double ClampToWindow(double battleTime, double t0, double t1, double fps)
        {
            double last = t1 - 1.0 / fps;
            if (battleTime < t0) return t0;
            if (battleTime > last) return last;
            return battleTime;
        }

        // Video frame index displaying battle time t (frame n spans
        // [t0 + n/fps, t0 + (n+1)/fps)). The epsilon absorbs float noise from
        // clock arithmetic so exact frame boundaries quantize forward.
        public static int FrameForBattleTime(double battleTime, double t0, double fps)
            => (int)Math.Floor((battleTime - t0) * fps + 1e-6);

        // Signed clock/video disagreement in video frames (positive: video
        // is ahead of the battle clock).
        public static double DriftFrames(double videoTime, double battleTime, double t0, double fps)
            => (videoTime - (battleTime - t0)) * fps;

        // Gate P1 tolerance: synchronized within one video frame after settle.
        public static bool WithinOneFrame(double videoTime, double battleTime, double t0, double fps)
            => Math.Abs(DriftFrames(videoTime, battleTime, t0, fps)) <= 1.0 + 1e-6;
    }
}
