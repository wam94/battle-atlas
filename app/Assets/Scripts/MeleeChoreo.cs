using System;
using UnityEngine;

namespace BattleAtlas
{
    // ------------------------------------------------------------------
    // Angle-v2 vocabulary, P3: the wall fight ("hand to hand, and of the
    // most desperate character; ... men climbing over the wall, and
    // fighting the enemy in his own trenches" — Peyton, or-27-1863,
    // csa-1c-pic-1-garnett.md EC5.4; Fry's clubbed-guns melee vocabulary,
    // csa-3c-het-3-fry.md EC6).
    //
    // Pure choreography math for the resolver's `melee` segment action —
    // NOT combat AI and NOT a wound system: casualties inside a melee
    // segment still come exclusively from the compiled casualty schedule
    // and resolve through the existing sober wound vocabulary
    // (violence-and-representation.md §3 — no new wound classes, no gore;
    // contact is abstracted inside the clips themselves, §5/§6).
    //
    // Per slot, deterministic from the shared FNV hash (scrub-invariant):
    //
    //   * a small front-rank subset forms GRAPPLE PAIRS with the wired
    //     opponent unit (segment.meleeOpponentId, set by the data wave):
    //     the pair meets at the deterministic midpoint between their two
    //     roster positions and plays Melee_Grapple_A / _B (authored
    //     anti-phase) facing each other until either man falls;
    //   * everyone else works short bouts — clubbed-musket swing, bayonet
    //     thrust, or parry by hash — separated by ready pauses, facing
    //     the opponent line. Bouts are bounded (clip length + rest), and
    //     the segment itself is expected to be brief (the wall-breach
    //     minutes); the data wave owns segment durations.
    //
    // No committed bundle carries a `melee` segment yet: every existing
    // action path resolves bit-identically (film safety).
    // ------------------------------------------------------------------
    public static class MeleeChoreo
    {
        public const float GrappleShare = 0.22f;   // front-rank hash share
        public const float PairSeparationM = 1.05f; // bodies never intersect
        public const float PairStaggerS = 4f;       // pairs lock over 0..4 s
        public const float StepInDur = 1.5f;        // walk into the clinch
        public const float ReturnDur = 2.0f;        // walk back after it ends

        // Angle-v2 DATA-wave production bound: a pair only forms when each
        // man's clinch anchor is within this reach of his roster slot. The
        // committed melee wiring pits a ~348 m brigade line against a
        // ~60 m regiment across the wall gap: the proportional file map
        // would otherwise send flank files sprinting tens of meters to
        // midpoints far from either line (the vocab wave's residual #1
        // presumed similar-width demo lines). Center files — the ones
        // actually AT the wall — stay inside this reach; distant files
        // keep the unpaired bout work.
        public const float MaxStepInM = 8f;
        public const float BoutStaggerS = 3f;
        public const float BoutRestMinS = 1.5f;
        public const float BoutRestMaxS = 4.0f;

        public enum Role : byte { ClubSwing = 0, BayonetThrust = 1, Parry = 2 }

        public struct Pair
        {
            public float t0;          // when the clinch locks
            public float tEnd;        // when it dissolves (either man falls)
            public Vector2 anchor;    // this slot's stance point (macro)
            public float facingDeg;   // toward the opponent
            public bool lead;         // plays Grapple_A (lead) or _B
            public int partnerSlot;   // the opposing roster's slot id
        }

        static string Key(string seed, string unitId, string segId) =>
            seed + "|" + unitId + "|" + segId + "|melee";

        // Unpaired bout role for this slot (pure).
        public static Role RoleOf(
            string seed, UnitRuntime ur, AngleBundleSegment seg, int slot)
        {
            float r = AngleEnvironmentLayout.Hash01(
                Key(seed, ur.unit.unitId, seg.id), slot * 3 + 1);
            if (r < 1f / 3f) return Role.ClubSwing;
            if (r < 2f / 3f) return Role.BayonetThrust;
            return Role.Parry;
        }

        // Bout-cycle clip for an unpaired slot at battle time t (pure).
        // Ready pauses separate bounded bouts; the stagger keeps the line
        // from swinging in unison.
        public static (ClipId clip, float clipTime) BoutAt(
            string seed, UnitRuntime ur, AngleBundleSegment seg, int slot,
            float segStart, float t)
        {
            string key = Key(seed, ur.unit.unitId, seg.id);
            ClipId clip = RoleOf(seed, ur, seg, slot) switch
            {
                Role.ClubSwing => ClipId.MeleeClubSwing,
                Role.BayonetThrust => ClipId.MeleeBayonetThrust,
                _ => ClipId.MeleeParry,
            };
            float stagger = BoutStaggerS *
                AngleEnvironmentLayout.Hash01(key, slot * 5 + 2);
            float rest = BoutRestMinS + (BoutRestMaxS - BoutRestMinS) *
                AngleEnvironmentLayout.Hash01(key, slot * 5 + 4);
            float dur = KitClips.Duration(clip);
            float cycle = dur + rest;
            float local = t - segStart - stagger;
            if (local < 0f)
                return (ClipId.StandReady,
                    KitClips.Phase(ClipId.StandReady, t + stagger * 5f));
            float c = local % cycle;
            if (c < dur) return (clip, c);
            return (ClipId.StandReady,
                KitClips.Phase(ClipId.StandReady, t + stagger * 5f));
        }

        // The opposed unit of a wired melee segment, or null. Both sides
        // must point at each other (the data wave wires symmetrically).
        public static UnitRuntime Opponent(
            AngleActionContext ctx, UnitRuntime ur, AngleBundleSegment seg,
            float t)
        {
            if (seg.action != "melee" ||
                string.IsNullOrEmpty(seg.meleeOpponentId))
                return null;
            foreach (var cand in ctx.units)
            {
                if (cand.unit.unitId != seg.meleeOpponentId) continue;
                var oppSeg = cand.unit.SegmentAt(t);
                if (oppSeg.action == "melee" &&
                    oppSeg.meleeOpponentId == ur.unit.unitId)
                    return cand;
                return null;
            }
            return null;
        }

        // Files (front-rank slots) proportional map between rosters.
        static int MapFile(int file, int filesFrom, int filesTo) =>
            Mathf.Clamp((int)((long)file * filesTo / filesFrom), 0, filesTo - 1);

        // The lead-side grapple candidate that owns follower file `vf`:
        // the FIRST candidate front-rank lead file mapping onto vf (both
        // sides run this same selection, so they always agree).
        static int LeadFileFor(
            string seed, UnitRuntime lead, AngleBundleSegment leadSeg,
            int vf, int filesLead, int filesFollow)
        {
            string key = Key(seed, lead.unit.unitId, leadSeg.id);
            // candidate lead files mapping to vf form a contiguous window
            long lo = (long)vf * filesLead / filesFollow;
            long hi = ((long)vf + 1) * filesLead / filesFollow + 1;
            for (long f = lo; f <= hi && f < filesLead; f++)
            {
                if (MapFile((int)f, filesLead, filesFollow) != vf) continue;
                if (AngleEnvironmentLayout.Hash01(key, (int)f * 7 + 5)
                    < GrappleShare)
                    return (int)f;
            }
            return -1;
        }

        // Resolve this slot's grapple pair at time t, if one exists and is
        // still (or was) active at t. Returns false for: unwired segments,
        // rear-rank slots, non-candidates, dead partners. Pure.
        public static bool TryPair(
            AngleActionContext ctx, UnitRuntime ur, AngleBundleSegment seg,
            int slot, float t, out Pair pair)
        {
            pair = default;
            var opp = Opponent(ctx, ur, seg, t);
            if (opp == null) return false;

            int files = FormationRoster.Files(ur.slotCount);
            if (slot >= files) return false;   // front rank only
            int oppFiles = FormationRoster.Files(opp.slotCount);

            bool lead = string.CompareOrdinal(
                ur.unit.unitId, opp.unit.unitId) < 0;
            UnitRuntime leadUr = lead ? ur : opp;
            UnitRuntime followUr = lead ? opp : ur;
            int filesLead = lead ? files : oppFiles;
            int filesFollow = lead ? oppFiles : files;

            var leadSeg = leadUr.unit.SegmentAt(t);
            var followSeg = followUr.unit.SegmentAt(t);

            int leadFile, followFile;
            if (lead)
            {
                leadFile = slot;
                followFile = MapFile(leadFile, filesLead, filesFollow);
                // this lead must be the follower file's agreed owner
                if (LeadFileFor(ctx.seed, leadUr, leadSeg, followFile,
                        filesLead, filesFollow) != leadFile)
                    return false;
            }
            else
            {
                followFile = slot;
                leadFile = LeadFileFor(ctx.seed, leadUr, leadSeg, followFile,
                    filesLead, filesFollow);
                if (leadFile < 0) return false;
            }

            // front-rank slot ids == file indices (FormationRoster rank 0)
            int leadSlot = leadFile, followSlot = followFile;

            float segStart = Mathf.Max(leadSeg.t0, followSeg.t0);
            string leadKey = Key(ctx.seed, leadUr.unit.unitId, leadSeg.id);
            float pairT0 = segStart + PairStaggerS *
                AngleEnvironmentLayout.Hash01(leadKey, leadFile * 7 + 6);

            float leadFall = leadUr.casualties[leadSlot].fallT;
            float followFall = followUr.casualties[followSlot].fallT;
            // both men must reach the clinch alive
            if (leadFall <= pairT0 || followFall <= pairT0) return false;
            // the clinch dissolves ReturnDur BEFORE the melee window ends
            // (unless a fall ends it first), so the walk-back completes
            // inside the segment and the hand-off to the next segment's
            // roster position is continuous — no end-of-melee pop.
            float segEnd = Mathf.Min(leadSeg.t1, followSeg.t1) - ReturnDur;
            float tEnd = Mathf.Min(Mathf.Min(leadFall, followFall), segEnd);
            if (tEnd <= pairT0) return false;
            if (t < pairT0) return false;

            Vector2 posL = SoldierActionResolver.BasePosition(
                ctx, leadUr, leadSlot, pairT0,
                leadUr.unit.segments[
                    SoldierActionResolver.SegIndexAt(leadUr, pairT0)]);
            Vector2 posF = SoldierActionResolver.BasePosition(
                ctx, followUr, followSlot, pairT0,
                followUr.unit.segments[
                    SoldierActionResolver.SegIndexAt(followUr, pairT0)]);
            Vector2 d = posF - posL;
            if (d.magnitude < 1e-3f) return false;
            Vector2 dir = d.normalized;
            Vector2 mid = (posL + posF) / 2f;
            // production reach guard: each man walks at most MaxStepInM to
            // his clinch anchor (~the midpoint); pairs beyond it never form
            if (d.magnitude / 2f > MaxStepInM) return false;

            bool selfLead = lead;
            pair = new Pair
            {
                t0 = pairT0,
                tEnd = tEnd,
                lead = selfLead,
                partnerSlot = selfLead ? followSlot : leadSlot,
                anchor = selfLead
                    ? mid - dir * (PairSeparationM / 2f)
                    : mid + dir * (PairSeparationM / 2f),
                facingDeg = selfLead
                    ? Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg
                    : Mathf.Atan2(-dir.x, -dir.y) * Mathf.Rad2Deg,
            };
            return true;
        }

        // Position adjustment for a melee slot (alive or at his fall
        // moment): grapplers stand at (or blend to/from) their clinch
        // anchor; everyone else keeps his roster position. Pure.
        public static Vector2 PositionAt(
            AngleActionContext ctx, UnitRuntime ur, AngleBundleSegment seg,
            int slot, float t, Vector2 basePos)
        {
            if (!TryPair(ctx, ur, seg, slot, t, out var pair)) return basePos;
            if (t < pair.tEnd)
            {
                float k = Mathf.Clamp01((t - pair.t0) / StepInDur);
                return Vector2.Lerp(basePos, pair.anchor, k);
            }
            float back = Mathf.Clamp01((t - pair.tEnd) / ReturnDur);
            return Vector2.Lerp(pair.anchor, basePos, back);
        }
    }
}
