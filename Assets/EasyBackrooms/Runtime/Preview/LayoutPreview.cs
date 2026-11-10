using UnityEngine;

namespace EasyBackrooms
{
    // Add this to see layouts as gizmos
    [AddComponentMenu("EasyBackrooms/Layout Preview")]
    public sealed class LayoutPreview : MonoBehaviour
    {
        [Tooltip("The profile to preview.")]
        public BackroomsProfile profile;

        [Range(0, 4), Tooltip("How many chunks out from the center to draw.")]
        public int radius = 1;

        [Header("Colors")]
        public Color wallColor = new Color(0.85f, 0.75f, 0.35f, 1f);
        public Color pillarColor = new Color(0.6f, 0.55f, 0.3f, 1f);
        public Color roomColor = new Color(0.3f, 0.6f, 1f, 0.25f);
        public Color chunkColor = new Color(1f, 1f, 1f, 0.15f);
        public Color lightColor = new Color(1f, 1f, 0.8f, 1f);
        public Color deadLightColor = new Color(0.3f, 0.3f, 0.3f, 1f);

        void OnDrawGizmos()
        {
            if (profile == null) return;

            Gizmos.matrix = transform.localToWorldMatrix;
            for (int cy = -radius; cy <= radius; cy++)
            {
                for (int cx = -radius; cx <= radius; cx++)
                {
                    DrawChunk(LayoutGenerator.Generate(profile, new Vector2Int(cx, cy)));
                }
            }
            Gizmos.matrix = Matrix4x4.identity;
        }

        void DrawChunk(ChunkLayout layout)
        {
            int n = layout.Size;
            float c = profile.cellSize;
            float h = profile.wallHeight;
            float size = profile.ChunkWorldSize;
            var origin = new Vector3(layout.Coord.x * size, 0f, layout.Coord.y * size);

            Gizmos.color = chunkColor;
            Gizmos.DrawWireCube(origin + new Vector3(size * 0.5f, 0f, size * 0.5f), new Vector3(size, 0f, size));

            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    var center = origin + new Vector3((x + 0.5f) * c, 0f, (y + 0.5f) * c);

                    if (layout.IsReserved(x, y))
                    {
                        Gizmos.color = roomColor;
                        Gizmos.DrawCube(center, new Vector3(c, 0.02f, c));
                    }

                    if (layout.HasFlag(x, y, CellFlags.Light | CellFlags.DeadLight))
                    {
                        Gizmos.color = layout.HasFlag(x, y, CellFlags.Light) ? lightColor : deadLightColor;
                        Gizmos.DrawCube(center + Vector3.up * h, new Vector3(0.6f, 0.02f, 1.2f));
                    }

                    var kind = layout.GetCell(x, y);
                    if (kind == CellKind.Pillar)
                    {
                        Gizmos.color = pillarColor;
                        Gizmos.DrawCube(center + Vector3.up * h * 0.5f, new Vector3(c * 0.25f, h, c * 0.25f));
                    }
                    else if (kind == CellKind.Solid)
                    {
                        Gizmos.color = wallColor;
                        Gizmos.DrawCube(center + Vector3.up * h * 0.5f, new Vector3(c, h, c));
                    }
                }
            }

            Gizmos.color = wallColor;
            var vSize = new Vector3(0.1f, h, c);
            var hSize = new Vector3(c, h, 0.1f);

            for (int y = 0; y < n; y++)
            {
                for (int line = 0; line <= n; line++)
                {
                    if (layout.HasVWall(line, y))
                        Gizmos.DrawCube(origin + new Vector3(line * c, h * 0.5f, (y + 0.5f) * c), vSize);
                }
            }

            for (int line = 0; line <= n; line++)
            {
                for (int x = 0; x < n; x++)
                {
                    if (layout.HasHWall(x, line))
                        Gizmos.DrawCube(origin + new Vector3((x + 0.5f) * c, h * 0.5f, line * c), hSize);
                }
            }
        }
    }
}
