# Dossier: Merritt's Reserve Brigade, 1st Cavalry Division (reg-us-cav-1-3)

Pass 12 (the cavalry theater opener) · Pass-14 EC3 patch (the SCF
sheet-crop exercise) · Cast status: **in-build** (`us-cav-merritt`) ·
Achieved T-level: **T4 (July 3 grain — the Fairfield-pooling EC6
finding, now sheet-crop confirmed)** · 2026-07-14

Chain role: the CA-SCF-2 co-executor; the pass's best EC6 COMPOSITION
finding — the brigade's tablet casualty total pools TWO separate
actions on two separate fields, a first-class conflict this dossier
resolves at the "carry both, adopt neither silently" grade.

## EC1 — Identity & command

- Reserve Brigade, 1st Cavalry Division, Cavalry Corps: 6th
  Pennsylvania Cav., 1st, 2nd, 5th, 6th US Cav. (or-27-oob). **Brig.
  Gen. Wesley Merritt** throughout.
- **Officer pin [D]**: "Major [Samuel H.] Starr, of the Sixth U.S.
  Cavalry, was detached with his regiment toward Fairfield or
  Middletown; engaged a superior force of the enemy, not without
  success. His regiment lost heavily in officers and men, and I
  regret to say that the major himself... was seriously wounded,
  losing an arm" (or-27-1-merritt, No. 340, p. 943, fetch-verified
  this pass, PRIMARY).

## EC2 — Engaged strength

- **B&M-repro sum (hop, addressing-gettysburg-oob), per-regiment**:
  6th PA (A-D, F-H, K-M) 242 + 1st US (A-E, G-M) 362 + 2nd US 407 +
  5th US 306 + 6th US 475 = **1,792**.

## EC3 — Position anchors

1. **South Cavalry Field, marching from noon** [C ~12:00, primary]:
   Merritt's own report: "On the 3rd instant, in compliance with
   orders received from corps headquarters[,] I marched with the
   brigade about 12 m. to attack the enemy's right and rear"
   (or-27-1-merritt, p. 943, fetch-verified this pass) — "12 m." is
   OR shorthand for "12 meridian" (noon), not a distance; read
   alongside his next clause, "I marched on the Gettysburg road about
   4 miles, where my advance and skirmishers were engaged" (the
   distance figure), the two together give the march's start time and
   length independently of any tablet.
2. **Fairfield, off-map**: the 6th US Cavalry's detached fight is
   explicitly OFF the main heightmap square — the oob-register's
   standing off-map ruling (Fairfield listed alongside East Cavalry
   Field, Westminster, Cashtown) already carries this; this dossier is
   the first pass-12 unit to substantiate it with a primary quote.
3. **PASS-14 EC3 SHEET-CROP PATCH (the pass-13 gap closed):**
   `12440022` (manifest id `j3-03`, sha256-verified against
   `bachelder-manifest.json`) read this pass with `crop_sheet.py` +
   `georef_maps.py`. Merritt's regulars are drawn in a block directly
   beside Farnsworth's Brigade's own block (full read and coordinates:
   `us-cav-3-1-farnsworth.md` EC3, this pass) — "6th PA" at local
   (2405, 1698) and "1 U.S." / "2 U.S." sub-blocks at (2483, 1496) /
   (2536, 1460), ~250-350 m from Farnsworth's brigade label — a
   terrain-relative (unit-to-unit) confirmation that the two brigades
   fought from immediately adjacent ground, consistent with both
   dossiers' report/tablet narrative of a joint advance. `12440023`
   (`j3-04`, the 5 P.M. sheet) draws no troop blocks in this sector —
   an honest negative, not a contradiction (the action had closed by
   5 P.M. per CA-SCF-2). Radius: ~31 m absolute (sheet
   `estAbsUncertaintyM`); 15-30 m terrain-relative for the
   block-to-block separation, per `spatial-evidence.md`. No standalone
   position anchor beyond the block itself was legible for Merritt's
   6th US Cavalry detachment (EC3.2, Fairfield) — off-map, so not
   expected on a main-field sheet.

## EC4 — Movement legs

1. [C ~12:00] March "about 4 miles" on the Gettysburg road, engaging
   the enemy at the advance/skirmish line (Merritt's report, above).
2. [C ~17:30, triple-agreement with the Farnsworth dossier's CA-SCF-2]:
   the sustained fight: "Here the brigade drove the enemy more than a
   mile, routing him from strong places, stone fences, and
   barricade[s]. This fight lasted about four hours (some time after
   the cannonading had ceased on the right), and was finally brought
   to a close by a heavy rain" (Merritt's report, verbatim,
   fetch-verified this pass) — an independent, REPORT-grade
   corroboration of the tablet-class ~17:30 charge/close timing (the
   cannonade-relative phrasing also ties this action's end to
   CA-J3A-5, cross-theater, tier D).
3. [D] The 6th US Cavalry's SEPARATE detachment toward Fairfield (EC3.2)
   — its own leg, not part of the South Cavalry Field advance.

## EC5 — Activity record

1. **The stone-fence advance** [C ~13:00-17:00]: "for more than a
   mile drove them from stone fences barricades and other positions
   being engaged four hours" (brigade tablet, gettysburg.stonesentinels.com,
   "Reserve Brigade, 1st Division, Cavalry Corps" — independently
   agreeing with Merritt's own report wording almost verbatim, a
   report=tablet near-exact agreement, the theater's cleanest such
   pair).
2. **Fairfield, the 6th US Cavalry's separate action** [D]: "engaged a
   superior force of the enemy, not without success" (Merritt's
   report) — the regiment fought creditably despite being isolated
   and overmatched; Starr's arm lost is the pin (EC1).

## EC6 — Casualty apportionment

- **Brigade tablet total: Killed 1 officer + 27 men, Wounded 12
  officers + 104 men, Captured or Missing 6 officers + 268 men, Total
  418** (gettysburg.stonesentinels.com).
- **THE FAIRFIELD-POOLING FINDING (first-class EC6 conflict, this
  pass)**: the 268-man captured/missing figure is disproportionate to
  a "drove the enemy... routing him" South Cavalry Field action and
  is explained by Merritt's own report — the 6th US Cavalry, detached
  to Fairfield, "lost heavily in officers and men" in an isolated
  fight against a superior force, a class of action (surrounded,
  overmatched, high-capture) that produces exactly this casualty
  SHAPE. **Canonical treatment for this dossier (not an ED ruling,
  informal per-unit application of the ED-49/ED-52 scope discipline):**
  the brigade tablet's 418 total is a POOLED figure across two
  separate fields (South Cavalry Field + Fairfield), not a South-
  Cavalry-Field-only total; render the 6th US Cavalry's casualties
  against its Fairfield action, and the other four regiments' against
  South Cavalry Field, rather than distributing the pooled total
  uniformly across the brigade's on-map presence. No per-regiment
  split is authored this pass (no regiment-grain casualty table was
  fetched) — the finding is the COMPOSITION fact, not a computed
  split.

## Conflicts & negative evidence

- None beyond the EC6 pooling finding (which is itself the dossier's
  headline, not an unresolved gap).

## Chain anchors substantiated

- **CA-SCF-1 (ED-72 ADOPTED, pass 13)**: ~noon march start — a
  PRIMARY (Merritt's own report), the strongest single clock in
  either half of the cavalry theater.
- **CA-SCF-2 (ED-72 ADOPTED, pass 13)**: the ~17:30 charge/close —
  Merritt's report's "about four hours... brought to a close by a
  heavy rain" independently corroborates the tablet-class triple
  agreement found in the Farnsworth dossier.

## ED candidates proposed

- Fed **ED-72**, ADOPTED at the start of pass 13; the Fairfield-
  pooling finding is cited in the ED-72 adoption text's rationale
  (not a separate ED — a per-unit application of standing doctrine).
  EC3's pass-14 patch closes the SCF-side sheet-crop gap the pass-13
  note left open.

## Source register

or-27-1-merritt (No. 340, OR 27/1 pp. 943-946, fetched this pass;
PRIMARY, report-nominal, no clockProfile assessed this pass — but
note EC4.2's cannonade-relative phrasing as a future clockProfile
anchor candidate) · gettysburg-stonesentinels-reservebde1div-cav
(marker/tablet page, fetched 2026-07-12) · bachelder-j3-03 (manifest id
`j3-03`, sheet 12440022, sha256-verified, read 2026-07-14) ·
bachelder-j3-04 (manifest id `j3-04`, sheet 12440023, sha256-verified,
read 2026-07-14 — negative check) · us-cav-3-1-farnsworth.md
(shared sheet-crop reads) · addressing-gettysburg-oob (B&M-repro hop) ·
or-27-oob.
