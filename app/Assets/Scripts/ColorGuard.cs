using System;
using UnityEngine;

namespace BattleAtlas
{
    // ------------------------------------------------------------------
    // Angle-v2 vocabulary, P4: colors falling and passing.
    //
    // "Every flag in the brigade excepting one was captured at or within
    // the works" — Shepard, or-27-2-shepard (csa-3c-het-3-fry.md EC5.4):
    // 1st Tenn 3 color-bearers down, 13th Ala 3, 14th Tenn 4, 7th Tenn 3.
    //
    // A unit whose bundle entry carries `colorParty > 0` stages a
    // deterministic color-guard SUCCESSION CHAIN: the center-front man
    // carries the colors; when the compiled casualty schedule fells him,
    // the colors lie where he fell (Grounded) until the next living man
    // of the chain takes them up (Colors_Pickup) and carries on; when the
    // chain is exhausted the colors stay on the ground (Down). Whether a
    // grounded flag is CAPTURED is a semantic event the data wave owns —
    // this class only depicts fall / ground / pickup.
    //
    // Sober-representation notes (violence-and-representation.md):
    //   * the bearer is a ROLE attached to a procedural slot, never an
    //     identified person (§1) — succession counts are wired at the
    //     regiment level by the data wave against the cited totals (§2);
    //   * a falling bearer resolves through the same compiled casualty
    //     entry (cause, wound, timing) as any other man — only his fall
    //     CLIP differs (Colors_Bearer_Fall keeps the staff in shot);
    //   * no committed bundle carries colorParty yet: every existing
    //     bundle resolves bit-identically (film safety).
    //
    // Everything here is a pure function of (bundle, seed, t).
    // ------------------------------------------------------------------
    public static class ColorGuard
    {
        // colors lie on the ground before the next man reaches them
        // (reach + stoop; the pickup clip itself then takes RaiseDur)
        public const float PickupDelay = 4f;
        public static float RaiseDur => KitClips.Duration(ClipId.ColorsPickup);
        public static float FallDur => KitClips.Duration(ClipId.ColorsBearerFall);

        public enum Phase : byte
        {
            Carried = 0,        // a living bearer holds the colors
            BearerFalling = 1,  // the bearer is going down, staff tipping
            Grounded = 2,       // colors on the ground, successor coming
            Raising = 3,        // the next man plays Colors_Pickup
            Down = 4,           // chain exhausted; colors stay down
        }

        public struct ColorsState
        {
            public Phase phase;
            public int bearerSlot;   // holder/raiser; -1 when Grounded/Down
            public float posX, posZ; // staff position (macro meters)
            public float sinceT;     // when this phase began
        }

        // Chain slot i: front-rank files walking outward from the center
        // (0, +1, -1, +2, -2, ...) — the color guard stands at the center
        // of the line. Front-rank slot ids equal file indices.
        public static int ChainSlot(int slotCount, int i)
        {
            int files = FormationRoster.Files(slotCount);
            int center = (files - 1) / 2;
            int offset = (i + 1) / 2;
            int file = (i % 2 == 1) ? center + offset : center - offset;
            return Mathf.Clamp(file, 0, files - 1);
        }

        public static int ChainLength(UnitRuntime ur) =>
            Mathf.Min(ur.unit.colorParty,
                FormationRoster.Files(ur.slotCount));

        // The colors' full succession state at battle time t. Pure: walks
        // the compiled casualty schedule along the chain.
        public static ColorsState StateAt(
            AngleActionContext ctx, UnitRuntime ur, float t)
        {
            int n = ChainLength(ur);
            if (n <= 0)
                throw new InvalidOperationException(
                    $"{ur.unit.unitId}: ColorGuard queried without a " +
                    "colorParty (bundle field is 0)");

            float carryFrom = ur.unit.perSecond.t0;
            bool raising = false;   // does the current carry start with a pickup?
            for (int i = 0; i < n; i++)
            {
                int bearer = ChainSlot(ur.slotCount, i);
                float fallT = ur.casualties[bearer].fallT;
                if (fallT <= carryFrom) continue;   // already down; next man

                if (t < carryFrom + (raising ? RaiseDur : 0f) && raising)
                {
                    Vector2 up = GroundPosition(ctx, ur, i, carryFrom);
                    return new ColorsState
                    {
                        phase = Phase.Raising,
                        bearerSlot = bearer,
                        posX = up.x,
                        posZ = up.y,
                        sinceT = carryFrom,
                    };
                }
                if (t < fallT)
                {
                    Vector2 p = SoldierActionResolver.ResolvedPosition(
                        ctx, ur, bearer, t);
                    return new ColorsState
                    {
                        phase = Phase.Carried,
                        bearerSlot = bearer,
                        posX = p.x,
                        posZ = p.y,
                        sinceT = carryFrom + (raising ? RaiseDur : 0f),
                    };
                }

                // the bearer has fallen; the colors lie at his fall spot
                Vector2 g = SoldierActionResolver.ResolvedPosition(
                    ctx, ur, bearer, fallT);
                if (t < fallT + FallDur)
                    return new ColorsState
                    {
                        phase = Phase.BearerFalling,
                        bearerSlot = bearer,
                        posX = g.x,
                        posZ = g.y,
                        sinceT = fallT,
                    };

                // find the next chain man still alive when pickup begins
                float pickupT = fallT + PickupDelay;
                int j = i + 1;
                while (j < n &&
                       ur.casualties[ChainSlot(ur.slotCount, j)].fallT
                           <= pickupT)
                    j++;
                if (j >= n)
                    return new ColorsState
                    {
                        phase = Phase.Down,
                        bearerSlot = -1,
                        posX = g.x,
                        posZ = g.y,
                        sinceT = fallT,
                    };
                if (t < pickupT)
                    return new ColorsState
                    {
                        phase = Phase.Grounded,
                        bearerSlot = -1,
                        posX = g.x,
                        posZ = g.y,
                        sinceT = fallT,
                    };

                // hand the loop to the successor
                i = j - 1;          // the for-increment lands on j
                carryFrom = pickupT;
                raising = true;
            }

            // chain exhausted before the window ended
            int last = ChainSlot(ur.slotCount, n - 1);
            Vector2 lg = SoldierActionResolver.ResolvedPosition(
                ctx, ur, last, ur.casualties[last].fallT);
            return new ColorsState
            {
                phase = Phase.Down,
                bearerSlot = -1,
                posX = lg.x,
                posZ = lg.y,
                sinceT = ur.casualties[last].fallT,
            };
        }

        // Where the colors lay when chain member i's pickup began: the
        // previous bearer's fall position.
        static Vector2 GroundPosition(
            AngleActionContext ctx, UnitRuntime ur, int i, float pickupT)
        {
            // the man whose fall grounded the colors is the latest chain
            // member before i whose fall precedes pickupT
            for (int k = i - 1; k >= 0; k--)
            {
                int prev = ChainSlot(ur.slotCount, k);
                float fallT = ur.casualties[prev].fallT;
                if (fallT < pickupT)
                    return SoldierActionResolver.ResolvedPosition(
                        ctx, ur, prev, fallT);
            }
            return SoldierActionResolver.ResolvedPosition(
                ctx, ur, ChainSlot(ur.slotCount, i), pickupT);
        }

        // Was `slot` the acting bearer at the moment he fell? (Decides
        // Colors_Bearer_Fall vs the standard hash fall.)
        public static bool WasBearerAtFall(
            AngleActionContext ctx, UnitRuntime ur, int slot, float fallT)
        {
            if (ur.unit.colorParty <= 0) return false;
            var s = StateAt(ctx, ur, fallT - 1e-3f);
            return (s.phase == Phase.Carried || s.phase == Phase.Raising) &&
                   s.bearerSlot == slot;
        }
    }
}
