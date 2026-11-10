using UnityEngine;
using UnityEngine.Rendering;

namespace EasyBackrooms
{
    // Fog and ambient light for the scene
    [ExecuteAlways]
    [AddComponentMenu("EasyBackrooms/Backrooms Atmosphere")]
    public sealed class BackroomsAtmosphere : MonoBehaviour
    {
        [Header("Fog")]
        [Tooltip("Hides the far edge of the loaded chunks.")]
        public bool fog = true;

        public Color fogColor = new Color(0.24f, 0.21f, 0.09f);

        [Range(0f, 0.3f)]
        public float fogDensity = 0.04f;

        [Header("Ambient")]
        [Tooltip("Lights up the spots no real light reaches.")]
        public bool setAmbient = true;

        public Color ambientColor = new Color(0.34f, 0.29f, 0.12f);

        [Range(0f, 1f), Tooltip("Lower stops the outdoor sky showing in reflections.")]
        public float reflectionIntensity = 0.15f;

        [Header("Camera")]
        [Tooltip("Paint the camera background the fog color so the distance blends in.")]
        public bool matchCameraBackground = true;

        void OnEnable() => Apply();
        void OnValidate() => Apply();

        public void Apply()
        {
            RenderSettings.fog = fog;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;

            if (setAmbient)
            {
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = ambientColor;
            }

            RenderSettings.reflectionIntensity = reflectionIntensity;

            if (!matchCameraBackground) return;
            var cam = Camera.main;
            if (cam == null) return;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = fogColor;
        }
    }
}
