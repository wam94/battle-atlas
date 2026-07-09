using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// Pins the symbol shader/material contract the HdrpMigrationTests way
// (asset exists, wired, property surface intact): the committed material
// ASSET must reference the house symbol shader — runtime-created
// materials render magenta in device builds (the stripping lesson) — and
// the shader must expose exactly the MPB surface BattleDirector writes
// per unit (_BaseColor fill, _FillStyle provenance, _BorderWeight
// echelon). Visual verdicts (does the hatch read, does the double border
// read) ride the owner's editor session, not this suite.
public class SymbolMaterialTests
{
    const string MaterialPath = "Assets/Battle/UnitSymbol.mat";
    const string ShaderName = "BattleAtlas/UnitSymbol";

    [Test]
    public void MaterialAsset_ExistsAndReferencesTheSymbolShader()
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        Assert.IsNotNull(mat, MaterialPath + " missing");
        Assert.IsNotNull(mat.shader, "material lost its shader reference");
        Assert.AreEqual(ShaderName, mat.shader.name);
        // committed defaults: white fill (the MPB overrides per unit),
        // documented-solid style, full-weight border
        Assert.AreEqual(Color.white, mat.GetColor("_BaseColor"));
        Assert.AreEqual(0f, mat.GetFloat("_FillStyle"));
        Assert.AreEqual(1f, mat.GetFloat("_BorderWeight"));
    }

    [Test]
    public void Shader_ExposesTheMpbSurfaceAndHdrpForwardOnlyPass()
    {
        var shader = Shader.Find(ShaderName);
        Assert.IsNotNull(shader, ShaderName + " missing");
        foreach (string prop in new[] { "_BaseColor", "_FillStyle", "_BorderWeight" })
            Assert.GreaterOrEqual(shader.FindPropertyIndex(prop), 0,
                $"{ShaderName} must expose {prop} — BattleDirector writes it per unit");
        // with HDRP active, the RenderPipeline-tagged SubShader wins and
        // its single pass is ForwardOnly (the CustomShaders_ pin's rule)
        var mat = new Material(shader);
        Assert.AreEqual(1, mat.passCount, "one HDRP pass expected");
        Assert.AreEqual("ForwardOnly", mat.GetPassName(0),
            "the HDRP SubShader must be the active one");
        Object.DestroyImmediate(mat);
    }

    [Test]
    public void AtlasScene_WiresSymbolMaterialOnTheDirector()
    {
        // text-level assert so the suite never has to open (and disturb)
        // the scene. Ratchet: before the editor re-serializes the scene
        // with the new field this ignores loudly; once the field exists it
        // must reference the committed material asset — an unset slot
        // renders via the unitMaterial fallback, minus every cartographic
        // cue this plan exists to add.
        string scene = File.ReadAllText("Assets/Scenes/Atlas.unity");
        if (!scene.Contains("symbolMaterial:"))
            Assert.Ignore("Atlas.unity not yet re-saved with the symbolMaterial " +
                "field — owner session: wire Assets/Battle/UnitSymbol.mat onto " +
                "BattleDirector and save the scene");
        string guid = AssetDatabase.AssetPathToGUID(MaterialPath);
        Assert.IsNotEmpty(guid, MaterialPath + " missing");
        StringAssert.Contains($"symbolMaterial: {{fileID: 2100000, guid: {guid}",
            scene, "BattleDirector.symbolMaterial must reference " + MaterialPath);
    }
}
