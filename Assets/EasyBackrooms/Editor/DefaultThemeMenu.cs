using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace EasyBackrooms.Editor
{
    public static class DefaultThemeMenu
    {
        [MenuItem("Assets/Create/EasyBackrooms/Default Theme", priority = 2)]
        static void Menu()
        {
            EditorAssetUtil.Select(CreateTheme(EditorAssetUtil.SelectedFolder() + "/BackroomsTheme.asset"));
        }

        public static BackroomsTheme CreateTheme(string path)
        {
            path = AssetDatabase.GenerateUniqueAssetPath(path);
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
            var panelTextures = LightPanelTextureMaker.GetOrCreateDefault();
            var litPanel = MakePanelMaterial(theme, "Light Panel", true, panelTextures);
            var deadPanel = MakePanelMaterial(theme, "Dead Panel", false, panelTextures);

            var floor = EditorAssetUtil.AddSubAsset<FloorBuilder>(theme, "Floor");
            floor.material = carpet;
            floor.tileSize = 2f;
            floor.shadows = ShadowCastingMode.Off;

            var roof = EditorAssetUtil.AddSubAsset<CeilingBuilder>(theme, "Ceiling");
            roof.material = ceiling;
            roof.tileSize = 1.5f;
            roof.shadows = ShadowCastingMode.Off;

            var walls = EditorAssetUtil.AddSubAsset<WallBuilder>(theme, "Walls");
            walls.material = wallpaper;
            walls.tileSize = 1f;

            var pillars = EditorAssetUtil.AddSubAsset<PillarBuilder>(theme, "Pillars");
            pillars.material = wallpaper;
            pillars.tileSize = 1f;
            pillars.width = 1.6f;

            var trimMaterial = NewLit("Baseboard");
            Tint(trimMaterial, new Color(0.42f, 0.36f, 0.2f));
            trimMaterial.SetFloat("_Smoothness", 0.35f);
            AssetDatabase.AddObjectToAsset(trimMaterial, theme);

            var trim = EditorAssetUtil.AddSubAsset<BaseboardBuilder>(theme, "Baseboard");
            trim.material = trimMaterial;
            trim.wallThickness = walls.thickness;
            trim.addCollider = false;
            trim.shadows = ShadowCastingMode.Off;

            var lit = EditorAssetUtil.AddSubAsset<LightPanelBuilder>(theme, "Light Panels");
            lit.kind = PanelKind.Lit;
            lit.tileGrid = roof.tileSize * 0.5f;
            lit.panelTiles = new Vector2Int(1, 1);
            lit.material = litPanel;
            lit.addCollider = false;
            lit.shadows = ShadowCastingMode.Off;

            var dead = EditorAssetUtil.AddSubAsset<LightPanelBuilder>(theme, "Dead Panels");
            dead.kind = PanelKind.Dead;
            dead.tileGrid = roof.tileSize * 0.5f;
            dead.panelTiles = new Vector2Int(1, 1);
            dead.material = deadPanel;
            dead.addCollider = false;
            dead.shadows = ShadowCastingMode.Off;

            theme.builders.Add(floor);
            theme.builders.Add(roof);
            theme.builders.Add(walls);
            theme.builders.Add(pillars);
            theme.builders.Add(trim);
            theme.builders.Add(lit);
            theme.builders.Add(dead);

            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();
            return theme;
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

        static Material MakePanelMaterial(BackroomsTheme theme, string name, bool glowing, PanelTextures textures)
        {
            var material = glowing ? NewGlow(name, textures.Emission) : NewLit(name);

            if (!glowing)
            {
                material.SetFloat("_Smoothness", 0.3f);
                if (textures.DeadColor != null)
                {
                    material.SetTexture("_BaseMap", textures.DeadColor);
                    material.SetTexture("_MainTex", textures.DeadColor);
                }

                if (textures.Normal != null)
                {
                    material.SetTexture("_BumpMap", textures.Normal);
                    material.EnableKeyword("_NORMALMAP");
                }
            }

            AssetDatabase.AddObjectToAsset(material, theme);
            return material;
        }

        // Unlit HDR so it always glows
        static Material NewGlow(string name, Texture2D glowMap)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Texture");
            var material = new Material(shader) { name = name };

            var glow = new Color(1f, 0.98f, 0.93f) * 4f;
            glow.a = 1f;
            material.SetColor("_BaseColor", glow);
            material.SetColor("_Color", glow);

            if (glowMap != null)
            {
                material.SetTexture("_BaseMap", glowMap);
                material.SetTexture("_MainTex", glowMap);
            }

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
