using UnityEngine;

namespace EasyBackrooms
{
    // Put this after Light Panels
    [CreateAssetMenu(menuName = "EasyBackrooms/Stages/Dark Zones", fileName = "DarkZones")]
    public sealed class DarkZoneStage : LayoutStage
    {
        [Min(1f), Tooltip("Rough size of a dark zone in cells.")]
        public float zoneSize = 40f;

        [Range(0f, 1f), Tooltip("Higher means more of the world is dark. 0.3 is a few rare patches.")]
        public float darkness = 0.3f;

        public override void Apply(ChunkLayout layout, ref DetRandom rng)
        {
            uint h = Hash.Of(layout.WorldSeed, Salt, 77);
            float ox = (h & 0xFFFF) * 0.0173f;
            float oy = (h >> 16) * 0.0173f;

            for (int y = 0; y < layout.Size; y++)
            {
                for (int x = 0; x < layout.Size; x++)
                {
                    if (!layout.HasFlag(x, y, CellFlags.Light)) continue;

                    // World space so zones cross chunks
                    var w = layout.WorldCell(x, y);
                    float n = Mathf.PerlinNoise(ox + w.x / zoneSize, oy + w.y / zoneSize);
                    if (n > darkness) continue;

                    layout.SetFlag(x, y, CellFlags.Light, false);
                    layout.SetFlag(x, y, CellFlags.DeadLight, true);
                }
            }
        }
    }
}
