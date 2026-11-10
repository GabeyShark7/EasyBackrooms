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
        public float fogDensity = 0.028f;

        [Header("Fake Bounce Light")]
        [Tooltip("Fakes light bouncing around the room since it cant be baked in an endless world.")]
        public bool setAmbient = true;

        [Tooltip("Light hitting the floor from above.")]
        public Color floorAmbient = new Color(0.16f, 0.14f, 0.06f);

        [Tooltip("Light hitting the walls from the side.")]
        public Color wallAmbient = new Color(0.26f, 0.22f, 0.09f);

        [Tooltip("Light bouncing up off the carpet onto the ceiling.")]
        public Color ceilingAmbient = new Color(0.34f, 0.29f, 0.13f);

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
                RenderSettings.ambientMode = AmbientMode.Trilight;
                RenderSettings.ambientSkyColor = floorAmbient;
                RenderSettings.ambientEquatorColor = wallAmbient;
                RenderSettings.ambientGroundColor = ceilingAmbient;
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
