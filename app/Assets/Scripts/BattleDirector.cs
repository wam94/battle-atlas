using System.Collections.Generic;
using UnityEngine;

namespace BattleAtlas
{
    // Loads the battle JSON, spawns a block marker per unit, and poses every
    // marker each frame from the clock's current time. Simple primitives for
    // this phase; instanced formation rendering arrives with the zoom ladder.
    public class BattleDirector : MonoBehaviour
    {
        public TextAsset battleJson;
        public Terrain terrain;
        public BattleClock clock;
        // a real material ASSET (not a runtime instance): asset references keep
        // the shader in device builds, where runtime-created materials render
        // magenta because the shader was stripped
        public Material unitMaterial;

        const float MarkerHeight = 3f;
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        // corners first (order is load-bearing for tests), then edge midpoints —
        // a denser ring catches ground rising inside the footprint, not just at
        // its extremes
        static readonly (float dx, float dz)[] CornerOffsets =
        {
            (-0.5f, -0.5f), (0.5f, -0.5f), (-0.5f, 0.5f), (0.5f, 0.5f),
            (0f, -0.5f), (0f, 0.5f), (-0.5f, 0f), (0.5f, 0f),
        };

        readonly List<(UnitTrack track, Transform marker)> units = new();
        readonly Vector2[] samplePoints = new Vector2[9];

        public static Color SideColor(string side) => side switch
        {
            "union" => new Color(0.23f, 0.35f, 0.61f),       // deep blue
            "confederate" => new Color(0.63f, 0.31f, 0.31f), // muted red
            _ => Color.gray,
        };

        // Fills the buffer with world-XZ sample points under a unit: center,
        // footprint corners, and edge midpoints, rotated by facing. The marker
        // sits on the MAX ground height of these so rising ground inside the
        // footprint can't poke through the block.
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
            BattleDto battle = BattleLoader.Parse(battleJson.text);
            clock.EndTime = battle.endTime;
            foreach (UnitDto u in battle.units)
            {
                var marker = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                marker.name = $"unit {u.id} ({u.name})";
                marker.localScale = new Vector3(u.frontage_m, MarkerHeight, u.depth_m);
                var renderer = marker.GetComponent<Renderer>();
                renderer.sharedMaterial = unitMaterial;
                var block = new MaterialPropertyBlock();
                block.SetColor(BaseColorId, SideColor(u.side));
                renderer.SetPropertyBlock(block);
                Object.Destroy(marker.GetComponent<Collider>()); // not clickable yet
                units.Add((new UnitTrack(u), marker));
            }
        }

        void Update()
        {
            foreach (var (track, marker) in units)
            {
                UnitState s = track.StateAt(clock.CurrentTime);
                FootprintSamplePoints(s.posXZ, s.facingDeg,
                    track.Unit.frontage_m, track.Unit.depth_m, samplePoints);
                float groundY = float.MinValue;
                foreach (Vector2 p in samplePoints)
                {
                    float y = terrain.SampleHeight(new Vector3(p.x, 0f, p.y));
                    if (y > groundY) groundY = y;
                }
                groundY += terrain.transform.position.y;
                var (pos, rot) = MarkerPose(s, groundY, MarkerHeight);
                marker.SetPositionAndRotation(pos, rot);
            }
        }
    }
}
