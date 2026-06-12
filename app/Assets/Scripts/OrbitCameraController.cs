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
            if (Input.touchCount == 1)
            {
                Vector2 d = Input.GetTouch(0).deltaPosition;
                yawDeg += d.x * orbitSpeed;
                pitchDeg = OrbitMath.ClampPitch(pitchDeg - d.y * orbitSpeed);
            }
            else if (Input.touchCount == 2)
            {
                Touch t0 = Input.GetTouch(0);
                Touch t1 = Input.GetTouch(1);
                float prev = ((t0.position - t0.deltaPosition) -
                              (t1.position - t1.deltaPosition)).magnitude;
                float curr = (t0.position - t1.position).magnitude;
                // ignore pinch when fingers nearly touch: the ratio explodes and
                // teleports the camera to a clamp limit in a single frame
                if (prev > 20f && curr > 20f)
                    distance = OrbitMath.ClampDistance(distance * (prev / curr));

                Vector2 avg = (t0.deltaPosition + t1.deltaPosition) * 0.5f;
                Quaternion yawRot = Quaternion.Euler(0f, yawDeg, 0f);
                pivot -= (yawRot * Vector3.right * avg.x + yawRot * Vector3.forward * avg.y)
                         * panSpeed * distance * 0.001f;
            }

#if UNITY_EDITOR
            // mouse fallback so the editor Play button is usable
            if (Input.GetMouseButton(0))
            {
                yawDeg += Input.GetAxis("Mouse X") * 3f;
                pitchDeg = OrbitMath.ClampPitch(pitchDeg - Input.GetAxis("Mouse Y") * 3f);
            }
            distance = OrbitMath.ClampDistance(
                distance * (1f - Input.mouseScrollDelta.y * 0.05f));
#endif

            transform.position = OrbitMath.CameraPosition(pivot, yawDeg, pitchDeg, distance);
            transform.LookAt(pivot);
        }
    }
}
