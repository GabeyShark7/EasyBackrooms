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

        [MenuItem("GameObject/EasyBackrooms/Layout Preview", false, 11)]
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
