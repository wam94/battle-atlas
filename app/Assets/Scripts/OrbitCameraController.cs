using UnityEngine;

namespace BattleAtlas
{
    public class OrbitCameraController : MonoBehaviour
    {
        public Vector3 pivot;
        public float yawDeg;
        public float pitchDeg = 45f;
        public float distance = 4000f;
        public float orbitSpeed = 0.2f;
        public float panSpeed = 1.0f;
        public float tiltSpeed = 0.1f;

        // Phase 11 unit follow: when set, the pivot tracks this provider
        // every frame (the HUD supplies the selected unit's ground anchor).
        // A null RESULT means the target is gone — follow drops. Manual
        // panning is deliberate camera intent and also drops it.
        public System.Func<Vector3?> followPivot;

        void Awake()
        {
            Application.targetFrameRate = 60;
            // battlefield is ~8.5 km across; URP default far plane (1000 m) clips
            // most of it. Near plane raised to keep depth precision at this scale.
            var cam = GetComponent<Camera>();
            if (cam != null)
            {
                cam.nearClipPlane = 10f;
                cam.farClipPlane = 20000f;
            }
        }

        void LateUpdate()
        {
            bool anyTouchOverHud = false;
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (AtlasHud.PointerBusy(Input.GetTouch(i).position))
                {
                    anyTouchOverHud = true;
                    break;
                }
            }

            if (anyTouchOverHud)
            {
                // scrubbing, not flying: leave the camera alone this frame
            }
            else if (Input.touchCount == 1)
            {
                // one finger: pan (drag the map) — deliberate camera intent,
                // so a follow target is released
                Vector2 d = Input.GetTouch(0).deltaPosition;
                if (d.sqrMagnitude > 0f) followPivot = null;
                pivot += OrbitMath.PanWorldDelta(yawDeg, d, distance, panSpeed);
            }
            else if (Input.touchCount == 2)
            {
                Touch t0 = Input.GetTouch(0);
                Touch t1 = Input.GetTouch(1);
                // touch indices aren't stable across frames; pair by fingerId so a
                // transient third touch can't swap fingers and spike the twist 180°
                if (t0.fingerId > t1.fingerId)
                {
                    (t0, t1) = (t1, t0);
                }
                Vector2 p0 = t0.position, p1 = t1.position;
                Vector2 q0 = p0 - t0.deltaPosition, q1 = p1 - t1.deltaPosition;

                // pinch: zoom (guarded against near-zero finger separation)
                float prev = (q0 - q1).magnitude;
                float curr = (p0 - p1).magnitude;
                if (prev > 20f && curr > 20f)
                    distance = OrbitMath.ClampDistance(distance * (prev / curr));

                // twist: rotate. yaw+ spins the scene the same direction as a
                // CCW finger twist; flip the sign here if device feel disagrees.
                yawDeg += OrbitMath.TwistDegrees(q0, q1, p0, p1);

                // two-finger vertical drag: tilt (fingers up = more oblique)
                float avgY = (t0.deltaPosition.y + t1.deltaPosition.y) * 0.5f;
                pitchDeg = OrbitMath.ClampPitch(pitchDeg - avgY * tiltSpeed);
            }

#if UNITY_EDITOR
            // mouse fallback so the editor Play button is usable.
            // PointerBusy replaces the IMGUI hotControl guard: a drag that
            // started on (or is captured by) the retained-mode HUD — or any
            // pointer while Soldier View / a modal owns the screen — must
            // not fly the camera.
            if (!AtlasHud.PointerBusy(Input.mousePosition))
            {
                if (Input.GetMouseButton(0))
                {
                    yawDeg += Input.GetAxis("Mouse X") * 3f;
                    pitchDeg = OrbitMath.ClampPitch(pitchDeg - Input.GetAxis("Mouse Y") * 3f);
                }
                distance = OrbitMath.ClampDistance(
                    distance * (1f - Input.mouseScrollDelta.y * 0.05f));
            }
#endif

            if (followPivot != null)
            {
                Vector3? target = followPivot();
                if (target.HasValue) pivot = target.Value;
                else followPivot = null; // target gone; hold the last pivot
            }

            transform.position = OrbitMath.CameraPosition(pivot, yawDeg, pitchDeg, distance);
            transform.LookAt(pivot);
        }
    }
}
