# Gate P11 — owner verification checklist

**Gate (plan §12 Phase 11):** a new user can locate Pickett's Charge,
understand the approach, enter the hero viewpoint during its valid window,
seek and play it, inspect why the reconstruction made its choices, and
return — without developer assistance.

You are the tester, acting as that new user. The executor's
self-verification (EditMode + PlayMode suites green headlessly, HUD
screenshots) does **not** close this gate — this session does. Budget:
~20 minutes. Play the part: don't use developer knowledge you wouldn't
have from the screen.

## Setup (one command)

```bash
./scripts/p11-demo.sh
```

(Stages the P10 production media into
`app/Assets/StreamingAssets/SoldierView/` — media is gitignored, the
script copies it from `docs/benchmarks/captures/p10-gate/` or
`$P11_MEDIA_DIR` and verifies the release checksums — then validates
viewpoint metadata and the generated attribution outputs, and opens the
editor.) Then open `Assets/Scenes/Atlas.unity` and press Play.

The IMGUI strip is gone: the retained-mode HUD (UI Toolkit) should show a
masthead top-left (title · phase · conditions), chips top-right
(Contours / Reading light / Credits), and the transport + timeline at the
bottom.

## The loop

1. **Locate Pickett's Charge.** From the opening view, using only the
   HUD: the masthead names the day and phase; the timeline carries small
   tick markers and one shaded band. Hover the markers — each shows its
   wall-clock time, a one-line detail, and the claim/editorial citation
   that places it. Click "The charge steps off" (15:05): the clock jumps
   there, paused. **Pass if** you can tell, without asking, what the band
   on the timeline is (the hero viewpoint's valid window, 15:16–15:27).
2. **Understand the approach.** Press play at 10× or 60× and watch
   Pickett's division cross the valley (speeds are now 1×/10×/60×).
   Click Garnett's brigade ribbon: the source drawer opens on the left —
   name, "Confederate infantry brigade", live strength, current activity
   (it should say *moving in line*, and *firing* only when an authored
   event window is live), confidence, and the Sources list (the track
   segment's citation first, then the unit's engagement events, live
   ones flagged). **Pass if** the drawer's strength visibly falls as the
   charge advances and every line traces to a citation or says
   "no reliable record".
3. **Follow.** In the drawer press "Follow unit": the camera pivot rides
   the brigade. Orbit/zoom still work while following; panning (touch) or
   pressing the button again releases it. Close the drawer (✕) —
   selection clears.
4. **Entry only inside the window.** Before 15:16 there must be NO
   "Enter Soldier View" button anywhere. Scrub into the band (the clock
   between 15:16 and 15:27): the entry marker appears above the timeline
   — "Enter Soldier View — With Garnett's Line". Scrub past 15:27: gone.
5. **Content warning, exactly once.** Click the entry marker. The
   graphic-content warning modal must appear BEFORE any video (first
   launch only — if you've acknowledged an earlier build on this machine,
   delete the `battleatlas.soldierview.contentwarning.ackVersion`
   PlayerPrefs key or run once with a fresh user). Read it: warning text,
   then "Whose eyes are these?" — the representative-observer
   disclosure. Click **Stay in the Atlas** first: modal closes, nothing
   plays. Re-enter, click **I understand**: a short fade, then the
   1440p render, synchronized to the battle clock (speed pinned to 1×).
6. **Play/seek the viewpoint.** Play and pause a few times — clock and
   video move and stop together. Drag the Soldier View slider to at
   least five targets including both extremes. Seeks should settle
   imperceptibly (P10 measured median ~34 ms, worst ~107 ms — the frame
   holds during settle by design; a "settling…" badge appears only if a
   seek stalls beyond 150 ms). **Flag** any seek where the hold is
   visible enough to bother you — the proxy-frame transition plumbing
   exists and this session decides whether it's needed.
7. **Full vs proxy.** The picture should be the 1440p full encode (no
   "proxy resolution" badge). Optional: quit Play mode, rename the full
   .mp4 away, replay — entry falls back to the proxy WITH the badge and
   a console warning; restore the file after. Deleting both must refuse
   entry with a clear on-screen message.
8. **Inspect the reconstruction from inside.** While in Soldier View
   press "Sources": the drawer opens over the video with Garnett's
   identity/strength/activity at the current second, the track and event
   citations, plus the viewpoint's own entry — editorial note (the
   observer exemption ED-22) and its claim ids. **Pass if** you can
   answer "why does the reconstruction think this unit is here now?"
   from the screen alone.
9. **Return to the exact Atlas.** Note the battle clock (e.g. 15:22:41),
   exit ("Exit to Atlas"): after the fade the Atlas shows the SAME
   second, the speed you had before entering, and the camera exactly
   where you left it (it is locked while inside). Play resumes normally.
10. **Credits.** Top-right "Credits": the third-party attribution list
    (36+ entries, generated from the asset manifest) scrolls; spot-check
    one Sketchfab and one Freesound entry against
    `docs/assets/THIRD_PARTY_ASSETS.md`. Close it.
11. **Chips.** Contours and Reading light toggle as before (they are the
    same terrain/sun switches, ported off IMGUI). The reading-light chip
    still says "(presentation)".
12. **Determinism spot-check.** Scrub 13:00 → 16:00 twice; identical
    frames both passes (symbols, labels, drawer contents at the same t).

## Known conditions (not failures)

- The dev timecode viewpoint no longer surfaces an entry button (it is
  marked `development: true`); PlayMode tests still use it.
- Speed 300× retired with the IMGUI strip (plan §10 pins 1×/10×/60×).
- Marker hover details show in the transport row (left of the timeline),
  not as floating tooltips.
- Entering with media absent shows the on-screen refusal — that is the
  P1 contract's graceful degradation, not a bug.

## Verdict

Close the gate in the session notes with pass/fail per item; anything
flagged in 6 (seek feel) decides whether Phase 12 schedules the
proxy-frame seek transition. Media staging for a fresh machine is
`./scripts/p11-demo.sh` with `$P11_MEDIA_DIR` pointing at the release
download (GitHub Releases carry `p10-release-manifest.json` checksums).
