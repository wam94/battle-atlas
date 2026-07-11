#!/usr/bin/env python3
# RESEARCH TOOLING: run from data/maps/bachelder/ after fetch_bachelder_maps.sh.
# Needs numpy+Pillow+opj_decompress (not part of the test suite deps).
"""Build reconstruction/spatial/bachelder-manifest.json from the working
acquisition + registration outputs in data/maps/. Run from
data/maps/bachelder/ inside the worktree."""
import json, math, os, re, subprocess, sys

sys.path.insert(0, os.path.abspath("../../../reconstruction/scripts"))
from georef_maps import (Similarity, ImgSimilarity, fit_similarity, compose)

GENERATED = "2026-07-11"

# ---- reference-sheet tie points (eyeballed at full res this pass) ----
REF = "12440022"
TIES = [
    {"name": "peach-orchard-crossroads", "img": [4022, 7642],
     "local": [3164.4, 3633.8], "role": "tie",
     "basis": "OSM node 4856642708 (Emmitsburg Rd x Wheatfield Rd), historic crossroads",
     "pinNote": "crossing of Emmitsburg Rd and the Wheatfield Rd at the Peach Orchard NW corner, Wentz house NE quadrant"},
    {"name": "taneytown-x-wheatfield", "img": [5762, 8424],
     "local": [4764.9, 2763.0], "role": "tie",
     "basis": "OSM node (Taneytown Rd x Wheatfield Rd)",
     "pinNote": "N-S Taneytown Rd meets the E-W cross road, L. Bricker farm SE"},
    {"name": "town-diamond", "img": [5678, 4138],
     "local": [4875.8, 6835.9], "role": "tie",
     "basis": "landmark-anchors doc #4 (Lincoln Square centroid; AREA, +/-25 m)",
     "pinNote": "Baltimore/Carlisle x Chambersburg/York crossing at the '412' benchmark"},
    {"name": "fairfield-x-blackhorsetavern", "img": [888, 5750],
     "local": [337.5, 5582.8], "role": "tie",
     "basis": "OSM node 105711642 (Fairfield Rd x Black Horse Tavern Rd)",
     "pinNote": "junction SE of the Marsh Creek bridge at Black Horse Tavern"},
    {"name": "mummasburg-x-herrsridge", "img": [3533, 1370],
     "local": [2979.1, 9513.1], "role": "tie",
     "basis": "OSM node 105767786 (Mummasburg Rd x Herrs Ridge Rd)",
     "pinNote": "crossing E of the S. Hertzel farm"},
    {"name": "codori-barn", "img": [4889, 6496],
     "local": [4020.4, 4635.4], "role": "check",
     "basis": "landmark-anchors doc #7 (OSM Codori Barn building)",
     "pinNote": "SW large building of the N. Codori farmstead; which 1873 square = the modern barn is ambiguous (+/-20 m)"},
    {"name": "bryan-farm", "img": [5321, 5969],
     "local": [4477.8, 5145.8], "role": "check",
     "basis": "landmark-anchors doc #10 (OSM Bryan House building)",
     "pinNote": "centroid of the two-building BRYAN cluster (+/-20 m)"},
]

RIGHTS_RUMSEY = {
    "basis": "underlying work: US government-commissioned survey map, 1873 base printed pre-1930 (public domain); Bachelder manuscript overlay delivered to the War Department 1886, first published 1995 (see 17 USC 303(a) discussion in docs/research/2026-07-02-bachelder-timed-set-acquisition.md section 3.2). Scan: David Rumsey Map Collection, formal license CC BY-NC-SA 3.0; Rumsey permissions page additionally grants publication use with credit.",
    "credit": "David Rumsey Map Collection, David Rumsey Map Center, Stanford Libraries",
    "repoPosture": "full-res rasters gitignored + fetch-reproducible; positions read off the sheets are uncopyrightable facts (Feist framing, README licensing section); low-res documentation crops only (sheet8-crops precedent)."
}

def main():
    cat = json.load(open("catalog.json"))
    sums = dict(line.split()[::-1] for line in open("sha256sums.txt")
                if line.strip())
    sums = {k: v for k, v in
            (l.strip().split(None, 1)[::-1] for l in open("sha256sums.txt"))}
    # normalize: file -> sha
    sums = {}
    for line in open("sha256sums.txt"):
        sha, fname = line.split()
        sums[fname] = sha
    reg = json.load(open("georef_working.json"))
    ecf = json.load(open("georef_ecf.json")) if os.path.exists("georef_ecf.json") else {}

    # reference fit
    ref_sim, res = fit_similarity([(t["img"], t["local"]) for t in TIES
                                   if t["role"] == "tie"])
    tie_names = [t["name"] for t in TIES if t["role"] == "tie"]
    per_point = dict(zip(tie_names, [round(r, 1) for r in res]))
    for t in TIES:
        if t["role"] == "check":
            d = math.dist(ref_sim.apply(t["img"]), t["local"])
            per_point[t["name"] + " (check)"] = round(d, 1)
    rms = math.sqrt(sum(r * r for r in res) / len(res))

    def dims(f):
        out = subprocess.run(["opj_dump", "-i", f], capture_output=True,
                             text=True).stdout
        m = re.search(r"x1=(\d+), y1=(\d+)", out)
        return int(m.group(1)), int(m.group(2))

    sheets = []
    for s in cat["sheets"]:
        fid = s["file"][:-4]
        w, h = dims(s["file"])
        entry = {
            "id": s["id"], "file": s["file"],
            "day": s["day"], "window": s["window"], "sheetNo": s["sheet_no"],
            "rumseyListNo": s["rumsey_list_no"], "iaIdentifier": s["ia"],
            "urls": {
                "jp2": f"https://archive.org/download/{s['ia']}/{s['file']}",
                "itemPage": f"https://archive.org/details/{s['ia']}",
            },
            "sha256": sums[s["file"]],
            "sizeBytes": os.path.getsize(s["file"]),
            "widthPx": w, "heightPx": h,
            "license": RIGHTS_RUMSEY,
        }
        src = reg.get(fid) or ecf.get(fid)
        if src is not None and fid.startswith("12440"):
            c, d_, ex, ey = (src["toRef"] if fid != REF else [1, 0, 0, 0])
            comp = compose(ImgSimilarity(c, d_, ex, ey), ref_sim)
            patch_rms_px = src["patchResidualPx"]
            unc = math.sqrt(rms ** 2 + (patch_rms_px * comp.scale) ** 2)
            entry["georef"] = {
                "a": comp.a, "b": comp.b, "tx": comp.tx, "ty": comp.ty,
                "metersPerPixel": round(comp.scale, 5),
                "rotationDeg": round(comp.rotation_deg, 3),
                "method": ("reference tie-point fit" if fid == REF else
                           "patch phase-correlation to reference sheet, composed with reference fit"),
                "patchResidualPx": patch_rms_px,
                "estAbsUncertaintyM": round(unc, 1),
            }
        elif src is not None:  # ECF sheets
            entry["georef"] = src  # already composed dict
        sheets.append(entry)

    doc = {
        "title": "Bachelder timed troop-position map set + Elliott burial map: acquisition + georeference manifest",
        "generated": GENERATED,
        "status": "PROPOSAL - audit-spatial-1 branch",
        "frame": "battlefield-local meters: local_x = EPSG:26918 easting - 304208.0216360274, local_z = northing - 4404534.271124143 (data/heightmap/heightmap.json origin)",
        "transformConvention": "local = [a -b; b a] * (img_x, -img_y) + (tx, ty)  -- tool/src/overlay.ts similarity; apply via reconstruction/scripts/georef_maps.py",
        "acquisition": "reconstruction/scripts/fetch_bachelder_maps.sh reproduces data/maps/ from the pinned URLs + sha256 below",
        "referenceSheet": REF,
        "referenceTiePoints": TIES,
        "referenceFit": {
            "a": ref_sim.a, "b": ref_sim.b, "tx": ref_sim.tx, "ty": ref_sim.ty,
            "metersPerPixel": round(ref_sim.scale, 5),
            "rotationDeg": round(ref_sim.rotation_deg, 3),
            "rmsResidualM": round(rms, 1),
            "perPointResidualM": per_point,
            "note": "rotation includes UTM 18N grid convergence at Gettysburg (about -1.4 deg) plus print/scan orientation; residuals mix pin error, 1863-vs-modern road-junction drift, and paper distortion. Affine experiments reduced road-junction residuals but were not adopted (overlay.ts similarity parity; see spatial-evidence doc).",
        },
        "uncertaintyGuidance": {
            "absolutePositionFloorM": round(2 * rms, 0),
            "relativeOnSheetM": "10-20 (patch registration residuals; use for unit-to-unit offsets read off one sheet)",
            "note": "EC3 anchors read off any main-field sheet should carry an uncertainty radius >= max(estAbsUncertaintyM, feature legibility); 2x rms ~ 60 m is the recommended default for absolute placements.",
        },
        "sheets": sheets,
    }

    # Elliott
    ell_sha = subprocess.run(["shasum", "-a", "256", "../elliott/cw0332000.jp2"],
                             capture_output=True, text=True).stdout.split()[0]
    ew, eh = dims("../elliott/cw0332000.jp2")
    ell_ties = [
        {"name": "town-diamond", "img": [4250, 3045], "local": [4875.8, 6835.9],
         "role": "tie", "basis": "landmark-anchors doc #4"},
        {"name": "peach-orchard-crossroads", "img": [2073, 7487],
         "local": [3164.4, 3633.8], "role": "tie", "basis": "OSM node 4856642708"},
        {"name": "codori-barn", "img": [3190, 6187], "local": [4020.4, 4635.4],
         "role": "check", "basis": "landmark-anchors doc #7"},
    ]
    esim = None
    from georef_maps import similarity_from_two_points
    esim = similarity_from_two_points(ell_ties[0]["img"], ell_ties[0]["local"],
                                      ell_ties[1]["img"], ell_ties[1]["local"])
    chk = math.dist(esim.apply(ell_ties[2]["img"]), ell_ties[2]["local"])
    doc["elliott"] = {
        "id": "elliott-1864", "file": "cw0332000.jp2",
        "title": "Elliott's map of the battlefield of Gettysburg (S.G. Elliott, surveyed ~spring 1864)",
        "urls": {
            "jp2": "https://tile.loc.gov/storage-services/service/gmd/gmd382/g3824/g3824g/cw0332000.jp2",
            "itemPage": "https://www.loc.gov/item/99447500/",
            "iiifInfo": "https://tile.loc.gov/image-services/iiif/service:gmd:gmd382:g3824:g3824g:cw0332000/info.json",
        },
        "sha256": ell_sha, "sizeBytes": os.path.getsize("../elliott/cw0332000.jp2"),
        "widthPx": ew, "heightPx": eh,
        "license": {"basis": "published 1864, US public domain; Library of Congress scan, no known restrictions",
                     "credit": "Library of Congress, Geography and Map Division (LC Civil War Maps 332)"},
        "tiePoints": ell_ties,
        "georef": {
            "a": esim.a, "b": esim.b, "tx": esim.tx, "ty": esim.ty,
            "metersPerPixel": round(esim.scale, 5),
            "rotationDeg": round(esim.rotation_deg, 3),
            "method": "two-point exact solve (overlay.ts workflow); Codori check residual below",
            "codoriCheckResidualM": round(chk, 1),
            "estAbsUncertaintyM": 100.0,
            "note": "Elliott's planimetry is loose away from the town-PO tie line (Codori check ~%s m); treat burial-feature positions as +/-100 m class evidence." % round(chk),
        },
    }

    out = "../../../reconstruction/spatial/bachelder-manifest.json"
    json.dump(doc, open(out, "w"), indent=1)
    print("wrote", out, "| refFit rms", round(rms, 1), "m | perPoint", per_point,
          "| elliott codori check", round(chk, 1), "m")

if __name__ == "__main__":
    main()
