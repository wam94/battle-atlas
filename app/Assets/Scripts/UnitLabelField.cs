using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace BattleAtlas
{
    // World-space unit labels (atlas-cartography plan, D3): a fixed pool
    // of TextMeshPro labels (TMP ships inside com.unity.ugui 2.0.0 — no
    // extra package), billboarded to the camera and distance-scaled to
    // constant screen size. World-space deliberately: screen-space labels
    // belong to the Phase 11 UI-Toolkit migration, and world-space labels
    // survive that migration untouched. BattleDirector fills fixed
    // candidate slots alongside its render pass; this component projects
    // them, runs LabelLayout.Declutter, and drives the pool by
    // enable/disable only — zero steady-state allocation. Added and
    // Init'd by BattleDirector.Start (the ObscurationField pattern); the
    // pool itself is created in THIS component's Start, which only runs
    // in play mode — headless EditMode drives of the director never
    // instantiate TMP objects.
    public class UnitLabelField : MonoBehaviour
    {
        // label budget (plan D3): 48 pooled TMP components, created once
        public const int PoolSize = 48;
        // TMP point size; the constant-screen-size factor rides the
        // transform scale (LabelLayout.LabelScale), not the font
        public const float FontSize = 36f;

        BattleDirector director;
        Camera cam;
        // Phase 11: the HUD hides the label layer while Soldier View owns
        // the screen (labels would bleed into the video's letterbox bands)
        public bool Hidden;
        TextMeshPro[] pool;
        // caller-owned LabelLayout.Declutter buffers, sized once to the
        // director's fixed slot capacity; `shown` doubles as last frame's
        // state (the documented aliasing contract — sticky hysteresis)
        Rect[] rects;
        float[] effectiveScratch;
        int[] orderScratch;
        bool[] shown;
        int[] results;

        public void Init(BattleDirector battleDirector, Camera lodCamera)
        {
            director = battleDirector;
            cam = lodCamera;
        }

        void Start()
        {
            if (director == null)
            {
                Debug.LogWarning("UnitLabelField was never Init'd; disabling");
                enabled = false;
                return;
            }
            int capacity = director.LabelSlotCount;
            rects = new Rect[capacity];
            effectiveScratch = new float[capacity];
            orderScratch = new int[capacity];
            shown = new bool[capacity];
            results = new int[PoolSize];
            pool = new TextMeshPro[PoolSize];
            // the halo ink material (cartography slice 1): a SHARED asset —
            // the default TMP atlas with a dark outline, so map lettering
            // reads against the terrain palette whatever hue the label
            // carries. Asset reference (Resources), never a runtime
            // material instance — the magenta/stripping doctrine. Missing:
            // warn once and keep the un-haloed default, nothing else breaks.
            var inkMaterial = Resources.Load<Material>("UI/UnitLabelInk");
            if (inkMaterial == null)
                Debug.LogWarning(
                    "UnitLabelField: Resources/UI/UnitLabelInk.mat missing — "
                    + "labels render without their contrast halo");
            for (int i = 0; i < PoolSize; i++)
            {
                var go = new GameObject($"unit label {i}");
                go.transform.SetParent(transform, false);
                go.SetActive(false);
                var label = go.AddComponent<TextMeshPro>();
                label.fontSize = FontSize;
                label.alignment = TextAlignmentOptions.Center;
                label.textWrappingMode = TextWrappingModes.NoWrap;
                if (inkMaterial != null)
                    label.fontSharedMaterial = inkMaterial;
                // map ink casts no shadow — same rule as the symbols
                var renderer = go.GetComponent<MeshRenderer>();
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                pool[i] = label;
            }
        }

        // After BattleDirector.Update filled the candidate slots: project,
        // declutter, and drive the pool. LateUpdate so every director in
        // the scene has already written this frame's candidates.
        void LateUpdate()
        {
            if (pool == null) return;
            if (cam == null || Hidden)
            {
                HideAll();
                return;
            }
            int capacity = director.LabelSlotCount;
            float[] priorities = director.LabelPriorities;
            Vector3[] positions = director.LabelPositions;
            string[] texts = director.LabelTexts;
            Color[] colors = director.LabelColors;
            float[] scales = director.LabelScales;
            for (int i = 0; i < capacity; i++)
            {
                if (float.IsPositiveInfinity(priorities[i]))
                    continue; // absent this frame — rect never read
                Vector3 sp = cam.WorldToScreenPoint(positions[i]);
                if (sp.z <= 0f)
                {
                    // behind the camera: withdraw the candidate (the
                    // buffers are per-frame scratch, refilled next Update)
                    priorities[i] = float.PositiveInfinity;
                    continue;
                }
                // the slot's scale multiplier widens its claim too — a
                // corps headline reserves headline space
                Rect r = LabelLayout.ScreenRect(
                    new Vector2(sp.x, sp.y), texts[i].Length);
                if (scales[i] != 1f)
                {
                    float grow = scales[i];
                    r = new Rect(
                        r.center.x - r.width * grow / 2f,
                        r.center.y - r.height * grow / 2f,
                        r.width * grow, r.height * grow);
                }
                rects[i] = r;
            }
            int shownCount = LabelLayout.Declutter(
                capacity, priorities, rects, shown,
                LabelLayout.StickyBonus, PoolSize,
                effectiveScratch, orderScratch, shown, results);

            Vector3 camPos = cam.transform.position;
            Quaternion camRot = cam.transform.rotation;
            for (int k = 0; k < shownCount; k++)
            {
                int i = results[k];
                TextMeshPro label = pool[k];
                if (!label.gameObject.activeSelf)
                    label.gameObject.SetActive(true);
                // set-on-change only: TMP re-lays-out (and allocates) on
                // assignment, and the slot->pool mapping is stable while
                // the shown set is
                if (label.text != texts[i]) label.text = texts[i];
                if (label.color != colors[i]) label.color = colors[i];
                // billboard + constant screen size, times the slot's grain
                // multiplier (corps headline > division subhead > unit)
                label.transform.SetPositionAndRotation(positions[i], camRot);
                float scale = LabelLayout.LabelScale(
                    Vector3.Distance(camPos, positions[i])) * scales[i];
                label.transform.localScale = new Vector3(scale, scale, scale);
            }
            for (int k = shownCount; k < PoolSize; k++)
                if (pool[k].gameObject.activeSelf)
                    pool[k].gameObject.SetActive(false);
        }

        void HideAll()
        {
            for (int k = 0; k < PoolSize; k++)
                if (pool[k].gameObject.activeSelf)
                    pool[k].gameObject.SetActive(false);
            // no camera means no layout ran — clear the sticky state so a
            // camera that appears later starts cold instead of stale
            for (int i = 0; i < shown.Length; i++) shown[i] = false;
        }
    }
}
