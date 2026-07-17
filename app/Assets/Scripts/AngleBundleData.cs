using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BattleAtlas
{
    // Runtime DTOs + loader for the compiled tactical artifact
    // Assets/Battle/Angle/angle.bundle.json (Reconstruction V2 Phase 5).
    // Phase 8 is the first consumer: the deterministic action/casualty/VFX
    // scene resolves every soldier from this data.
    //
    // JsonUtility parses only DECLARED fields and skips the rest, so the
    // bundle's nested `route` arrays and the dynamic-keyed `claimsIndex`
    // are simply not declared here (route geometry is not needed at
    // runtime: perSecond carries the compiled track).
    [Serializable]
    public class AngleBundle
    {
        public string format;
        public string checksum;
        // ED-21 (never re-rolled): the battle seed for all hash-drawn
        // staging noise (victim draws, step phase, yaw, waver). Pinned by
        // the compiler at the reviewed-and-shipped bundle's checksum so
        // provenance-only recompiles (sources/claims metadata) cannot
        // re-roll the film; bumped deliberately, with owner sign-off,
        // only when choreography content actually changes.
        public string stagingSeed;
        public AngleBundleSlice slice;
        public AngleBundleClock clock;
        public List<AngleBundleUnit> units;

        public string StagingSeed =>
            string.IsNullOrEmpty(stagingSeed) ? checksum : stagingSeed;

        public AngleBundleUnit Unit(string unitId)
        {
            foreach (var u in units)
                if (u.unitId == unitId) return u;
            throw new InvalidOperationException($"bundle has no unit '{unitId}'");
        }
    }

    [Serializable]
    public class AngleBundleSlice
    {
        public float t0;
        public float t1;
    }

    [Serializable]
    public class AngleBundleClock
    {
        public int startTimeSecondsSinceMidnight;
    }

    [Serializable]
    public class AngleBundleUnit
    {
        public string unitId;
        public string name;
        public string side;         // "confederate" | "union"
        public string arm;          // "infantry" | "artillery"
        public int startStrength;
        public List<AngleBundleSegment> segments;
        public List<AngleCasualtyProfile> casualtyProfiles;
        public AnglePerSecond perSecond;

        // Angle-v2 vocabulary (P4 colors): length of the unit's color-
        // guard succession chain (0 = no color party staged — the default
        // for every committed bundle; JsonUtility leaves the field 0 when
        // absent, so existing bundles resolve bit-identically). The v2
        // DATA wave sets this per regiment against the attested
        // bearer-down counts (or-27-2-shepard). See ColorGuard.cs.
        public int colorParty;

        // Segment active at battle time t (segments are contiguous and
        // ordered by the Phase 5 compiler; validated at load).
        public AngleBundleSegment SegmentAt(float t)
        {
            for (int i = segments.Count - 1; i >= 0; i--)
                if (t >= segments[i].t0) return segments[i];
            return segments[0];
        }

        public Vector2 PositionAt(float t)
        {
            int n = perSecond.x.Count;
            float ft = Mathf.Clamp(t - perSecond.t0, 0f, n - 1);
            int i = Mathf.Min((int)ft, n - 2);
            float frac = ft - i;
            return new Vector2(
                Mathf.Lerp(perSecond.x[i], perSecond.x[i + 1], frac),
                Mathf.Lerp(perSecond.z[i], perSecond.z[i + 1], frac));
        }

        public float FacingAt(float t)
        {
            int n = perSecond.facingDeg.Count;
            float ft = Mathf.Clamp(t - perSecond.t0, 0f, n - 1);
            int i = Mathf.Min((int)ft, n - 2);
            return Mathf.LerpAngle(
                perSecond.facingDeg[i], perSecond.facingDeg[i + 1], ft - i);
        }

        public float StrengthAt(float t)
        {
            int n = perSecond.strength.Count;
            int i = Mathf.Clamp(Mathf.RoundToInt(t - perSecond.t0), 0, n - 1);
            return perSecond.strength[i];
        }
    }

    [Serializable]
    public class AngleBundleSegment
    {
        public string id;
        public string action;        // §6.3 vocabulary
        public float t0;
        public float t1;
        public string formationFrom; // line | line_disordered | scattered | routed
        public string formationTo;
        public string paceProfile;
        public string provenance;
        public List<string> obstacleIds;

        // Angle-v2 vocabulary (P3 melee): the enemy unit this melee
        // segment fights (unset/empty for every committed bundle and for
        // every non-melee action). A `melee` segment with an opponent set
        // lets the resolver form deterministic grapple pairs across the
        // two rosters; without it the melee resolves to unpaired
        // swing/thrust/parry work only. The v2 DATA wave wires it.
        public string meleeOpponentId;

        public float Progress(float t) =>
            t1 <= t0 ? 1f : Mathf.Clamp01((t - t0) / (t1 - t0));
    }

    [Serializable]
    public class AngleCasualtyProfile
    {
        public string id;
        public float t0;
        public float t1;
        public int count;
        public string intensityCurve;   // uniform | rising | falling | spike
        public AngleCauseMix causeMix;
        public string assessment;
    }

    // The bundle's causeMix object has a small closed key set; missing keys
    // parse as 0.
    [Serializable]
    public class AngleCauseMix
    {
        public float musketry;
        public float canister;
        public float shell;
        public float unknown;
    }

    [Serializable]
    public class AnglePerSecond
    {
        public float t0;
        public List<float> x;
        public List<float> z;
        public List<float> facingDeg;
        public List<float> strength;
        public List<int> segmentIndex;
    }

    public static class AngleBundleLoader
    {
        public static string DefaultPath => Path.Combine(
            Application.dataPath, "Battle", "Angle", "angle.bundle.json");

        public static AngleBundle Load(string path = null)
        {
            path ??= DefaultPath;
            if (!File.Exists(path))
                throw new InvalidOperationException($"missing bundle {path}");
            var bundle = JsonUtility.FromJson<AngleBundle>(File.ReadAllText(path));
            Validate(bundle);
            return bundle;
        }

        // Loud-failure validation (plan §12 Phase 8: invalid inputs fail,
        // never silently degrade).
        public static void Validate(AngleBundle b)
        {
            if (b.format != "angle-bundle/1")
                throw new InvalidOperationException($"unknown bundle format '{b.format}'");
            if (b.units == null || b.units.Count == 0)
                throw new InvalidOperationException("bundle has no units");
            int seconds = (int)(b.slice.t1 - b.slice.t0) + 1;
            foreach (var u in b.units)
            {
                var ps = u.perSecond;
                if (ps.x.Count != seconds || ps.z.Count != seconds ||
                    ps.facingDeg.Count != seconds || ps.strength.Count != seconds)
                    throw new InvalidOperationException(
                        $"{u.unitId}: perSecond arrays must cover {seconds} seconds");
                if (u.startStrength <= 0)
                    throw new InvalidOperationException($"{u.unitId}: bad startStrength");
                float prevT1 = b.slice.t0;
                foreach (var s in u.segments)
                {
                    if (!Mathf.Approximately(s.t0, prevT1))
                        throw new InvalidOperationException(
                            $"{u.unitId}: segment {s.id} not contiguous at t={s.t0}");
                    prevT1 = s.t1;
                }
                if (!Mathf.Approximately(prevT1, b.slice.t1))
                    throw new InvalidOperationException(
                        $"{u.unitId}: segments do not reach slice end");
                int casTotal = 0;
                foreach (var p in u.casualtyProfiles) casTotal += p.count;
                if (casTotal > u.startStrength)
                    throw new InvalidOperationException(
                        $"{u.unitId}: casualties {casTotal} exceed strength {u.startStrength}");
            }
        }
    }
}
