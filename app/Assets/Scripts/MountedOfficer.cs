using System;
using UnityEngine;

namespace BattleAtlas
{
    // ------------------------------------------------------------------
    // Angle-v2 vocabulary, P5: a mounted officer falls.
    //
    // OWNER-APPROVED 2026-07-15: "ship p5 mounted officers falling"
    // (proposed ED-81 records the ruling; see angle-v2-vocab.md). This is
    // the V&R §1 named-officer rule's trigger condition — the vocabulary
    // is deliberately ANONYMOUS: a mounted officer figure attached to a
    // unit, never a named person. Where the record names the man (Garnett,
    // "shot from his horse... within about 25 paces of the stone wall,"
    // peyton-or-1863, csa-1c-pic-1-garnett.md EC1/EC4), attaching the
    // figure to that unit at that moment is a DATA-WAVE decision that
    // must cite its sources; nothing is wired here.
    //
    // The fall is staged as loss, not spectacle (V&R §5/§6): the horse
    // rears once under the hit, the rider leaves the saddle in a single
    // articulated slide and lies still (the standard persistent-body
    // rule), and the riderless horse bolts rearward out of the fight. No
    // wound vocabulary is added; no blood is staged for the officer
    // beyond the existing cause-class table if the data wave routes his
    // fall through a casualty profile.
    //
    // Pure functions of (bundle, seed, spec, t) throughout.
    // ------------------------------------------------------------------

    public enum HorseClipId : byte
    {
        HorseStand = 0,
        HorseWalk = 1,
        HorseRear = 2,
        HorseBolt = 3,
    }

    // The horse rig's clip table (characters/kit/horse.py authors the
    // actions; the horse is a project-owned procedural mesh — no external
    // asset, per the CC0/CC-BY-only rule).
    public static class HorseClips
    {
        public const int Count = 4;

        static readonly string[] Names =
        {
            "Horse_Stand", "Horse_Walk", "Horse_Rear", "Horse_Bolt",
        };

        static readonly float[] Durations = { 3f, 1.6f, 2.5f, 0.8f };
        static readonly bool[] LoopFlags = { true, true, false, true };

        public static string Name(HorseClipId id) => Names[(int)id];
        public static float Duration(HorseClipId id) => Durations[(int)id];
        public static bool IsLoop(HorseClipId id) => LoopFlags[(int)id];

        public static float Phase(HorseClipId id, float sinceStart)
        {
            float d = Durations[(int)id];
            if (LoopFlags[(int)id])
                return sinceStart - d * (float)Math.Floor(sinceStart / d);
            return Math.Min(Math.Max(sinceStart, 0f), d - 1f / 48f);
        }
    }

    // Where a mounted officer rides and when he falls. The data wave
    // compiles these from cited records; the vocabulary wave only defines
    // the shape (and exercises it in the staged evidence bundle).
    [Serializable]
    public struct MountedOfficerSpec
    {
        public string officerId;    // stable hash identity (NOT a name)
        public string unitId;       // the unit he rides with
        public float fallT;         // battle time of the hit
        public float backOffsetM;   // meters behind the line (unit frame)
        public float alongOffsetM;  // meters along the frontage (+right)
    }

    public struct MountedOfficerState
    {
        public float posX, posZ;          // the horse (macro meters)
        public float facingDeg;           // the horse
        public HorseClipId horseClip;
        public float horseClipTime;
        public bool horseVisible;         // false once the bolt leaves the field
        public ClipId riderClip;          // RideSeat / RiderFall
        public float riderClipTime;
        public float riderPosX, riderPosZ;
        public float riderFacingDeg;
        public bool riderDown;            // fallen and persisting
    }

    public static class MountedOfficer
    {
        // the rider leaves the saddle this far into the horse's rear
        public const float RiderFallStart = 0.55f;
        public const float BoltSpeedMps = 5.5f;
        public const float BoltRangeM = 160f;   // then the horse has left the scene
        public const float BoltYawJitterDeg = 25f;
        public const float WalkThresholdMps = 0.25f;

        public static MountedOfficerState Resolve(
            AngleActionContext ctx, MountedOfficerSpec spec, float t)
        {
            var ur = ctx.Unit(spec.unitId);
            t = Mathf.Clamp(t, ctx.bundle.slice.t0, ctx.bundle.slice.t1);

            var s = new MountedOfficerState { horseVisible = true };

            if (t < spec.fallT)
            {
                Vector2 p = RidePosition(ctx, ur, spec, t);
                float facing = ur.unit.FacingAt(t);
                s.posX = p.x;
                s.posZ = p.y;
                s.facingDeg = facing;
                bool walking = RideSpeed(ctx, ur, spec, t) >= WalkThresholdMps;
                s.horseClip = walking
                    ? HorseClipId.HorseWalk : HorseClipId.HorseStand;
                s.horseClipTime = HorseClips.Phase(s.horseClip, t);
                s.riderClip = ClipId.RideSeat;
                s.riderClipTime = KitClips.Phase(ClipId.RideSeat, t);
                s.riderPosX = p.x;
                s.riderPosZ = p.y;
                s.riderFacingDeg = facing;
                return s;
            }

            // the hit: everything after is anchored at the fall point
            Vector2 fallPos = RidePosition(ctx, ur, spec, spec.fallT);
            float fallFacing = ur.unit.FacingAt(spec.fallT);
            float rearDur = HorseClips.Duration(HorseClipId.HorseRear);

            // rider
            s.riderFacingDeg = fallFacing;
            s.riderPosX = fallPos.x;
            s.riderPosZ = fallPos.y;
            if (t < spec.fallT + RiderFallStart)
            {
                s.riderClip = ClipId.RideSeat;
                s.riderClipTime = KitClips.Phase(ClipId.RideSeat, t);
            }
            else
            {
                float fdur = KitClips.Duration(ClipId.RiderFall);
                float since = t - spec.fallT - RiderFallStart;
                s.riderClip = ClipId.RiderFall;
                s.riderClipTime = Mathf.Min(since, fdur - 1f / 48f);
                s.riderDown = since >= fdur;
            }

            // horse
            if (t < spec.fallT + rearDur)
            {
                s.posX = fallPos.x;
                s.posZ = fallPos.y;
                s.facingDeg = fallFacing;
                s.horseClip = HorseClipId.HorseRear;
                s.horseClipTime = t - spec.fallT;
                return s;
            }

            // riderless: the horse bolts rearward, out of the fight
            float boltT0 = spec.fallT + rearDur;
            float yaw = fallFacing + 180f + BoltYawJitterDeg *
                (2f * AngleEnvironmentLayout.Hash01(
                    "mounted|" + spec.officerId + "|bolt", 1) - 1f);
            float dist = BoltSpeedMps * (t - boltT0);
            float r = yaw * Mathf.Deg2Rad;
            var dir = new Vector2(Mathf.Sin(r), Mathf.Cos(r));
            Vector2 hp = fallPos + dir * Mathf.Min(dist, BoltRangeM);
            s.posX = hp.x;
            s.posZ = hp.y;
            s.facingDeg = yaw;
            s.horseClip = HorseClipId.HorseBolt;
            s.horseClipTime = HorseClips.Phase(HorseClipId.HorseBolt, t - boltT0);
            s.horseVisible = dist <= BoltRangeM;
            return s;
        }

        // The officer's riding station: unit frame, behind the line.
        static Vector2 RidePosition(
            AngleActionContext ctx, UnitRuntime ur, MountedOfficerSpec spec,
            float t)
        {
            Vector2 centroid = ur.unit.PositionAt(t);
            float facing = SoldierActionResolver.FrameFacingAt(ur, t);
            return FormationRoster.ToWorld(centroid, facing,
                new Vector2(spec.alongOffsetM, -spec.backOffsetM));
        }

        static float RideSpeed(
            AngleActionContext ctx, UnitRuntime ur, MountedOfficerSpec spec,
            float t)
        {
            float ta = Mathf.Max(t - 0.6f, ctx.bundle.slice.t0);
            float tb = Mathf.Min(t + 0.6f, ctx.bundle.slice.t1);
            if (tb - ta < 1e-3f) return 0f;
            Vector2 a = RidePosition(ctx, ur, spec, ta);
            Vector2 b = RidePosition(ctx, ur, spec, tb);
            return (b - a).magnitude / (tb - ta);
        }
    }
}
