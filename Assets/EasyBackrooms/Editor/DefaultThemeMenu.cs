using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace EasyBackrooms.Editor
{
    public static class DefaultThemeMenu
    {
        [MenuItem("Assets/Create/EasyBackrooms/Default Theme", priority = 2)]
        static void CreateDefaultTheme()
        {
            string path = AssetDatabase.GenerateUniqueAssetPath(EditorAssetUtil.SelectedFolder() + "/BackroomsTheme.asset");
            var theme = ScriptableObject.CreateInstance<BackroomsTheme>();
            AssetDatabase.CreateAsset(theme, path);

            // Looks for textures by these names
            var carpet = MakeMaterial(theme, "Carpet", "carpet");
            var carpetTint = new Color(1f, 0.93f, 0.72f);
            carpet.SetColor("_BaseColor", carpetTint);
            carpet.SetColor("_Color", carpetTint);
            var ceiling = MakeMaterial(theme, "Ceiling Tiles", "ceiling_tiles");
            var wallpaper = MakeMaterial(theme, "Wallpaper", "wallpaper");

            var floor = EditorAssetUtil.AddSubAsset<FloorBuilder>(theme, "Floor");
            floor.material = carpet;
            floor.tileSize = 2f;
            floor.shadows = ShadowCastingMode.Off;

            var roof = EditorAssetUtil.AddSubAsset<CeilingBuilder>(theme, "Ceiling");
            roof.material = ceiling;
            roof.tileSize = 1.6f;
            roof.shadows = ShadowCastingMode.Off;

            var walls = EditorAssetUtil.AddSubAsset<WallBuilder>(theme, "Walls");
            walls.material = wallpaper;
            walls.tileSize = 1f;

            var pillars = EditorAssetUtil.AddSubAsset<PillarBuilder>(theme, "Pillars");
            pillars.material = wallpaper;
            pillars.tileSize = 1f;

            theme.builders.Add(floor);
            theme.builders.Add(roof);
            theme.builders.Add(walls);
            theme.builders.Add(pillars);

            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();
            Selection.activeObject = theme;
            EditorGUIUtility.PingObject(theme);
        }

        static Material MakeMaterial(BackroomsTheme theme, string name, string textureSet)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = new Material(shader) { name = name };

            var color = FindTexture(textureSet + "_color");
            var normal = FindTexture(textureSet + "_normal");

            if (color != null)
            {
                material.SetTexture("_BaseMap", color);
                material.SetTexture("_MainTex", color);
            }

            if (normal != null)
            {
                material.SetTexture("_BumpMap", normal);
                material.EnableKeyword("_NORMALMAP");
            }

            // Roughness gets wired up in Phase 4
            material.SetFloat("_Smoothness", 0.15f);

            AssetDatabase.AddObjectToAsset(material, theme);
            return material;
        }

        static Texture2D FindTexture(string fileName)
        {
            foreach (string guid in AssetDatabase.FindAssets(fileName + " t:Texture2D"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileNameWithoutExtension(path) == fileName)
                    return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }

            Debug.LogWarning($"EasyBackrooms: could not find a texture named {fileName}. Drag one onto the material yourself.");
            return null;
        }
    }
}
