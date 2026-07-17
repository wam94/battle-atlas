"""Deterministic Soldier View audio stems (plan §9.3, §12 Phase 9).

The mix is a pure function of the battle clock, exactly like the visuals:
every musket report is one resolver-confirmed discharge from the compiled
fire cycles, every cannon report one scheduled shot, every strike thump
one compiled impact, every wounded voice one scheduled casualty — all
exported by `GateP9Render.ExportAudioEvents` (the same event streams the
renderer draws), placed with distance delay at 343 m/s, distance
attenuation, distance low-pass, and constant-power panning relative to
the hero camera's own heading track. Sample-variant choice and every
sparse gate is a hash of (seed, stem, event index) — no RNG state, no
time-of-day, so the same events JSON and source pack produce
byte-identical stems (ED-23; tested).

Layers -> stems (§9.3):
  ambience        wind / insects / rural beds (unmodeled-continuous)
  artillery       near + distant cannon reports
  musketry        individual reports; massed fire IS the rolling volley
  projectiles     pass-by/ricochet for fire directed at the observer's line
  strikes         canister/shell earth impacts near the unit
  movement        observer footfalls, the marching file around him,
                  fence-rail crossings
  voices_unit     unit noise + shout bursts at segment transitions
                  (generic, unattributed; no worded named-person dialogue)
  voices_wounded  sober, sparse, tied to scheduled nearby casualties (§9.2)
  breathing       near-camera exertion, scaled by gait + chaos

Deliberate omissions, recorded in ED-23: no friendly musketry/reload
foley (Garnett's brigade has no compiled fire segment in this window);
no smoke muffling (not acoustically justifiable at these ranges); no
pre-slice bombardment residue.

Usage (from reconstruction/):
  uv run python scripts/build_viewpoint_audio.py \
      --events ../docs/benchmarks/captures/p9-gate/p9-audio-events.json \
      --out ../docs/benchmarks/captures/p9-gate/stems \
      [--t0 8610 --t1 8670] [--pack ../app/Assets/ThirdParty/Audio]

Outputs <out>/<stem>.wav (stereo 16-bit 44.1 kHz) + <out>/mix.wav +
<out>/stems.sha256. Mux with the proof video via scripts/p9-encode.sh.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import math
import wave
from pathlib import Path

import numpy as np

SR = 44100
SPEED_OF_SOUND = 343.0  # m/s (§9.3 distance delay)
TAIL_S = 2.5            # room for delayed arrivals after t1

REPO = Path(__file__).resolve().parent.parent.parent
DEFAULT_PACK = REPO / "app/Assets/ThirdParty/Audio"

# --- authored mix constants (documented, deterministic) -------------------
STEM_GAIN = {
    "ambience": 0.32,
    "artillery": 0.95,
    "musketry": 0.85,
    "projectiles": 0.5,
    "strikes": 0.8,
    "movement": 0.55,
    "voices_unit": 0.4,
    "voices_wounded": 0.45,
    "breathing": 0.28,
}
MASTER_GAIN = 0.9

MUSKET_REF_M = 9.0      # distance where a musket report is unity gain
CANNON_REF_M = 30.0
VOICE_REF_M = 6.0
STRIKE_REF_M = 10.0
ROLLOFF = 1.15

WHIZ_FRACTION = 0.05    # of enemy discharges within whiz range
WHIZ_RANGE_M = 320.0
GROAN_FRACTION = 0.3    # of nearby scheduled casualties voiced (sober)
GROAN_RANGE_M = 32.0

# observer-segment actions that carry a shout burst (voices_unit); shared
# with generate_captions.py so a caption exists exactly where a shout does.
# webb-cushing slice: fire_by_rank / fire_independent / close_gap added for
# the DEFENDING observers (the line opening fire and the seal are the loud
# human moments of the receiving side). The shipped garnett viewpoint has
# none of these actions in its segment list, so its mix and captions are
# byte-unchanged.
NOISY_ACTIONS = {"cross_obstacle", "take_canister", "waver",
                 "fall_back", "rout", "breach",
                 "fire_by_rank", "fire_independent", "close_gap"}


def h01(seed: str, stem: str, i: int, salt: int = 0) -> float:
    digest = hashlib.sha256(f"{seed}|{stem}|{i}|{salt}".encode()).digest()
    return int.from_bytes(digest[:8], "big") / float(1 << 64)


def read_wav_mono(path: Path) -> np.ndarray:
    with wave.open(str(path), "rb") as w:
        assert w.getframerate() == SR, f"{path}: expected {SR} Hz"
        n = w.getnframes()
        data = np.frombuffer(w.readframes(n), dtype=np.int16)
        if w.getnchannels() == 2:
            data = data.reshape(-1, 2).mean(axis=1)
    return np.asarray(data, dtype=np.float32) / 32768.0


def one_pole_lp(x: np.ndarray, cutoff_hz: float) -> np.ndarray:
    """Deterministic one-pole low-pass (distance air absorption)."""
    a = math.exp(-2.0 * math.pi * cutoff_hz / SR)
    b = 1.0 - a
    y = np.empty_like(x)
    acc = 0.0
    # scipy-free sequential filter; clips are short so this is fine
    for i in range(len(x)):
        acc = b * x[i] + a * acc
        y[i] = acc
    return y


class SamplePack:
    """Loads clip variants per category dir; pre-filters far versions."""

    def __init__(self, root: Path):
        self.root = root
        self.cache: dict[str, list[np.ndarray]] = {}

    def variants(self, category: str, prefix: str | None = None,
                 far: bool = False) -> list[np.ndarray]:
        key = f"{category}|{prefix}|{far}"
        if key in self.cache:
            return self.cache[key]
        cat = self.root / category
        paths = sorted(p for p in cat.rglob("*.wav")
                       if prefix is None or p.name.startswith(prefix))
        if not paths:
            raise FileNotFoundError(
                f"no clips for category={category} prefix={prefix} under {cat}")
        clips = [read_wav_mono(p) for p in paths]
        if far:
            clips = [one_pole_lp(c, 900.0) for c in clips]
        self.cache[key] = clips
        return clips

    def pick(self, clips: list[np.ndarray], seed: str, stem: str,
             i: int) -> np.ndarray:
        return clips[int(h01(seed, stem, i, 7) * len(clips)) % len(clips)]


class ObserverTrack:
    """10 Hz camera track from the export; interpolated, pure."""

    def __init__(self, rows: list[dict]):
        self.t = np.array([r["t"] for r in rows], dtype=np.float64)
        self.x = np.array([r["x"] for r in rows], dtype=np.float64)
        self.z = np.array([r["z"] for r in rows], dtype=np.float64)
        heading = np.radians([r["headingDeg"] for r in rows])
        self.hx = np.sin(heading)
        self.hz = np.cos(heading)
        self.chaos = np.array([r["chaos"] for r in rows], dtype=np.float64)
        self.loco = np.array(
            [1.0 if r["clip"] in ("March", "RouteStep", "DoubleQuick",
                                  "RoutedRun") else 0.0 for r in rows])

    def sample(self, t: float) -> tuple[float, float, float, float, float, float]:
        i = float(np.interp(t, self.t, np.arange(len(self.t))))
        lo = int(i)
        hi = min(lo + 1, len(self.t) - 1)
        f = i - lo

        def lerp(a):
            return float(a[lo] * (1 - f) + a[hi] * f)
        return (lerp(self.x), lerp(self.z), lerp(self.hx), lerp(self.hz),
                lerp(self.chaos), lerp(self.loco))


def geometry(track: ObserverTrack, t: float, ex: float, ez: float
             ) -> tuple[float, float]:
    """(distance m, pan in [-1,1]) of an event at (ex, ez) at time t."""
    ox, oz, hx, hz, _, _ = track.sample(t)
    dx, dz = ex - ox, ez - oz
    r = math.hypot(dx, dz)
    if r < 1e-6:
        return 0.0, 0.0
    # bearing relative to camera heading; right = positive pan
    rx = hz * dx - hx * dz          # component along camera-right
    fw = hx * dx + hz * dz          # component along camera-forward
    az = math.atan2(rx, fw)
    return r, math.sin(az) * 0.85


class StemWriter:
    def __init__(self, t0: float, t1: float):
        self.t0 = t0
        self.n = int((t1 - t0 + TAIL_S) * SR)
        self.buf = np.zeros((self.n, 2), dtype=np.float32)

    def add(self, when: float, clip: np.ndarray, gain: float, pan: float):
        start = int((when - self.t0) * SR)
        if start >= self.n or start + len(clip) <= 0:
            return
        a = max(0, start)
        b = min(self.n, start + len(clip))
        seg = clip[a - start: b - start]
        # constant-power pan
        ang = (pan + 1.0) * (math.pi / 4.0)
        gl, gr = math.cos(ang) * gain, math.sin(ang) * gain
        self.buf[a:b, 0] += seg * gl
        self.buf[a:b, 1] += seg * gr

    def add_bed(self, bed: np.ndarray, envelope: np.ndarray | None,
                gain: float):
        """Loop a mono bed over the whole stem with 1 s crossfades."""
        xfade = SR
        period = max(1, len(bed) - xfade)
        out = np.zeros(self.n, dtype=np.float32)
        pos = 0
        k = 0
        while pos < self.n:
            b = min(self.n, pos + len(bed))
            seg = bed[: b - pos].copy()
            if k > 0:
                nx = min(xfade, len(seg))
                seg[:nx] *= np.linspace(0.0, 1.0, nx, dtype=np.float32)
                prev_tail = bed[period: period + nx]
                pt = prev_tail[: max(0, min(nx, self.n - pos))].copy()
                pt *= np.linspace(1.0, 0.0, len(pt), dtype=np.float32)
                out[pos: pos + len(pt)] += pt
            out[pos:b] += seg
            pos += period
            k += 1
        if envelope is not None:
            out *= envelope[: len(out)]
        self.buf[:, 0] += out * gain * 0.7071
        self.buf[:, 1] += out * gain * 0.7071

    def write(self, path: Path) -> float:
        """Write 16-bit stereo; returns the fileGain the buffer was
        scaled by (dense musketry can exceed 0 dBFS in-buffer; the mix
        path uses the unclipped float buffer, and stems.json records
        each stem's fileGain so `mix = Σ stem/fileGain × STEM_GAIN`
        reconstructs the authoritative mix exactly from the files)."""
        path.parent.mkdir(parents=True, exist_ok=True)
        peak = float(np.max(np.abs(self.buf))) if self.buf.size else 0.0
        file_gain = 1.0 if peak <= 0.99 else 0.99 / peak
        x = np.clip(self.buf * file_gain, -1.0, 1.0)
        xi = np.round(x * 32767.0).astype(np.int16)
        with wave.open(str(path), "wb") as w:
            w.setnchannels(2)
            w.setsampwidth(2)
            w.setframerate(SR)
            w.writeframes(xi.tobytes())
        return file_gain


def distance_gain(r: float, ref: float) -> float:
    return min(1.0, (ref / max(r, ref * 0.5)) ** ROLLOFF)


def arrival(t: float, r: float) -> float:
    return t + r / SPEED_OF_SOUND


def build(events: dict, pack_root: Path, out_dir: Path,
          t0: float | None, t1: float | None) -> dict[str, str]:
    seed = events["seed"]
    w0 = float(t0 if t0 is not None else events["window"]["t0"])
    w1 = float(t1 if t1 is not None else events["window"]["t1"])
    # Observer identity (webb-cushing slice): newer event exports carry
    # observerUnit/observerSide so a DEFENDING (Union) observer mixes
    # correctly — the whiz layer keys on enemy fire relative to the
    # observer's side, and the strike stem keeps every compiled impact
    # within earshot instead of the garnett-era own-unit shortcut. The
    # legacy garnett export has neither key; both defaults reproduce the
    # shipped garnett stems byte-for-byte.
    obs_unit = events.get("observerUnit")          # None => legacy export
    obs_side = events.get("observerSide", "confederate")
    enemy_side = "union" if obs_side == "confederate" else "confederate"
    track = ObserverTrack(events["observer"])
    pack = SamplePack(pack_root)
    stems: dict[str, StemWriter] = {}

    def stem(name: str) -> StemWriter:
        if name not in stems:
            stems[name] = StemWriter(w0, w1)
        return stems[name]

    def in_window(at: float, slack: float = 0.0) -> bool:
        return w0 - slack <= at <= w1 + TAIL_S

    # --- ambience beds -----------------------------------------------------
    amb = stem("ambience")
    for i, bed_prefix in enumerate(("fs-meadow-trp", "fs-meadow-wind")):
        beds = pack.variants("Ambience", prefix=bed_prefix)
        amb.add_bed(beds[0], None, 0.5 if i == 0 else 0.35)

    # --- artillery ----------------------------------------------------------
    art = stem("artillery")
    near = pack.variants("Cannon", prefix="fs-cannon-tman95") + \
        pack.variants("Cannon", prefix="fs-cannon-andykub") + \
        pack.variants("Cannon", prefix="fs-cannon-vishwajay") + \
        pack.variants("Cannon", prefix="fs-cannon-qubodup")
    far = pack.variants("Cannon", prefix="fs-cannon-distant")
    for i, e in enumerate(events["cannonDischarges"]):
        r, pan = geometry(track, e["t"], e["x"], e["z"])
        at = arrival(e["t"], r)
        if not in_window(at):
            continue
        clips = far if r > 420.0 else near
        clip = pack.pick(clips, seed, "artillery", i)
        art.add(at, clip, distance_gain(r, CANNON_REF_M), pan)

    # --- musketry (mass fire = the rolling volley) ---------------------------
    mus = stem("musketry")
    shots = pack.variants("Musket")
    shots_far = pack.variants("Musket", far=True)
    whiz = stem("projectiles")
    whiz_clips = pack.variants("Projectile")
    for i, e in enumerate(events["musketDischarges"]):
        r, pan = geometry(track, e["t"], e["x"], e["z"])
        at = arrival(e["t"], r)
        if in_window(at):
            clips = shots_far if r > 260.0 else shots
            clip = pack.pick(clips, seed, "musketry", i)
            mus.add(at, clip, distance_gain(r, MUSKET_REF_M), pan)
        # §9.3 projectile layer: a hash minority of ENEMY fire inside
        # whiz range passes near the observer's file (relative to the
        # observer's side; legacy garnett exports default to enemy=union)
        if (e["side"] == enemy_side and r < WHIZ_RANGE_M and
                h01(seed, "whiz", i) < WHIZ_FRACTION):
            ball_at = e["t"] + r / 300.0  # minie ball mean flight speed
            if in_window(ball_at):
                clip = pack.pick(whiz_clips, seed, "whiz", i)
                # pass-by pans through the OPPOSITE side it came from
                whiz.add(ball_at, clip, 0.5 * distance_gain(r, 60.0), -pan)

    # --- strikes -------------------------------------------------------------
    stk = stem("strikes")
    dirt = pack.variants("Strike")
    for i, e in enumerate(events["strikes"]):
        if obs_unit is None and e.get("unitId") != "csa-garnett":
            continue  # legacy export: only the observer's unit's impacts
            # were near enough (garnett-era shortcut, kept byte-exact)
        r, pan = geometry(track, e["t"], e["x"], e["z"])
        at = arrival(e["t"], r)
        if not in_window(at) or r > 120.0:
            continue
        clip = pack.pick(dirt, seed, "strikes", i)
        stk.add(at, clip, distance_gain(r, STRIKE_REF_M), pan)

    # --- movement -------------------------------------------------------------
    mov = stem("movement")
    steps = pack.variants("Movement", prefix="fs-step-") + \
        pack.variants("Movement", prefix="fs-walk-")
    runs = pack.variants("Movement", prefix="fs-run-")
    rails = pack.variants("Movement", prefix="fs-rail-")
    for i, f in enumerate(events["footfalls"]):
        if not in_window(f["t"]):
            continue
        own = pack.pick(
            runs if f["clip"] in ("DoubleQuick", "RoutedRun") else steps,
            seed, "movement", i)
        mov.add(f["t"], own, 0.8, 0.0)
        # the file marching around him: same cadence, hash-spread copies
        for k in range(2):
            dt = 0.12 * (h01(seed, "file-steps", i, k) - 0.5)
            side = 1.0 if h01(seed, "file-side", i, k) < 0.5 else -1.0
            other = pack.pick(steps, seed, "file-clip", i * 2 + k)
            mov.add(f["t"] + 0.07 + dt, other, 0.30, side * 0.45)
    for i, c in enumerate(events["crossings"]):
        r, pan = geometry(track, c["t"], c["x"], c["z"])
        at = c["t"] if c["self"] else arrival(c["t"], r)
        if not in_window(at) or r > 25.0:
            continue
        clip = pack.pick(rails, seed, "rails", i)
        mov.add(at, clip, 0.9 if c["self"] else 0.5 * distance_gain(r, 4.0),
                0.0 if c["self"] else pan)

    # --- voices: unit noise at segment transitions ---------------------------
    vu = stem("voices_unit")
    shout_bursts = pack.variants("Voice", prefix="fs-shout-")
    mob_bed = pack.variants("Voice", prefix="fs-mob-")[0]
    for i, seg in enumerate(events["observerSegments"]):
        if seg["action"] not in NOISY_ACTIONS:
            continue
        at = seg["t0"] + 0.8
        if not in_window(at):
            continue
        clip = pack.pick(shout_bursts, seed, "orders", i)
        vu.add(at, clip, 0.7, 0.15 * (h01(seed, "orders-pan", i) - 0.5))
    # low mob rumble scaled by the observer's chaos level
    env = np.zeros(int((w1 - w0 + TAIL_S) * SR), dtype=np.float32)
    for i in range(0, len(env), SR // 10):
        t = w0 + i / SR
        _, _, _, _, chaos, _ = track.sample(t)
        env[i: i + SR // 10] = chaos
    vu.add_bed(mob_bed, env, 0.6)

    # --- voices: the wounded (sober, sparse, §9.2) ----------------------------
    vw = stem("voices_wounded")
    groans = pack.variants("Voice", prefix="fs-groan-")
    for i, e in enumerate(events["casualties"]):
        r, pan = geometry(track, e["t"], e["x"], e["z"])
        if r > GROAN_RANGE_M:
            continue
        if h01(seed, "groan", i) >= GROAN_FRACTION:
            continue
        at = arrival(e["t"] + 1.2 + 1.5 * h01(seed, "groan-dt", i), r)
        if not in_window(at):
            continue
        clip = pack.pick(groans, seed, "groan-clip", i)
        vw.add(at, clip, 0.8 * distance_gain(r, VOICE_REF_M), pan)
        # the wounded who crawl groan again, later and quieter
        if e["crawls"]:
            at2 = at + 6.0 + 4.0 * h01(seed, "groan-again", i)
            if in_window(at2):
                vw.add(at2, pack.pick(groans, seed, "groan-clip2", i),
                       0.5 * distance_gain(r, VOICE_REF_M), pan)

    # --- breathing (near camera, exertion-scaled) -----------------------------
    br = stem("breathing")
    breath = pack.variants("Voice", prefix="fs-breath-")[0]
    envb = np.zeros(int((w1 - w0 + TAIL_S) * SR), dtype=np.float32)
    for i in range(0, len(envb), SR // 10):
        t = w0 + i / SR
        _, _, _, _, chaos, loco = track.sample(t)
        envb[i: i + SR // 10] = 0.25 + 0.45 * loco + 0.30 * chaos
    br.add_bed(breath, envb, 1.0)

    # --- write stems + mix -----------------------------------------------------
    out_dir.mkdir(parents=True, exist_ok=True)
    digests: dict[str, str] = {}
    file_gains: dict[str, float] = {}
    mix = np.zeros_like(stem("ambience").buf)
    for name in sorted(STEM_GAIN):
        s = stems.get(name) or StemWriter(w0, w1)
        path = out_dir / f"{name}.wav"
        file_gains[name] = s.write(path)
        digests[name] = hashlib.sha256(path.read_bytes()).hexdigest()
        mix += s.buf * STEM_GAIN[name]
    mix = np.tanh(mix * MASTER_GAIN)  # deterministic soft limiter
    mw = StemWriter(w0, w1)
    mw.buf = mix.astype(np.float32)
    mix_path = out_dir / "mix.wav"
    mw.write(mix_path)
    digests["mix"] = hashlib.sha256(mix_path.read_bytes()).hexdigest()

    (out_dir / "stems.json").write_text(json.dumps({
        "window": {"t0": w0, "t1": w1},
        "seed": seed,
        "stemGains": STEM_GAIN,
        "masterGain": MASTER_GAIN,
        "fileGains": file_gains,
        "note": "mix.wav = tanh(masterGain * Σ stem/fileGain × stemGain); "
                "soft limiter tanh; stems stored 16-bit with fileGain "
                "normalization so the mix is exactly regenerable from them.",
    }, indent=2) + "\n")
    sha_lines = "".join(f"{v}  {k}.wav\n" for k, v in sorted(digests.items()))
    (out_dir / "stems.sha256").write_text(sha_lines)
    return digests


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--events", required=True, type=Path)
    ap.add_argument("--out", required=True, type=Path)
    ap.add_argument("--pack", type=Path, default=DEFAULT_PACK)
    ap.add_argument("--t0", type=float, default=None)
    ap.add_argument("--t1", type=float, default=None)
    args = ap.parse_args()

    events = json.loads(args.events.read_text())
    digests = build(events, args.pack, args.out, args.t0, args.t1)
    for k, v in sorted(digests.items()):
        print(f"{k}: {v[:16]}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
