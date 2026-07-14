using System.IO;
using BattleAtlas;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace BattleAtlas.EditorTools
{
    // Imports the traced map-furniture layer (data/map-furniture/
    // map-furniture.json -- roads, hydrology, town-block massing, railroad;
    // docs/format/map-furniture-format.md) onto the Atlas terrain: copies
    // the committed JSON into Assets/Generated (TextAssets must live under
    // Assets -- same idempotent File.Copy pattern LandcoverImporter uses
    // for trees.json/fences.json), creates the five muted-ink material
    // assets, and wires a MapFurnitureField component.
    //
    // Separate from LandcoverImporter on purpose: landcover bakes into the
    // terrain's splatmap/TerrainLayers (an area/texture concern this data
    // never touches), while map furniture is drawn as its own vector mesh
    // layer. Both are called from CartographyStage.PrepareScene.
    public static class MapFurnitureImporter
    {
        const string GeneratedDir = "Assets/Generated";

        // Muted, documentary ink palette -- matches LandcoverImporter's
        // hex-literal convention (docs/superpowers/plans/2026-07-01-
        // landcover.md's "sand-table palette"). Roads/rail/streams/blocks
        // sit BELOW unit ribbons and labels by construction
        // (MapFurnitureMeshBuilder's lift constants), not by draw order,
        // so these colors only need to read against the terrain, not
        // fight the symbol grammar.
        static readonly Color RoadColor = HexColor("#5C4630");
        static readonly Color StreamColor = HexColor("#4A6B7A");
        static readonly Color RailColor = HexColor("#302A26");
        static readonly Color TownBlockFillColor = HexColor("#B7A788");
        static readonly Color TownBlockOutlineColor = HexColor("#6B5A42");

        static Color HexColor(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color c);
            return c;
        }

        [MenuItem("BattleAtlas/Import Map Furniture")]
        public static void Import()
        {
            var terrainGo = GameObject.Find("Gettysburg Terrain");
            if (terrainGo == null)
            {
                Debug.LogError(
                    "MapFurnitureImporter: no 'Gettysburg Terrain' GameObject in the scene; " +
                    "run BattleAtlas/Import Heightmap first. Aborting.");
                return;
            }

            string dir = Path.GetFullPath(
                Path.Combine(Application.dataPath, "../../data/map-furniture"));
            string jsonSrc = Path.Combine(dir, "map-furniture.json");
            if (!File.Exists(jsonSrc))
            {
                Debug.LogError(
                    $"MapFurnitureImporter: {jsonSrc} not found; run " +
                    "reconstruction/scripts/trace_map_furniture.py first. Aborting.");
                return;
            }

            var field = terrainGo.GetComponent<MapFurnitureField>();
            if (field == null) field = terrainGo.AddComponent<MapFurnitureField>();

            field.mapFurnitureJson = CopyJsonAsset(jsonSrc, "MapFurniture.json");
            field.roadMaterial = CreateUnlitMaterial("MapFurnitureRoadInk", RoadColor);
            field.streamMaterial = CreateUnlitMaterial("MapFurnitureStreamInk", StreamColor);
            field.railMaterial = CreateUnlitMaterial("MapFurnitureRailInk", RailColor);
            field.townBlockFillMaterial = CreateUnlitMaterial("MapFurnitureTownBlockFill", TownBlockFillColor);
            field.townBlockOutlineMaterial = CreateUnlitMaterial("MapFurnitureTownBlockOutline", TownBlockOutlineColor);

            EditorUtility.SetDirty(field);
            EditorSceneManager.MarkSceneDirty(terrainGo.scene);
            AssetDatabase.SaveAssets();
            Debug.Log("MapFurnitureImporter: map-furniture.json imported and wired onto " +
                "'Gettysburg Terrain' (MapFurnitureField).");
        }

        static TextAsset CopyJsonAsset(string sourcePath, string assetFileName)
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "Generated"));
            string assetPath = $"{GeneratedDir}/{assetFileName}";
            string destPath = Path.Combine(Application.dataPath, "Generated", assetFileName);
            File.Copy(sourcePath, destPath, overwrite: true);
            AssetDatabase.ImportAsset(assetPath);
            return AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
        }

        // Idempotent: a stock HDRP Unlit material with a fixed base color --
        // no textures, no runtime material creation (the ink lives in an
        // asset, matching the cartography slice's "material ASSETS, no
        // runtime materials" rule). The project's active SRP is HDRP (see
        // ProjectSettings/GraphicsSettings.asset's m_CustomRenderPipeline;
        // the rest of the Angle/Atlas tooling already targets "HDRP/Lit" /
        // "HDRP/Unlit" -- AngleActionStage.cs's flashMat is the precedent
        // for _UnlitColor + HDMaterial.ValidateMaterial). Re-running the
        // importer replaces the asset in place rather than accumulating
        // duplicates.
        static Material CreateUnlitMaterial(string assetName, Color color)
        {
            string assetPath = $"{GeneratedDir}/{assetName}.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(assetPath) != null)
                AssetDatabase.DeleteAsset(assetPath);

            Shader shader = Shader.Find("HDRP/Unlit");
            if (shader == null)
                throw new System.InvalidOperationException(
                    "MapFurnitureImporter: shader 'HDRP/Unlit' not found -- " +
                    "is the HDRP package installed?");

            var material = new Material(shader) { name = assetName };
            material.SetColor("_UnlitColor", color);
            material.enableInstancing = true;
            // opaque is the shader default (_SurfaceType 0); ValidateMaterial
            // sets up the keywords/render queue HDRP's material inspector
            // would otherwise configure for us.
            HDMaterial.ValidateMaterial(material);

            Directory.CreateDirectory(Path.Combine(Application.dataPath, "Generated"));
            AssetDatabase.CreateAsset(material, assetPath);
            return material;
        }
    }
}
