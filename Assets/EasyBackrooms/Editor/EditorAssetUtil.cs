using System.IO;
using UnityEditor;
using UnityEngine;

namespace EasyBackrooms.Editor
{
    internal static class EditorAssetUtil
    {
        public static string SelectedFolder()
        {
            if (Selection.activeObject == null) return "Assets";

            string selected = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(selected)) return "Assets";

            return AssetDatabase.IsValidFolder(selected)
                ? selected
                : Path.GetDirectoryName(selected).Replace('\\', '/');
        }

        public static T AddSubAsset<T>(Object host, string name) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            asset.name = name;
            AssetDatabase.AddObjectToAsset(asset, host);
            return asset;
        }

        public static T FindFirst<T>() where T : Object
        {
            foreach (string guid in AssetDatabase.FindAssets("t:" + typeof(T).Name))
            {
                var asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null) return asset;
            }
            return null;
        }
    }
}
