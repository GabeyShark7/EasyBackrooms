using UnityEngine;

namespace EasyBackrooms
{
    // Builds a fixed patch of chunks
    [AddComponentMenu("EasyBackrooms/Chunk Builder")]
    public sealed class ChunkBuilder : MonoBehaviour
    {
        [Tooltip("Decides the layout.")]
        public BackroomsProfile profile;

        [Tooltip("Decides the look. Holds your materials.")]
        public BackroomsTheme theme;

        [Range(0, 4), Tooltip("How many chunks out from the center to build.")]
        public int radius = 1;

        [Tooltip("Build automatically on play if nothing is built yet.")]
        public bool buildOnStart = true;

        void Start()
        {
            if (buildOnStart && GetComponentInChildren<ChunkView>() == null) Build();
        }

        public void Build()
        {
            Clear();

            if (profile == null || theme == null)
            {
                Debug.LogWarning("EasyBackrooms: drag a profile and a theme onto the Chunk Builder first.", this);
                return;
            }

            for (int cy = -radius; cy <= radius; cy++)
            {
                for (int cx = -radius; cx <= radius; cx++)
                {
                    var layout = LayoutGenerator.Generate(profile, new Vector2Int(cx, cy));
                    ChunkMesher.Build(layout, profile, theme, transform, cx == radius, cy == radius);
                }
            }
        }

        public void Clear()
        {
            foreach (var view in GetComponentsInChildren<ChunkView>(true))
            {
                view.DestroyMeshes();
                if (Application.isPlaying) Destroy(view.gameObject);
                else DestroyImmediate(view.gameObject);
            }
        }
    }
}
