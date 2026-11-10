using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EasyBackrooms.Editor
{
    // Name textures right and it sets up
    public sealed class BackroomsTextureImporter : AssetPostprocessor
    {
        static readonly string[] NormalSuffixes = { "_normal" };
        static readonly string[] LinearSuffixes = { "_rough", "_smooth", "_metal", "_ao", "_mask", "_height" };

        void OnPreprocessTexture()
        {
            var importer = (TextureImporter)assetImporter;

            // First import only so edits stay
            if (!importer.importSettingsMissing) return;
            Apply(importer, Path.GetFileNameWithoutExtension(assetPath));
        }

        public static bool Apply(TextureImporter importer, string fileName)
        {
            string name = fileName.ToLowerInvariant();

            if (EndsWithAny(name, NormalSuffixes))
            {
                importer.textureType = TextureImporterType.NormalMap;
                return true;
            }

            if (EndsWithAny(name, LinearSuffixes))
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = false;
                importer.alphaIsTransparency = false;
                return true;
            }

            return false;
        }

        static bool EndsWithAny(string name, string[] suffixes)
        {
            foreach (var suffix in suffixes)
            {
                if (name.EndsWith(suffix)) return true;
            }
            return false;
        }

        [MenuItem("Assets/EasyBackrooms/Fix Texture Imports", priority = 1000)]
        static void FixSelected()
        {
            int fixedCount = 0;
            foreach (string path in SelectedTexturePaths())
            {
                if (AssetImporter.GetAtPath(path) is not TextureImporter importer) continue;
                if (!Apply(importer, Path.GetFileNameWithoutExtension(path))) continue;
                importer.SaveAndReimport();
                fixedCount++;
            }

            Debug.Log($"EasyBackrooms: fixed import settings on {fixedCount} textures.");
        }

        [MenuItem("Assets/EasyBackrooms/Make Smoothness Maps", priority = 1001)]
        static void MakeSmoothness()
        {
            int made = 0;
            foreach (string path in SelectedTexturePaths())
            {
                if (!Path.GetFileNameWithoutExtension(path).ToLowerInvariant().EndsWith("_rough")) continue;
                var rough = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (rough != null && SmoothnessMapMaker.FromRoughness(rough) != null) made++;
            }

            Debug.Log($"EasyBackrooms: made {made} smoothness maps from _rough textures.");
        }

        static IEnumerable<string> SelectedTexturePaths()
        {
            var seen = new HashSet<string>();
            foreach (var obj in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path)) continue;

                if (AssetDatabase.IsValidFolder(path))
                {
                    foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { path }))
                    {
                        string p = AssetDatabase.GUIDToAssetPath(guid);
                        if (seen.Add(p)) yield return p;
                    }
                }
                else if (obj is Texture2D && seen.Add(path))
                {
                    yield return path;
                }
            }
        }
    }
}
