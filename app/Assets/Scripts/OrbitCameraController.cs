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
                distance = OrbitMath.ClampDistance(
                    distance * (prev / Mathf.Max(curr, 1f)));

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
