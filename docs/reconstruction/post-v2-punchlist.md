# Post-V2 punchlist — owner feedback across Gates P6–P11

Collected verbatim-sourced deferrals from gate reviews. Each item is a
candidate for its own focused slice (research → plan → execute → gate),
per the owner's direction at Gate P11: "for things like this… worth
taking as individual slices to get right on a deep dive."

## Deep-dive slices (each warrants its own plan)

1. **Theater label legibility** (P11: "the labels are there but not easy
   to see. this will need some thinking"). Altitude-driven label LOD
   (echelon filtering by zoom: corps → division → brigade), halo/plate
   contrast against terrain, collision decluttering that actually drops
   labels instead of overlapping, possibly leader-line offsets.
2. **Unit-type differentiation in the ribbon language** (P11: "i dont
   see any differentiation among unit types (e.g. cannons)"). Artillery
   battery glyphs (gun-count ticks), cavalry cut, HQ markers — the old
   slab language encoded arm by height/shade; the ribbon language must
   encode it explicitly.
3. **Macro movement/orientation quality** (P11: "unit movements and
   orientations… still rather crude"). Facing conventions during
   maneuvers, wheel representation at ribbon scale, path smoothing at
   the macro grain.
4. **Charge-phase intensity and carnage** (P11: "a lack of action during
   the actual charge — the scale of the carnage isnt clear, the soldier
   is not on the frontline himself, and there's a lot of standing
   around"). Research-first slice: per-phase casualty pacing estimates
   (where in the advance did losses actually concentrate), behavior
   under fire (marching grimly vs taking cover — the clip vocabulary
   exists: brace/flinch/waver/kneel), and observer placement options
   (a forward-file variant viewpoint, or additional viewpoints per the
   plan's §3.4 extensions: webb-wall, cushing-canister).
5. **Fence-crossing animation read** (P10: "they look ridiculous going
   over the fences"). Climb-clip re-authoring with hand plants and
   weight shift; possibly per-rail contact.
6. **Sound enrichment** (P10: "we'll enrich the sound further later";
   ED-23 gaps): friendly musketry/reload foley where attested, an
   evidence-based distant-artillery layer, worded drill commands,
   equipment jingle, lossless source re-acquisition.
7. **Species-accurate vegetation** (P7: "making the trees accurate to
   gettysburg pennsylvania"). Oak/chestnut-oak/sassafras copse per
   claim-copse-form-1863; orchard species; witness-tree research.

## Smaller fixes (batchable)

- Soldier View: no play-speed designator visible (P11) — display the
  forced 1× state. **(Assigned to Phase 12.)**
- Crowd interpenetration at close range in packed lines (P9: "messy
  behavior by the models").
- Tile-edge haze wall + tactical-height banding (P7; Phase 10/11
  composition work never picked it up — macro context around the crop).
- Editor Game-view UI scaling at simulated QHD (P11 session note) —
  consider ScaleWithScreenSize or a documented editor workflow.
- Seek worst-case ~101–107 ms vs the ~100 ms revisit trigger (P10/P11)
  — proxy-frame transition go/no-go from feel.
- Letterbox slivers need black backing on non-16:9 windows (P11 report).
- Contested styling still dataless at macro grain (shader slot + drawer
  word ready; needs contested-flagged data).
- Atlas 2.5× vertical exaggeration vs V2 true-scale doctrine (locked
  decision allows a "clearly separate non-geometric relief
  presentation" — decide and document).
- Angle bundle unread by the interactive runtime (claims reach the
  drawer only via viewpoint claimIds today).

## Standing praise to preserve (do not regress)

- Field of vision / sense of scale in Soldier View (P10).
- Depth of the sound mix; the bodies (P10).
- Smoke behavior and read after the P8 fix round (P8: "audio and smoke
  is great").
- The approaching-the-firing-line comprehension effect (P9).
