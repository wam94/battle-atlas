using System;
using System.IO;
using UnityEditor;
using UnityEditor.Rendering.HighDefinition;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace BattleAtlas.EditorTools
{
    // Phase 4 (Angle V2 plan §12): the one-shot URP→HDRP project migration
    // plus the two versioned HDRP quality profiles and their switch points.
    //
    // Profiles:
    //   - PLAYBACK (Assets/Settings/HDRP/BattleAtlasHDRP_Playback.asset):
    //     real-time Atlas use. No SSR, no volumetrics — the Atlas is
    //     documentary cartography and must hold frame rate on the M1 floor.
    //   - OFFLINE  (Assets/Settings/HDRP/BattleAtlasHDRP_Offline.asset):
    //     offline render quality, unconstrained by frame rate (§3.2): SSR,
    //     SSAO, volumetrics, motion vectors (Recorder-accumulation-ready),
    //     16-bit color buffer.
    //
    // Switching: the project's committed default is PLAYBACK
    // (GraphicsSettings.defaultRenderPipeline; all quality levels defer to
    // it). "BattleAtlas ▸ HDRP ▸ Use Offline Profile (Session)" swaps the
    // session's pipeline for interactive inspection; the headless offline
    // renderer (AtlasHdrpRender) binds the offline profile itself, so no
    // persistent settings flip is ever required for a render.
    //
    // Exposure calibration (the bake-off's "HDRP reads dim/flat" fix,
    // recorded in docs/benchmarks/2026-07-08-v2-phase4-hdrp.md): the sun is
    // physical — SunMaxLux at high elevation, scaled by SunDirector's
    // elevation ramp — against a FIXED exposure of FixedExposureEv100
    // (sunny-afternoon EV100, Gate P3 parity). Custom BattleAtlas shaders
    // (SoldierVertexTint, Flag) deliberately bypass physical lighting and
    // read the normalized _BattleSun* globals instead; see their headers.
    public static class HdrpMigration
    {
        public const string SettingsDir = "Assets/Settings/HDRP";
        public const string PlaybackAssetPath = SettingsDir + "/BattleAtlasHDRP_Playback.asset";
        public const string OfflineAssetPath = SettingsDir + "/BattleAtlasHDRP_Offline.asset";
        public const string AtlasVolumeProfilePath = SettingsDir + "/AtlasVolumeProfile.asset";
        public const string OfflineVolumeProfilePath = SettingsDir + "/OfflineRenderVolumeProfile.asset";

        // Sun illuminance at high elevation (clear July afternoon; the Gate
        // P3 bake-off rendered 95k lux AT 15:20, i.e. mid-ramp — 100k base
        // reproduces that through SunDirector's elevation-keyed ramp).
        public const float SunMaxLux = 100000f;
        // Fixed sunny-afternoon exposure, identical to the approved Gate P3
        // HDRP frames. Fixed (not automatic) so offline renders and playback
        // agree and stay deterministic.
        public const float FixedExposureEv100 = 13.2f;
        // Atlas shadow reach: the strategic camera sits ~0.5–3 km out; this
        // matches the bake-off's eye-level tuning without paying for the
        // full 8 km battlefield diagonal.
        public const float PlaybackShadowDistance = 3000f;
        public const float OfflineShadowDistance = 5000f;

        // --- batch entry point -------------------------------------------

        // Unity -batchmode -executeMethod BattleAtlas.EditorTools.HdrpMigration.MigrateAll
        public static void MigrateAll()
        {
            int exitCode = 0;
            try
            {
                CreateProfiles();
                ConvertMaterials();
                ConvertGeneratedTerrainMaterial();
                MigrateAtlasScene();
                ActivateHdrpPlayback();
                AssetDatabase.SaveAssets();
                Debug.Log("HdrpMigration.MigrateAll: complete");
            }
            catch (Exception e)
            {
                Debug.LogError($"HdrpMigration.MigrateAll failed: {e}");
                exitCode = 1;
            }
            if (Application.isBatchMode) EditorApplication.Exit(exitCode);
        }

        // --- profiles ------------------------------------------------------

        [MenuItem("BattleAtlas/HDRP/Create Or Update Profiles")]
        public static void CreateProfiles()
        {
            Directory.CreateDirectory(Path.Combine(
                Application.dataPath, "Settings/HDRP"));

            var playback = LoadOrCreate<HDRenderPipelineAsset>(PlaybackAssetPath);
            var ps = playback.currentPlatformRenderPipelineSettings;
            ps.supportSSR = false;
            ps.supportSSRTransparent = false;
            ps.supportSSAO = false;          // Atlas relief legibility is baked (relief-in-albedo)
            ps.supportVolumetrics = false;   // real-time budget; smoke is instanced geometry
            ps.supportMotionVectors = true;  // TAA-capable if the owner enables it
            ps.colorBufferFormat = RenderPipelineSettings.ColorBufferFormat.R11G11B10;
            playback.currentPlatformRenderPipelineSettings = ps;
            EditorUtility.SetDirty(playback);

            var offline = LoadOrCreate<HDRenderPipelineAsset>(OfflineAssetPath);
            var os = offline.currentPlatformRenderPipelineSettings;
            os.supportSSR = true;
            os.supportSSRTransparent = true;
            os.supportSSAO = true;
            os.supportVolumetrics = true;    // §3.2: volumetric fog and smoke
            os.supportMotionVectors = true;  // Recorder accumulation motion blur
            os.colorBufferFormat = RenderPipelineSettings.ColorBufferFormat.R16G16B16A16;
            offline.currentPlatformRenderPipelineSettings = os;
            EditorUtility.SetDirty(offline);

            CreateAtlasVolumeProfile();
            CreateOfflineVolumeProfile();
            AssetDatabase.SaveAssets();
            Debug.Log("HdrpMigration: profiles created/updated under Assets/Settings/HDRP");
        }

        // The Atlas scene's volume: physically based sky + the fixed
        // calibration exposure + the URP SampleSceneProfile's post chain
        // (Neutral tonemap, bloom 0.25, vignette 0.2) so the before/after
        // comparison isolates the pipeline, not a re-grade. Fog stays OFF
        // exactly like the URP scene (RenderSettings.fog was 0).
        static void CreateAtlasVolumeProfile()
        {
            var profile = LoadOrCreateProfile(AtlasVolumeProfilePath);

            var env = GetOrAdd<VisualEnvironment>(profile);
            env.skyType.overrideState = true;
            env.skyType.value = (int)SkyType.PhysicallyBased;
            env.skyAmbientMode.overrideState = true;
            env.skyAmbientMode.value = SkyAmbientMode.Dynamic;

            GetOrAdd<PhysicallyBasedSky>(profile);

            var exposure = GetOrAdd<UnityEngine.Rendering.HighDefinition.Exposure>(profile);
            exposure.mode.overrideState = true;
            exposure.mode.value = ExposureMode.Fixed;
            exposure.fixedExposure.overrideState = true;
            exposure.fixedExposure.value = FixedExposureEv100;

            var shadows = GetOrAdd<HDShadowSettings>(profile);
            shadows.maxShadowDistance.overrideState = true;
            shadows.maxShadowDistance.value = PlaybackShadowDistance;

            var tone = GetOrAdd<UnityEngine.Rendering.HighDefinition.Tonemapping>(profile);
            tone.mode.overrideState = true;
            tone.mode.value = UnityEngine.Rendering.HighDefinition.TonemappingMode.Neutral;

            var bloom = GetOrAdd<UnityEngine.Rendering.HighDefinition.Bloom>(profile);
            bloom.threshold.overrideState = true;
            bloom.threshold.value = 1f;
            bloom.intensity.overrideState = true;
            bloom.intensity.value = 0.25f;
            bloom.scatter.overrideState = true;
            bloom.scatter.value = 0.5f;

            var vignette = GetOrAdd<UnityEngine.Rendering.HighDefinition.Vignette>(profile);
            vignette.intensity.overrideState = true;
            vignette.intensity.value = 0.2f;

            EditorUtility.SetDirty(profile);
        }

        // Offline additions layered over the Atlas look: volumetric-capable
        // fog, SSAO/SSR overrides, and a longer shadow reach. Exposure and
        // tonemap stay IDENTICAL to playback — offline quality must not
        // silently re-grade the record.
        static void CreateOfflineVolumeProfile()
        {
            var profile = LoadOrCreateProfile(OfflineVolumeProfilePath);

            var fog = GetOrAdd<UnityEngine.Rendering.HighDefinition.Fog>(profile);
            fog.enabled.overrideState = true;
            fog.enabled.value = true;
            // volumetric-capable fog (§3.2) tuned so the MACRO Atlas frame
            // stays readable: the Gate P3 eye-level density (900 m mean free
            // path) whites out a 4 km orbit view. Phase 7/9 author their own
            // eye-level volumes for the Angle slice per §8.3.
            fog.meanFreePath.overrideState = true;
            fog.meanFreePath.value = 8000f;
            fog.baseHeight.overrideState = true;
            fog.baseHeight.value = 0f;
            fog.maximumHeight.overrideState = true;
            fog.maximumHeight.value = 400f;
            fog.enableVolumetricFog.overrideState = true;
            fog.enableVolumetricFog.value = true;

            var ao = GetOrAdd<UnityEngine.Rendering.HighDefinition.ScreenSpaceAmbientOcclusion>(profile);
            ao.intensity.overrideState = true;
            ao.intensity.value = 1f;

            var ssr = GetOrAdd<UnityEngine.Rendering.HighDefinition.ScreenSpaceReflection>(profile);
            ssr.enabled.overrideState = true;
            ssr.enabled.value = true;

            var shadows = GetOrAdd<HDShadowSettings>(profile);
            shadows.maxShadowDistance.overrideState = true;
            shadows.maxShadowDistance.value = OfflineShadowDistance;

            EditorUtility.SetDirty(profile);
        }

        // --- material conversion -------------------------------------------

        // URP/Lit → HDRP/Lit for the Battle material assets. _BaseColor,
        // _Smoothness and _Metallic carry over by name; the smoke/dust
        // transparency (alpha blend, no depth write) is re-expressed through
        // HDRP's _SurfaceType + ValidateMaterial. MaterialPropertyBlock
        // _BaseColor overrides (per-side / per-bucket colors) keep working:
        // HDRP/Lit declares _BaseColor in UnityPerMaterial exactly like URP.
        [MenuItem("BattleAtlas/HDRP/Convert Battle Materials")]
        public static void ConvertMaterials()
        {
            var hdrpLit = Shader.Find("HDRP/Lit");
            if (hdrpLit == null)
                throw new InvalidOperationException("HDRP/Lit shader not found");

            ConvertLit("Assets/Battle/UnitMarker.mat", hdrpLit, transparent: false);
            ConvertLit("Assets/Battle/Smoke.mat", hdrpLit, transparent: true);
            ConvertLit("Assets/Battle/Dust.mat", hdrpLit, transparent: true);
            AssetDatabase.SaveAssets();
            Debug.Log("HdrpMigration: Battle materials converted to HDRP/Lit");
        }

        static void ConvertLit(string path, Shader hdrpLit, bool transparent)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
                throw new InvalidOperationException($"{path} not found");
            if (mat.shader == hdrpLit)
            {
                Debug.Log($"HdrpMigration: {path} already HDRP/Lit");
                return;
            }

            Color baseColor = mat.GetColor("_BaseColor");
            float smoothness = mat.HasProperty("_Smoothness") ? mat.GetFloat("_Smoothness") : 0.5f;
            float metallic = mat.HasProperty("_Metallic") ? mat.GetFloat("_Metallic") : 0f;

            mat.shader = hdrpLit;
            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic", metallic);
            if (transparent)
            {
                mat.SetFloat("_SurfaceType", 1f); // transparent, alpha blend
                mat.SetFloat("_EnableFogOnTransparent", 1f);
            }
            HDMaterial.ValidateMaterial(mat);
            mat.enableInstancing = true;
            EditorUtility.SetDirty(mat);
        }

        // The gitignored generated terrain material (Assets/Generated/
        // LandcoverTerrain.mat, referenced by GUID from Atlas.unity): swap
        // its shader in place so the GUID — and the whole imported terrain —
        // survives. LandcoverImporter now creates HDRP/TerrainLit directly
        // for future re-imports. The two load-bearing URP tricks carry over
        // verbatim: relief-in-albedo needs nothing from the shader, and the
        // alpha=0 anti-gloss trick works because HDRP TerrainLit computes
        // smoothness = albedo.a * _Smoothness{i} when no mask map exists
        // (TerrainLit_Splatmap.hlsl DefaultMask) — same as URP.
        [MenuItem("BattleAtlas/HDRP/Convert Generated Terrain Material")]
        public static void ConvertGeneratedTerrainMaterial()
        {
            const string path = "Assets/Generated/LandcoverTerrain.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Debug.LogWarning(
                    $"HdrpMigration: {path} not found (fresh checkout without a terrain " +
                    "import?); run BattleAtlas ▸ Import Land Cover to regenerate it as HDRP");
                return;
            }
            var shader = Shader.Find("HDRP/TerrainLit");
            if (shader == null)
                throw new InvalidOperationException("HDRP/TerrainLit shader not found");
            if (mat.shader == shader)
            {
                Debug.Log($"HdrpMigration: {path} already HDRP/TerrainLit");
                return;
            }
            mat.shader = shader;
            // same per-pixel-normal setup the URP material used (the property
            // and keyword names are identical in HDRP's TerrainLit)
            mat.SetFloat("_EnableInstancedPerPixelNormal", 1f);
            mat.EnableKeyword("_TERRAIN_INSTANCED_PERPIXEL_NORMAL");
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            Debug.Log($"HdrpMigration: {path} converted to HDRP/TerrainLit");
        }

        // --- scene migration -----------------------------------------------

        // Atlas.unity: URP additional-data components out, HDRP ones in, the
        // sun gets physical units, the Global Volume gets the HDRP profile.
        [MenuItem("BattleAtlas/HDRP/Migrate Atlas Scene")]
        public static void MigrateAtlasScene()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Atlas.unity");

            // camera
            var cam = UnityEngine.Object.FindAnyObjectByType<Camera>();
            if (cam == null)
                throw new InvalidOperationException("Atlas.unity has no Camera");
            var urpCam = cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            if (urpCam != null) UnityEngine.Object.DestroyImmediate(urpCam, true);
            if (cam.GetComponent<HDAdditionalCameraData>() == null)
            {
                var hdCam = cam.gameObject.AddComponent<HDAdditionalCameraData>();
                // parity with the P0 URP baseline captures: no TAA — Phase 10
                // gets AA from Recorder accumulation instead
                hdCam.antialiasing = HDAdditionalCameraData.AntialiasingMode.None;
            }

            // sun
            var sunDirector = UnityEngine.Object.FindAnyObjectByType<SunDirector>();
            if (sunDirector == null || sunDirector.sun == null)
                throw new InvalidOperationException("Atlas.unity has no SunDirector.sun");
            Light sun = sunDirector.sun;
            var urpLight = sun.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalLightData>();
            if (urpLight != null) UnityEngine.Object.DestroyImmediate(urpLight, true);
            var hdLight = sun.GetComponent<HDAdditionalLightData>();
            if (hdLight == null) hdLight = sun.gameObject.AddComponent<HDAdditionalLightData>();
            sun.lightUnit = LightUnit.Lux;
            sun.intensity = SunMaxLux;
            hdLight.angularDiameter = 0.53f; // the solar disc
            sun.shadows = LightShadows.Soft;

            // volume
            var volume = UnityEngine.Object.FindAnyObjectByType<Volume>();
            if (volume == null)
                throw new InvalidOperationException("Atlas.unity has no Volume");
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(AtlasVolumeProfilePath);
            if (profile == null)
                throw new InvalidOperationException(
                    $"{AtlasVolumeProfilePath} missing; run CreateProfiles first");
            volume.sharedProfile = profile;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("HdrpMigration: Atlas.unity migrated to HDRP");
        }

        // --- pipeline activation ------------------------------------------

        // Persist HDRP PLAYBACK as the project's active pipeline. Quality
        // levels defer to the graphics default (single source of truth); the
        // old URP assets stay in Assets/Settings for history per the plan.
        [MenuItem("BattleAtlas/HDRP/Use Playback Profile")]
        public static void ActivateHdrpPlayback()
        {
            var playback = AssetDatabase.LoadAssetAtPath<HDRenderPipelineAsset>(PlaybackAssetPath);
            if (playback == null)
                throw new InvalidOperationException(
                    $"{PlaybackAssetPath} missing; run CreateProfiles first");
            GraphicsSettings.defaultRenderPipeline = playback;
            // clear per-quality-level overrides (the old Mobile-level URP
            // asset reference) so the graphics default is the single source
            // of truth; no public per-level setter exists, so this edits
            // ProjectSettings/QualitySettings.asset directly
            var qualityAsset = AssetDatabase.LoadAllAssetsAtPath(
                "ProjectSettings/QualitySettings.asset");
            if (qualityAsset.Length > 0)
            {
                var so = new SerializedObject(qualityAsset[0]);
                var levels = so.FindProperty("m_QualitySettings");
                if (levels == null)
                    throw new InvalidOperationException(
                        "QualitySettings.asset has no m_QualitySettings; Unity format changed");
                for (int i = 0; i < levels.arraySize; i++)
                    levels.GetArrayElementAtIndex(i)
                        .FindPropertyRelative("customRenderPipeline")
                        .objectReferenceValue = null;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            AssetDatabase.SaveAssets();
            Debug.Log("HdrpMigration: HDRP playback profile is the project default");
        }

        // Session-only swap for interactive offline-quality inspection; the
        // headless renderer never needs this (it binds the asset itself).
        [MenuItem("BattleAtlas/HDRP/Use Offline Profile (Session)")]
        public static void UseOfflineProfileSession()
        {
            var offline = AssetDatabase.LoadAssetAtPath<HDRenderPipelineAsset>(OfflineAssetPath);
            if (offline == null)
                throw new InvalidOperationException(
                    $"{OfflineAssetPath} missing; run CreateProfiles first");
            QualitySettings.renderPipeline = offline;
            Debug.Log("HDRP OFFLINE profile active for this session " +
                      "(quality-level override; not saved). Use 'Restore Playback (Session)'.");
        }

        [MenuItem("BattleAtlas/HDRP/Restore Playback (Session)")]
        public static void RestorePlaybackSession()
        {
            QualitySettings.renderPipeline = null;
            Debug.Log("HDRP playback profile restored (quality-level override cleared).");
        }

        // --- helpers -------------------------------------------------------

        static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }
            return asset;
        }

        static VolumeProfile LoadOrCreateProfile(string path)
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }
            return profile;
        }

        static T GetOrAdd<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (profile.TryGet(out T component))
                return component;
            component = profile.Add<T>(false);
            component.name = typeof(T).Name;
            AssetDatabase.AddObjectToAsset(component, profile);
            return component;
        }
    }
}
