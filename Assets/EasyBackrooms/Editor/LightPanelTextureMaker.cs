using System.IO;
using UnityEditor;
using UnityEngine;

namespace EasyBackrooms.Editor
{
    public sealed class PanelTextures
    {
        public Texture2D Color;
        public Texture2D Emission;
        public Texture2D Normal;
        public Texture2D DeadColor;
    }

    public static class LightPanelTextureMaker
    {
        // Makes settings and art if missing
        public static PanelTextures GetOrCreateDefault()
        {
            var settings = EditorAssetUtil.FindFirst<LightPanelTextureSettings>();
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<LightPanelTextureSettings>();
                string folder = EditorAssetUtil.EnsureFolder(DefaultFolder(settings));
                AssetDatabase.CreateAsset(settings, folder + "/LightPanelTextures.asset");
                AssetDatabase.SaveAssets();
            }

            return Generate(settings);
        }

        public static PanelTextures Load(LightPanelTextureSettings settings)
        {
            string prefix = Prefix(settings);
            return new PanelTextures
            {
                Color = AssetDatabase.LoadAssetAtPath<Texture2D>(prefix + "_color.png"),
                Emission = AssetDatabase.LoadAssetAtPath<Texture2D>(prefix + "_emission.png"),
                Normal = AssetDatabase.LoadAssetAtPath<Texture2D>(prefix + "_normal.png"),
                DeadColor = AssetDatabase.LoadAssetAtPath<Texture2D>(prefix + "_dead_color.png")
            };
        }

        public static PanelTextures Generate(LightPanelTextureSettings s)
        {
            int w = Mathf.Max(64, s.width);
            int h = Mathf.Max(16, Mathf.RoundToInt(w / Mathf.Max(0.5f, s.aspect)));
            float aspect = (float)w / h;

            var color = new Color32[w * h];
            var emission = new Color32[w * h];
            var normal = new Color32[w * h];
            var dead = new Color32[w * h];

            float ox = s.seed * 17.31f;
            float oy = s.seed * 9.73f;
            var aged = Color.Lerp(s.diffuserColor, s.diffuserColor * s.yellowTint, s.yellowing);
            var glowTint = Color.Lerp(Color.white, s.yellowTint, s.yellowing * 0.6f);
            var flatNormal = new Color32(128, 128, 255, 255);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int i = x + y * w;

                    // Units where panel height is 1
                    float px = (x + 0.5f) / h;
                    float py = (y + 0.5f) / h;
                    float edge = Mathf.Min(Mathf.Min(px, aspect - px), Mathf.Min(py, 1f - py));

                    if (edge < s.frameWidth)
                    {
                        float lip = edge > s.frameWidth * 0.75f ? 0.7f : 1f;
                        float grain = 0.95f + 0.05f * Mathf.PerlinNoise(ox + px * 40f, oy + py * 4f);
                        var frame = s.frameColor * (lip * grain);
                        frame.a = 1f;

                        color[i] = frame;
                        dead[i] = frame;
                        // Dim frame so it reads as metal
                        emission[i] = (frame * 0.1f).WithAlpha();
                        normal[i] = flatNormal;
                        continue;
                    }

                    float inner = edge - s.frameWidth;
                    float edgeFade = Mathf.SmoothStep(0f, 1f, inner / Mathf.Max(0.001f, s.edgeFalloff * 0.5f));

                    // Little pyramid prisms
                    float cell = 1f / s.prismCells;
                    float fx = Mathf.Repeat(px / cell, 1f) * 2f - 1f;
                    float fy = Mathf.Repeat(py / cell, 1f) * 2f - 1f;
                    float dirX = Mathf.Abs(fx) > Mathf.Abs(fy) ? Mathf.Sign(fx) : 0f;
                    float dirY = dirX == 0f ? Mathf.Sign(fy) : 0f;
                    float facetShade = 1f + 0.5f * s.prismStrength * (dirX * 0.6f + dirY * 0.4f);

                    float tube = 0f;
                    for (int t = 0; t < s.tubes; t++)
                    {
                        float c = (t + 0.5f) / s.tubes;
                        float d = (py - c) / (0.3f / s.tubes);
                        tube += Mathf.Exp(-d * d);
                    }
                    tube = Mathf.Clamp01(tube);
                    float tubeShade = s.tubes > 0 ? Mathf.Lerp(1f - s.tubeGlow, 1f + s.tubeGlow * 0.3f, tube) : 1f;

                    float n1 = Mathf.PerlinNoise(ox + px * 3f, oy + py * 3f);
                    float n2 = Mathf.PerlinNoise(ox + px * 12f, oy + py * 12f);
                    float grime = Mathf.Clamp01((n1 * 0.7f + n2 * 0.3f - 0.45f) * 2.5f) * s.dirt;
                    grime += (1f - edgeFade) * s.dirt * 0.4f;
                    if (Mathf.PerlinNoise(ox + px * 70f, oy + py * 70f) > 0.83f) grime += s.dirt * 0.6f;
                    grime = Mathf.Clamp01(grime);

                    float light = Mathf.Lerp(0.55f, 1f, edgeFade) * facetShade * tubeShade * (1f - grime * 0.6f);
                    emission[i] = (glowTint * Mathf.Clamp01(light)).WithAlpha();

                    var stain = new Color(0.55f, 0.48f, 0.35f);
                    var lit = Color.Lerp(aged, aged * stain, grime) * Mathf.Lerp(0.9f, 1f, facetShade - 0.5f);
                    color[i] = lit.WithAlpha();

                    var off = Color.Lerp(s.deadColor, s.deadColor * stain, Mathf.Clamp01(grime * 1.4f)) * facetShade;
                    dead[i] = off.WithAlpha();

                    var n = new Vector3(dirX * s.prismStrength, dirY * s.prismStrength, 1f).normalized;
                    normal[i] = new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1f);
                }
            }

            string prefix = Prefix(s);
            Write(prefix + "_color.png", color, w, h, false);
            Write(prefix + "_emission.png", emission, w, h, false);
            Write(prefix + "_normal.png", normal, w, h, true);
            Write(prefix + "_dead_color.png", dead, w, h, false);

            Debug.Log($"EasyBackrooms: made light panel textures at {prefix}_*.png");
            return Load(s);
        }

        static Color WithAlpha(this Color c)
        {
            c.a = 1f;
            return c;
        }

        static void Write(string path, Color32[] pixels, int w, int h, bool isNormal)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, isNormal);
            tex.SetPixels32(pixels);
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            if (AssetImporter.GetAtPath(path) is TextureImporter importer)
            {
                // Clamp stops the frame bleeding
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.textureType = isNormal ? TextureImporterType.NormalMap : TextureImporterType.Default;
                importer.SaveAndReimport();
            }
        }

        static string Prefix(LightPanelTextureSettings settings)
        {
            string path = AssetDatabase.GetAssetPath(settings);
            string folder = string.IsNullOrEmpty(path) ? DefaultFolder(settings) : Path.GetDirectoryName(path).Replace('\\', '/');
            return folder + "/" + settings.fileName;
        }

        static string DefaultFolder(ScriptableObject settings)
        {
            // Art folder next to this package
            string root = EditorAssetUtil.PackageRoot();
            return EditorAssetUtil.IsWritable(root)
                ? root + "/Art/light_panel"
                : "Assets/EasyBackrooms Generated/light_panel";
        }
    }

    [CustomEditor(typeof(LightPanelTextureSettings))]
    public sealed class LightPanelTextureSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();

            if (GUILayout.Button("Generate Textures", GUILayout.Height(28)))
                LightPanelTextureMaker.Generate((LightPanelTextureSettings)target);

            EditorGUILayout.HelpBox("Writes color, emission, normal and dead color PNGs next to this asset. Materials using them update automatically.", MessageType.Info);
        }
    }
}
