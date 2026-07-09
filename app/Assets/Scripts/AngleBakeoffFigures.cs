using UnityEngine;

namespace BattleAtlas
{
    // Issues the bake-off's dynamic instanced draws (100 soldiers + smoke
    // puffs) through Graphics.RenderMeshInstanced — deliberately the SAME
    // draw path the Atlas uses for units, because Phase 3 must verify that
    // path under HDRP (plan §12 Phase 3 default-decision criteria).
    //
    // Matrices are built once from AngleBakeoffLayout + the staged terrain
    // (deterministic; rebuild-safe). Materials are injected by the staging
    // code because they are pipeline-specific (URP/Lit vs HDRP/Lit).
    // ExecuteAlways so an owner opening the staged scene sees figures in the
    // editor; the headless render script calls IssueDraws() explicitly
    // before each render request submission.
    [ExecuteAlways]
    public class AngleBakeoffFigures : MonoBehaviour
    {
        public Material csaMaterial;
        public Material usaMaterial;
        public Material smokeMaterial;
        public Terrain terrain;

        Mesh csaMesh;    // advancing pose
        Mesh usaMesh;    // standing pose
        Mesh csaHeadMesh; // head + kepi overlay (see HeadMesh)
        Mesh usaHeadMesh;
        Mesh puffMesh;
        Matrix4x4[] csaMatrices;
        Matrix4x4[] usaMatrices;
        Matrix4x4[] puffMatrices;
        Material headMaterial;

        public void Build()
        {
            if (terrain == null) terrain = Terrain.activeTerrain;
            if (terrain == null) return;

            csaMesh = InstancedMeshes.BuildSoldierAdvancing();
            usaMesh = InstancedMeshes.BuildSoldier();
            // pipeline-default Lit shaders ignore the figures' vertex-color
            // bands, so the dark kepi cue is re-added as a second instanced
            // pass over the head/hat volume (same matrices)
            csaHeadMesh = HeadMesh(1.52f, 0.34f);
            usaHeadMesh = HeadMesh(1.55f, 0f);
            if (headMaterial == null && csaMaterial != null)
            {
                headMaterial = new Material(csaMaterial)
                {
                    color = new Color(0.16f, 0.15f, 0.16f),
                };
                headMaterial.SetColor("_BaseColor", new Color(0.16f, 0.15f, 0.16f));
            }
            puffMesh = InstancedMeshes.BuildPuff();

            csaMatrices = SoldierMatrices(AngleBakeoffLayout.CsaSoldiers());
            usaMatrices = SoldierMatrices(AngleBakeoffLayout.UsaSoldiers());

            var puffs = AngleBakeoffLayout.SmokePuffs();
            puffMatrices = new Matrix4x4[puffs.Count];
            for (int i = 0; i < puffs.Count; i++)
            {
                var (pos, radius) = puffs[i];
                float y = Ground(pos) + radius * 0.55f;
                puffMatrices[i] = Matrix4x4.TRS(
                    new Vector3(pos.x, y, pos.y),
                    Quaternion.Euler(0f, 90f * FormationLayout.Jitter("puff-yaw", i, 7), 0f),
                    Vector3.one * radius);
            }
        }

        Matrix4x4[] SoldierMatrices(
            System.Collections.Generic.List<(Vector2 pos, float facingDeg)> soldiers)
        {
            var m = new Matrix4x4[soldiers.Count];
            for (int i = 0; i < soldiers.Count; i++)
            {
                var (pos, facing) = soldiers[i];
                m[i] = Matrix4x4.TRS(
                    new Vector3(pos.x, Ground(pos), pos.y),
                    Quaternion.Euler(0f, facing, 0f),
                    Vector3.one);
            }
            return m;
        }

        float Ground(Vector2 xz) =>
            terrain.transform.position.y +
            terrain.SampleHeight(new Vector3(xz.x, 0f, xz.y));

        void Update() => IssueDraws();

        // Headless render requests never tick Update, and draws issued
        // before the request are not part of the request's frame — so also
        // issue during beginCameraRendering, which fires inside the SRP
        // render loop for every camera (URP and HDRP alike).
        void OnEnable() =>
            UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering += OnBeginCamera;

        void OnDisable() =>
            UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= OnBeginCamera;

        void OnBeginCamera(
            UnityEngine.Rendering.ScriptableRenderContext ctx, Camera cam) => IssueDraws();

        // One frame's worth of instanced draws. Safe to call multiple times
        // per frame (the render script calls it immediately before each
        // SubmitRenderRequest so the draws exist in that frame).
        public void IssueDraws()
        {
            if (csaMatrices == null || csaMatrices.Length == 0) Build();
            if (csaMatrices == null) return; // no terrain yet

            Draw(csaMaterial, csaMesh, csaMatrices, true);
            Draw(usaMaterial, usaMesh, usaMatrices, true);
            Draw(headMaterial, csaHeadMesh, csaMatrices, true);
            Draw(headMaterial, usaHeadMesh, usaMatrices, true);
            Draw(smokeMaterial, puffMesh, puffMatrices, false);
        }

        // hat brim + crown over the figure's head position (pose-specific
        // headY/forward). Slightly larger than the underlying head/hat boxes
        // so it overlays them cleanly.
        static Mesh HeadMesh(float headY, float forward)
        {
            var verts = new System.Collections.Generic.List<Vector3>();
            var tris = new System.Collections.Generic.List<int>();
            var m = new Mesh { name = "BakeoffHead" };
            AddBoxLocal(verts, tris, new Vector3(0f, headY + 0.115f, forward + 0.02f),
                new Vector3(0.29f, 0.13f, 0.29f));
            AddBoxLocal(verts, tris, new Vector3(0f, headY, forward),
                new Vector3(0.25f, 0.25f, 0.25f));
            m.SetVertices(verts);
            m.SetTriangles(tris, 0);
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }

        static void AddBoxLocal(
            System.Collections.Generic.List<Vector3> verts,
            System.Collections.Generic.List<int> tris, Vector3 c, Vector3 s)
        {
            int b = verts.Count;
            Vector3 h = s * 0.5f;
            verts.Add(c + new Vector3(-h.x, -h.y, -h.z));
            verts.Add(c + new Vector3(h.x, -h.y, -h.z));
            verts.Add(c + new Vector3(h.x, -h.y, h.z));
            verts.Add(c + new Vector3(-h.x, -h.y, h.z));
            verts.Add(c + new Vector3(-h.x, h.y, -h.z));
            verts.Add(c + new Vector3(h.x, h.y, -h.z));
            verts.Add(c + new Vector3(h.x, h.y, h.z));
            verts.Add(c + new Vector3(-h.x, h.y, h.z));
            int[] quads = { 0,1,5,4, 1,2,6,5, 2,3,7,6, 3,0,4,7, 4,5,6,7, 3,2,1,0 };
            for (int q = 0; q < quads.Length; q += 4)
            {
                tris.Add(b + quads[q]); tris.Add(b + quads[q + 2]); tris.Add(b + quads[q + 1]);
                tris.Add(b + quads[q]); tris.Add(b + quads[q + 3]); tris.Add(b + quads[q + 2]);
            }
        }

        static void Draw(Material mat, Mesh mesh, Matrix4x4[] matrices, bool shadows)
        {
            if (mat == null || mesh == null || matrices == null || matrices.Length == 0)
                return;
            var rp = new RenderParams(mat)
            {
                shadowCastingMode = shadows
                    ? UnityEngine.Rendering.ShadowCastingMode.On
                    : UnityEngine.Rendering.ShadowCastingMode.Off,
                receiveShadows = true,
                worldBounds = new Bounds(
                    new Vector3(400f, 200f, 400f), new Vector3(900f, 500f, 900f)),
            };
            Graphics.RenderMeshInstanced(rp, mesh, 0, matrices);
        }
    }
}
