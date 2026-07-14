using System;
using UnityEngine;

namespace BattleAtlas
{
    // Total War-style flight for the Atlas game view (RTS camera slice):
    // pivot/yaw/pitch/distance orbit, unchanged touch gestures, and — new
    // this slice — first-class desktop input (trackpad-first: two-finger
    // scroll zooms/rotates, WASD/arrows pan, Q/E rotate, R/F zoom,
    // left-drag pans, option/alt-drag rotates), terrain awareness (the
    // pivot rides sampled ground height and stays inside the battlefield;
    // the camera never dips within minTerrainClearanceM of the terrain
    // under it), and critically-damped smoothing so live flight eases
    // while a direct pose assignment still snaps instantly (see the
    // pivot/yawDeg/pitchDeg/distance properties below — the six existing
    // capture harnesses pose the camera exactly as before).
    public class OrbitCameraController : MonoBehaviour
    {
        // ---------------------------------------------------------- pose
        //
        // pivot/yawDeg/pitchDeg/distance are the public pose contract.
        // ASSIGNING one is a snap: it sets both the flight target and the
        // live (rendered) pose to the same value with zero velocity, so
        // SmoothDamp is a no-op next frame — "targets = current, damping
        // is identity." That is exactly how the six capture harnesses
        // (DayExpansion*/PhaseSwitch/Cartography capture harnesses,
        // AtlasHdrpRender) already pose the camera — direct field
        // assignment — so this slice needed no changes there. Live desktop
        // input instead nudges the private target* fields directly (see
        // Handle* below) and lets LateUpdate's damping ease the visible
        // camera toward them; touch keeps assigning through these
        // properties (unchanged touch feel — immediate, no lag).
        public Vector3 pivot
        {
            get => targetPivot;
            set
            {
                Vector3 v = heightSampler != null ? RideTerrain(value) : value;
                targetPivot = livePivot = v;
                pivotVelocity = Vector3.zero;
            }
        }

        public float yawDeg
        {
            get => targetYawDeg;
            set { targetYawDeg = liveYawDeg = value; yawVelocity = 0f; }
        }

        public float pitchDeg
        {
            get => targetPitchDeg;
            set
            {
                float v = OrbitMath.ClampPitchForDistance(
                    value, targetDistance, OrbitMath.MinDistance, descentKneeDistanceM,
                    OrbitMath.MaxPitchDeg, descentMaxPitchAtFloorDeg);
                targetPitchDeg = livePitchDeg = v;
                pitchVelocity = 0f;
            }
        }

        public float distance
        {
            get => targetDistance;
            set
            {
                float v = OrbitMath.ClampDistance(value);
                targetDistance = liveDistance = v;
                distanceVelocity = 0f;
            }
        }

        Vector3 targetPivot;
        float targetYawDeg;
        float targetPitchDeg = 45f;
        float targetDistance = 4000f;

        Vector3 livePivot;
        float liveYawDeg;
        float livePitchDeg = 45f;
        float liveDistance = 4000f;

        Vector3 pivotVelocity;
        float yawVelocity, pitchVelocity, distanceVelocity;

        // ------------------------------------------------- feel constants
        // (all serialized — cheap verdict-round tuning, plan §"Feel")

        // touch / legacy mouse-fallback speeds (unchanged from pre-slice)
        public float orbitSpeed = 0.2f;
        public float panSpeed = 1.0f;
        public float tiltSpeed = 0.1f;

        // WASD / arrows: yaw-relative pan, distance-scaled, Shift-fast
        public float keyPanSpeed = 0.1f;
        public float shiftFastMultiplier = 3f;

        // Q/E rotate, R/F zoom (plain keyboard — no cursor semantics)
        public float keyRotateSpeedDegPerSec = 90f;
        public float keyZoomSpeedPerSec = 1.5f;

        // left-drag pan / option-alt-drag rotate
        public float dragThresholdPx = 4f;
        public float mouseDragRotateSpeedDegPerPx = 0.2f;

        // two-finger trackpad scroll: vertical = zoom, horizontal = rotate
        public float scrollZoomSpeed = 0.05f;
        public float scrollRotateSpeedDegPerNotch = 3f;

        // critically-damped smoothing time constants: input moves the
        // target* fields above, this is how fast the live/rendered pose
        // eases to catch up
        public float pivotSmoothTime = 0.15f;
        public float yawSmoothTime = 0.15f;
        public float pitchSmoothTime = 0.15f;
        public float distanceSmoothTime = 0.15f;

        // terrain awareness
        public float minTerrainClearanceM = 3f;
        public float descentKneeDistanceM = 150f;
        public float descentMaxPitchAtFloorDeg = 35f;
        public float nearClipAtAltitudeM = 10f;
        public float nearClipAtFloorM = 0.3f;
        public float zoomAnchorMaxRayDistM = 20000f;

        // Phase 11 unit follow: when set, the pivot tracks this provider
        // every frame (the HUD supplies the selected unit's ground anchor).
        // A null RESULT means the target is gone — follow drops. Manual
        // panning is deliberate camera intent and also drops it; rotating
        // or zooming does not (matches the touch twist/pinch gestures,
        // which never touched followPivot either).
        public System.Func<Vector3?> followPivot;

        // Terrain awareness: injected by whoever spawns/bootstraps the
        // camera (AtlasHud.Start, from the loaded BattleDirector's own
        // GroundHeightAt) — the SAME sampler the click-picker and the
        // symbol drape use, so pivot ride/clearance/zoom-anchor all agree
        // with the displayed terrain. Null is tolerated (EditMode rigs,
        // the HDRP capture tool, any scene without a battle) — terrain
        // features are then simply inert, matching pre-slice behavior.
        public Func<float, float, float> heightSampler;
        public Vector2 boundsMinXZ = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        public Vector2 boundsMaxXZ = new Vector2(float.PositiveInfinity, float.PositiveInfinity);

        Camera cam;
        Vector2 lastMousePos;
        bool leftDown, leftDragConfirmed;
        Vector2 leftDownPos;

        void Awake()
        {
            Application.targetFrameRate = 60;
            // battlefield is ~8.5 km across; URP default far plane (1000 m) clips
            // most of it. Near plane raised to keep depth precision at this scale
            // (dynamic-near-clip in LateUpdate eases it down again near the
            // 5 m zoom floor).
            cam = GetComponent<Camera>();
            if (cam != null)
            {
                cam.nearClipPlane = nearClipAtAltitudeM;
                cam.farClipPlane = 20000f;
            }
            lastMousePos = Input.mousePosition;
        }

        void LateUpdate()
        {
            Vector2 mousePos = Input.mousePosition;
            bool pointerBusy = AtlasHud.PointerBusy(mousePos);
            bool inputLocked = AtlasHud.InputLocked;

            HandleTouch();
            if (!inputLocked) HandleKeyboard();
            HandleMouse(pointerBusy, inputLocked, mousePos);
            lastMousePos = mousePos;

            if (followPivot != null)
            {
                Vector3? target = followPivot();
                if (target.HasValue) targetPivot = target.Value;
                else followPivot = null; // target gone; hold the last pivot
            }

            if (heightSampler != null) targetPivot = RideTerrain(targetPivot);

            float dt = Time.deltaTime;
            livePivot = Vector3.SmoothDamp(
                livePivot, targetPivot, ref pivotVelocity, pivotSmoothTime, Mathf.Infinity, dt);
            liveYawDeg = Mathf.SmoothDampAngle(
                liveYawDeg, targetYawDeg, ref yawVelocity, yawSmoothTime, Mathf.Infinity, dt);
            liveDistance = Mathf.SmoothDamp(
                liveDistance, targetDistance, ref distanceVelocity, distanceSmoothTime,
                Mathf.Infinity, dt);
            // pitch eases toward the descent-curve ceiling at the CURRENT
            // (live) distance, not just the raw target, so the ceiling
            // catches up smoothly as the camera itself descends into it
            float pitchTarget = OrbitMath.ClampPitchForDistance(
                targetPitchDeg, liveDistance, OrbitMath.MinDistance, descentKneeDistanceM,
                OrbitMath.MaxPitchDeg, descentMaxPitchAtFloorDeg);
            livePitchDeg = Mathf.SmoothDamp(
                livePitchDeg, pitchTarget, ref pitchVelocity, pitchSmoothTime, Mathf.Infinity, dt);
            livePitchDeg = OrbitMath.ClampPitchForDistance(
                livePitchDeg, liveDistance, OrbitMath.MinDistance, descentKneeDistanceM,
                OrbitMath.MaxPitchDeg, descentMaxPitchAtFloorDeg);

            if (heightSampler != null) livePivot = RideTerrain(livePivot);

            Vector3 camPos = OrbitMath.CameraPosition(livePivot, liveYawDeg, livePitchDeg, liveDistance);
            if (heightSampler != null)
                camPos = OrbitMath.ResolveTerrainClearance(camPos, heightSampler, minTerrainClearanceM);
            transform.position = camPos;
            transform.LookAt(livePivot);

            if (cam != null)
                cam.nearClipPlane = OrbitMath.DynamicNearClip(
                    liveDistance, OrbitMath.MinDistance, descentKneeDistanceM,
                    nearClipAtAltitudeM, nearClipAtFloorM);
        }

        // Battlefield-bounds clamp + terrain-height ride, shared by the
        // pivot property setter (a snap) and LateUpdate (continuous).
        Vector3 RideTerrain(Vector3 p)
        {
            p = OrbitMath.ClampPivotToBounds(p, boundsMinXZ, boundsMaxXZ);
            p.y = heightSampler(p.x, p.z);
            return p;
        }

        void HandleTouch()
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
        }

        // WASD/arrows (pan), Q/E (rotate), R/F (zoom) — gated by the
        // caller on AtlasHud.InputLocked (keyboard flight is muted while a
        // modal or Soldier View owns the screen; it needs no pointer
        // position, unlike PointerBusy).
        void HandleKeyboard()
        {
            float x = 0f, y = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y -= 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y += 1f;
            if (x != 0f || y != 0f)
            {
                bool fast = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                targetPivot += OrbitMath.KeyboardPanWorldDelta(
                    targetYawDeg, new Vector2(x, y), targetDistance, keyPanSpeed,
                    Time.deltaTime, fast, shiftFastMultiplier);
                followPivot = null; // deliberate camera intent, as the touch pan
            }

            float rotate = 0f;
            if (Input.GetKey(KeyCode.Q)) rotate -= 1f;
            if (Input.GetKey(KeyCode.E)) rotate += 1f;
            if (rotate != 0f)
                targetYawDeg += rotate * keyRotateSpeedDegPerSec * Time.deltaTime;

            // F: zoom in (closer to the ground). R: zoom out (rise away) —
            // mnemonic matches common fly-cam elevation binds (R/rise,
            // F/fall), mapped here onto the orbit's zoom distance.
            float zoom = 0f;
            if (Input.GetKey(KeyCode.F)) zoom += 1f;
            if (Input.GetKey(KeyCode.R)) zoom -= 1f;
            if (zoom != 0f)
                targetDistance = OrbitMath.ClampDistance(
                    targetDistance * (1f - zoom * keyZoomSpeedPerSec * Time.deltaTime));
        }

        // Left-drag (pan, with a click-vs-drag pixel threshold so
        // UnitPicker's click-select — which fires on the mouse-down frame,
        // in BattleDirector.HandleSelectionClick — still reads a clean
        // click and the camera doesn't nudge on the jitter), option/alt-
        // drag (rotate), and two-finger trackpad scroll (vertical = zoom,
        // horizontal = rotate).
        void HandleMouse(bool pointerBusy, bool inputLocked, Vector2 mousePos)
        {
            if (inputLocked)
            {
                // a modal/Soldier View grabbed the screen mid-drag — drop
                // the in-flight drag state cleanly rather than resume a
                // stale one when input frees up
                leftDown = leftDragConfirmed = false;
                return;
            }

            if (Input.GetMouseButtonDown(0) && !pointerBusy)
            {
                leftDown = true;
                leftDragConfirmed = false;
                leftDownPos = mousePos;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                leftDown = leftDragConfirmed = false;
            }

            // once a drag is confirmed it rides out the button hold even if
            // the cursor later crosses a HUD strip — only the START of a
            // drag needs a clean (non-HUD) mouse-down
            if (leftDown && Input.GetMouseButton(0))
            {
                if (!leftDragConfirmed
                    && (mousePos - leftDownPos).sqrMagnitude
                        >= dragThresholdPx * dragThresholdPx)
                {
                    leftDragConfirmed = true;
                }
                if (leftDragConfirmed)
                {
                    Vector2 delta = mousePos - lastMousePos;
                    bool altHeld = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
                    if (altHeld)
                    {
                        targetYawDeg += delta.x * mouseDragRotateSpeedDegPerPx;
                        targetPitchDeg -= delta.y * mouseDragRotateSpeedDegPerPx;
                    }
                    else
                    {
                        targetPivot += OrbitMath.PanWorldDelta(
                            targetYawDeg, delta, targetDistance, panSpeed);
                        followPivot = null; // deliberate camera intent
                    }
                }
            }

            if (pointerBusy) return;
            Vector2 scroll = Input.mouseScrollDelta;
            if (scroll.y != 0f)
                ApplyCursorAnchoredZoom(
                    OrbitMath.ClampDistance(targetDistance * (1f - scroll.y * scrollZoomSpeed)),
                    mousePos);
            if (scroll.x != 0f)
                targetYawDeg += scroll.x * scrollRotateSpeedDegPerNotch;
        }

        // Cursor-anchored zoom (trackpad-native two-finger vertical
        // scroll): zooming IN pulls the pivot toward the cursor's terrain
        // point by the same fraction the distance just shrank — the
        // ground under the cursor stays roughly put. Zooming OUT (or no
        // height sampler / no camera) leaves the pivot alone — the
        // pivot-anchored fallback, same as an off-viewport cursor or a ray
        // that finds no ground (OrbitMath.ZoomAnchorPoint).
        void ApplyCursorAnchoredZoom(float newDistance, Vector2 cursorScreenPos)
        {
            float oldDistance = targetDistance;
            targetDistance = newDistance;
            if (cam == null || heightSampler == null || newDistance >= oldDistance) return;
            float frac = (oldDistance - newDistance) / oldDistance;
            Vector3 viewport = cam.ScreenToViewportPoint(cursorScreenPos);
            Ray ray = cam.ScreenPointToRay(cursorScreenPos);
            Vector3 anchor = OrbitMath.ZoomAnchorPoint(
                viewport, ray.origin, ray.direction, heightSampler,
                zoomAnchorMaxRayDistM, targetPivot);
            targetPivot = OrbitMath.ZoomAnchorPivot(targetPivot, anchor, frac);
        }
    }
}
