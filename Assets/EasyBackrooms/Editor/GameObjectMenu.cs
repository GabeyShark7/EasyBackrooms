using UnityEditor;
using UnityEngine;

namespace EasyBackrooms.Editor
{
    public static class GameObjectMenu
    {
        [MenuItem("GameObject/EasyBackrooms/Layout Preview", false, 10)]
        static void CreateLayoutPreview(MenuCommand command)
        {
            var go = new GameObject("Backrooms Layout Preview");
            var preview = go.AddComponent<LayoutPreview>();
            preview.profile = FindFirstProfile();

            GameObjectUtility.SetParentAndAlign(go, command.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create Layout Preview");
            Selection.activeObject = go;

            if (preview.profile == null)
                Debug.LogWarning("EasyBackrooms: no profile found. Right click in the Project window and pick Create > EasyBackrooms > Default Profile, then drag it onto the preview.");
        }

        static BackroomsProfile FindFirstProfile()
        {
            // Grabs a profile so you dont have to
            foreach (string guid in AssetDatabase.FindAssets("t:" + nameof(BackroomsProfile)))
            {
                var profile = AssetDatabase.LoadAssetAtPath<BackroomsProfile>(AssetDatabase.GUIDToAssetPath(guid));
                if (profile != null) return profile;
            }
            return null;
        }
    }
}
