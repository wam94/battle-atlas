#!/usr/bin/env python3
"""Build the tabletop unit-activity master table (post-V2 punchlist item 0).

One row per unit in the macro battle file, with in-build movement and
casualty metrics computed from the keyframes, research-coverage signals
mined from docs/research and the V2 claims corpus, and empty
owner-consultation columns to be layered in.

Regenerable: computed columns are overwritten on rebuild; consultation
columns (owner-edited) are PRESERVED by re-reading the existing workbook
before writing, keyed by unit id.

Usage:
    cd reconstruction && uv run --with openpyxl python scripts/build_unit_audit.py
Output:
    docs/reconstruction/audit/unit-master-table.xlsx (+ .csv companion
    of computed columns only, for git diffs)
"""

import csv
import json
import math
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
BATTLE = ROOT / "app/Assets/Battle/gettysburg-july3.json"
CLAIMS = ROOT / "reconstruction/claims/angle.claims.json"
RESEARCH_DIRS = [ROOT / "docs/research"]
OUT_DIR = ROOT / "docs/reconstruction/audit"
XLSX = OUT_DIR / "unit-master-table.xlsx"
CSV_OUT = OUT_DIR / "unit-master-table.csv"

MOVE_THRESHOLD_M = 100.0  # path length below this ≈ dressing/jitter, not movement

CONSULT_COLS = [
    "Class (attested-static / unauthored / authored-ok)",
    "Historical actions summary (owner/consult)",
    "Historical casualties (killed/wounded/missing, source)",
    "Proposed changes (keyframes/events to author)",
    "Priority (1-3)",
    "Notes",
]


def arm_of(name: str) -> str:
    n = name.lower()
    if "battery" in n or "artillery" in n:
        return "artillery"
    if "cavalry" in n:
        return "cavalry"
    return "infantry"


def research_token(name: str) -> str:
    """A distinctive grep token: commander possessive stem or regiment number."""
    m = re.match(r"^(.*?)'s\b", name)
    if m:
        return m.group(1).split()[-1]  # "Garnett's Brigade" -> "Garnett"
    m = re.match(r"^(\d+(?:st|nd|rd|th)\s+\w+)", name)
    if m:
        return m.group(1)  # "13th Vermont Infantry" -> "13th Vermont"
    return name.split("(")[0].strip().split(",")[0]


def path_metrics(kfs):
    path = 0.0
    for a, b in zip(kfs, kfs[1:]):
        path += math.hypot(b["x"] - a["x"], b["z"] - a["z"])
    net = math.hypot(kfs[-1]["x"] - kfs[0]["x"], kfs[-1]["z"] - kfs[0]["z"])
    return round(path, 1), round(net, 1)


def main():
    battle = json.loads(BATTLE.read_text())
    units = battle["units"]
    events = battle["events"]
    claims = json.loads(CLAIMS.read_text())
    claim_subjects = {}
    for c in claims["claims"] if isinstance(claims, dict) else claims:
        claim_subjects.setdefault(c["subjectId"], 0)
        claim_subjects[c["subjectId"]] += 1

    research_texts = {}
    for d in RESEARCH_DIRS:
        for f in sorted(d.glob("*.md")):
            research_texts[f.name] = f.read_text(errors="ignore")

    parents = {u.get("parent") for u in units if u.get("parent")}

    rows = []
    for u in units:
        kfs = u["keyframes"]
        path, net = path_metrics(kfs)
        moves = path >= MOVE_THRESHOLD_M
        conf = [k.get("confidence", "") for k in kfs]
        documented = sum(1 for c in conf if c == "documented")
        inferred = sum(1 for c in conf if c == "inferred")
        unknown = sum(1 for c in conf if c == "unknown")
        cited = sum(1 for k in kfs if k.get("citation"))
        s0, s1 = kfs[0]["strength"], kfs[-1]["strength"]
        unit_events = [e for e in events if e.get("unitId") == u["id"]]
        token = research_token(u["name"])
        mention_docs = [
            fn for fn, txt in research_texts.items() if token and token in txt
        ]
        echelon = (
            "child-regiment"
            if u.get("parent")
            else ("parent-brigade" if u["id"] in parents else "standalone")
        )
        formations = "/".join(sorted({k["formation"] for k in kfs}))
        has_info = bool(mention_docs) or documented > 0 or u["id"] in claim_subjects
        rows.append(
            {
                "Unit ID": u["id"],
                "Name": u["name"],
                "Side": u["side"],
                "Echelon": echelon,
                "Arm": arm_of(u["name"]),
                "Start strength": s0,
                "End strength": s1,
                "In-build casualties (n)": s0 - s1,
                "Keyframes": len(kfs),
                "KF documented": documented,
                "KF inferred": inferred,
                "KF unknown": unknown,
                "KF cited": cited,
                "Path length (m)": path,
                "Net displacement (m)": net,
                "Moves in build?": "yes" if moves else "no",
                "Formations seen": formations,
                "Fire events": len(unit_events),
                "V2 claims": claim_subjects.get(u["id"], 0),
                "Research docs mentioning": len(mention_docs),
                "Research token": token,
                "Movement info available?": "yes" if has_info else "VERIFY",
            }
        )

    rows.sort(key=lambda r: (r["Side"], r["Arm"], r["Echelon"], r["Name"]))

    # preserve owner consultation columns from an existing workbook
    preserved = {}
    if XLSX.exists():
        from openpyxl import load_workbook

        wb_old = load_workbook(XLSX, data_only=True)
        ws_old = wb_old["Units"]
        headers = [c.value for c in ws_old[1]]
        for r in ws_old.iter_rows(min_row=2, values_only=True):
            rec = dict(zip(headers, r))
            uid = rec.get("Unit ID")
            if uid:
                preserved[uid] = {c: rec.get(c) for c in CONSULT_COLS if c in rec}

    OUT_DIR.mkdir(parents=True, exist_ok=True)

    from openpyxl import Workbook
    from openpyxl.styles import Font, PatternFill
    from openpyxl.utils import get_column_letter

    wb = Workbook()
    ws = wb.active
    ws.title = "Units"
    computed_cols = list(rows[0].keys())
    all_cols = (
        computed_cols[:7]
        + ["In-build casualty %"]
        + computed_cols[7:]
        + CONSULT_COLS
    )
    arial = Font(name="Arial", size=10)
    bold = Font(name="Arial", size=10, bold=True)
    yellow = PatternFill("solid", fgColor="FFF2CC")
    red_font = Font(name="Arial", size=10, color="CC0000", bold=True)

    ws.append(all_cols)
    for c in ws[1]:
        c.font = bold

    for i, r in enumerate(rows, start=2):
        vals = [r[c] for c in computed_cols[:7]]
        vals.append(f"=IF(F{i}=0,0,1-G{i}/F{i})")  # casualty % as a live formula
        vals += [r[c] for c in computed_cols[7:]]
        p = preserved.get(r["Unit ID"], {})
        vals += [p.get(c) for c in CONSULT_COLS]
        ws.append(vals)
        for c in ws[i]:
            c.font = arial
        ws.cell(row=i, column=8).number_format = "0.0%"
        if r["Movement info available?"] == "VERIFY":
            ws.cell(row=i, column=all_cols.index("Movement info available?") + 1).font = red_font
        for cc in CONSULT_COLS:
            ws.cell(row=i, column=all_cols.index(cc) + 1).fill = yellow

    widths = {1: 22, 2: 38, 3: 12, 4: 15, 5: 10, 16: 22, 20: 16}
    for idx, col in enumerate(all_cols, start=1):
        ws.column_dimensions[get_column_letter(idx)].width = widths.get(
            idx, 26 if col in CONSULT_COLS else 13
        )
    ws.freeze_panes = "C2"
    ws.auto_filter.ref = f"A1:{get_column_letter(len(all_cols))}{len(rows)+1}"

    legend = wb.create_sheet("Legend")
    legend_rows = [
        ["Tabletop unit-activity audit — master table (punchlist item 0)"],
        [""],
        ["Computed columns (white) are regenerated by "
         "reconstruction/scripts/build_unit_audit.py — do not hand-edit."],
        ["Yellow columns are the owner-consultation layer and SURVIVE "
         "regeneration (matched by Unit ID)."],
        ["'Moves in build?' = path length >= "
         f"{MOVE_THRESHOLD_M:.0f} m across all keyframes."],
        ["'Movement info available?' = any of: documented keyframes, V2 "
         "claims about the unit, or research-doc mentions of the token. "
         "VERIFY (red) = none found — per the owner, these units are "
         "likely researched and the gap is ours; verify before assuming "
         "the record is silent."],
        ["'In-build casualty %' is a live formula on start/end strength "
         "(the strengths themselves come from authored keyframes)."],
        ["Class values: attested-static (record supports stillness) / "
         "unauthored (recorded movement missing from the build) / "
         "authored-ok (current track already right)."],
        ["Example consultation row: see csa-garnett (filled as a format "
         "example from the V2 corpus — ED-2/ED-8, casualty profiles)."],
    ]
    for lr in legend_rows:
        legend.append(lr)
    for row in legend.iter_rows():
        for c in row:
            c.font = arial
    legend.column_dimensions["A"].width = 110

    # one example consultation row, from data already in the repo
    if "csa-garnett" not in preserved or not any(
        (preserved.get("csa-garnett") or {}).values()
    ):
        for i, r in enumerate(rows, start=2):
            if r["Unit ID"] == "csa-garnett":
                ex = [
                    "authored-ok",
                    "Advanced in Pickett's front line from Spangler's Woods; "
                    "halted under fire at the Emmitsburg Road fences "
                    "(~15:16-15:18, ED-12/claim-fences-high-climb); crossed "
                    "under canister; reached the wall; repulsed ~15:25+ "
                    "(V2 corpus: 7 segments, ED-2 timing frame).",
                    "In-build decay 1480->539 over the slice; V2 casualty "
                    "profiles reconcile exactly (e.g. "
                    "cas-garnett-angle-approach: 510, rising, "
                    "musketry/canister). Battle-total K/W/M per OOB research "
                    "doc - layer in.",
                    "None needed for the slice window (Angle cast).",
                    "3",
                    "Format example row - overwrite freely.",
                ]
                for j, cc in enumerate(CONSULT_COLS):
                    ws.cell(row=i, column=all_cols.index(cc) + 1, value=ex[j])
                break

    wb.save(XLSX)

    with open(CSV_OUT, "w", newline="") as f:
        w = csv.DictWriter(f, fieldnames=computed_cols)
        w.writeheader()
        w.writerows(rows)

    movers = sum(1 for r in rows if r["Moves in build?"] == "yes")
    verify = [r["Unit ID"] for r in rows if r["Movement info available?"] == "VERIFY"]
    print(f"units: {len(rows)} | move in build: {movers} | static: {len(rows)-movers}")
    print(f"VERIFY (no coverage signal found): {len(verify)}")
    for v in verify[:15]:
        print("  ", v)
    print(f"wrote {XLSX.relative_to(ROOT)} and {CSV_OUT.relative_to(ROOT)}")


if __name__ == "__main__":
    main()
