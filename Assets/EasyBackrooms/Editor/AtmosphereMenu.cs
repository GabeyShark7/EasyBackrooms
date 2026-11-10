using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace EasyBackrooms.Editor
{
    public static class AtmosphereMenu
    {
        [MenuItem("GameObject/EasyBackrooms/Atmosphere", false, 12)]
        static void CreateAtmosphere(MenuCommand command)
        {
            var go = new GameObject("Backrooms Atmosphere");
            GameObjectUtility.SetParentAndAlign(go, command.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create Backrooms Atmosphere");

            go.AddComponent<BackroomsAtmosphere>();

            var volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = CreateProfile();

            var cam = Camera.main;
            if (cam != null)
            {
                var data = cam.GetUniversalAdditionalCameraData();
                Undo.RecordObject(data, "Enable Post Processing");
                data.renderPostProcessing = true;
            }

            // The sun ruins the mood so its off
            foreach (var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (light.type != LightType.Directional || !light.gameObject.activeSelf) continue;
                Undo.RecordObject(light.gameObject, "Disable Sun");
                light.gameObject.SetActive(false);
                Debug.Log($"EasyBackrooms: turned off {light.name} so the backrooms stay indoors. Turn it back on if you want it.", light);
            }

            Selection.activeObject = go;
        }

        static VolumeProfile CreateProfile()
        {
            string folder = "Assets";
            var backroomsProfile = EditorAssetUtil.FindFirst<BackroomsProfile>();
            if (backroomsProfile != null)
                folder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(backroomsProfile)).Replace('\\', '/');

            string path = AssetDatabase.GenerateUniqueAssetPath(folder + "/BackroomsVolume.asset");
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, path);

            var bloom = Add<Bloom>(profile);
            bloom.intensity.Override(1.1f);
            bloom.threshold.Override(0.9f);
            bloom.scatter.Override(0.75f);
            bloom.tint.Override(new Color(1f, 0.93f, 0.75f));

            var grain = Add<FilmGrain>(profile);
            grain.type.Override(FilmGrainLookup.Medium1);
            grain.intensity.Override(0.2f);

            var vignette = Add<Vignette>(profile);
            vignette.intensity.Override(0.3f);
            vignette.smoothness.Override(0.45f);

            var tonemap = Add<Tonemapping>(profile);
            tonemap.mode.Override(TonemappingMode.Neutral);

            // Pushes it all toward mustard
            var color = Add<ColorAdjustments>(profile);
            color.postExposure.Override(0.3f);
            color.contrast.Override(12f);
            color.saturation.Override(12f);
            color.colorFilter.Override(new Color(1f, 0.93f, 0.68f));

            var balance = Add<WhiteBalance>(profile);
            balance.temperature.Override(18f);
            balance.tint.Override(-6f);

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }

        static T Add<T>(VolumeProfile profile) where T : VolumeComponent
        {
            var component = profile.Add<T>(true);
            component.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(component, profile);
            return component;
        }
    }
}
