using System.Collections.Generic;
using UnityEngine;

namespace BattleAtlas
{
    // The battle's soundscape: a small pool of AudioSources whose gains and
    // positions are recomputed EVERY frame as pure functions of t and the
    // authored engagement events (AcousticMath.EventGain + each event's own
    // unit track) — the audio twin of ObscurationField, created and wired by
    // BattleDirector in Start (AddComponent + Init). The three loops are
    // synthesized once (AudioSynth) and never seek: only gain and position
    // depend on t, so scrubbing in either direction is trivially safe.
    // A PAUSED clock is SILENT (user decision, Task 7 session 2026-07-02:
    // ambient-under-idle read as a bug, not ambience). The whole soundscape
    // ducks to zero through the same anti-click slew when Playing is false
    // and swells back on play — the visible smoke still reflects t while
    // paused; only the sound treats pause as "not now."
    public class AcousticField : MonoBehaviour
    {
        // 1 ambient 2D source always on + up to 7 spatialized event sources
        public const int SpatialSourceCount = 7;
        // per-kind audibility tiers: rumble carries across the theater,
        // crackle is tactical — musketry becomes audible at roughly the
        // zoom where regiments resolve into figures (1400 m sits just
        // inside the 1500 m soldier tier)
        const float RumbleMaxDistance = 6000f;
        const float CrackleMaxDistance = 1400f;
        const float RumbleMinDistance = 150f;
        const float CrackleMinDistance = 50f;
        const float RumbleGain = 0.9f;
        const float CrackleGain = 0.8f;
        const float AmbientGain = 0.12f;
        // anti-click slew: a full-scale volume change is spread over
        // <= 150 ms. Perceptual smoothing, NOT state: the target itself is
        // recomputed from t every frame — the one honest exception to
        // "everything is a function of t" (phase plan, audio decisions).
        const float SlewSeconds = 0.15f;
        // a source is free for reassignment only once its volume has slewed
        // to silence — swapping a still-audible source would click
        const float SilenceEpsilon = 0.005f;
        // fixed-line emitters longer than this (the ~1800 m Seminary Ridge
        // gun line) read as a ribbon, not a point: they claim TWO source
        // slots at the line's third-points
        const float SplitSegmentMeters = 900f;
        // sources float a little above the emitter's ground
        const float SourceHeight = 5f;

        // one assignable emitter: an event, or one end of a long segment
        // event. Precomputed in Init; per-frame work is value-only.
        struct Candidate
        {
            public EventDto Ev;
            public UnitTrack Track;  // moving emitter; null = fixed segment
            public Vector2 FixedPos; // segment midpoint / third-point
            public bool Crackle;     // musketry -> crackle, artillery -> rumble
        }

        BattleClock clock;
        Terrain terrain;
        Candidate[] candidates;
        // parallel per-frame scratch, allocated once in Init
        float[] gains;      // EventGain at t
        float[] scores;     // gain x camera-distance attenuation (ranking only)
        Vector3[] positions;
        bool[] isWinner;
        int[] winners;

        AudioClip rumbleClip;
        AudioClip crackleClip;
        AudioSource ambientSource;
        AudioSource[] sources;
        int[] assigned;        // candidate index per source, -1 = none
        bool[] sourceIsCrackle; // which clip the source currently loops
        Camera listenerCamera;
        float groundYBase;

        // Called by BattleDirector after parsing. Emitter positions resolve
        // from each event's OWN unit's track at t — parent- and
        // regiment-level emitters alike, independent of the rendered LOD
        // tier (battle-format.md attach-level rule).
        public void Init(
            BattleDto battle, Dictionary<string, UnitTrack> tracksById,
            BattleClock clock, Terrain terrain)
        {
            this.clock = clock;
            this.terrain = terrain;

            var list = new List<Candidate>();
            int eventCount = battle.events != null ? battle.events.Count : 0;
            for (int i = 0; i < eventCount; i++)
            {
                EventDto ev = battle.events[i];
                bool crackle = ev.kind == "musketry";
                if (!string.IsNullOrEmpty(ev.unitId))
                {
                    list.Add(new Candidate
                    { Ev = ev, Track = tracksById[ev.unitId], Crackle = crackle });
                    continue;
                }
                var a = new Vector2(ev.x, ev.z);
                var b = new Vector2(ev.x2, ev.z2);
                if ((b - a).magnitude > SplitSegmentMeters)
                {
                    // two voices at the third-points: a two-mile gun line
                    // must not collapse to a single point in the pan field
                    list.Add(new Candidate
                    { Ev = ev, FixedPos = Vector2.Lerp(a, b, 1f / 3f), Crackle = crackle });
                    list.Add(new Candidate
                    { Ev = ev, FixedPos = Vector2.Lerp(a, b, 2f / 3f), Crackle = crackle });
                }
                else
                {
                    list.Add(new Candidate
                    { Ev = ev, FixedPos = Vector2.Lerp(a, b, 0.5f), Crackle = crackle });
                }
            }
            candidates = list.ToArray();
            gains = new float[candidates.Length];
            scores = new float[candidates.Length];
            positions = new Vector3[candidates.Length];
            isWinner = new bool[candidates.Length];
            winners = new int[SpatialSourceCount];
        }

        void Start()
        {
            // clips synthesized once here — pure fills, deterministic, no
            // licensing surface (see AudioSynth header re: CREDITS)
            rumbleClip = AudioSynth.CreateClip("rumble", AudioSynth.FillRumble);
            crackleClip = AudioSynth.CreateClip("crackle", AudioSynth.FillCrackle);
            AudioClip ambientClip = AudioSynth.CreateClip("ambient", AudioSynth.FillAmbient);
            listenerCamera = Camera.main;

            ambientSource = NewSource("acoustic ambient", spatial: false);
            ambientSource.clip = ambientClip;
            // starts silent; Update swells it in once the clock is Playing
            ambientSource.volume = 0f;
            ambientSource.Play();

            sources = new AudioSource[SpatialSourceCount];
            assigned = new int[SpatialSourceCount];
            sourceIsCrackle = new bool[SpatialSourceCount];
            for (int i = 0; i < SpatialSourceCount; i++)
            {
                sources[i] = NewSource($"acoustic source {i}", spatial: true);
                sources[i].clip = rumbleClip;
                ConfigureKind(sources[i], crackle: false);
                sources[i].volume = 0f;
                // started once and left looping forever at volume 0 when
                // idle — loops NEVER seek; only gain and position track t
                sources[i].Play();
                assigned[i] = -1;
            }
        }

        AudioSource NewSource(string name, bool spatial)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var src = go.AddComponent<AudioSource>();
            src.loop = true;
            src.playOnAwake = false;
            src.spatialBlend = spatial ? 1f : 0f;
            // scrubbing at 60x battle speed would pitch-shriek any doppler
            src.dopplerLevel = 0f;
            src.rolloffMode = AudioRolloffMode.Logarithmic;
            return src;
        }

        static void ConfigureKind(AudioSource src, bool crackle)
        {
            src.maxDistance = crackle ? CrackleMaxDistance : RumbleMaxDistance;
            src.minDistance = crackle ? CrackleMinDistance : RumbleMinDistance;
        }

        void Update()
        {
            if (candidates == null || sources == null) return; // pre-Init frame
            // no camera, no listener: hold the pool silent rather than
            // ranking against a bogus origin
            if (listenerCamera == null) return;

            float t = clock.CurrentTime;
            groundYBase = terrain.transform.position.y;
            Vector3 camPos = listenerCamera.transform.position;

            // 1) score every candidate: EventGain x a log-rolloff proxy of
            // what the listener would hear. The proxy only RANKS — the real
            // distance attenuation is the AudioSource's own 3D rolloff.
            for (int i = 0; i < candidates.Length; i++)
            {
                isWinner[i] = false;
                float gain = AcousticMath.EventGain(candidates[i].Ev.t0, candidates[i].Ev.t1, t);
                gains[i] = gain;
                if (gain <= 0f)
                {
                    scores[i] = 0f;
                    continue;
                }
                Vector2 posXZ = candidates[i].Track != null
                    ? candidates[i].Track.StateAt(t).posXZ
                    : candidates[i].FixedPos;
                float ground = groundYBase + terrain.SampleHeight(
                    new Vector3(posXZ.x, 0f, posXZ.y));
                positions[i] = new Vector3(posXZ.x, ground + SourceHeight, posXZ.y);
                float dist = Vector3.Distance(camPos, positions[i]);
                float minDist = candidates[i].Crackle ? CrackleMinDistance : RumbleMinDistance;
                float maxDist = candidates[i].Crackle ? CrackleMaxDistance : RumbleMaxDistance;
                scores[i] = dist > maxDist
                    ? 0f
                    : gain * (minDist / Mathf.Max(dist, minDist));
            }

            // 2) top-K selection, deterministic and thrash-stable: ties
            // break toward the LOWER candidate index, and candidate order
            // is the canonical events order (t0, then id) — the plan's
            // sort-by-gain-then-id rule with zero per-frame allocation
            for (int k = 0; k < SpatialSourceCount; k++)
            {
                int best = -1;
                float bestScore = 0f;
                for (int i = 0; i < candidates.Length; i++)
                {
                    if (isWinner[i] || scores[i] <= bestScore) continue;
                    bestScore = scores[i];
                    best = i;
                }
                winners[k] = best;
                if (best < 0) break;
                isWinner[best] = true;
            }

            // 3) drive the pool. A source keeps its candidate while it stays
            // a winner (position follows the track, volume follows the
            // gain); a loser's target drops to 0; only a SILENT source is
            // rebound to a waiting winner — so every audible transition is
            // covered by the slew and nothing pops
            float maxDelta = Time.unscaledDeltaTime / SlewSeconds;
            // pause-silence master: rides the same slew, so pausing fades
            // rather than cuts (and un-pausing swells rather than pops)
            float master = clock.Playing ? 1f : 0f;
            ambientSource.volume = Mathf.MoveTowards(
                ambientSource.volume, AmbientGain * master, maxDelta);
            for (int i = 0; i < SpatialSourceCount; i++)
            {
                int c = assigned[i];
                bool keep = c >= 0 && isWinner[c];
                if (keep)
                {
                    isWinner[c] = false; // consumed: no second source binds it
                    sources[i].transform.position = positions[c];
                }
                float kindGain = c >= 0 && candidates[c].Crackle ? CrackleGain : RumbleGain;
                float target = keep ? gains[c] * kindGain * master : 0f;
                float volume = Mathf.MoveTowards(sources[i].volume, target, maxDelta);
                sources[i].volume = volume;
                if (!keep && volume <= SilenceEpsilon)
                    assigned[i] = -1;
            }
            // silent sources pick up any winners still waiting
            for (int k = 0; k < SpatialSourceCount; k++)
            {
                int c = winners[k];
                if (c < 0) break;
                if (!isWinner[c]) continue; // already carried by its source
                for (int i = 0; i < SpatialSourceCount; i++)
                {
                    if (assigned[i] >= 0 || sources[i].volume > SilenceEpsilon) continue;
                    assigned[i] = c;
                    isWinner[c] = false;
                    if (sourceIsCrackle[i] != candidates[c].Crackle)
                    {
                        // clip swap restarts the loop at phase 0 — these are
                        // steady textures, phase is meaningless, and the
                        // source is silent right now; still never a seek
                        sourceIsCrackle[i] = candidates[c].Crackle;
                        sources[i].clip = candidates[c].Crackle ? crackleClip : rumbleClip;
                        ConfigureKind(sources[i], candidates[c].Crackle);
                        sources[i].Play();
                    }
                    sources[i].transform.position = positions[c];
                    break;
                }
            }
        }
    }
}
