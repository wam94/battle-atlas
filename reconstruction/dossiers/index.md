# Unit dossiers — index

Six-class evidence dossiers per `docs/reconstruction/audit/unit-truth-spec.md`.
One file per unit; headings are schema-fixed so a later pass can compile them
to claims. Times are Gettysburg local mean time (ED-31); every timed fact
carries its tier in square brackets:

- `[A vs CA-…]` — stated relative to a canonical anchor (±1–3 min)
- `[B: inputs]` — physics-derived, inputs shown (±2–5 min)
- `[C via <source> clockProfile ±n]` — witness clock corrected per ED-25
- `[D]` — sequence-only (before/after brackets)
- `[E]` — window-level presence only

Citations are `(source-id, locator)`; source ids resolve in
`reconstruction/sources/sources.json`. Facts with **no tier bracket are
untimed** (identity, strength, materiel). Negative evidence and conflicts are
first-class and live in their own section. New editorial calls are PROPOSED
as ED candidates (ED-40+ as of pass 2 — the pass-1 candidates ED-32…ED-38
and the spatial program's monument profile, renumbered ED-39, were adopted
2026-07-11), never self-adopted.

## Fixed heading schema

```
# Dossier: <unit name> (<register id>)
<status block: pass, cast status, achieved T-level, date>
## EC1 — Identity & command
## EC2 — Engaged strength
## EC3 — Position anchors
## EC4 — Movement legs
## EC5 — Activity record
## EC6 — Casualty apportionment
## Conflicts & negative evidence
## Chain anchors substantiated
## ED candidates proposed
## Source register (ids + access notes)
```

## Pass 1 — anchor-executors of the July 3 afternoon chain (ED-24)

Sequencing doctrine layer 1 (unit-truth-spec): the units that enacted the
adopted chain anchors. The Angle cast (Cushing, Webb's regiments, the CSA
brigades) is already T5 via the V2 claim corpus and is not re-dossier'd here.

| Dossier | Register id | Executes | Achieved T |
|---|---|---|---|
| [csa-1c-wsh-3-miller.md](csa-1c-wsh-3-miller.md) | reg-csa-1c-wsh-3 | CA-J3A-1 (fires the signal guns) | T3 |
| [csa-1c-bn-wash-eshleman.md](csa-1c-bn-wash-eshleman.md) | reg-csa-1c-bn-wash (+ reg-csa-1c-arty-cmd context) | CA-J3A-1 (order chain), bombardment line | T4 |
| [csa-1c-bn-alexander.md](csa-1c-bn-alexander.md) | reg-csa-1c-bn-alexander | CA-J3A-2/3 (the clock-bearer's battalion) | T4 |
| [us-arty-hq-hunt.md](us-arty-hq-hunt.md) | reg-us-arty-hq | CA-J3A-4/5 (conservation order) | T2 (command record) |
| [us-ar-1v-mcgilvery.md](us-ar-1v-mcgilvery.md) | reg-us-ar-1v | CA-J3A-4 (the slackening IS his line's silence), CA-J3A-10 | T4 |
| [us-xi-arty-osborn.md](us-xi-arty-osborn.md) | reg-us-xi-arty | CA-J3A-4/5 (the cease-fire ruse) | T4 |
| [us-ii-arty-hazard.md](us-ii-arty-hazard.md) | reg-us-ii-arty | CA-J3A-4 counterpoint (the line that did NOT slacken by order) | T4 |
| [us-btty-woodruff.md](us-btty-woodruff.md) | reg-us-ii-b4 | CA-J3A-4/5 counterpoint; Pettigrew-front repulse | T4 |
| [us-btty-arnold.md](us-btty-arnold.md) | reg-us-ii-b2 | CA-J3A-4/5 counterpoint; Fry/Marshall-front repulse | T4 |
| [us-btty-brown.md](us-btty-brown.md) | reg-us-ii-b3 | CA-J3A-4 (his mid-cannonade withdrawal is the II Corps line's one ordered slackening) | T4 |
| [us-btty-rorty.md](us-btty-rorty.md) | reg-us-ii-b1 | CA-J3A-4/5 counterpoint; Kemper-front repulse | T4 |
| [us-i-3-3-stannard.md](us-i-3-3-stannard.md) | reg-us-i-3-3 | CA-J3A-8 (the change of front) | T4 (T5 inside the Angle window) |

Companion documents: `docs/reconstruction/audit/dossier-pass-1.md` (pass
report), `docs/reconstruction/audit/dossier-overlay.json` (master-table
consultation layer, applied by `reconstruction/scripts/build_unit_audit.py`).
