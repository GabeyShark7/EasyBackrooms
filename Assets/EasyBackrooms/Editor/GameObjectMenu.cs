using UnityEditor;
using UnityEngine;

namespace EasyBackrooms.Editor
{
    public static class GameObjectMenu
    {
        [MenuItem("GameObject/EasyBackrooms/Backrooms World", false, 10)]
        static void CreateBackroomsWorld(MenuCommand command)
        {
            var world = Create<BackroomsWorld>("Backrooms", command);

            // Grabs assets so you dont have to
            world.profile = EditorAssetUtil.FindFirst<BackroomsProfile>();
            world.theme = EditorAssetUtil.FindFirst<BackroomsTheme>();

            if (world.profile == null || world.theme == null)
                Debug.LogWarning("EasyBackrooms: missing a profile or theme. Right click in the Project window and pick Create > EasyBackrooms > Default Profile and Default Theme.");

            var cam = Camera.main;
            if (cam != null && cam.GetComponent<FlyCamera>() == null)
            {
                Undo.AddComponent<FlyCamera>(cam.gameObject);
                Undo.RecordObject(cam.transform, "Move Camera");
                cam.transform.position = new Vector3(1f, 1.6f, 1f);
            }
        }

        [MenuItem("GameObject/EasyBackrooms/Backrooms Lights", false, 11)]
        static void CreateLights(MenuCommand command)
        {
            var lights = Create<PanelLightManager>("Backrooms Lights", command);
            lights.world = Object.FindFirstObjectByType<BackroomsWorld>();
            lights.buzz = FindClip("backrooms_buzz");
            lights.flicker = EditorAssetUtil.FindFirst<RandomFlicker>() ?? CreateFlicker();
        }

        static AudioClip FindClip(string fileName)
        {
            foreach (string guid in AssetDatabase.FindAssets(fileName + " t:AudioClip"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileNameWithoutExtension(path) == fileName)
                    return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            }
            return null;
        }

        static RandomFlicker CreateFlicker()
        {
            string folder = "Assets";
            var profile = EditorAssetUtil.FindFirst<BackroomsProfile>();
            if (profile != null)
                folder = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(profile)).Replace('\\', '/');

            var flicker = ScriptableObject.CreateInstance<RandomFlicker>();
            AssetDatabase.CreateAsset(flicker, AssetDatabase.GenerateUniqueAssetPath(folder + "/RandomFlicker.asset"));
            AssetDatabase.SaveAssets();
            return flicker;
        }

        [MenuItem("GameObject/EasyBackrooms/Layout Preview", false, 13)]
        static void CreateLayoutPreview(MenuCommand command)
        {
            var preview = Create<LayoutPreview>("Backrooms Layout Preview", command);
            preview.profile = EditorAssetUtil.FindFirst<BackroomsProfile>();

            if (preview.profile == null)
                Debug.LogWarning("EasyBackrooms: no profile found. Right click in the Project window and pick Create > EasyBackrooms > Default Profile.");
        }

        static T Create<T>(string name, MenuCommand command) where T : Component
        {
            var go = new GameObject(name);
            var component = go.AddComponent<T>();
            GameObjectUtility.SetParentAndAlign(go, command.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            Selection.activeObject = go;
            return component;
        }
    }
}
