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

        const float MarkerHeight = 3f;

        readonly List<(UnitTrack track, Transform marker)> units = new();

        public static Color SideColor(string side) => side switch
        {
            "union" => new Color(0.23f, 0.35f, 0.61f),       // deep blue
            "confederate" => new Color(0.63f, 0.31f, 0.31f), // muted red
            _ => Color.gray,
        };

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
                marker.GetComponent<Renderer>().material.color = SideColor(u.side);
                Object.Destroy(marker.GetComponent<Collider>()); // not clickable yet
                units.Add((new UnitTrack(u), marker));
            }
        }

        void Update()
        {
            foreach (var (track, marker) in units)
            {
                UnitState s = track.StateAt(clock.CurrentTime);
                float groundY = terrain.SampleHeight(
                    new Vector3(s.posXZ.x, 0f, s.posXZ.y)) + terrain.transform.position.y;
                var (pos, rot) = MarkerPose(s, groundY, MarkerHeight);
                marker.SetPositionAndRotation(pos, rot);
            }
        }
    }
}
