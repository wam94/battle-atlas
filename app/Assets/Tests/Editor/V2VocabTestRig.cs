using System.Collections.Generic;
using BattleAtlas;

/// <summary>
/// Shared synthetic-bundle builders for the Angle-v2 vocabulary suites
/// (melee, colors, mounted officer, halt-and-fire). Follows the
/// FightProneResolverTests.BuildRiseContext pattern: small hand-built
/// bundles, pure compile, no committed content touched.
/// </summary>
static class V2VocabTestRig
{
    public static AngleBundleSegment Seg(
        string id, string action, float t0, float t1,
        string opponent = null, List<string> obstacles = null)
        => new AngleBundleSegment
        {
            id = id,
            action = action,
            t0 = t0,
            t1 = t1,
            formationFrom = "line",
            formationTo = "line",
            paceProfile = "static",
            provenance = "editorial",
            obstacleIds = obstacles ?? new List<string>(),
            meleeOpponentId = opponent,
        };

    public static AngleCasualtyProfile Profile(
        string id, float t0, float t1, int count)
        => new AngleCasualtyProfile
        {
            id = id,
            t0 = t0,
            t1 = t1,
            count = count,
            intensityCurve = "uniform",
            causeMix = new AngleCauseMix { musketry = 1f },
            assessment = "editorial",
        };

    // A unit whose centroid follows per-second waypoints (linear between
    // the given (t, x) samples; constant z/facing).
    public static AngleBundleUnit Unit(
        string unitId, string side, float t0, float t1,
        float x, float z, float facingDeg, int strength,
        List<AngleBundleSegment> segments,
        List<AngleCasualtyProfile> profiles = null,
        (float t, float x)[] xTrack = null,
        int colorParty = 0)
    {
        int seconds = (int)(t1 - t0) + 1;
        var ps = new AnglePerSecond
        {
            t0 = t0,
            x = new List<float>(),
            z = new List<float>(),
            facingDeg = new List<float>(),
            strength = new List<float>(),
            segmentIndex = new List<int>(),
        };
        for (int i = 0; i < seconds; i++)
        {
            float t = t0 + i;
            float xi = x;
            if (xTrack != null)
            {
                xi = xTrack[xTrack.Length - 1].x;
                for (int k = 0; k < xTrack.Length - 1; k++)
                {
                    if (t <= xTrack[k + 1].t)
                    {
                        float f = (t - xTrack[k].t) /
                            (xTrack[k + 1].t - xTrack[k].t);
                        xi = xTrack[k].x +
                            (xTrack[k + 1].x - xTrack[k].x) *
                            UnityEngine.Mathf.Clamp01(f);
                        break;
                    }
                }
            }
            ps.x.Add(xi);
            ps.z.Add(z);
            ps.facingDeg.Add(facingDeg);
            ps.strength.Add(strength);
            int si = segments.Count - 1;
            for (int s = 0; s < segments.Count; s++)
                if (t >= segments[s].t0 && t < segments[s].t1) { si = s; break; }
            ps.segmentIndex.Add(si);
        }
        return new AngleBundleUnit
        {
            unitId = unitId,
            name = unitId,
            side = side,
            arm = "infantry",
            startStrength = strength,
            segments = segments,
            casualtyProfiles = profiles ?? new List<AngleCasualtyProfile>(),
            perSecond = ps,
            colorParty = colorParty,
        };
    }

    public static AngleActionContext Compile(
        string seed, float t0, float t1,
        Dictionary<string, List<UnityEngine.Vector2>> obstacles,
        params AngleBundleUnit[] units)
    {
        var bundle = new AngleBundle
        {
            format = "angle-bundle/1",
            stagingSeed = seed,
            slice = new AngleBundleSlice { t0 = t0, t1 = t1 },
            clock = new AngleBundleClock
            { startTimeSecondsSinceMidnight = 46800 },
            units = new List<AngleBundleUnit>(units),
        };
        return AngleActionContext.Compile(bundle, bundle.StagingSeed, obstacles);
    }
}
