# Authoring Tool MVP Implementation Plan (Battle Atlas, Phase 3)

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** A browser-based authoring tool that traces unit paths over a georeferenced map and exports schema-validated battle JSON the Unity app loads unchanged — proven by round-tripping the placeholder battle.

**Architecture:** Three workstreams. (1) **Format**: a written spec + JSON Schema at `docs/format/`, making the battle format a first-class contract instead of folklore in code comments. (2) **Unity hardening**: the loader validates what the schema promises (provenance fields, vocabularies, endTime), and the HUD shows wall-clock time via `startTime`. (3) **The tool** (`tool/`): vanilla TypeScript + Vite, MapLibre GL for the map, proj4 for UTM↔WGS84, Ajv for validation. All non-DOM logic lives in pure modules (`geo`, `model`, `validate`, `io`, `overlay`) with Vitest coverage; DOM wiring stays thin.

**Tech Stack:** Node 22 / npm, Vite + TypeScript + Vitest, maplibre-gl, proj4, ajv · Unity changes tested via the Unity MCP (editor stays open; controller runs those tasks inline).

**Format decisions locked here:**
- `startTime` = seconds since local midnight of the battle day (placeholder: 0). HUD shows `startTime + t` as clock time when `startTime > 0`. This resolves the "nothing consumes startTime" review finding.
- Keyframes gain optional `confidence` (`"documented" | "inferred" | "unknown"`, default `"unknown"`) and `citation` (string). Export rule (the no-faking gate): `confidence == "documented"` requires a non-empty citation.
- Vocabularies: `side ∈ {union, confederate}`; `formation ∈ {column, line, skirmish, scattered, routed}`.
- Frontage/depth stay unit-level in this phase (the spec's per-keyframe form is a documented future extension, noted in the format doc).

**Execution notes:** Unity-side tasks (2–3) are executed by the controller inline via the Unity MCP (editor open, `run_tests` for verification). Web tasks go to subagents; all their verification is plain CLI (`npm test`). Work on branch `authoring-tool`.

---

### Task 1: Branch + format spec + JSON Schema

**Files:**
- Create: `docs/format/battle-format.md`
- Create: `docs/format/battle.schema.json`

- [ ] **Step 1:** `git checkout -b authoring-tool`

- [ ] **Step 2:** Create `docs/format/battle-format.md`:

```markdown
# Battle Track Format

The contract between the authoring tool (writer), the pipeline (future writer),
and the Unity app (reader). JSON, UTF-8. Machine-checkable schema:
[battle.schema.json](battle.schema.json). The Unity loader re-validates the
load-bearing rules at runtime (BattleData.cs).

## Conventions

- **Positions** are battlefield-local meters: `x` east, `z` north from the
  terrain's SW corner. Identical to Unity world XZ. The local frame is anchored
  to UTM zone 18N (EPSG:26918) by `origin_utm_e` / `origin_utm_n` in
  `data/heightmap/heightmap.json`; tools convert via
  `utm = origin + local`, then UTM→WGS84 for display on web maps.
- **Time**: `t` is seconds from battle start. `startTime` is seconds since
  local midnight of the battle day (e.g. 13:00 = 46800); display clock time is
  `startTime + t`. `endTime` is the battle duration in seconds (same axis as
  `t`), and must be ≥ every keyframe `t`.
- **Facing** is compass degrees: 0 = north (+Z in Unity), 90 = east.
- **Interpolation**: linear position/strength between keyframes, shortest-arc
  for facing; a segment carries its START keyframe's formation; state clamps
  before the first and after the last keyframe.

## Structure

| Field | Type | Rules |
|---|---|---|
| `name` | string | required |
| `startTime` | number | ≥ 0, seconds since local midnight |
| `endTime` | number | > 0, battle duration |
| `units[]` | array | ≥ 1 entry |
| `units[].id` | string | required, unique |
| `units[].name` | string | required |
| `units[].side` | string | `union` \| `confederate` |
| `units[].frontage_m` | number | > 0 |
| `units[].depth_m` | number | > 0 |
| `units[].keyframes[]` | array | ≥ 1, `t` strictly increasing |
| `keyframes[].t` | number | ≥ 0 |
| `keyframes[].x`, `.z` | number | battlefield-local meters |
| `keyframes[].facing` | number | compass degrees |
| `keyframes[].formation` | string | `column` \| `line` \| `skirmish` \| `scattered` \| `routed` |
| `keyframes[].strength` | number | ≥ 0, effectives at this moment |
| `keyframes[].confidence` | string | optional; `documented` \| `inferred` \| `unknown` (default `unknown`) |
| `keyframes[].citation` | string | optional; REQUIRED non-empty when `confidence == "documented"` |

## The no-faking gate

The authoring tool refuses to export a keyframe claiming `documented`
confidence without a citation. Provenance renders in-app (documented = solid,
inferred = ghosted, unknown = explicit "no reliable record").

## Planned extensions (not yet valid)

- Per-keyframe `frontage_m`/`depth_m` (formation morphing)
- `engagement` flags (drives smoke/audio), path splines, comms/moments files
```

- [ ] **Step 3:** Create `docs/format/battle.schema.json`:

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "$id": "battle.schema.json",
  "title": "Battle Atlas track data",
  "type": "object",
  "required": ["name", "startTime", "endTime", "units"],
  "additionalProperties": false,
  "properties": {
    "name": { "type": "string", "minLength": 1 },
    "startTime": { "type": "number", "minimum": 0 },
    "endTime": { "type": "number", "exclusiveMinimum": 0 },
    "units": {
      "type": "array",
      "minItems": 1,
      "items": {
        "type": "object",
        "required": ["id", "name", "side", "frontage_m", "depth_m", "keyframes"],
        "additionalProperties": false,
        "properties": {
          "id": { "type": "string", "minLength": 1 },
          "name": { "type": "string", "minLength": 1 },
          "side": { "enum": ["union", "confederate"] },
          "frontage_m": { "type": "number", "exclusiveMinimum": 0 },
          "depth_m": { "type": "number", "exclusiveMinimum": 0 },
          "keyframes": {
            "type": "array",
            "minItems": 1,
            "items": {
              "type": "object",
              "required": ["t", "x", "z", "facing", "formation", "strength"],
              "additionalProperties": false,
              "properties": {
                "t": { "type": "number", "minimum": 0 },
                "x": { "type": "number" },
                "z": { "type": "number" },
                "facing": { "type": "number" },
                "formation": { "enum": ["column", "line", "skirmish", "scattered", "routed"] },
                "strength": { "type": "number", "minimum": 0 },
                "confidence": { "enum": ["documented", "inferred", "unknown"] },
                "citation": { "type": "string" }
              },
              "if": { "properties": { "confidence": { "const": "documented" } }, "required": ["confidence"] },
              "then": { "required": ["citation"], "properties": { "citation": { "minLength": 1 } } }
            }
          }
        }
      }
    }
  }
}
```

(Strictly-increasing `t` and unique unit ids are beyond draft-07 expressiveness — they're enforced in code on both sides; the format doc says so.)

- [ ] **Step 4:** Commit:

```bash
git add docs/format
git commit -m "docs: battle format spec + JSON schema (provenance, vocabularies, startTime semantics)"
```

---

### Task 2: Unity loader hardening (controller-inline, via Unity MCP)

**Files:**
- Modify: `app/Assets/Scripts/BattleData.cs`
- Modify: `app/Assets/Tests/Editor/BattleLoaderTests.cs`

- [ ] **Step 1:** Add to `KeyframeDto` (after `strength`):

```csharp
        public string confidence; // "documented" | "inferred" | "unknown"; empty = unknown
        public string citation;
```

- [ ] **Step 2:** Extend `BattleLoader.Parse` validation. After the existing keyframe-time loop inside the unit loop, add:

```csharp
                if (unit.frontage_m <= 0f || unit.depth_m <= 0f)
                    throw new ArgumentException(
                        $"unit '{unit.id}' frontage/depth must be positive");
                KeyframeDto lastKf = unit.keyframes[unit.keyframes.Count - 1];
                if (lastKf.t > battle.endTime)
                    throw new ArgumentException(
                        $"unit '{unit.id}' keyframe t {lastKf.t} exceeds battle endTime {battle.endTime}");
```

- [ ] **Step 3:** Add tests to `BattleLoaderTests` (append inside the class):

```csharp
    [Test]
    public void Parse_RejectsKeyframeBeyondEndTime()
    {
        string bad = ValidJson.Replace(@"""endTime"": 100", @"""endTime"": 50");
        var ex = Assert.Throws<System.ArgumentException>(() => BattleLoader.Parse(bad));
        StringAssert.Contains("endTime", ex.Message);
    }

    [Test]
    public void Parse_RejectsNonPositiveFrontage()
    {
        string bad = ValidJson.Replace(@"""frontage_m"": 200", @"""frontage_m"": 0");
        var ex = Assert.Throws<System.ArgumentException>(() => BattleLoader.Parse(bad));
        StringAssert.Contains("u1", ex.Message);
    }

    [Test]
    public void Parse_ReadsProvenanceFields()
    {
        string json = ValidJson.Replace(
            @"{""t"": 0, ""x"": 100,",
            @"{""t"": 0, ""confidence"": ""documented"", ""citation"": ""test source"", ""x"": 100,");
        BattleDto b = BattleLoader.Parse(json);
        Assert.AreEqual("documented", b.units[0].keyframes[0].confidence);
        Assert.AreEqual("test source", b.units[0].keyframes[0].citation);
    }
```

- [ ] **Step 4:** Refresh + compile via MCP, run EditMode tests: expect **36 passed** (33 + 3).

- [ ] **Step 5:** Commit: `git add app/Assets/Scripts/BattleData.cs app/Assets/Tests/Editor/BattleLoaderTests.cs && git commit -m "feat: loader validates format contract; provenance fields parsed"`

---

### Task 3: HUD wall-clock time (controller-inline, via Unity MCP)

**Files:**
- Modify: `app/Assets/Scripts/BattleClock.cs` (ClockMath + BattleClock)
- Modify: `app/Assets/Scripts/TimelineHud.cs`
- Modify: `app/Assets/Scripts/BattleDirector.cs`
- Modify: `app/Assets/Tests/Editor/ClockMathTests.cs`

- [ ] **Step 1:** Add to `ClockMath`:

```csharp
        // startTime is seconds since local midnight of the battle day; shows
        // the wall clock the participants lived ("14:32:10"), not elapsed time
        public static string FormatClockTime(float startTime, float t)
        {
            int s = Mathf.FloorToInt(startTime + t);
            return $"{s / 3600 % 24:D2}:{s / 60 % 60:D2}:{s % 60:D2}";
        }
```

Add a `public float StartTime;` field to `BattleClock` (after `CurrentTime`).

- [ ] **Step 2:** In `BattleDirector.Start`, after `clock.EndTime = battle.endTime;` add `clock.StartTime = battle.startTime;`

- [ ] **Step 3:** In `TimelineHud.OnGUI`, replace the time label line with:

```csharp
            string timeLabel = clock.StartTime > 0f
                ? ClockMath.FormatClockTime(clock.StartTime, clock.CurrentTime)
                : ClockMath.FormatTime(clock.CurrentTime);
            GUI.Label(new Rect(140, top + 14, 100, 24), timeLabel);
```

- [ ] **Step 4:** Add test to `ClockMathTests`:

```csharp
    [Test]
    public void FormatClockTime_AnchorsToLocalMidnight()
    {
        Assert.AreEqual("13:00:30", ClockMath.FormatClockTime(46800f, 30f));
        Assert.AreEqual("00:00:05", ClockMath.FormatClockTime(0f, 5f));
    }
```

- [ ] **Step 5:** MCP refresh + tests: expect **37 passed**. Commit: `git add app/Assets/Scripts app/Assets/Tests && git commit -m "feat: HUD shows wall-clock battle time via startTime"`

---

### Task 4: Tool scaffolding

**Files:**
- Create: `tool/package.json`, `tool/tsconfig.json`, `tool/vite.config.ts`, `tool/index.html`, `tool/src/main.ts`, `tool/tests/smoke.test.ts`
- Modify: `.gitignore`

- [ ] **Step 1:** Create `tool/package.json`:

```json
{
  "name": "battle-atlas-authoring",
  "private": true,
  "version": "0.1.0",
  "type": "module",
  "scripts": {
    "dev": "vite",
    "build": "vite build",
    "test": "vitest run",
    "typecheck": "tsc --noEmit"
  },
  "dependencies": {
    "ajv": "^8.17.0",
    "maplibre-gl": "^5.0.0",
    "proj4": "^2.15.0"
  },
  "devDependencies": {
    "@types/proj4": "^2.5.5",
    "typescript": "^5.6.0",
    "vite": "^6.0.0",
    "vitest": "^2.1.0"
  }
}
```

(If `npm install` reports an unsatisfiable version, relax that one caret range to the nearest available major — record the change in the commit message.)

`tool/tsconfig.json`:

```json
{
  "compilerOptions": {
    "target": "ES2022",
    "module": "ESNext",
    "moduleResolution": "bundler",
    "strict": true,
    "noUncheckedIndexedAccess": true,
    "resolveJsonModule": true,
    "types": ["vite/client"],
    "skipLibCheck": true
  },
  "include": ["src", "tests"]
}
```

`tool/vite.config.ts`:

```typescript
import { defineConfig } from "vite";

export default defineConfig({
  server: { port: 5180 },
});
```

`tool/index.html`:

```html
<!doctype html>
<html lang="en">
  <head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Battle Atlas — Authoring</title>
  </head>
  <body>
    <div id="app"></div>
    <script type="module" src="/src/main.ts"></script>
  </body>
</html>
```

`tool/src/main.ts`:

```typescript
const app = document.querySelector<HTMLDivElement>("#app")!;
app.textContent = "Battle Atlas authoring tool";
```

`tool/tests/smoke.test.ts`:

```typescript
import { describe, expect, it } from "vitest";

describe("toolchain", () => {
  it("runs typescript tests", () => {
    expect(1 + 1).toBe(2);
  });
});
```

- [ ] **Step 2:** Append to root `.gitignore`:

```
tool/node_modules/
tool/dist/
```

- [ ] **Step 3:** `cd tool && npm install && npm test && npm run typecheck` — expect 1 passing test, clean typecheck.

- [ ] **Step 4:** Commit `tool/` (including `package-lock.json`) + `.gitignore`: `git add tool .gitignore && git commit -m "feat: authoring tool scaffolding (vite + ts + vitest)"`

---

### Task 5: Geo module with cross-language golden fixtures

**Files:**
- Create: `tool/src/geo.ts`
- Create: `tool/tests/fixtures/geo-golden.json` (generated, committed)
- Test: `tool/tests/geo.test.ts`

- [ ] **Step 1: Generate golden fixtures with the PYTHON pipeline's pyproj** (cross-language truth, per the no-faking rule):

```bash
cd pipeline && uv run python - <<'EOF' > ../tool/tests/fixtures/geo-golden.json
import json
from pyproj import Transformer
from terrain_pipeline import config

t = Transformer.from_crs(config.UTM_CRS, config.WGS84_CRS, always_xy=True)
bounds = config.utm_square_bounds()
origin_e, origin_n = bounds[0], bounds[1]
cases = []
for name, local_x, local_z in [
    ("sw-corner", 0.0, 0.0),
    ("center", 4253.6, 4253.6),
    ("little-round-top-ish", 3800.0, 2300.0),
]:
    lon, lat = t.transform(origin_e + local_x, origin_n + local_z)
    cases.append({"name": name, "local": [local_x, local_z], "lonLat": [lon, lat]})
print(json.dumps({
    "origin_utm_e": origin_e, "origin_utm_n": origin_n, "cases": cases
}, indent=2))
EOF
mkdir -p tool/tests/fixtures  # (run before the heredoc if missing)
```

- [ ] **Step 2: Write the failing test** — `tool/tests/geo.test.ts`:

```typescript
import { describe, expect, it } from "vitest";
import golden from "./fixtures/geo-golden.json";
import { Battlefield } from "../src/geo";

const bf = new Battlefield(golden.origin_utm_e, golden.origin_utm_n);

describe("Battlefield geo conversions", () => {
  for (const c of golden.cases) {
    it(`localToLonLat matches pyproj for ${c.name}`, () => {
      const [lon, lat] = bf.localToLonLat(c.local[0], c.local[1]);
      expect(lon).toBeCloseTo(c.lonLat[0], 6); // ~0.1 m at this latitude
      expect(lat).toBeCloseTo(c.lonLat[1], 6);
    });

    it(`lonLatToLocal round-trips for ${c.name}`, () => {
      const [x, z] = bf.lonLatToLocal(c.lonLat[0], c.lonLat[1]);
      expect(x).toBeCloseTo(c.local[0], 2); // centimeters
      expect(z).toBeCloseTo(c.local[1], 2);
    });
  }
});
```

- [ ] **Step 3:** Run `npm test` — expect failure (`../src/geo` missing).

- [ ] **Step 4: Implement** — `tool/src/geo.ts`:

```typescript
import proj4 from "proj4";

// NAD83 / UTM zone 18N — the pipeline's CRS (see pipeline/terrain_pipeline/config.py)
proj4.defs(
  "EPSG:26918",
  "+proj=utm +zone=18 +ellps=GRS80 +towgs84=0,0,0,0,0,0,0 +units=m +no_defs +type=crs",
);

// Battlefield-local meters (x east, z north from the terrain SW corner) anchored
// to UTM by the heightmap's origin. The same convention as the Unity app —
// see docs/format/battle-format.md.
export class Battlefield {
  constructor(
    readonly originUtmE: number,
    readonly originUtmN: number,
  ) {}

  localToLonLat(x: number, z: number): [number, number] {
    const [lon, lat] = proj4("EPSG:26918", "WGS84", [
      this.originUtmE + x,
      this.originUtmN + z,
    ]);
    return [lon, lat];
  }

  lonLatToLocal(lon: number, lat: number): [number, number] {
    const [e, n] = proj4("WGS84", "EPSG:26918", [lon, lat]);
    return [e - this.originUtmE, n - this.originUtmN];
  }
}
```

- [ ] **Step 5:** `npm test` — all green. Commit: `git add tool/src/geo.ts tool/tests && git commit -m "feat: tool geo conversions, golden-tested against pyproj"`

---

### Task 6: Model module (interpolation port)

**Files:**
- Create: `tool/src/model.ts`
- Test: `tool/tests/model.test.ts`

- [ ] **Step 1: Write the failing tests** — `tool/tests/model.test.ts` (mirrors the C# UnitTrack suite so the two implementations can't drift silently):

```typescript
import { describe, expect, it } from "vitest";
import { stateAt, type Unit } from "../src/model";

function makeUnit(kfs: Array<[number, number, number, number, number]>): Unit {
  return {
    id: "u",
    name: "Test",
    side: "union",
    frontage_m: 100,
    depth_m: 20,
    keyframes: kfs.map(([t, x, z, facing, strength]) => ({
      t, x, z, facing, formation: "line", strength,
    })),
  };
}

describe("stateAt (must match Unity UnitTrack semantics)", () => {
  it("interpolates position and strength linearly", () => {
    const u = makeUnit([[0, 0, 0, 0, 1000], [10, 100, 200, 0, 800]]);
    const s = stateAt(u, 5);
    expect(s.x).toBeCloseTo(50);
    expect(s.z).toBeCloseTo(100);
    expect(s.strength).toBeCloseTo(900);
  });

  it("clamps before first and after last", () => {
    const u = makeUnit([[10, 5, 6, 90, 100], [20, 50, 60, 90, 90]]);
    expect(stateAt(u, 0).x).toBeCloseTo(5);
    expect(stateAt(u, 99).x).toBeCloseTo(50);
  });

  it("interpolates facing across the north wrap", () => {
    const u = makeUnit([[0, 0, 0, 350, 1], [10, 0, 0, 10, 1]]);
    const f = stateAt(u, 5).facing;
    const delta = ((f - 0 + 540) % 360) - 180;
    expect(delta).toBeCloseTo(0);
  });

  it("picks the correct segment among many", () => {
    const u = makeUnit([[0, 0, 0, 0, 1], [10, 10, 0, 0, 1], [20, 30, 0, 0, 1], [40, 70, 0, 0, 1]]);
    expect(stateAt(u, 15).x).toBeCloseTo(20);
    expect(stateAt(u, 30).x).toBeCloseTo(50);
  });

  it("takes formation from the segment start, including exact interior keyframes", () => {
    const u = makeUnit([[0, 0, 0, 0, 1], [10, 10, 0, 0, 1], [20, 30, 0, 0, 1]]);
    u.keyframes[0]!.formation = "column";
    u.keyframes[1]!.formation = "line";
    u.keyframes[2]!.formation = "scattered";
    expect(stateAt(u, 5).formation).toBe("column");
    expect(stateAt(u, 10).formation).toBe("line");
  });
});
```

- [ ] **Step 2:** `npm test` — fails (module missing).

- [ ] **Step 3: Implement** — `tool/src/model.ts`:

```typescript
// Mirrors docs/format/battle-format.md and the Unity app's BattleData/UnitTrack.
// The interpolation here MUST match UnitTrack.StateAt — the authoring preview
// must show authors exactly what the app will show viewers.

export type Side = "union" | "confederate";
export type Formation = "column" | "line" | "skirmish" | "scattered" | "routed";
export type Confidence = "documented" | "inferred" | "unknown";

export interface Keyframe {
  t: number;
  x: number;
  z: number;
  facing: number;
  formation: Formation;
  strength: number;
  confidence?: Confidence;
  citation?: string;
}

export interface Unit {
  id: string;
  name: string;
  side: Side;
  frontage_m: number;
  depth_m: number;
  keyframes: Keyframe[];
}

export interface Battle {
  name: string;
  startTime: number;
  endTime: number;
  units: Unit[];
}

export interface UnitState {
  x: number;
  z: number;
  facing: number;
  strength: number;
  formation: Formation;
}

const lerp = (a: number, b: number, u: number) => a + (b - a) * u;

function lerpAngle(a: number, b: number, u: number): number {
  const delta = ((b - a + 540) % 360) - 180; // shortest arc, like Mathf.LerpAngle
  return a + delta * u;
}

export function stateAt(unit: Unit, t: number): UnitState {
  const kfs = unit.keyframes;
  const first = kfs[0]!;
  const last = kfs[kfs.length - 1]!;
  if (t <= first.t) return fromKeyframe(first);
  if (t >= last.t) return fromKeyframe(last);

  // first index with kf.t > t (binary upper bound, same as UnitTrack)
  let lo = 0;
  let hi = kfs.length;
  while (lo < hi) {
    const mid = (lo + hi) >> 1;
    if (kfs[mid]!.t <= t) lo = mid + 1;
    else hi = mid;
  }
  const a = kfs[lo - 1]!;
  const b = kfs[lo]!;
  const u = (t - a.t) / (b.t - a.t);
  return {
    x: lerp(a.x, b.x, u),
    z: lerp(a.z, b.z, u),
    facing: lerpAngle(a.facing, b.facing, u),
    strength: lerp(a.strength, b.strength, u),
    formation: a.formation,
  };
}

function fromKeyframe(k: Keyframe): UnitState {
  return { x: k.x, z: k.z, facing: k.facing, strength: k.strength, formation: k.formation };
}
```

- [ ] **Step 4:** `npm test` green. Commit: `git add tool/src/model.ts tool/tests/model.test.ts && git commit -m "feat: tool battle model with Unity-matched interpolation"`

---

### Task 7: Validation module

**Files:**
- Create: `tool/src/validate.ts`
- Create: `tool/tests/fixtures/placeholder_battle.json` (copied from `app/Assets/Battle/placeholder_battle.json`)
- Test: `tool/tests/validate.test.ts`

- [ ] **Step 1:** `cp app/Assets/Battle/placeholder_battle.json tool/tests/fixtures/placeholder_battle.json`

- [ ] **Step 2: Write the failing tests** — `tool/tests/validate.test.ts`:

```typescript
import { describe, expect, it } from "vitest";
import placeholder from "./fixtures/placeholder_battle.json";
import { validateBattle } from "../src/validate";

describe("validateBattle", () => {
  it("accepts the placeholder battle", () => {
    const result = validateBattle(placeholder);
    expect(result.errors).toEqual([]);
    expect(result.ok).toBe(true);
  });

  it("rejects non-increasing keyframe times", () => {
    const bad = structuredClone(placeholder) as any;
    bad.units[0].keyframes[1].t = bad.units[0].keyframes[0].t;
    const result = validateBattle(bad);
    expect(result.ok).toBe(false);
    expect(result.errors.join(" ")).toContain("strictly increase");
  });

  it("rejects duplicate unit ids", () => {
    const bad = structuredClone(placeholder) as any;
    bad.units[1].id = bad.units[0].id;
    expect(validateBattle(bad).ok).toBe(false);
  });

  it("enforces the no-faking gate: documented requires citation", () => {
    const bad = structuredClone(placeholder) as any;
    bad.units[0].keyframes[0].confidence = "documented";
    const result = validateBattle(bad);
    expect(result.ok).toBe(false);
    expect(result.errors.join(" ")).toMatch(/citation/i);
  });

  it("rejects keyframes beyond endTime", () => {
    const bad = structuredClone(placeholder) as any;
    bad.endTime = 10;
    expect(validateBattle(bad).ok).toBe(false);
  });

  it("rejects unknown side via schema", () => {
    const bad = structuredClone(placeholder) as any;
    bad.units[0].side = "martian";
    expect(validateBattle(bad).ok).toBe(false);
  });
});
```

- [ ] **Step 3:** `npm test` — fails.

- [ ] **Step 4: Implement** — `tool/src/validate.ts`:

```typescript
import Ajv from "ajv";
import schema from "../../docs/format/battle.schema.json";
import type { Battle } from "./model";

export interface ValidationResult {
  ok: boolean;
  errors: string[];
}

const ajv = new Ajv({ allErrors: true, allowUnionTypes: true });
const schemaValidate = ajv.compile(schema);

// Schema first, then the rules draft-07 can't express (see battle-format.md):
// strictly increasing t, unique unit ids, endTime covers all keyframes,
// and the no-faking gate (documented => citation).
export function validateBattle(data: unknown): ValidationResult {
  const errors: string[] = [];
  if (!schemaValidate(data)) {
    for (const e of schemaValidate.errors ?? [])
      errors.push(`${e.instancePath || "/"} ${e.message ?? "invalid"}`);
    return { ok: false, errors };
  }
  const battle = data as Battle;
  const seenIds = new Set<string>();
  for (const unit of battle.units) {
    if (seenIds.has(unit.id)) errors.push(`duplicate unit id '${unit.id}'`);
    seenIds.add(unit.id);
    for (let i = 1; i < unit.keyframes.length; i++) {
      if (unit.keyframes[i]!.t <= unit.keyframes[i - 1]!.t)
        errors.push(`unit '${unit.id}' keyframe times must strictly increase (index ${i})`);
    }
    const lastT = unit.keyframes[unit.keyframes.length - 1]!.t;
    if (lastT > battle.endTime)
      errors.push(`unit '${unit.id}' keyframe t ${lastT} exceeds endTime ${battle.endTime}`);
    for (const [i, kf] of unit.keyframes.entries()) {
      if (kf.confidence === "documented" && !kf.citation?.trim())
        errors.push(`unit '${unit.id}' keyframe ${i}: documented confidence requires a citation`);
    }
  }
  return { ok: errors.length === 0, errors };
}
```

Note: the schema's `if/then` also enforces the citation rule structurally; the code-level check produces the human-readable message the UI shows. Both fire — that's intentional redundancy on the no-faking gate.

- [ ] **Step 5:** `npm test` green (the schema import needs `resolveJsonModule`, already enabled). Commit: `git add tool/src/validate.ts tool/tests && git commit -m "feat: schema + rule validation with the documented-requires-citation gate"`

---

### Task 8: IO module (import/export round-trip)

**Files:**
- Create: `tool/src/io.ts`
- Test: `tool/tests/io.test.ts`

- [ ] **Step 1: Write the failing tests** — `tool/tests/io.test.ts`:

```typescript
import { describe, expect, it } from "vitest";
import placeholder from "./fixtures/placeholder_battle.json";
import { exportBattle, importBattle } from "../src/io";

describe("battle IO", () => {
  it("round-trips the placeholder battle byte-stably", () => {
    const battle = importBattle(JSON.stringify(placeholder));
    const out = exportBattle(battle);
    expect(JSON.parse(out)).toEqual(placeholder);
  });

  it("import rejects invalid battles with messages", () => {
    expect(() => importBattle('{"name":"x","startTime":0,"endTime":1,"units":[]}'))
      .toThrow(/minItems|fewer/i);
  });

  it("export refuses an invalid battle", () => {
    const battle = importBattle(JSON.stringify(placeholder));
    battle.units[0]!.keyframes[0]!.confidence = "documented";
    expect(() => exportBattle(battle)).toThrow(/citation/i);
  });

  it("export emits keys in canonical order for clean diffs", () => {
    const battle = importBattle(JSON.stringify(placeholder));
    const out = exportBattle(battle);
    const firstKf = out.indexOf('"keyframes"');
    expect(out.indexOf('"id"')).toBeLessThan(firstKf);
  });
});
```

- [ ] **Step 2:** `npm test` — fails.

- [ ] **Step 3: Implement** — `tool/src/io.ts`:

```typescript
import type { Battle } from "./model";
import { validateBattle } from "./validate";

export function importBattle(json: string): Battle {
  const data = JSON.parse(json);
  const result = validateBattle(data);
  if (!result.ok) throw new Error(`invalid battle: ${result.errors.join("; ")}`);
  return data as Battle;
}

// Canonical key order keeps exports diffable in git and matches the committed
// placeholder asset's shape.
export function exportBattle(battle: Battle): string {
  const result = validateBattle(battle);
  if (!result.ok) throw new Error(`refusing to export: ${result.errors.join("; ")}`);
  const ordered = {
    name: battle.name,
    startTime: battle.startTime,
    endTime: battle.endTime,
    units: battle.units.map((u) => ({
      id: u.id,
      name: u.name,
      side: u.side,
      frontage_m: u.frontage_m,
      depth_m: u.depth_m,
      keyframes: u.keyframes.map((k) => ({
        t: k.t,
        x: k.x,
        z: k.z,
        facing: k.facing,
        formation: k.formation,
        strength: k.strength,
        ...(k.confidence !== undefined && { confidence: k.confidence }),
        ...(k.citation !== undefined && { citation: k.citation }),
      })),
    })),
  };
  return JSON.stringify(ordered, null, 2);
}
```

- [ ] **Step 4:** `npm test` green. Commit: `git add tool/src/io.ts tool/tests/io.test.ts && git commit -m "feat: validated battle import/export with canonical key order"`

---

### Task 9: Overlay georeference math

**Files:**
- Create: `tool/src/overlay.ts`
- Test: `tool/tests/overlay.test.ts`

- [ ] **Step 1: Write the failing tests** — `tool/tests/overlay.test.ts`:

```typescript
import { describe, expect, it } from "vitest";
import { similarityFromTwoPoints, applySimilarity } from "../src/overlay";

describe("two-point similarity transform (scanned map georeferencing)", () => {
  // image pixel coords (y down) -> battlefield-local meters (z north/up)
  it("recovers scale, rotation, and translation from two tie points", () => {
    // synthetic: image is 2x scale, rotated 90deg CCW, shifted by (100, 50)
    // p_local = R(90) * 2 * p_img + (100, 50);  R(90)*(x,y) = (-y, x)
    const tie1 = { img: [10, 20] as const, local: [100 - 2 * 20, 50 + 2 * 10] as const };
    const tie2 = { img: [50, 80] as const, local: [100 - 2 * 80, 50 + 2 * 50] as const };
    const T = similarityFromTwoPoints(tie1, tie2);
    const p = applySimilarity(T, [30, 40]);
    expect(p[0]).toBeCloseTo(100 - 2 * 40, 6);
    expect(p[1]).toBeCloseTo(50 + 2 * 30, 6);
  });

  it("identity when ties are identity", () => {
    const T = similarityFromTwoPoints(
      { img: [0, 0], local: [0, 0] },
      { img: [100, 0], local: [100, 0] },
    );
    const p = applySimilarity(T, [25, 75]);
    expect(p[0]).toBeCloseTo(25, 6);
    expect(p[1]).toBeCloseTo(-75, 6); // image y-down flips to local z-up
  });

  it("throws when tie points coincide", () => {
    expect(() =>
      similarityFromTwoPoints({ img: [1, 1], local: [0, 0] }, { img: [1, 1], local: [5, 5] }),
    ).toThrow();
  });
});
```

- [ ] **Step 2:** `npm test` — fails.

- [ ] **Step 3: Implement** — `tool/src/overlay.ts`:

```typescript
// Georeference a scanned historical map with two tie points: solve the
// similarity transform (uniform scale + rotation + translation) from image
// pixels (y down) to battlefield-local meters (z north, y up). Two points is
// the fastest workflow that still handles rotated scans; affine (3 pts) is a
// future upgrade for skewed scans.

export interface Similarity {
  a: number; // = s*cos(theta)
  b: number; // = s*sin(theta)
  tx: number;
  ty: number;
}

export interface TiePoint {
  img: readonly [number, number];
  local: readonly [number, number];
}

export function similarityFromTwoPoints(p: TiePoint, q: TiePoint): Similarity {
  // Work in a y-up image frame so rotation comes out conventional:
  // local = [a -b; b a] * (imgX, -imgY) + (tx, ty)
  const x1 = p.img[0], y1 = -p.img[1];
  const x2 = q.img[0], y2 = -q.img[1];
  const u1 = p.local[0], v1 = p.local[1];
  const u2 = q.local[0], v2 = q.local[1];
  const dx = x2 - x1, dy = y2 - y1;
  const det = dx * dx + dy * dy;
  if (det < 1e-12) throw new Error("tie points coincide in image space");
  const du = u2 - u1, dv = v2 - v1;
  const a = (dx * du + dy * dv) / det;
  const b = (dx * dv - dy * du) / det;
  return { a, b, tx: u1 - (a * x1 - b * y1), ty: v1 - (b * x1 + a * y1) };
}

export function applySimilarity(T: Similarity, img: readonly [number, number]): [number, number] {
  const x = img[0], y = -img[1];
  return [T.a * x - T.b * y + T.tx, T.b * x + T.a * y + T.ty];
}
```

- [ ] **Step 4:** `npm test` green. Commit: `git add tool/src/overlay.ts tool/tests/overlay.test.ts && git commit -m "feat: two-point similarity georeferencing for scanned maps"`

---

### Task 10: Map shell UI

**Files:**
- Create: `tool/src/ui/map.ts`, `tool/src/style.css`
- Modify: `tool/src/main.ts`, `tool/index.html`

- [ ] **Step 1:** `tool/src/ui/map.ts`:

```typescript
import maplibregl from "maplibre-gl";
import "maplibre-gl/dist/maplibre-gl.css";
import type { Battlefield } from "../geo";

// OSM raster style — fine for an internal tool; respect the tile usage policy
// (low volume, single user).
const OSM_STYLE: maplibregl.StyleSpecification = {
  version: 8,
  sources: {
    osm: {
      type: "raster",
      tiles: ["https://tile.openstreetmap.org/{z}/{x}/{y}.png"],
      tileSize: 256,
      attribution: "© OpenStreetMap contributors",
    },
  },
  layers: [{ id: "osm", type: "raster", source: "osm" }],
};

export function createMap(container: HTMLElement, bf: Battlefield, sizeM: number): maplibregl.Map {
  const [centerLon, centerLat] = bf.localToLonLat(sizeM / 2, sizeM / 2);
  const map = new maplibregl.Map({
    container,
    style: OSM_STYLE,
    center: [centerLon, centerLat],
    zoom: 12.2,
  });

  map.on("load", () => {
    const corners: [number, number][] = [
      bf.localToLonLat(0, 0),
      bf.localToLonLat(sizeM, 0),
      bf.localToLonLat(sizeM, sizeM),
      bf.localToLonLat(0, sizeM),
      bf.localToLonLat(0, 0),
    ];
    map.addSource("battlefield-bounds", {
      type: "geojson",
      data: { type: "Feature", properties: {}, geometry: { type: "LineString", coordinates: corners } },
    });
    map.addLayer({
      id: "battlefield-bounds",
      type: "line",
      source: "battlefield-bounds",
      paint: { "line-color": "#c9a227", "line-width": 2, "line-dasharray": [3, 2] },
    });
  });

  return map;
}
```

- [ ] **Step 2:** `tool/src/style.css`:

```css
:root { font-family: -apple-system, system-ui, sans-serif; }
body { margin: 0; display: flex; height: 100vh; }
#map { flex: 1; }
#sidebar {
  width: 340px; overflow-y: auto; border-left: 1px solid #ccc;
  padding: 12px; box-sizing: border-box; font-size: 13px;
}
#sidebar h2 { font-size: 14px; margin: 12px 0 6px; }
#sidebar label { display: block; margin: 4px 0 2px; color: #555; }
#sidebar input, #sidebar select { width: 100%; box-sizing: border-box; }
.row { display: flex; gap: 6px; }
.row > * { flex: 1; }
button { cursor: pointer; }
.kf { border: 1px solid #ddd; border-radius: 4px; padding: 6px; margin: 6px 0; }
.kf.selected { border-color: #c9a227; background: #fdf8e7; }
.error { color: #b00020; white-space: pre-wrap; }
.muted { color: #888; }
```

- [ ] **Step 3:** Replace `tool/index.html` body and `tool/src/main.ts`:

`index.html` body:

```html
  <body>
    <div id="map"></div>
    <div id="sidebar">
      <h1 style="font-size:16px">Battle Atlas — Authoring</h1>
      <label>heightmap.json (georeference anchor)
        <input type="file" id="heightmap-file" accept=".json" />
      </label>
      <div id="workspace" class="muted">Load data/heightmap/heightmap.json to begin.</div>
    </div>
    <script type="module" src="/src/main.ts"></script>
  </body>
```

`tool/src/main.ts`:

```typescript
import "./style.css";
import { Battlefield } from "./geo";
import { createMap } from "./ui/map";
import { initWorkspace } from "./ui/workspace";

const fileInput = document.querySelector<HTMLInputElement>("#heightmap-file")!;
const workspaceEl = document.querySelector<HTMLDivElement>("#workspace")!;

fileInput.addEventListener("change", async () => {
  const file = fileInput.files?.[0];
  if (!file) return;
  const meta = JSON.parse(await file.text());
  if (typeof meta.origin_utm_e !== "number" || typeof meta.width_m !== "number") {
    workspaceEl.textContent = "That doesn't look like heightmap.json (missing origin_utm_e/width_m).";
    return;
  }
  const bf = new Battlefield(meta.origin_utm_e, meta.origin_utm_n);
  const map = createMap(document.querySelector("#map")!, bf, meta.width_m);
  initWorkspace(workspaceEl, map, bf);
});
```

(`initWorkspace` arrives in Task 11; for THIS task create a stub `tool/src/ui/workspace.ts` exporting an `initWorkspace` that renders "workspace ready" — Task 11 replaces it.)

```typescript
import type maplibregl from "maplibre-gl";
import type { Battlefield } from "../geo";

export function initWorkspace(el: HTMLElement, _map: maplibregl.Map, _bf: Battlefield): void {
  el.textContent = "workspace ready";
}
```

- [ ] **Step 4:** `npm test && npm run typecheck` green; `npm run dev` and verify manually in a browser: map loads, gold dashed battlefield square sits around Gettysburg PA (controller may verify via Playwright/Chrome MCP screenshot if available, else defer visual check to the user).

- [ ] **Step 5:** Commit: `git add tool && git commit -m "feat: map shell with OSM base and battlefield bounds"`

---

### Task 11: Authoring workspace UI (roster, keyframes, paths, preview)

**Files:**
- Replace: `tool/src/ui/workspace.ts`
- Create: `tool/src/ui/pathlayer.ts`

This is the largest UI task: a sidebar listing units (add/delete, edit identity fields), a keyframe list for the selected unit (click map to append a keyframe at the current draft time; edit t/facing/formation/strength/confidence/citation; delete), a map layer drawing each unit's path polyline + keyframe dots (selected unit highlighted), a time slider previewing interpolated unit positions as oriented rectangles (via `stateAt` from model.ts), and Import/Export buttons (file download / file input using io.ts, errors rendered in the sidebar).

`tool/src/ui/pathlayer.ts` (map rendering, pure-ish: builds GeoJSON from battle + Battlefield):

```typescript
import type maplibregl from "maplibre-gl";
import type { Battle } from "../model";
import { stateAt } from "../model";
import type { Battlefield } from "../geo";

export function battleToGeoJSON(battle: Battle, bf: Battlefield, selectedUnitId: string | null) {
  const paths: GeoJSON.Feature[] = [];
  const dots: GeoJSON.Feature[] = [];
  for (const unit of battle.units) {
    const coords = unit.keyframes.map((k) => bf.localToLonLat(k.x, k.z));
    paths.push({
      type: "Feature",
      properties: { side: unit.side, selected: unit.id === selectedUnitId },
      geometry: { type: "LineString", coordinates: coords },
    });
    unit.keyframes.forEach((k, i) => {
      dots.push({
        type: "Feature",
        properties: { side: unit.side, unitId: unit.id, index: i, selected: unit.id === selectedUnitId },
        geometry: { type: "Point", coordinates: bf.localToLonLat(k.x, k.z) },
      });
    });
  }
  return {
    paths: { type: "FeatureCollection" as const, features: paths },
    dots: { type: "FeatureCollection" as const, features: dots },
  };
}

export function previewToGeoJSON(battle: Battle, bf: Battlefield, t: number) {
  const features: GeoJSON.Feature[] = battle.units.map((unit) => {
    const s = stateAt(unit, t);
    // oriented footprint rectangle (local meters -> lon/lat ring)
    const rad = ((90 - s.facing) * Math.PI) / 180; // compass -> math angle of unit FORWARD
    const fwd = [Math.cos(rad), Math.sin(rad)];
    const right = [Math.sin(rad), -Math.cos(rad)];
    const hw = unit.frontage_m / 2;
    const hd = unit.depth_m / 2;
    const cornersLocal: [number, number][] = [
      [s.x - right[0]! * hw - fwd[0]! * hd, s.z - right[1]! * hw - fwd[1]! * hd],
      [s.x + right[0]! * hw - fwd[0]! * hd, s.z + right[1]! * hw - fwd[1]! * hd],
      [s.x + right[0]! * hw + fwd[0]! * hd, s.z + right[1]! * hw + fwd[1]! * hd],
      [s.x - right[0]! * hw + fwd[0]! * hd, s.z - right[1]! * hw + fwd[1]! * hd],
    ];
    const ring = [...cornersLocal, cornersLocal[0]!].map(([x, z]) => bf.localToLonLat(x, z));
    return {
      type: "Feature",
      properties: { side: unit.side },
      geometry: { type: "Polygon", coordinates: [ring] },
    };
  });
  return { type: "FeatureCollection" as const, features };
}

export function installBattleLayers(map: maplibregl.Map): void {
  map.addSource("unit-paths", { type: "geojson", data: emptyFC() });
  map.addSource("unit-dots", { type: "geojson", data: emptyFC() });
  map.addSource("unit-preview", { type: "geojson", data: emptyFC() });
  map.addLayer({
    id: "unit-preview", type: "fill", source: "unit-preview",
    paint: {
      "fill-color": ["match", ["get", "side"], "union", "#3b5a9c", "confederate", "#a05050", "#888888"],
      "fill-opacity": 0.65,
    },
  });
  map.addLayer({
    id: "unit-paths", type: "line", source: "unit-paths",
    paint: {
      "line-color": ["match", ["get", "side"], "union", "#3b5a9c", "confederate", "#a05050", "#888888"],
      "line-width": ["case", ["get", "selected"], 3, 1.5],
      "line-opacity": ["case", ["get", "selected"], 0.95, 0.5],
    },
  });
  map.addLayer({
    id: "unit-dots", type: "circle", source: "unit-dots",
    paint: {
      "circle-radius": ["case", ["get", "selected"], 5, 3],
      "circle-color": ["match", ["get", "side"], "union", "#3b5a9c", "confederate", "#a05050", "#888888"],
      "circle-stroke-width": 1,
      "circle-stroke-color": "#ffffff",
    },
  });
}

const emptyFC = () => ({ type: "FeatureCollection" as const, features: [] });
```

`tool/src/ui/workspace.ts` — full replacement (sidebar state machine):

```typescript
import type maplibregl from "maplibre-gl";
import type { Battlefield } from "../geo";
import type { Battle, Confidence, Formation, Side, Unit } from "../model";
import { exportBattle, importBattle } from "../io";
import { validateBattle } from "../validate";
import { battleToGeoJSON, installBattleLayers, previewToGeoJSON } from "./pathlayer";

const FORMATIONS: Formation[] = ["column", "line", "skirmish", "scattered", "routed"];
const CONFIDENCES: Confidence[] = ["unknown", "inferred", "documented"];

export function initWorkspace(el: HTMLElement, map: maplibregl.Map, bf: Battlefield): void {
  let battle: Battle = { name: "untitled battle", startTime: 0, endTime: 3600, units: [] };
  let selectedUnitId: string | null = null;
  let draftTime = 0;

  const installLayers = () => installBattleLayers(map);
  if (map.isStyleLoaded()) installLayers();
  else map.on("load", installLayers);

  map.on("click", (e) => {
    const unit = battle.units.find((u) => u.id === selectedUnitId);
    if (!unit) return;
    const [x, z] = bf.lonLatToLocal(e.lngLat.lng, e.lngLat.lat);
    const lastT = unit.keyframes.length ? unit.keyframes[unit.keyframes.length - 1]!.t : -1;
    unit.keyframes.push({
      t: Math.max(draftTime, lastT + 1),
      x: Math.round(x), z: Math.round(z),
      facing: 90, formation: "line",
      strength: unit.keyframes.length ? unit.keyframes[unit.keyframes.length - 1]!.strength : 1000,
    });
    render();
  });

  function syncMap(): void {
    if (!map.getSource("unit-paths")) return;
    const gj = battleToGeoJSON(battle, bf, selectedUnitId);
    (map.getSource("unit-paths") as maplibregl.GeoJSONSource).setData(gj.paths);
    (map.getSource("unit-dots") as maplibregl.GeoJSONSource).setData(gj.dots);
    (map.getSource("unit-preview") as maplibregl.GeoJSONSource).setData(
      previewToGeoJSON(battle, bf, draftTime),
    );
  }

  function render(): void {
    syncMap();
    el.replaceChildren();
    const frag = document.createDocumentFragment();

    // battle header
    frag.append(h2("Battle"));
    frag.append(labeled("name", textInput(battle.name, (v) => { battle.name = v; })));
    frag.append(row(
      labeled("startTime (s since midnight)", numInput(battle.startTime, (v) => { battle.startTime = v; })),
      labeled("endTime (s)", numInput(battle.endTime, (v) => { battle.endTime = v; render(); })),
    ));

    // time scrubber for preview
    frag.append(h2(`Preview time: ${fmt(draftTime)}`));
    const slider = document.createElement("input");
    slider.type = "range"; slider.min = "0"; slider.max = String(battle.endTime); slider.value = String(draftTime);
    slider.addEventListener("input", () => { draftTime = Number(slider.value); syncMap(); el.querySelector("h2.time")!.textContent = `Preview time: ${fmt(draftTime)}`; });
    const timeH2 = h2(`Preview time: ${fmt(draftTime)}`); timeH2.className = "time";
    frag.replaceChild(timeH2, frag.querySelectorAll("h2")[1]!);
    frag.append(slider);

    // roster
    frag.append(h2("Units"));
    for (const unit of battle.units) {
      const btn = document.createElement("button");
      btn.textContent = `${unit.id === selectedUnitId ? "▶ " : ""}${unit.name} (${unit.side}, ${unit.keyframes.length} kf)`;
      btn.style.display = "block"; btn.style.width = "100%"; btn.style.margin = "2px 0";
      btn.addEventListener("click", () => { selectedUnitId = unit.id === selectedUnitId ? null : unit.id; render(); });
      frag.append(btn);
    }
    const addBtn = document.createElement("button");
    addBtn.textContent = "+ add unit";
    addBtn.addEventListener("click", () => {
      const id = `unit-${battle.units.length + 1}`;
      battle.units.push({ id, name: id, side: "union", frontage_m: 200, depth_m: 30, keyframes: [] });
      selectedUnitId = id; render();
    });
    frag.append(addBtn);

    // selected unit editor
    const unit = battle.units.find((u) => u.id === selectedUnitId);
    if (unit) frag.append(unitEditor(unit));

    // io
    frag.append(h2("File"));
    const exportBtn = document.createElement("button");
    exportBtn.textContent = "Export battle JSON";
    const errBox = document.createElement("div"); errBox.className = "error";
    exportBtn.addEventListener("click", () => {
      try {
        const out = exportBattle(battle);
        errBox.textContent = "";
        const a = document.createElement("a");
        a.href = URL.createObjectURL(new Blob([out], { type: "application/json" }));
        a.download = "battle.json";
        a.click();
      } catch (err) { errBox.textContent = String(err); }
    });
    const importInput = document.createElement("input");
    importInput.type = "file"; importInput.accept = ".json";
    importInput.addEventListener("change", async () => {
      const f = importInput.files?.[0];
      if (!f) return;
      try {
        battle = importBattle(await f.text());
        selectedUnitId = null; draftTime = 0;
        errBox.textContent = ""; render();
      } catch (err) { errBox.textContent = String(err); }
    });
    const liveErrors = validateBattle(battle);
    const status = document.createElement("div");
    status.className = liveErrors.ok ? "muted" : "error";
    status.textContent = liveErrors.ok ? "battle is valid" : liveErrors.errors.join("\n");
    frag.append(row(exportBtn), labeled("import battle JSON", importInput), status, errBox);

    el.append(frag);
  }

  function unitEditor(unit: Unit): DocumentFragment {
    const frag = document.createDocumentFragment();
    frag.append(h2(`Unit: ${unit.id}`));
    frag.append(labeled("name", textInput(unit.name, (v) => { unit.name = v; })));
    const sideSel = select(["union", "confederate"], unit.side, (v) => { unit.side = v as Side; render(); });
    frag.append(labeled("side", sideSel));
    frag.append(row(
      labeled("frontage_m", numInput(unit.frontage_m, (v) => { unit.frontage_m = v; })),
      labeled("depth_m", numInput(unit.depth_m, (v) => { unit.depth_m = v; })),
    ));
    const del = document.createElement("button");
    del.textContent = "delete unit";
    del.addEventListener("click", () => {
      battle.units = battle.units.filter((u) => u.id !== unit.id);
      selectedUnitId = null; render();
    });
    frag.append(del);

    frag.append(h2("Keyframes (click map to append at preview time)"));
    unit.keyframes.forEach((kf, i) => {
      const box = document.createElement("div");
      box.className = "kf";
      box.append(row(
        labeled("t", numInput(kf.t, (v) => { kf.t = v; render(); })),
        labeled("facing", numInput(kf.facing, (v) => { kf.facing = v; syncMap(); })),
        labeled("strength", numInput(kf.strength, (v) => { kf.strength = v; })),
      ));
      box.append(row(
        labeled("formation", select(FORMATIONS, kf.formation, (v) => { kf.formation = v as Formation; })),
        labeled("confidence", select(CONFIDENCES, kf.confidence ?? "unknown", (v) => { kf.confidence = v as Confidence; render(); })),
      ));
      box.append(labeled("citation", textInput(kf.citation ?? "", (v) => { kf.citation = v || undefined; })));
      box.append(new Text(`local (${kf.x}, ${kf.z})`));
      const del = document.createElement("button");
      del.textContent = "delete keyframe"; del.style.marginLeft = "8px";
      del.addEventListener("click", () => { unit.keyframes.splice(i, 1); render(); });
      box.append(del);
      frag.append(box);
    });
    return frag;
  }

  // small DOM helpers
  function h2(text: string): HTMLHeadingElement { const e = document.createElement("h2"); e.textContent = text; return e; }
  function labeled(text: string, input: HTMLElement): HTMLLabelElement {
    const l = document.createElement("label"); l.textContent = text; l.append(input); return l;
  }
  function textInput(value: string, onChange: (v: string) => void): HTMLInputElement {
    const i = document.createElement("input"); i.type = "text"; i.value = value;
    i.addEventListener("change", () => onChange(i.value)); return i;
  }
  function numInput(value: number, onChange: (v: number) => void): HTMLInputElement {
    const i = document.createElement("input"); i.type = "number"; i.value = String(value);
    i.addEventListener("change", () => onChange(Number(i.value))); return i;
  }
  function select(options: readonly string[], value: string, onChange: (v: string) => void): HTMLSelectElement {
    const s = document.createElement("select");
    for (const o of options) { const opt = document.createElement("option"); opt.value = o; opt.textContent = o; s.append(opt); }
    s.value = value; s.addEventListener("change", () => onChange(s.value)); return s;
  }
  function row(...children: HTMLElement[]): HTMLDivElement {
    const d = document.createElement("div"); d.className = "row"; d.append(...children); return d;
  }
  function fmt(t: number): string {
    const s = Math.floor(battle.startTime + t);
    const pad = (n: number) => String(n).padStart(2, "0");
    return `${pad(Math.floor(s / 3600) % 24)}:${pad(Math.floor(s / 60) % 60)}:${pad(s % 60)}`;
  }

  render();
}
```

**Implementation note for the worker:** the slider re-render dance above (replacing the time `h2`) is the planned shape but if it fights the DOM, simplify: give the time heading a stable id and update `textContent` directly. Keep `render()` full-rebuild — this is an internal tool; 60fps DOM diffing is YAGNI. If `frag.querySelectorAll` on a DocumentFragment proves unreliable, restructure to build the header sections sequentially instead — behavior over structure.

- [ ] **Verify:** `npm test && npm run typecheck` green (pathlayer's `battleToGeoJSON`/`previewToGeoJSON` get a unit test: selected flag set, preview polygon centroid ≈ unit position — write it in `tool/tests/pathlayer.test.ts`):

```typescript
import { describe, expect, it } from "vitest";
import { battleToGeoJSON, previewToGeoJSON } from "../src/ui/pathlayer";
import { Battlefield } from "../src/geo";
import golden from "./fixtures/geo-golden.json";
import placeholder from "./fixtures/placeholder_battle.json";

const bf = new Battlefield(golden.origin_utm_e, golden.origin_utm_n);

describe("pathlayer geojson", () => {
  it("marks the selected unit", () => {
    const gj = battleToGeoJSON(placeholder as any, bf, "atk-a");
    const selected = gj.paths.features.filter((f) => f.properties!.selected);
    expect(selected.length).toBe(1);
  });

  it("preview polygon centroid sits at the interpolated position", () => {
    const gj = previewToGeoJSON(placeholder as any, bf, 0);
    const ring = (gj.features[0]!.geometry as GeoJSON.Polygon).coordinates[0]!;
    const cx = ring.slice(0, 4).reduce((s, c) => s + c[0]!, 0) / 4;
    const cy = ring.slice(0, 4).reduce((s, c) => s + c[1]!, 0) / 4;
    const unit0 = (placeholder as any).units[0];
    const [lon, lat] = bf.localToLonLat(unit0.keyframes[0].x, unit0.keyframes[0].z);
    expect(cx).toBeCloseTo(lon, 6);
    expect(cy).toBeCloseTo(lat, 6);
  });
});
```

- [ ] **Commit:** `git add tool && git commit -m "feat: authoring workspace (roster, keyframes, path drawing, preview scrubber, io)"`

---

### Task 12: Historical map overlay UI

**Files:**
- Create: `tool/src/ui/overlayui.ts`
- Modify: `tool/src/ui/workspace.ts` (add an "Overlay" section calling it)

`tool/src/ui/overlayui.ts`:

```typescript
import type maplibregl from "maplibre-gl";
import type { Battlefield } from "../geo";
import { applySimilarity, similarityFromTwoPoints, type TiePoint } from "../overlay";

// Workflow: load a scanned map image; click two points ON THE IMAGE (shown in
// a modal canvas), then their true locations ON THE MAP; the similarity
// transform places the image as a raster source with adjustable opacity.
export function initOverlayUI(
  container: HTMLElement, map: maplibregl.Map, bf: Battlefield,
): void {
  const fileInput = document.createElement("input");
  fileInput.type = "file"; fileInput.accept = "image/*";
  const status = document.createElement("div"); status.className = "muted";
  const opacity = document.createElement("input");
  opacity.type = "range"; opacity.min = "0"; opacity.max = "100"; opacity.value = "60";

  let imgUrl: string | null = null;
  let imgSize: [number, number] | null = null;
  let imgPts: [number, number][] = [];
  let mapPts: [number, number][] = [];

  fileInput.addEventListener("change", async () => {
    const f = fileInput.files?.[0];
    if (!f) return;
    imgUrl = URL.createObjectURL(f);
    const probe = new Image();
    probe.onload = () => {
      imgSize = [probe.naturalWidth, probe.naturalHeight];
      imgPts = []; mapPts = [];
      pickImagePoint(1);
    };
    probe.src = imgUrl;
  });

  function pickImagePoint(n: 1 | 2): void {
    status.textContent = `Click tie point ${n} on the IMAGE…`;
    const modal = document.createElement("div");
    modal.style.cssText =
      "position:fixed;inset:0;background:rgba(0,0,0,.7);display:flex;align-items:center;justify-content:center;z-index:10";
    const img = new Image();
    img.src = imgUrl!;
    img.style.cssText = "max-width:90vw;max-height:90vh;cursor:crosshair";
    img.addEventListener("click", (e) => {
      const r = img.getBoundingClientRect();
      const sx = imgSize![0] / r.width;
      const sy = imgSize![1] / r.height;
      imgPts.push([(e.clientX - r.left) * sx, (e.clientY - r.top) * sy]);
      modal.remove();
      pickMapPoint(n);
    });
    modal.append(img);
    document.body.append(modal);
  }

  function pickMapPoint(n: 1 | 2): void {
    status.textContent = `Now click where tie point ${n} really is on the MAP…`;
    map.once("click", (e) => {
      mapPts.push([e.lngLat.lng, e.lngLat.lat]);
      if (n === 1) pickImagePoint(2);
      else placeOverlay();
    });
  }

  function placeOverlay(): void {
    const ties: TiePoint[] = imgPts.map((img, i) => ({
      img: img as [number, number],
      local: bf.lonLatToLocal(mapPts[i]![0], mapPts[i]![1]),
    }));
    const T = similarityFromTwoPoints(ties[0]!, ties[1]!);
    const [w, h] = imgSize!;
    const cornersImg: [number, number][] = [[0, 0], [w, 0], [w, h], [0, h]];
    const coords = cornersImg.map((c) =>
      bf.localToLonLat(...applySimilarity(T, c)),
    ) as [[number, number], [number, number], [number, number], [number, number]];
    if (map.getLayer("hist-overlay")) { map.removeLayer("hist-overlay"); map.removeSource("hist-overlay"); }
    map.addSource("hist-overlay", { type: "image", url: imgUrl!, coordinates: coords });
    map.addLayer(
      { id: "hist-overlay", type: "raster", source: "hist-overlay", paint: { "raster-opacity": 0.6 } },
      "unit-preview", // keep overlays under the authored content
    );
    status.textContent = "Overlay placed. Adjust opacity below; reload the image to redo tie points.";
  }

  opacity.addEventListener("input", () => {
    if (map.getLayer("hist-overlay"))
      map.setPaintProperty("hist-overlay", "raster-opacity", Number(opacity.value) / 100);
  });

  const h = document.createElement("h2");
  h.textContent = "Historical map overlay";
  container.append(h, fileInput, status, opacity);
}
```

Wire into `workspace.ts`: inside `render()`, after the File section, append a div and call `initOverlayUI(thatDiv, map, bf)` ONCE (guard with a module-level flag or build it outside `render()` into a stable sidebar section — overlay state must survive re-renders; restructure `el` into two children: `#dynamic` re-rendered and `#overlay-section` built once).

- [ ] **Verify:** `npm test && npm run typecheck` green; manual: load an image, two tie points, see it draped under unit paths.
- [ ] **Commit:** `git add tool && git commit -m "feat: scanned-map overlay with two-point georeferencing"`

---

### Task 13: Round-trip proof + docs

**Files:**
- Modify: `README.md` (tool section)
- Possibly: `app/Assets/Battle/placeholder_battle.json` (only if round-trip normalization differs)

- [ ] **Step 1 (controller, inline):** Node one-liner round-trip: import the committed placeholder asset through `tool/src/io.ts`, export, diff against the original. Expect identical (modulo trailing newline). If formatting differs trivially, regenerate the asset from the tool's canonical exporter and re-run Unity EditMode tests via MCP (36+ tests) to prove the app accepts tool output.

```bash
cd tool && npx vitest run tests/io.test.ts   # the round-trip test IS this proof
```

If the round-trip test ever requires normalizing the committed asset (key order or
formatting), regenerate `app/Assets/Battle/placeholder_battle.json` with the tool's
exporter, update the fixture copy, and re-run the Unity EditMode suite via MCP
(expect all green) to prove the app accepts tool-emitted output.

- [ ] **Step 2:** Add to `README.md` after the pipeline section:

```markdown
## Authoring tool

`tool/` is a browser app for authoring battle data: trace unit paths over a
georeferenced map (with scanned historical maps as overlays), edit keyframes
with provenance, preview interpolated movement, and export schema-validated
battle JSON (`docs/format/battle-format.md`).

```bash
cd tool
npm install
npm run dev    # http://localhost:5180 — load data/heightmap/heightmap.json to begin
npm test
```
```

- [ ] **Step 3:** Commit: `git add README.md tool app/Assets/Battle 2>/dev/null; git commit -m "docs: authoring tool usage; round-trip verified"`

---

## Done =

The authoring loop exists end to end: open the tool, see Gettysburg with the battlefield square, drape a scanned map, place units, click out paths with provenance, scrub a preview that interpolates exactly like the app, export JSON that the schema, the tool validator, AND the Unity loader all accept. Phase 4 (per spec build order) is the zoom ladder; the long-tail authoring of real July 3 data can begin any time after this ships.

## Risks

- **MapLibre/proj4 version drift** vs the plan's pinned majors — workers may bump minors freely; majors get noted in commit messages.
- **OSM tile usage**: single-author internal use is within policy; if it ever isn't, swap the style URL for a local tile cache (one constant).
- **UI tasks are manual-verification-heavy**: pure logic is tested; the DOM shell gets a Playwright/Chrome-MCP screenshot if available in-session, else user eyes on return.
