using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace EasyBackrooms.Editor
{
    // Every scene setup step lives here
    public static class BackroomsSetup
    {
        public const string DefaultFolder = "Assets/Backrooms";

        [MenuItem("GameObject/EasyBackrooms/Complete Backrooms", false, 0)]
        static void CompleteMenu(MenuCommand command)
        {
            CreateComplete(DefaultFolder, false);
        }

        [MenuItem("Tools/EasyBackrooms/Create Demo Scene")]
        static void DemoMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            string root = EditorAssetUtil.PackageRoot();
            string folder = EditorAssetUtil.IsWritable(root) ? root + "/Samples/Demo" : "Assets/EasyBackrooms Demo";
            folder = EditorAssetUtil.EnsureFolder(folder);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            CreateComplete(folder, true);
            EditorSceneManager.SaveScene(scene, folder + "/BackroomsDemo.unity");
            Debug.Log($"EasyBackrooms: demo scene saved to {folder}/BackroomsDemo.unity. Press Play.");
        }

        // Drop in a scene and press Play
        public static void CreateComplete(string folder, bool freshAssets)
        {
            folder = EditorAssetUtil.EnsureFolder(folder);

            var profile = freshAssets ? null : EditorAssetUtil.FindFirst<BackroomsProfile>();
            if (profile == null) profile = DefaultProfileMenu.CreateProfile(folder + "/BackroomsProfile.asset");

            var theme = freshAssets ? null : EditorAssetUtil.FindFirst<BackroomsTheme>();
            if (theme == null) theme = DefaultThemeMenu.CreateTheme(folder + "/BackroomsTheme.asset");

            var world = CreateWorld(null, profile, theme);
            CreateLights(null, world, folder);
            CreateAtmosphere(null, folder);
            var player = CreatePlayer(null, world);

            Selection.activeObject = player.gameObject;
        }

        public static BackroomsWorld CreateWorld(GameObject parent, BackroomsProfile profile, BackroomsTheme theme)
        {
            var world = New<BackroomsWorld>("Backrooms", parent);
            world.profile = profile;
            world.theme = theme;

            if (profile == null || theme == null)
                Debug.LogWarning("EasyBackrooms: missing a profile or theme. Right click in the Project window and pick Create > EasyBackrooms > Default Profile and Default Theme.");

            return world;
        }

        public static PanelLightManager CreateLights(GameObject parent, BackroomsWorld world, string folder)
        {
            var lights = New<PanelLightManager>("Backrooms Lights", parent);
            lights.world = world;
            lights.buzz = EditorAssetUtil.FindClip("backrooms_buzz");

            var flicker = EditorAssetUtil.FindFirst<RandomFlicker>();
            if (flicker == null)
            {
                flicker = ScriptableObject.CreateInstance<RandomFlicker>();
                string path = AssetDatabase.GenerateUniqueAssetPath(EditorAssetUtil.EnsureFolder(folder) + "/RandomFlicker.asset");
                AssetDatabase.CreateAsset(flicker, path);
                AssetDatabase.SaveAssets();
            }
            lights.flicker = flicker;

            // Sounds found by name if present
            var crackle = lights.gameObject.AddComponent<FlickerSound>();
            crackle.lights = lights;
            crackle.clips = EditorAssetUtil.FindClipsContaining("flicker");

            var ambient = lights.gameObject.AddComponent<AmbientLoop>();
            var tones = EditorAssetUtil.FindClipsContaining("ambient");
            if (tones.Length > 0) ambient.clip = tones[0];

            return lights;
        }

        public static GameObject CreateAtmosphere(GameObject parent, string folder)
        {
            var atmosphere = New<BackroomsAtmosphere>("Backrooms Atmosphere", parent);
            AtmosphereMenu.AddVolume(atmosphere.gameObject, folder);

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

            RenderSetupMenu.Setup();
            atmosphere.Apply();
            return atmosphere.gameObject;
        }

        public static FirstPersonPlayer CreatePlayer(GameObject parent, BackroomsWorld world)
        {
            var player = New<FirstPersonPlayer>("Player", parent);
            var go = player.gameObject;

            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
                Undo.RegisterCreatedObjectUndo(camGo, "Create Camera");
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }

            var fly = cam.GetComponent<FlyCamera>();
            if (fly != null) Undo.DestroyObjectImmediate(fly);

            Undo.SetTransformParent(cam.transform, go.transform, "Move Camera Into Player");
            cam.transform.localPosition = new Vector3(0f, 1.63f, 0f);
            cam.transform.localRotation = Quaternion.identity;
            cam.nearClipPlane = 0.05f;

            var data = cam.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = true;

            player.cameraRoot = cam.transform;
            player.spawnInWorld = world;

            var steps = Undo.AddComponent<FootstepAudio>(go);
            steps.player = player;
            steps.clips = EditorAssetUtil.FindClipsContaining("step");

            if (world != null)
            {
                Undo.RecordObject(world, "Follow Player");
                world.target = go.transform;
                go.transform.position = world.FindSpawnPoint();
            }

            return player;
        }

        static T New<T>(string name, GameObject parent) where T : Component
        {
            var go = new GameObject(name);
            GameObjectUtility.SetParentAndAlign(go, parent);
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            Selection.activeObject = go;
            return go.AddComponent<T>();
        }
    }
}
