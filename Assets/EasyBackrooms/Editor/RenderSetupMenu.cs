using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace EasyBackrooms.Editor
{
    // Tunes your URP asset for nicer light
    public static class RenderSetupMenu
    {
        [MenuItem("Tools/EasyBackrooms/Setup Realistic Lighting")]
        public static void Setup()
        {
            if (GraphicsSettings.currentRenderPipeline is not UniversalRenderPipelineAsset asset)
            {
                Debug.LogWarning("EasyBackrooms: this project is not using URP so lighting setup was skipped.");
                return;
            }

            var so = new SerializedObject(asset);
            Undo.RecordObject(asset, "Setup Realistic Lighting");
            SetBool(so, "m_AdditionalLightShadowsSupported", true);
            SetBool(so, "m_SoftShadowsSupported", true);
            SetInt(so, "m_AdditionalLightsShadowmapResolution", 4096);
            SetInt(so, "m_AdditionalLightsShadowResolutionTierHigh", 1024);
            SetInt(so, "m_AdditionalLightsPerObjectLimit", 8);

            // Smooth edges and HDR color
            SetInt(so, "m_MSAA", 4);
            SetBool(so, "m_SupportsHDR", true);
            SetEnum(so, "m_ColorGradingMode", 1);
            SetEnum(so, "m_SoftShadowQuality", 3);
            so.ApplyModifiedProperties();

            var renderer = DefaultRenderer(so);
            if (renderer != null) SetupAmbientOcclusion(renderer);

            AssetDatabase.SaveAssets();
            Debug.Log($"EasyBackrooms: tuned {asset.name} for soft shadows on extra lights and stronger ambient occlusion.", asset);
        }

        static ScriptableRendererData DefaultRenderer(SerializedObject asset)
        {
            var list = asset.FindProperty("m_RendererDataList");
            var index = asset.FindProperty("m_DefaultRendererIndex");
            if (list == null || list.arraySize == 0) return null;

            int i = index != null ? Mathf.Clamp(index.intValue, 0, list.arraySize - 1) : 0;
            return list.GetArrayElementAtIndex(i).objectReferenceValue as ScriptableRendererData;
        }

        static void SetupAmbientOcclusion(ScriptableRendererData renderer)
        {
            ScreenSpaceAmbientOcclusion ssao = null;
            foreach (var feature in renderer.rendererFeatures)
            {
                if (feature is ScreenSpaceAmbientOcclusion found) ssao = found;
            }

            if (ssao == null)
            {
                ssao = ScriptableObject.CreateInstance<ScreenSpaceAmbientOcclusion>();
                ssao.name = "ScreenSpaceAmbientOcclusion";
                AssetDatabase.AddObjectToAsset(ssao, renderer);

                var rendererSo = new SerializedObject(renderer);
                var features = rendererSo.FindProperty("m_RendererFeatures");
                var map = rendererSo.FindProperty("m_RendererFeatureMap");

                features.arraySize++;
                features.GetArrayElementAtIndex(features.arraySize - 1).objectReferenceValue = ssao;

                if (map != null && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(ssao, out _, out long localId))
                {
                    map.arraySize++;
                    map.GetArrayElementAtIndex(map.arraySize - 1).longValue = localId;
                }

                rendererSo.ApplyModifiedProperties();
            }

            // Darkens corners and wall bottoms
            var so = new SerializedObject(ssao);
            SetFloat(so, "m_Settings.Intensity", 1.6f);
            SetFloat(so, "m_Settings.Radius", 0.5f);
            SetFloat(so, "m_Settings.DirectLightingStrength", 0.5f);
            SetFloat(so, "m_Settings.Falloff", 60f);
            SetBool(so, "m_Settings.Downsample", false);
            SetBool(so, "m_Settings.AfterOpaque", false);
            SetEnum(so, "m_Settings.Source", 1);
            SetEnum(so, "m_Settings.Samples", 0);
            SetEnum(so, "m_Settings.BlurQuality", 0);
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(renderer);
        }

        static void SetBool(SerializedObject so, string name, bool value)
        {
            var p = so.FindProperty(name);
            if (p != null) p.boolValue = value;
        }

        static void SetInt(SerializedObject so, string name, int value)
        {
            var p = so.FindProperty(name);
            if (p != null) p.intValue = value;
        }

        static void SetEnum(SerializedObject so, string name, int value)
        {
            var p = so.FindProperty(name);
            if (p == null) return;
            if (p.propertyType == SerializedPropertyType.Enum) p.enumValueIndex = value;
            else p.intValue = value;
        }

        static void SetFloat(SerializedObject so, string name, float value)
        {
            var p = so.FindProperty(name);
            if (p != null) p.floatValue = value;
        }
    }
}
