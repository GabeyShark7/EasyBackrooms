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

        public static string EnsureFolder(string path)
        {
            path = path.Replace('\\', '/').TrimEnd('/');
            if (AssetDatabase.IsValidFolder(path)) return path;

            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
            return path;
        }

        // Folder that holds Runtime and Editor
        public static string PackageRoot()
        {
            foreach (string guid in AssetDatabase.FindAssets("EasyBackrooms.Editor t:AssemblyDefinitionAsset"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) != "EasyBackrooms.Editor") continue;
                string editorFolder = Path.GetDirectoryName(path).Replace('\\', '/');
                return Path.GetDirectoryName(editorFolder).Replace('\\', '/');
            }
            return "Assets/EasyBackrooms";
        }

        // Installed packages are read only
        public static bool IsWritable(string path) => path.StartsWith("Assets");

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

        public static AudioClip FindClip(string exactName)
        {
            foreach (string guid in AssetDatabase.FindAssets(exactName + " t:AudioClip"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == exactName)
                    return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            }
            return null;
        }

        public static AudioClip[] FindClipsContaining(string word)
        {
            var found = new System.Collections.Generic.List<AudioClip>();
            foreach (string guid in AssetDatabase.FindAssets("t:AudioClip"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!Path.GetFileNameWithoutExtension(path).ToLowerInvariant().Contains(word)) continue;
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip != null) found.Add(clip);
            }
            return found.ToArray();
        }

        public static void Select(Object asset)
        {
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }
}
