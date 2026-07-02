using System.Collections.Generic;
using UnityEngine;

namespace BattleAtlas
{
    // Loads the battle JSON, spawns a block marker per unit, and poses every
    // marker each frame from the clock's current time. Beyond BlocksOutDist
    // the block renders as before; inside SoldiersInDist it hides and an
    // instanced UnitFormationRenderer draws the unit as soldier ranks instead
    // (with hysteresis between the two thresholds to avoid flicker).
    public class BattleDirector : MonoBehaviour
    {
        public TextAsset battleJson;
        public Terrain terrain;
        public BattleClock clock;
        // a real material ASSET (not a runtime instance): asset references keep
        // the shader in device builds, where runtime-created materials render
        // magenta because the shader was stripped
        public Material unitMaterial;
        // material for instanced soldier figures; falls back to unitMaterial
        // when left unset in the inspector (e.g. older scenes)
        public Material soldierMaterial;

        // visible clearance of the block's top face above the highest ground in
        // its footprint (the block extends down to the lowest ground, embedding
        // in the hillside like a piece on a physical relief map)
        const float MarkerHeight = 6f;
        // LOD hysteresis: soldiers resolve in below SoldiersInDist, blocks
        // resolve back in beyond BlocksOutDist; the 150m band between them
        // prevents flicker when the camera hovers near the boundary
        const float SoldiersInDist = 1500f;
        const float BlocksOutDist = 1650f;
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        // corners first (order is load-bearing for tests), then edge midpoints —
        // a denser ring catches ground rising inside the footprint, not just at
        // its extremes
        static readonly (float dx, float dz)[] CornerOffsets =
        {
            (-0.5f, -0.5f), (0.5f, -0.5f), (-0.5f, 0.5f), (0.5f, 0.5f),
            (0f, -0.5f), (0f, 0.5f), (-0.5f, 0f), (0.5f, 0f),
        };

        // per-unit runtime state: the block marker plus everything needed to
        // switch it for instanced soldier ranks at close range. A small class
        // beats a growing tuple once it carries a latch and a renderer.
        class UnitEntry
        {
            public UnitTrack Track;
            public Transform Marker;
            public UnitFormationRenderer FormationRenderer;
            public bool SoldiersVisible;
        }

        readonly List<UnitEntry> units = new();
        readonly Vector2[] samplePoints = new Vector2[9];
        Mesh soldierMesh;
        Camera lodCamera;

        public static Color SideColor(string side)
        {
            switch (side)
            {
                case "union": return new Color(0.23f, 0.35f, 0.61f);       // deep blue
                case "confederate": return new Color(0.63f, 0.31f, 0.31f); // muted red
                default:
                    // gray keeps rendering but a typo'd side in authored data
                    // should be loud, not invisible
                    Debug.LogWarning($"unknown unit side '{side}', rendering gray");
                    return Color.gray;
            }
        }

        // Fills the buffer with world-XZ sample points under a unit: center,
        // footprint corners, and edge midpoints, rotated by facing. Update
        // stretches the block from the MIN to the MAX ground height of these,
        // embedding it in the slope.
        public static void FootprintSamplePoints(
            Vector2 centerXZ, float facingDeg, float frontage, float depth, Vector2[] buffer)
        {
            var rot = Quaternion.Euler(0f, facingDeg, 0f);
            buffer[0] = centerXZ;
            for (int i = 0; i < CornerOffsets.Length; i++)
            {
                Vector3 off = rot * new Vector3(
                    CornerOffsets[i].dx * frontage, 0f, CornerOffsets[i].dz * depth);
                buffer[i + 1] = new Vector2(centerXZ.x + off.x, centerXZ.y + off.z);
            }
        }

        // groundY: world-space terrain height under the unit. Marker pivot is
        // its center, so lift by half its height to sit on the ground.
        public static (Vector3 pos, Quaternion rot) MarkerPose(
            UnitState state, float groundY, float markerHeight)
        {
            var pos = new Vector3(state.posXZ.x, groundY + markerHeight / 2f, state.posXZ.y);
            // compass facing (0 = north = +Z) maps directly to Unity yaw
            var rot = Quaternion.Euler(0f, state.facingDeg, 0f);
            return (pos, rot);
        }

        void Start()
        {
            if (terrain == null)
            {
                // terrain re-imports replace the scene object and orphan the
                // serialized reference; fall back rather than NRE every frame
                terrain = Terrain.activeTerrain;
                Debug.LogWarning("BattleDirector.terrain was unset; using Terrain.activeTerrain");
            }
            if (soldierMaterial == null)
            {
                soldierMaterial = unitMaterial;
            }
            // asset reference keeps the shader in device builds; the
            // instancing flag itself is not stripped, so it's safe to set here
            soldierMaterial.enableInstancing = true;
            soldierMesh = InstancedMeshes.BuildSoldier();
            lodCamera = Camera.main;

            BattleDto battle = BattleLoader.Parse(battleJson.text);
            clock.EndTime = battle.endTime;
            clock.StartTime = battle.startTime;
            foreach (UnitDto u in battle.units)
            {
                var marker = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                marker.name = $"unit {u.id} ({u.name})";
                marker.localScale = new Vector3(u.frontage_m, MarkerHeight, u.depth_m); // y overwritten per-frame
                var renderer = marker.GetComponent<Renderer>();
                renderer.sharedMaterial = unitMaterial;
                var block = new MaterialPropertyBlock();
                block.SetColor(BaseColorId, SideColor(u.side));
                renderer.SetPropertyBlock(block);
                Object.Destroy(marker.GetComponent<Collider>()); // not clickable yet

                var formationRenderer = new UnitFormationRenderer(
                    u.id, u.frontage_m, u.depth_m, soldierMesh, soldierMaterial, SideColor(u.side));
                units.Add(new UnitEntry
                {
                    Track = new UnitTrack(u),
                    Marker = marker,
                    FormationRenderer = formationRenderer,
                    SoldiersVisible = false,
                });
            }
        }

        void Update()
        {
            float baseY = terrain.transform.position.y;
            foreach (UnitEntry entry in units)
            {
                UnitTrack track = entry.Track;
                Transform marker = entry.Marker;
                UnitState s = track.StateAt(clock.CurrentTime);

                // one representative height sample (unit center) is enough to
                // judge camera distance; the block path does its own denser
                // footprint sampling only when it's actually the one rendering
                float centerY = baseY + terrain.SampleHeight(new Vector3(s.posXZ.x, 0f, s.posXZ.y));
                Vector3 worldPos = new Vector3(s.posXZ.x, centerY, s.posXZ.y);
                // no camera (editor edge case): treat everything as far so the
                // familiar block path keeps working
                float dist = lodCamera != null
                    ? Vector3.Distance(lodCamera.transform.position, worldPos)
                    : float.MaxValue;
                entry.SoldiersVisible = dist < SoldiersInDist
                    || (entry.SoldiersVisible && dist < BlocksOutDist);

                if (entry.SoldiersVisible)
                {
                    marker.gameObject.SetActive(false);
                    entry.FormationRenderer.Render(s,
                        (x, z) => terrain.SampleHeight(new Vector3(x, 0f, z)) + baseY);
                    continue;
                }

                marker.gameObject.SetActive(true);
                FootprintSamplePoints(s.posXZ, s.facingDeg,
                    track.Unit.frontage_m, track.Unit.depth_m, samplePoints);
                float minY = float.MaxValue, maxY = float.MinValue;
                foreach (Vector2 p in samplePoints)
                {
                    float y = terrain.SampleHeight(new Vector3(p.x, 0f, p.y));
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
                // a rigid slab can't lie on ~20 m of relief: stretch the block
                // from the lowest ground under it to MarkerHeight above the
                // highest, so it embeds in the slope instead of floating or
                // letting the crest poke through its top
                float blockHeight = (maxY - minY) + MarkerHeight;
                Vector3 scale = marker.localScale;
                scale.y = blockHeight;
                marker.localScale = scale;
                var (pos, rot) = MarkerPose(s, baseY + minY, blockHeight);
                marker.SetPositionAndRotation(pos, rot);
            }
        }
    }
}
