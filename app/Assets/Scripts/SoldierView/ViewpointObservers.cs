using System.Collections.Generic;

namespace BattleAtlas
{
    // ------------------------------------------------------------------
    // Phase 9 observer-casualty policy (plan §6.5, §12 Phase 9).
    //
    // The Soldier View observer is a REPRESENTATIVE unnamed soldier, not
    // an identified person (§6.5 editorialNote) — and not a tracked
    // individual fate. The casualty schedule assigns aggregate,
    // reconstructed losses to slots (§6.4); if the observer slot drew one
    // of those reconstructed fates, the viewpoint would silently assert
    // "the man standing HERE fell at THIS second", which the evidence
    // never says, and the camera would end face-down in the wheat for the
    // remainder of the window.
    //
    // Policy (editorial decision ED-22, angle-editorial-decisions.md):
    // the observer slot of every committed viewpoint is EXEMPT from
    // victim selection. The unit's casualty totals are unchanged —
    // another slot draws the fate the observer would have drawn — and
    // reconciliation with compiled per-second strength still holds
    // exactly. The exemption is disclosed in the viewpoint's editorial
    // note; nothing else about the slot is special (same drill, same
    // formation position, same reactions).
    //
    // This table is the single source of truth consumed by
    // CasualtySchedule.Compile. An EditMode test pins it against the
    // committed StreamingAssets/SoldierView/viewpoints.json (entries with
    // a "dev-" id prefix are synthetic media-contract fixtures whose
    // camera never rides the resolver; they are not protected).
    // ------------------------------------------------------------------
    public static class ViewpointObservers
    {
        // unitId -> observer slot ids (sorted ascending).
        public static readonly IReadOnlyDictionary<string, int[]> ProtectedSlots =
            new Dictionary<string, int[]>
            {
                // garnett-road-to-angle (§3.4 required hero viewpoint):
                // slot 881 = rear rank of file 184 (documented deviation
                // from §6.5's illustrative slotId 184: the front-rank slot
                // shows an empty road in first person; from the rear rank
                // the whole front rank reads 1.3 m ahead)
                { "csa-garnett", new[] { 881 } },
                // iverson-forney-field (second film, design stage): slot
                // 184 = rear rank of file 24 in the 12th North Carolina
                // (319 slots -> 160 files) — the P9 rear-rank lesson
                // applied; 28 m in from the regiment's left flank so the
                // left three regiments' destruction plays out down-line
                // in frame-left. The 12th NC is the documented survivor
                // regiment, so the ED-22 exemption coincides with a real
                // survival population (see the viewpoint's editorialNote).
                { "csa-12nc", new[] { 184 } },
                // webb-wall (OP-2, the plan's named defender-side
                // extension): slot 230 = rear rank of file 73 in the 71st
                // Pennsylvania (313 slots -> 157 files) — the P9 rear-rank
                // lesson again; just left of the regimental center at the
                // outer-angle wall, in the compiled gap between two of
                // Cushing's pieces, so the front rank, the wall, and the
                // arriving charge all read from a forward-facing camera.
                // The receiving line ALSO takes casualties (48 of 313
                // in-window): the ED-22 exemption is disclosed in the
                // viewpoint's editorialNote, and the observer's survival
                // coincides with the regiment's documented majority.
                { "us-71pa", new[] { 230 } },
                // cushing-canister (OP-3, the plan's named gun-crew
                // extension): slot 44 = gun 2's rearmost left serving
                // position (crew slots are gun + 6*member; member 7 of 8,
                // 6.1 m behind the piece at the trail). NOT Lt. Cushing
                // (ED-3): no slot asserts his identity; his documented
                // death rides claim-cushing-death, never a slot fate.
                { "us-btty-cushing", new[] { 44 } },
            };

        public static bool IsProtected(string unitId, int slot)
        {
            if (!ProtectedSlots.TryGetValue(unitId, out var slots)) return false;
            for (int i = 0; i < slots.Length; i++)
                if (slots[i] == slot) return true;
            return false;
        }
    }
}
