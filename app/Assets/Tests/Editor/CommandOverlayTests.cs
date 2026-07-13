using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using BattleAtlas;

// The command overlay (cartography slice 1): unit id -> corps/division
// group tables feeding the theater/mid aggregate labels. The generated
// asset itself is pinned too — the overlay must cover the shipped battle.
public class CommandOverlayTests
{
    static readonly Dictionary<string, bool> Sides = new()
    {
        { "us-webb", true },
        { "us-hall", true },
        { "csa-garnett", false },
        { "csa-kemper", false },
        { "us-btty-cushing", true },
    };

    static CommandOverlayDoc Doc(params (string id, string corps, string division)[] rows)
    {
        var doc = new CommandOverlayDoc { units = new List<CommandOverlayUnitDto>() };
        foreach (var (id, corps, division) in rows)
            doc.units.Add(new CommandOverlayUnitDto
            {
                id = id, corps = corps, division = division,
            });
        return doc;
    }

    static bool? SideOf(string id) =>
        Sides.TryGetValue(id, out bool u) ? u : (bool?)null;

    [Test]
    public void Build_GroupsInFirstAppearanceOrderUppercased()
    {
        var groups = CommandGroups.Build(Doc(
            ("csa-garnett", "First Corps", "Pickett's Division"),
            ("csa-kemper", "First Corps", "Pickett's Division"),
            ("us-webb", "II Corps", "2nd Division, II Corps"),
            ("us-hall", "II Corps", "2nd Division, II Corps")),
            SideOf);
        Assert.AreEqual(2, groups.CorpsCount);
        Assert.AreEqual(2, groups.DivisionCount);
        // display register: UPPERCASE, applied once at build time
        Assert.AreEqual("FIRST CORPS", groups.CorpsLabels[0]);
        Assert.AreEqual("II CORPS", groups.CorpsLabels[1]);
        Assert.AreEqual("PICKETT'S DIVISION", groups.DivisionLabels[0]);
        Assert.IsFalse(groups.CorpsIsUnion[0]);
        Assert.IsTrue(groups.CorpsIsUnion[1]);
        Assert.AreEqual((0, 0), groups.GroupsOf("csa-kemper"));
        Assert.AreEqual((1, 1), groups.GroupsOf("us-hall"));
    }

    [Test]
    public void Build_CorpsDirectUnitJoinsNoDivision()
    {
        var groups = CommandGroups.Build(Doc(
            ("us-btty-cushing", "II Corps", "")), SideOf);
        Assert.AreEqual(1, groups.CorpsCount);
        Assert.AreEqual(0, groups.DivisionCount);
        Assert.AreEqual((0, -1), groups.GroupsOf("us-btty-cushing"));
    }

    [Test]
    public void Build_UnknownUnitsResolveToNoGroups()
    {
        var groups = CommandGroups.Build(Doc(
            ("us-webb", "II Corps", "2nd Division, II Corps"),
            // the overlay knows a unit the battle doesn't: skipped, loudly
            // absent from the tables rather than guessing a side
            ("us-ghost", "II Corps", "2nd Division, II Corps")), SideOf);
        Assert.AreEqual((-1, -1), groups.GroupsOf("us-ghost"));
        Assert.AreEqual((-1, -1), groups.GroupsOf("never-heard-of-it"));
        Assert.AreEqual((-1, -1), groups.GroupsOf(null));
        // and a null/empty doc builds an empty, harmless table
        Assert.AreEqual(0, CommandGroups.Build(null, SideOf).CorpsCount);
        Assert.AreEqual(0,
            CommandGroups.Build(new CommandOverlayDoc(), SideOf).CorpsCount);
    }

    [Test]
    public void GeneratedOverlay_CoversTheShippedBattle()
    {
        // the committed Resources asset must map every parentless unit of
        // the shipped battle to a corps (children inherit through their
        // family's parent at render time; the generator emits them too)
        var overlayAsset = Resources.Load<TextAsset>("Battle/command-overlay");
        Assert.IsNotNull(overlayAsset,
            "Resources/Battle/command-overlay.json missing — run "
            + "scripts/gen-command-overlay.py");
        var doc = JsonUtility.FromJson<CommandOverlayDoc>(overlayAsset.text);
        // every reconstructed phase file's units must be covered (ADR 0005:
        // one file per phase; the generator scans them all)
        string[] battleFiles =
        {
            "gettysburg-july3.json",
            "gettysburg-july2-afternoon.json",
            "gettysburg-july2-evening.json",
            "gettysburg-july1-morning.json",
            "gettysburg-july1-afternoon.json",
        };
        foreach (string file in battleFiles)
        {
            var battle = BattleLoader.Parse(System.IO.File.ReadAllText(
                Application.dataPath + "/Battle/" + file));
            var sides = new Dictionary<string, bool>();
            foreach (UnitDto u in battle.units) sides[u.id] = u.side == "union";
            var groups = CommandGroups.Build(doc,
                id => sides.TryGetValue(id, out bool isU) ? isU : (bool?)null);
            foreach (UnitDto u in battle.units)
            {
                (int corps, int _) = groups.GroupsOf(u.id);
                Assert.GreaterOrEqual(corps, 0,
                    $"unit '{u.id}' ({file}) has no corps in the command overlay");
            }
            // each field reads as a dozen-ish corps words, not a wall
            Assert.LessOrEqual(groups.CorpsCount, 16);
            Assert.GreaterOrEqual(groups.CorpsCount, 8);
        }
    }
}
