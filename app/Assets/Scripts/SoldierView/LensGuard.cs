using UnityEngine;

namespace BattleAtlas
{
    // ------------------------------------------------------------------
    // P10 must-fix (the Gate P9 teleport at t~8636): in first-person the
    // camera occupies a real man's post, but OTHER units' compiled tracks
    // may pass through that exact spot — Armistead's column advances
    // straight through Garnett's halted line, and slot 1014 passed
    // 0.06 m from the lens at t=8635.97 (slots 222..225 at 0.25-0.77 m,
    // t~8634.6). At walking pace a body crossing the camera's near plane
    // reads as a MATERIALIZATION: the man is invisible until his hat
    // sweeps the near plane, then fills the frame.
    //
    // A man cannot walk through the observer. Deflect any rendered figure
    // that enters the lens-guard circle around the camera's plan position
    // along the chord of its own motion, so it walks AROUND the observer
    // at arm's length instead of through his skull. Presentation-only:
    // logical soldier state (Gate P8 digests, casualty bookkeeping, VFX
    // and audio event positions) is untouched, exactly like the hidden
    // observer figure. Pure function of (resolved path, camera position,
    // t) -> deterministic and scrub-invariant.
    //
    // Radius: 0.6 m keeps a passing torso (half-width ~0.25 m) clear of
    // the near-plane sweep zone while leaving the observer's own
    // touch-of-elbows file (nominal 0.5 m spacing, measured >= 0.63 m
    // after formation jitter) essentially untouched.
    public static class LensGuard
    {
        public const float DefaultRadiusM = 0.6f;

        // Probe timestep for the figure's direction of travel.
        const float VelDt = 0.4f;
        // Below this speed the figure is standing; use a radial push
        // (its direction is stable because the figure barely moves).
        const float StandingSpeedMps = 0.25f;

        public static Vector2 Guarded(
            AngleActionContext ctx, int unitIndex, int slot, float t,
            Vector2 pos, Vector2 cam, float radius)
        {
            var rel = pos - cam;
            float dist = rel.magnitude;
            if (dist >= radius) return pos;

            var a = SoldierActionResolver.Resolve(ctx, unitIndex, slot, t - VelDt);
            var b = SoldierActionResolver.Resolve(ctx, unitIndex, slot, t + VelDt);
            var v = new Vector2(b.posX - a.posX, b.posZ - a.posZ) / (2f * VelDt);

            if (v.magnitude < StandingSpeedMps)
            {
                // standing inside the guard: push radially to the boundary
                if (dist < 1e-4f)
                    // exactly on the lens with no motion: deterministic
                    // fallback direction from the slot identity
                    rel = new Vector2(
                        Mathf.Sin(slot * 2.399963f), Mathf.Cos(slot * 2.399963f));
                return cam + rel.normalized * radius;
            }

            // walking: slide along the chord of its own travel direction —
            // enters and leaves the circle exactly where the raw path does,
            // so the deflection is continuous in t (zero at the boundary)
            var vHat = v.normalized;
            var nHat = new Vector2(vHat.y, -vHat.x);
            float lon = Vector2.Dot(rel, vHat);
            float lat = Vector2.Dot(rel, nHat);
            float chord = Mathf.Sqrt(Mathf.Max(radius * radius - lon * lon, 0f));
            float side = lat >= 0f ? 1f : -1f;
            return cam + vHat * lon + nHat * (side * chord);
        }
    }
}
