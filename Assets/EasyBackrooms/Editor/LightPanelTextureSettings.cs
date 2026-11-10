using UnityEngine;

namespace EasyBackrooms.Editor
{
    // Tweak these then press Generate
    [CreateAssetMenu(menuName = "EasyBackrooms/Light Panel Textures", fileName = "LightPanelTextures", order = 20)]
    public sealed class LightPanelTextureSettings : ScriptableObject
    {
        [Header("Output")]
        [Tooltip("Start of every file name this makes.")]
        public string fileName = "light_panel";

        [Min(64), Tooltip("Texture width in pixels.")]
        public int width = 512;

        [Range(0.5f, 8f), Tooltip("Width divided by height. Match your panel, 1 for a square panel.")]
        public float aspect = 1f;

        [Header("Frame")]
        [Range(0f, 0.2f), Tooltip("Metal frame thickness as a share of the panel height.")]
        public float frameWidth = 0.06f;

        public Color frameColor = new Color(0.8f, 0.78f, 0.72f);

        [Header("Diffuser")]
        public Color diffuserColor = new Color(0.97f, 0.95f, 0.87f);

        [Tooltip("Color of the plastic on burnt out panels.")]
        public Color deadColor = new Color(0.5f, 0.48f, 0.42f);

        [Range(4, 64), Tooltip("How many little prisms fit across the panel height.")]
        public int prismCells = 22;

        [Range(0f, 1f), Tooltip("How strong the prism pattern is.")]
        public float prismStrength = 0.35f;

        [Range(0f, 1f), Tooltip("How much the panel dims toward the frame.")]
        public float edgeFalloff = 0.35f;

        [Header("Tubes")]
        [Range(0, 6), Tooltip("Fluorescent tubes glowing through the plastic.")]
        public int tubes = 3;

        [Range(0f, 1f), Tooltip("How much the tubes stand out.")]
        public float tubeGlow = 0.3f;

        [Header("Wear")]
        [Range(0f, 1f), Tooltip("Dust, stains and dead bugs.")]
        public float dirt = 0.3f;

        [Range(0f, 1f), Tooltip("How yellowed the old plastic is.")]
        public float yellowing = 0.15f;

        public Color yellowTint = new Color(1f, 0.86f, 0.5f);

        [Tooltip("Change for different dirt.")]
        public int seed = 1;
    }
}
