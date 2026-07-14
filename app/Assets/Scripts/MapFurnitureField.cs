using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BattleAtlas
{
    // Renders the traced map-furniture layer (roads, hydrology, railroad,
    // town-block massing -- data/map-furniture/map-furniture.json,
    // MapFurnitureMeshBuilder) on the Atlas cartography terrain.
    //
    // Static geometry: unlike CropField/FenceField (which rebuild an
    // instance ring as the camera moves) or BattleDirector's per-unit
    // symbols (rebuilt on a dirty predicate), 1863 roads and streams do
    // not move -- the meshes are built ONCE in Start() and simply redrawn
    // every frame via Graphics.RenderMesh (BattleDirector's idiom: no
    // MeshFilter/MeshRenderer GameObjects, one explicit draw call per
    // class layer, shadows off).
    public class MapFurnitureField : MonoBehaviour
    {
        public TextAsset mapFurnitureJson;
        public Material roadMaterial;
        public Material streamMaterial;
        public Material railMaterial;
        public Material townBlockFillMaterial;
        public Material townBlockOutlineMaterial;

        Terrain terrain;
        float terrainBaseY;

        Mesh roadsMesh;
        Mesh streamsMesh;
        Mesh railMesh;
        Mesh townBlocksMesh; // submesh 0 = fill, submesh 1 = outline
        bool built;

        void Start()
        {
            terrain = Terrain.activeTerrain;
            if (terrain == null)
            {
                Debug.LogWarning("MapFurnitureField: no active terrain found; nothing to drape");
                return;
            }
            terrainBaseY = terrain.transform.position.y;

            if (mapFurnitureJson == null)
            {
                Debug.LogWarning("MapFurnitureField: no map-furniture JSON assigned; no roads/streams/town to render");
                return;
            }

            MapFurnitureDto dto = MapFurnitureData.Parse(mapFurnitureJson.text);
            BuildMeshes(dto);
            built = true;
        }

        float GroundY(float x, float z) =>
            terrainBaseY + terrain.SampleHeight(new Vector3(x, 0f, z));

        void BuildMeshes(MapFurnitureDto dto)
        {
            Func<float, float, float> groundY = GroundY;

            var roadVerts = new List<Vector3>(); var roadUvs = new List<Vector2>(); var roadTris = new List<int>();
            var streamVerts = new List<Vector3>(); var streamUvs = new List<Vector2>(); var streamTris = new List<int>();
            var railVerts = new List<Vector3>(); var railUvs = new List<Vector2>(); var railTris = new List<int>();
            var blockVerts = new List<Vector3>(); var blockUvs = new List<Vector2>();
            var blockFillTris = new List<int>(); var blockOutlineTris = new List<int>();

            foreach (MapFurnitureFeatureDto f in dto.features)
            {
                switch (f.cls)
                {
                    case "pike":
                    case "road":
                    case "lane":
                    {
                        List<Vector2> resampled = MapFurnitureMeshBuilder.Resample(
                            f.points, MapFurnitureMeshBuilder.MaxSegmentLengthM);
                        MapFurnitureMeshBuilder.AppendPolyline(
                            resampled, MapFurnitureMeshBuilder.RoadWidthFor(f.cls),
                            MapFurnitureMeshBuilder.RoadLiftM, dashed: false, groundY,
                            roadVerts, roadUvs, roadTris);
                        break;
                    }
                    case "creek":
                    case "run":
                    {
                        List<Vector2> resampled = MapFurnitureMeshBuilder.Resample(
                            f.points, MapFurnitureMeshBuilder.MaxSegmentLengthM);
                        MapFurnitureMeshBuilder.AppendPolyline(
                            resampled, MapFurnitureMeshBuilder.StreamWidthFor(f.cls),
                            MapFurnitureMeshBuilder.StreamLiftM, dashed: false, groundY,
                            streamVerts, streamUvs, streamTris);
                        break;
                    }
                    case "rail_finished":
                    case "rail_unfinished":
                    {
                        List<Vector2> resampled = MapFurnitureMeshBuilder.Resample(
                            f.points, MapFurnitureMeshBuilder.MaxSegmentLengthM);
                        bool dashed = f.cls == "rail_unfinished";
                        MapFurnitureMeshBuilder.AppendPolyline(
                            resampled, MapFurnitureMeshBuilder.RailWidthM,
                            MapFurnitureMeshBuilder.RailLiftM, dashed, groundY,
                            railVerts, railUvs, railTris);
                        break;
                    }
                    case "town_block":
                    {
                        // adjacent traced clusters share an edge; inset the
                        // RENDERED footprint so the block reads distinct
                        // from its neighbor instead of one solid mass (see
                        // MapFurnitureMeshBuilder.TownBlockInsetM)
                        List<Vector2> inset = MapFurnitureMeshBuilder.InsetTowardCentroid(
                            f.points, MapFurnitureMeshBuilder.TownBlockInsetM);
                        MapFurnitureMeshBuilder.AppendPolygonFill(
                            inset, MapFurnitureMeshBuilder.TownBlockLiftM, groundY,
                            blockVerts, blockUvs, blockFillTris);
                        MapFurnitureMeshBuilder.AppendPolygonOutline(
                            inset, MapFurnitureMeshBuilder.TownBlockOutlineWidthM,
                            MapFurnitureMeshBuilder.TownBlockLiftM, groundY,
                            blockVerts, blockUvs, blockOutlineTris);
                        break;
                    }
                    default:
                        Debug.LogWarning($"MapFurnitureField: unknown feature class '{f.cls}' on '{f.id}', skipped");
                        break;
                }
            }

            roadsMesh = MakeMesh("MapFurniture Roads", roadVerts, roadUvs, roadTris);
            streamsMesh = MakeMesh("MapFurniture Streams", streamVerts, streamUvs, streamTris);
            railMesh = MakeMesh("MapFurniture Railroad", railVerts, railUvs, railTris);
            townBlocksMesh = MakeSubmeshMesh(
                "MapFurniture TownBlocks", blockVerts, blockUvs, blockFillTris, blockOutlineTris);
        }

        static Mesh MakeMesh(string name, List<Vector3> verts, List<Vector2> uvs, List<int> tris)
        {
            var mesh = new Mesh { name = name };
            if (verts.Count == 0) return mesh;
            // static, built once -- 32-bit indices cost a few KB and remove
            // any risk of the combined-class buffer crossing the 16-bit cap
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        static Mesh MakeSubmeshMesh(
            string name, List<Vector3> verts, List<Vector2> uvs,
            List<int> fillTris, List<int> outlineTris)
        {
            var mesh = new Mesh { name = name };
            if (verts.Count == 0) return mesh;
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.subMeshCount = 2;
            mesh.SetTriangles(fillTris, 0);
            mesh.SetTriangles(outlineTris, 1);
            mesh.RecalculateBounds();
            return mesh;
        }

        void Update()
        {
            if (!built) return;
            DrawIfNotEmpty(roadsMesh, roadMaterial, 0);
            DrawIfNotEmpty(streamsMesh, streamMaterial, 0);
            DrawIfNotEmpty(railMesh, railMaterial, 0);
            DrawIfNotEmpty(townBlocksMesh, townBlockFillMaterial, 0);
            DrawIfNotEmpty(townBlocksMesh, townBlockOutlineMaterial, 1);
        }

        static void DrawIfNotEmpty(Mesh mesh, Material material, int submesh)
        {
            if (mesh == null || mesh.vertexCount == 0 || material == null) return;
            var rp = new RenderParams(material)
            {
                shadowCastingMode = ShadowCastingMode.Off,
                worldBounds = mesh.bounds,
            };
            Graphics.RenderMesh(rp, mesh, submesh, Matrix4x4.identity);
        }
    }
}
