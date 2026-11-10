using System.IO;
using UnityEditor;
using UnityEngine;

namespace EasyBackrooms.Editor
{
    // URP wants smooth so we flip rough
    public static class SmoothnessMapMaker
    {
        public static string SmoothPathFor(string roughPath)
        {
            string dir = Path.GetDirectoryName(roughPath).Replace('\\', '/');
            string name = Path.GetFileNameWithoutExtension(roughPath);
            int cut = name.ToLowerInvariant().LastIndexOf("_rough");
            string baseName = cut >= 0 ? name.Substring(0, cut) : name;
            return $"{dir}/{baseName}_smooth.png";
        }

        public static Texture2D FromRoughness(Texture2D rough)
        {
            string roughPath = AssetDatabase.GetAssetPath(rough);
            if (string.IsNullOrEmpty(roughPath)) return null;

            int w = rough.width;
            int h = rough.height;

            var rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var previous = RenderTexture.active;
            Graphics.Blit(rough, rt);
            RenderTexture.active = rt;

            var readback = new Texture2D(w, h, TextureFormat.RGBA32, false, true);
            readback.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            readback.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);

            // Metal in red and smooth in alpha
            var pixels = readback.GetPixels32();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color32(0, 0, 0, (byte)(255 - pixels[i].r));
            }
            readback.SetPixels32(pixels);

            string outPath = SmoothPathFor(roughPath);
            File.WriteAllBytes(outPath, readback.EncodeToPNG());
            Object.DestroyImmediate(readback);

            AssetDatabase.ImportAsset(outPath, ImportAssetOptions.ForceUpdate);
            if (AssetImporter.GetAtPath(outPath) is TextureImporter importer)
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = false;
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.alphaIsTransparency = false;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(outPath);
        }
    }
}
