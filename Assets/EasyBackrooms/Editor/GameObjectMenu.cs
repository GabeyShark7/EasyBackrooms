using UnityEditor;
using UnityEngine;

namespace EasyBackrooms.Editor
{
    public static class GameObjectMenu
    {
        [MenuItem("GameObject/EasyBackrooms/Chunk Builder", false, 10)]
        static void CreateChunkBuilder(MenuCommand command)
        {
            var builder = Create<ChunkBuilder>("Backrooms", command);

            // Grabs assets so you dont have to
            builder.profile = EditorAssetUtil.FindFirst<BackroomsProfile>();
            builder.theme = EditorAssetUtil.FindFirst<BackroomsTheme>();

            if (builder.profile == null || builder.theme == null)
                Debug.LogWarning("EasyBackrooms: missing a profile or theme. Right click in the Project window and pick Create > EasyBackrooms > Default Profile and Default Theme.");
            else
                builder.Build();
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
