using UnityEngine;

namespace EasyBackrooms
{
    [CreateAssetMenu(menuName = "EasyBackrooms/Stages/Open Rooms", fileName = "OpenRooms")]
    public sealed class OpenRoomStage : LayoutStage
    {
        [Min(0), Tooltip("Most rooms that can appear in one chunk.")]
        public int maxRooms = 2;

        [Range(0f, 1f), Tooltip("Chance for each of those rooms to actually spawn.")]
        public float roomChance = 0.5f;

        [Min(2), Tooltip("Smallest room side in cells.")]
        public int minSize = 3;

        [Min(2), Tooltip("Biggest room side in cells.")]
        public int maxSize = 7;

        public override void Apply(ChunkLayout layout, ref DetRandom rng)
        {
            int n = layout.Size;
            int lo = Mathf.Min(minSize, n);
            int hi = Mathf.Clamp(maxSize, lo, n);

            for (int r = 0; r < maxRooms; r++)
            {
                if (!rng.Chance(roomChance)) continue;

                int w = rng.Range(lo, hi + 1);
                int h = rng.Range(lo, hi + 1);
                int x0 = rng.Range(0, n - w + 1);
                int y0 = rng.Range(0, n - h + 1);

                for (int y = y0; y < y0 + h; y++)
                {
                    for (int x = x0; x < x0 + w; x++)
                    {
                        layout.SetCell(x, y, CellKind.Open);
                        layout.SetReserved(x, y, true);
                        if (x > x0) layout.SetWall(x, y, Side.West, false);
                        if (y > y0) layout.SetWall(x, y, Side.South, false);
                    }
                }
            }
        }
    }
}
