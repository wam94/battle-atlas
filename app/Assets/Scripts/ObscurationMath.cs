using UnityEngine;

namespace BattleAtlas
{
    // One live puff of powder smoke or dust at battle time t, in
    // battlefield-local XZ meters (height is the renderer's business).
    public struct Puff
    {
        public Vector2 posXZ;
        public float age01;  // age / life: 0 = just emitted, -> 1 = about to fade
        public float radius; // meters, grows linearly with age
    }

    // Per-kind emission parameters. Starting values from the phase plan;
    // tuned by eye in the Task 7 editor session — tuning lives HERE, never
    // inline at call sites.
    public struct PuffParams
    {
        public float cadence; // seconds between emission ticks
        public float life;    // seconds a puff lives
        public float radius0; // meters at emission
        public float radius1; // meters at end of life
        public float jitterM; // hash-jitter envelope around the emit position

        public static PuffParams For(string kind)
        {
            switch (kind)
            {
                case "artillery_fire": // grey-white gun smoke, long-lived
                    return new PuffParams
                    { cadence = 4f, life = 90f, radius0 = 4f, radius1 = 14f, jitterM = 12f };
                case "musketry": // white, short-lived, along the firing line
                    return new PuffParams
                    { cadence = 3f, life = 30f, radius0 = 2f, radius1 = 6f, jitterM = 8f };
                // dust is DERIVED from unit velocity, never authored — not a
                // loader kind (battle-format.md "Engagement events"); the
                // params live here so all emission tuning shares one table
                case "dust": // tan, ground-hugging, trailing the unit
                    return new PuffParams
                    { cadence = 6f, life = 45f, radius0 = 2f, radius1 = 8f, jitterM = 5f };
                default:
                    // a typo'd kind must be loud (the loader already rejects
                    // unknown authored kinds; this catches internal misuse)
                    throw new System.ArgumentException($"unknown puff kind '{kind}'");
            }
        }
    }

    // Obscuration as pure math: for any battle time t, the full set of live
    // puffs an emission event has in the air — NO stateful particle
    // simulation. Puff j of an event is emitted at t0 + j*cadence at the
    // emitter's position at that moment, hash-jittered (FNV via
    // FormationLayout.Jitter, never Random), advected linearly downwind
    // with age, and aged into 4 opacity buckets. Everything is a function
    // of (event, wind, t), so scrubbing in either direction — or jump-cuts
    // — reproduces identical fields. Same determinism discipline as
    // FormationLayout; CPU-side t is an explicit parameter (the
    // _BattleWaveTime shader global is for shader-side motion only).
    public static class ObscurationMath
    {
        // segment emitters spread max(1, round(length / this)) puffs per
        // emission tick along the line
        public const float SegmentMetersPerPuff = 150f;
        // derived dust: central-difference speed over a 30s battle-time
        // window; units moving faster than the threshold shed dust
        public const float DustSampleHalfWindow = 15f;
        public const float DustSpeedThresholdMps = 0.5f;

        // Wind as an XZ drift velocity from the environment block:
        // windTowardDeg is the compass bearing smoke drifts TOWARD
        // (0 = north = +Z, 90 = east = +X); windMps 0 = calm = zero vector.
        public static Vector2 WindVector(float windTowardDeg, float windMps)
        {
            float rad = windTowardDeg * Mathf.Deg2Rad;
            return new Vector2(Mathf.Sin(rad) * windMps, Mathf.Cos(rad) * windMps);
        }

        // Age -> one of 4 discrete opacity buckets (the render-side
        // contract: one RenderMeshInstanced batch per bucket, alpha on the
        // bucket's MaterialPropertyBlock, <= 8 draw calls for the field).
        public static int AgeBucket(float age01) => Mathf.Clamp((int)(age01 * 4f), 0, 3);

        // Fills `buffer` with the event's live puffs at battle time t and
        // returns the count. `emitterPosAt` resolves a moving emitter's
        // position at an emission time — the caller reads it from the
        // event's OWN unit's track (UnitTrack.StateAt), never from what the
        // family LOD currently renders. Pass null for the fixed-segment
        // form (position from e.x/z..x2/z2). Also serves derived dust:
        // call with dust params' kind on a synthetic per-unit event and the
        // unit-track position function. Caller owns the buffer — zero
        // allocation here. Overflow clamps deterministically OLDEST-first
        // (ticks walk newest to oldest and stop when full): the dropped
        // puffs are the ones about to die anyway, and the same t always
        // drops the same ones. The one-time overflow warning is the
        // caller's job — this stays pure.
        public static int LivePuffs(
            EventDto e, System.Func<float, Vector2> emitterPosAt, Vector2 windMps, float t,
            Puff[] buffer)
        {
            PuffParams p = PuffParams.For(e.kind);
            if (t < e.t0) return 0;

            // newest tick: emitted at or before t...
            int jHigh = Mathf.FloorToInt((t - e.t0) / p.cadence);
            // ...and strictly before t1 (guard the float division against
            // producing a tick at exactly t1)
            int jWindowLast = Mathf.CeilToInt((e.t1 - e.t0) / p.cadence) - 1;
            while (jWindowLast >= 0 && e.t0 + jWindowLast * p.cadence >= e.t1) jWindowLast--;
            if (jHigh > jWindowLast) jHigh = jWindowLast;
            // oldest tick still alive: t - tEmit < life (same fuzz guard)
            int jLow = Mathf.FloorToInt((t - e.t0 - p.life) / p.cadence) + 1;
            while (t - (e.t0 + jLow * p.cadence) >= p.life) jLow++;
            if (jLow < 0) jLow = 0;
            if (jHigh < jLow) return 0;

            // segment emitters spread several puffs per tick along the line
            var segA = new Vector2(e.x, e.z);
            var segB = new Vector2(e.x2, e.z2);
            int perTick = emitterPosAt != null
                ? 1
                : Mathf.Max(1, Mathf.RoundToInt((segB - segA).magnitude / SegmentMetersPerPuff));

            int count = 0;
            for (int j = jHigh; j >= jLow && count < buffer.Length; j--)
            {
                float tEmit = e.t0 + j * p.cadence;
                float age = t - tEmit;
                float age01 = age / p.life;
                float radius = Mathf.Lerp(p.radius0, p.radius1, age01);
                for (int k = 0; k < perTick && count < buffer.Length; k++)
                {
                    // one hash index per puff, stable across ticks; salts
                    // pick independent axes, like FormationLayout's scatter
                    int idx = j * 8191 + k;
                    Vector2 basePos;
                    if (emitterPosAt != null)
                    {
                        basePos = emitterPosAt(tEmit);
                    }
                    else
                    {
                        // hash placement along the line, not an even comb —
                        // a gun line smokes irregularly, and evenly spaced
                        // puffs would moire against the wind drift
                        float along = (FormationLayout.Jitter(e.id, idx, 5) + 1f) * 0.5f;
                        basePos = Vector2.LerpUnclamped(segA, segB, along);
                    }
                    basePos.x += FormationLayout.Jitter(e.id, idx, 11) * p.jitterM;
                    basePos.y += FormationLayout.Jitter(e.id, idx, 23) * p.jitterM;
                    buffer[count++] = new Puff
                    {
                        posXZ = basePos + windMps * age, // linear downwind advection
                        age01 = age01,
                        radius = radius,
                    };
                }
            }
            return count;
        }

        // Derived dust's speed input: central-difference speed over Δ=30s of
        // battle time. Sampled symmetrically around t so scrub direction
        // can't flip the answer at the same t — the same reasoning as
        // BattleDirector's MovingSampleHalfWindow pose bias, at a Δ sized
        // for column marches instead of pose flicker. StateAt clamps at the
        // track ends, so the window degrades one-sided there instead of
        // misreading. Dust derives ONLY from units without children
        // (battle-format.md "Engagement events"): a decomposed brigade's
        // parent track still moves as the far-tier record of the same men,
        // and double-dusting a family would fake violence — the caller
        // enforces that rule; this is just the speed.
        public static float DustSpeedAt(UnitTrack track, float t)
        {
            Vector2 before = track.StateAt(t - DustSampleHalfWindow).posXZ;
            Vector2 after = track.StateAt(t + DustSampleHalfWindow).posXZ;
            return (after - before).magnitude / (2f * DustSampleHalfWindow);
        }
    }
}
