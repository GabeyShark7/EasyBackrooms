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
            // Tints pull your art toward mustard
            var carpet = MakeMaterial(theme, "Carpet", "carpet");
            Tint(carpet, new Color(1.1f, 1.25f, 0.95f));

            var ceiling = MakeMaterial(theme, "Ceiling Tiles", "ceiling_tiles");
            Tint(ceiling, new Color(0.98f, 0.9f, 0.6f));

            var wallpaper = MakeMaterial(theme, "Wallpaper", "wallpaper");
            Tint(wallpaper, new Color(1f, 0.95f, 0.72f));
            var litPanel = MakePanelMaterial(theme, "Light Panel", true);
            var deadPanel = MakePanelMaterial(theme, "Dead Panel", false);

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
            pillars.width = 1.2f;

            var lit = EditorAssetUtil.AddSubAsset<LightPanelBuilder>(theme, "Light Panels");
            lit.kind = PanelKind.Lit;
            lit.tileGrid = roof.tileSize * 0.5f;
            lit.material = litPanel;
            lit.addCollider = false;
            lit.shadows = ShadowCastingMode.Off;

            var dead = EditorAssetUtil.AddSubAsset<LightPanelBuilder>(theme, "Dead Panels");
            dead.kind = PanelKind.Dead;
            dead.tileGrid = roof.tileSize * 0.5f;
            dead.material = deadPanel;
            dead.addCollider = false;
            dead.shadows = ShadowCastingMode.Off;

            theme.builders.Add(floor);
            theme.builders.Add(roof);
            theme.builders.Add(walls);
            theme.builders.Add(pillars);
            theme.builders.Add(lit);
            theme.builders.Add(dead);

            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();
            Selection.activeObject = theme;
            EditorGUIUtility.PingObject(theme);
        }

        static void Tint(Material material, Color tint)
        {
            material.SetColor("_BaseColor", tint);
            material.SetColor("_Color", tint);
        }

        static Material NewLit(string name)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            return new Material(shader) { name = name };
        }

        static Material MakeMaterial(BackroomsTheme theme, string name, string textureSet)
        {
            var material = NewLit(name);

            var color = FindTexture(textureSet + "_color", true);
            var normal = FindTexture(textureSet + "_normal", true);
            var smooth = FindTexture(textureSet + "_smooth", false);

            if (smooth == null)
            {
                var rough = FindTexture(textureSet + "_rough", false);
                if (rough != null) smooth = SmoothnessMapMaker.FromRoughness(rough);
            }

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

            if (smooth != null)
            {
                material.SetTexture("_MetallicGlossMap", smooth);
                material.EnableKeyword("_METALLICSPECGLOSSMAP");
                material.SetFloat("_SmoothnessTextureChannel", 0f);
                material.SetFloat("_Smoothness", 1f);
            }
            else
            {
                material.SetFloat("_Smoothness", 0.15f);
            }

            AssetDatabase.AddObjectToAsset(material, theme);
            return material;
        }

        static Material MakePanelMaterial(BackroomsTheme theme, string name, bool glowing)
        {
            var material = NewLit(name);
            var baseColor = glowing ? Color.white : new Color(0.55f, 0.53f, 0.48f);
            material.SetColor("_BaseColor", baseColor);
            material.SetColor("_Color", baseColor);
            material.SetFloat("_Smoothness", 0.3f);

            if (glowing)
            {
                // Bright enough for bloom to catch
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", new Color(1f, 0.95f, 0.8f) * 4f);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            }

            AssetDatabase.AddObjectToAsset(material, theme);
            return material;
        }

        static Texture2D FindTexture(string fileName, bool warn)
        {
            foreach (string guid in AssetDatabase.FindAssets(fileName + " t:Texture2D"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileNameWithoutExtension(path) == fileName)
                    return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }

            if (warn) Debug.LogWarning($"EasyBackrooms: could not find a texture named {fileName}. Drag one onto the material yourself.");
            return null;
        }
    }
}
