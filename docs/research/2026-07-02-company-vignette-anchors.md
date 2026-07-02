# Company Fragments — Vignette Anchors, Catalogued, NOT Units

**Status:** Complete. Plan Task A7 (`docs/superpowers/plans/2026-07-02-descriptive.md`).
Companion to the regiment-track survey (`2026-07-02-regiment-track-sources.md`), whose §4
finding governs this document.

## Why these are anchors and not tracked units

**The survey's §4 finding is a verified negative: no map or text source places companies
systematically during the July 3, 1:00–4:00 PM window.** Bachelder's maps bottom out at the
regiment; the OR's regimental reports name companies only anecdotally (Smith's 71st PA
report names none at all); monumentation in the July 3 sector runs to regimental flank
markers only; modern cartography (ABT, Hess, Laino, Stanley) keeps the regiment floor. The
one apparent exception — the Gettysburg Animated app's "detached companies" — is a modern
commercial interpolation, not a source. Promoting companies to tracked units would
therefore manufacture keyframes no source can back: exactly the fabrication the format's
provenance system exists to prevent.

What the record DOES hold is **nine attested company-scale fragments** — moments where a
named company (or gun section) did something specific at a specific place. These are the
raw material for the spec's **vignette phase**: staged, labeled scene dressing at soldier
zoom, anchored to a tracked unit's keyframe, never independently scrubbed geometry. Each
is catalogued below with its citation and a confidence grade (HIGH / MED / LOW, following
the survey's grading).

Companies otherwise render as frontage-derived sub-divisions of their regiment's track —
display convention, labeled as such by the format doc.

---

## The nine attested fragments

### 1. 69th PA Cos. I, A, F, D at the Angle — the refused flank
**Confidence: HIGH** (McDermott 1889, verbatim; the gold-standard fragment).
"The first three companies, I, A and F, were ordered to change front and face these
flankers"; I and A executed the refusal; "the commander of Company F, George Thompson,
being killed before he could give the command, they remained at the wall" — the gap
"through which the enemy poured, enveloping the latter company"; Co. D "held the enemy at
bay, using their muskets as clubs."
**Citation:** McDermott, *A Brief History of the 69th Regiment Pennsylvania Veteran
Volunteers* (1889), https://archive.org/details/briefhistoryof00mcde (full text verified).
Hartwig's 69th PA article: http://www.gdg.org/Gettysburg%20Magazine/scott1.html.
**Anchor point:** `us-69pa` t=8700 keyframe (the regiment never leaves the wall; the
refusal, the dead captain of F, and D's clubbed muskets are the vignette).

### 2. Cushing's section split — two guns to the wall, four wrecks at the crest
**Confidence: HIGH** (NPS; Fuger corroboration in the repo library).
After the cannonade Cushing, with Webb's permission, ran his **two serviceable guns** by
hand down to the stone wall, canister piled beside; the other four guns remained as wrecks
at the crest.
**Citation:** NPS, https://www.nps.gov/articles/000/alonzo-cushing-gettysburg.htm; Fuger
1907 (repo pass 2/3). Sub-fragment, **LOW** (forum-grade, do not stage without upgrade):
71st PA men running one gun to the outer-angle apex
(https://civilwartalk.com/threads/the-california-regiment-mans-cushings-guns-at-the-angle.189081/).
**Anchor point:** `us-btty-cushing` t=8580 keyframe (~2:45–3:00 displacement window).

### 3. Cowan's 5+1 gun split — five guns SOUTH of the copse, one NORTH
**Confidence: HIGH** (Brown 1985 Filson PDF, read page-by-page in the survey pass).
**Five guns south of the copse ~100 ft from the wall; one gun (Sgt. Mullaly's) carried
north of the trees beside the 69th PA / Cushing.** ⚠️ Orientation corrected by the survey
(§4 item 3, disagreement 6): the earlier task-brief memory of "a single gun on the south
side" is **the reverse** of what Brown 1985 documents — do not regress this.
**Citation:** Kent Masterson Brown, "Double Canister at Ten Yards: Captain Andrew Cowan at
Gettysburg," *Filson Club History Quarterly* 59:3 (1985), free PDF:
https://filsonhistorical.org/wp-content/uploads/publicationpdfs/59-3-2_-Double-Canister-at-Ten-Yards-Captain-Andrew-Cowan-at-Gettysburg_Brown-Kent-Masterson.pdf
**Anchor point:** `us-btty-cowan` t=6300 (gallop-in) through t=8700 (double canister).

### 4. Brown's Battery B as a 4-gun unit; Lt. Milne detached to Cushing's
**Confidence: HIGH** (Brown 1985 Filson; Stone Sentinels).
Brown's B fought July 3 with **four serviceable guns** under Lt. Perrin (two disabled
July 2; Capt. Brown wounded July 2). **Lt. Joseph Milne was detached from Brown's to
Cushing's battery and was killed there** — "Milne's section" belongs with Cushing's guns
on July 3, not Brown's.
**Citation:** Brown 1985 Filson (above);
https://gettysburg.stonesentinels.com/union-monuments/rhode-island/rhode-island-battery-b/
**Anchor point:** `us-btty-brown` t=0 (4-gun line south of the copse) and t=6000
(withdrawal); Milne's death rides `us-btty-cushing` t=8700.

### 5. Arnold's left-section gun at the wall — the last double-shotted round
**Confidence: MED-HIGH** (Stone Sentinels; timing genuinely debated).
One left-section gun stayed at the wall while the other four withdrew; its last round was
double-shotted canister — traditionally fired into the 26th North Carolina (see the
`csa-26nc` t=8700 documented keyframe). ⚠️ **Timing debated** (before vs. during the
charge — survey disagreement 4): stage the vignette against whichever keyframe the track
adopts and name the dispute in the label.
**Citation:**
https://gettysburg.stonesentinels.com/union-monuments/rhode-island/rhode-island-battery-a/;
timing thread: https://civilwartalk.com/threads/when-did-arnold%E2%80%99s-battery-withdraw-on-july-3.150029/
**Anchor point:** `us-btty-arnold` t=8700; cross-anchor `csa-26nc` t=8700.

### 6. 106th PA Cos. A & B on the skirmish line in front of Cowan
**Confidence: HIGH** (Filson fn. 75 citing Ward 1906 p. 199).
The other eight companies had gone to Cemetery Hill on July 2 — "Webb's brigade" in our
window = 69th + 71st + 72nd PA + **two companies of the 106th**. Kept an anchor, not a
track, per the company rule; `us-webb`'s parent strength keeps the pair counted (the
pinned advisory warning on Webb's decomposition is this, working as designed).
**Citation:** Brown 1985 Filson fn. 75, citing Ward, *History of the One Hundred and
Sixth Regiment* (1906), p. 199.
**Anchor point:** the ground in front of `us-btty-cowan` t≥6300, on `us-webb`'s family.

### 7. Vermont wheel mechanics — first company pivots, successive companies wheel in
**Confidence: HIGH for the maneuver; MED for per-company detail** (Sturtevant 1910 not yet
page-fetched — survey open item 3).
"Change front forward on first company" is company-resolution kinematics **by
definition**: the first company pivots in place, successive companies wheel in
sequentially. The vignette is the accordion of the wheel itself — staged along the
13th/16th VT's tracked wheel keyframes, not as company units.
**Citation:** Sturtevant, *Pictorial History, Thirteenth Regiment Vermont Volunteers*
(1910) pp. 304–305, https://archive.org/details/cu31924030916187; 13th VT monument via
Stone Sentinels; Benedict, *Vermont in the Civil War* vol. 2 (1888).
**Anchor point:** `us-13vt` t=8400→8700 and `us-16vt` t=8400→8700 (and the 16th's
reversal t=9900→10380).

### 8. 8th Ohio skirmish split — reserve + line, companies UNNAMED
**Confidence: HIGH for the split; the July 3 PM company identities are NOT attested.**
Sawyer's July 2 detail names companies (Cos. A & I on the line under Capt. Nickerson, D in
support, B in the dawn fight), but his July 3 charge passage **names no companies**. Model
the vignette as reserve + skirmish line with **no company ids**. ("Cos. D and B as
designated skirmishers" surfaced only as a SNIPPET — do not cite it.)
**Citation:** Sawyer, *A Military History of the 8th Regiment Ohio Vol. Inf'y* (1881),
https://archive.org/details/amilitaryhistor00groogoog (full text verified in the survey
pass); OR 27/1:461–62.
**Anchor point:** `us-8oh` t=0 (road cut) and t=7800 (change of front to the fence).

### 9. University Greys (Co. A, 11th Mississippi) at the Brian wall
**Confidence: MED-HIGH** (rooted in postwar Mississippi accounts; the field marker is
regimental). The lone Confederate company fragment: ~31 men, the regiment's colors, 100%
casualties at the farthest point of the advance on Hays's front.
**Citation:** https://en.wikipedia.org/wiki/University_Greys;
https://www.thelocalvoice.net/oxford/150-years-ago-today-the-11th-mississippi-at-the-battle-of-gettysburg/;
http://www.gdg.org/research/OOB/Confederate/July1-3/greys.html
**Anchor point:** `csa-11miss` t=8700 (the documented Brian-barn-wall keyframe authored
in Task A6).

---

## Structural attributes recorded alongside (NOT anchors)

These are properties of units, already carried by the data, not stageable moments:

- **59th NY and 39th NY were four-company battalions** — 59th consolidated June 1863,
  182 men (encoded: `us-59ny` short `frontage_m` 34); 39th consolidated to Cos. A–D
  May 31, 1863, 332 men (rides `us-sherrill`'s display roster;
  https://museum.dmna.ny.gov/unit-history/infantry/39th-infantry-regiment). Encode as
  short-frontage regiments, never as company sets.
- **14th CT Cos. A/B carried Sharps rifles** — armament attribute for a future
  weapons/vignette pass; their distinct Bliss-farm action is morning, pre-window
  (survey §4 structural note). The 14th CT rides `us-smyth`'s display roster.
- **Rorty's battery** has gun-level anecdotes but no distinct section positions —
  excluded (survey §4).

## Usage rule

A vignette anchor is presentation, and says so on its label — the same
labeled-not-smuggled doctrine as the reading light and the vertical exaggeration. It
inherits its position and clock from the tracked unit's keyframe it anchors to; its
company-level content comes only from the citation above; and where the source names a
dispute (Arnold's timing, Cushing's wounds), the label carries the dispute. Nothing in
this file may be promoted to a tracked unit without new sources clearing the survey's
§4 bar.
