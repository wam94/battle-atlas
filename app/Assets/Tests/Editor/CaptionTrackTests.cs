using System;
using System.IO;
using NUnit.Framework;
using BattleAtlas;

// Pins the Phase 12 caption model (plan §12 P12: caption option for the
// Soldier View voice layers): parsing/validation of captions.json, the
// battle-time display window, overlap stacking (newest first, capped),
// and that the COMMITTED captions file parses and sits inside the hero
// viewpoint's window.
public class CaptionTrackTests
{
    static string Json(params (double t, double dur, string text)[] cs)
    {
        var sb = new System.Text.StringBuilder(
            "{\"viewpointId\":\"vp\",\"captions\":[");
        for (int i = 0; i < cs.Length; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append("{\"t\":").Append(cs[i].t)
              .Append(",\"dur\":").Append(cs[i].dur)
              .Append(",\"kind\":\"wounded\",\"text\":\"")
              .Append(cs[i].text).Append("\"}");
        }
        return sb.Append("]}").ToString();
    }

    [Test]
    public void FromJson_RejectsMissingTextUnsortedAndNonPositiveDuration()
    {
        Assert.Throws<ArgumentException>(() => CaptionTrack.FromJson("{}"));
        Assert.Throws<ArgumentException>(() =>
            CaptionTrack.FromJson(Json((10, 2, ""))));
        Assert.Throws<ArgumentException>(() =>
            CaptionTrack.FromJson(Json((10, 0, "[x]"))));
        Assert.Throws<ArgumentException>(() =>
            CaptionTrack.FromJson(Json((10, 2, "[a]"), (5, 2, "[b]"))));
    }

    [Test]
    public void TextAt_WindowsAndExpires()
    {
        var track = CaptionTrack.FromJson(Json((10, 2.5, "[a]")));
        Assert.AreEqual("", track.TextAt(9.99));
        Assert.AreEqual("[a]", track.TextAt(10.0));
        Assert.AreEqual("[a]", track.TextAt(12.49));
        Assert.AreEqual("", track.TextAt(12.5));
    }

    [Test]
    public void TextAt_StacksOverlaps_NewestFirst_CappedAtMaxLines()
    {
        var track = CaptionTrack.FromJson(Json(
            (10, 5, "[a]"), (11, 5, "[b]"), (12, 5, "[c]")));
        Assert.AreEqual("[a]", track.TextAt(10.5));
        Assert.AreEqual("[b]\n[a]", track.TextAt(11.5));
        // three live at t=12.5; only the newest MaxLines(=2) show
        Assert.AreEqual("[c]\n[b]", track.TextAt(12.5));
        // [a] expires at 15; [b] and [c] remain
        Assert.AreEqual("[c]\n[b]", track.TextAt(15.5));
    }

    [Test]
    public void CommittedCaptionsFile_ParsesAndSitsInsideTheHeroWindow()
    {
        string path = UnityEngine.Application.dataPath
            + "/StreamingAssets/SoldierView/captions.json";
        var track = CaptionTrack.FromJson(File.ReadAllText(path));
        Assert.AreEqual("garnett-road-to-angle", track.viewpointId);
        Assert.Greater(track.captions.Length, 0);
        foreach (CaptionDto c in track.captions)
        {
            // hero window t=8160..8820 plus the mix's arrival tail
            Assert.GreaterOrEqual(c.t, 8160.0);
            Assert.LessOrEqual(c.t, 8823.0);
            Assert.IsTrue(c.text.StartsWith("[") && c.text.EndsWith("]"),
                $"caption at t={c.t} is not a bracketed non-speech description");
        }
    }

    // ------------------------------------------------------------------
    // Multi-viewpoint caption selection (webb-cushing slice): each film's
    // captions mirror ITS OWN mix; the HUD must never show one film's
    // captions inside another film's soundscape.
    // ------------------------------------------------------------------

    [Test]
    public void ForViewpoint_SelectsTheMatchingTrackOnly()
    {
        var a = CaptionTrack.FromJson(Json((10, 2, "[a]")));
        a.viewpointId = "garnett-road-to-angle";
        var b = CaptionTrack.FromJson(Json((10, 2, "[b]")));
        b.viewpointId = "webb-wall";
        var tracks = new System.Collections.Generic.List<CaptionTrack> { a, b };

        Assert.AreSame(a, CaptionTrack.ForViewpoint(tracks, "garnett-road-to-angle"));
        Assert.AreSame(b, CaptionTrack.ForViewpoint(tracks, "webb-wall"));
        Assert.IsNull(CaptionTrack.ForViewpoint(tracks, "cushing-canister"),
            "a viewpoint with no caption track must show none, never another film's");
        Assert.IsNull(CaptionTrack.ForViewpoint(tracks, null));
        Assert.IsNull(CaptionTrack.ForViewpoint(null, "webb-wall"));
    }

    [Test]
    public void FileFor_KeepsTheGarnettFileAndNamesPerViewpointFiles()
    {
        Assert.AreEqual("SoldierView/captions.json",
            CaptionTrack.FileFor("garnett-road-to-angle"));
        Assert.AreEqual("SoldierView/captions-webb-wall.json",
            CaptionTrack.FileFor("webb-wall"));
        Assert.AreEqual("SoldierView/captions-cushing-canister.json",
            CaptionTrack.FileFor("cushing-canister"));
    }

    [Test]
    public void CommittedNewViewpointCaptionFiles_ParseAndSitInsideTheirWindows()
    {
        foreach (var (id, t0, t1) in new[]
        {
            ("webb-wall", 8160.0, 8820.5),
            ("cushing-canister", 8400.0, 8760.5),
        })
        {
            string path = UnityEngine.Application.dataPath
                + "/StreamingAssets/" + CaptionTrack.FileFor(id);
            var track = CaptionTrack.FromJson(File.ReadAllText(path));
            Assert.AreEqual(id, track.viewpointId);
            Assert.Greater(track.captions.Length, 0);
            foreach (CaptionDto c in track.captions)
            {
                Assert.GreaterOrEqual(c.t, t0);
                Assert.LessOrEqual(c.t, t1 + 2.5); // + the mix arrival tail
                Assert.IsTrue(c.text.StartsWith("[") && c.text.EndsWith("]"));
            }
        }
    }
}
