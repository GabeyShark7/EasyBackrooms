using UnityEngine;

namespace EasyBackrooms
{
    // Long one cell wide hallways
    [CreateAssetMenu(menuName = "EasyBackrooms/Stages/Corridors", fileName = "Corridors")]
    public sealed class CorridorStage : LayoutStage
    {
        [Min(0), Tooltip("Most corridors per chunk.")]
        public int maxCorridors = 2;

        [Range(0f, 1f), Tooltip("Chance for each corridor to spawn.")]
        public float chance = 0.5f;

        [Min(2), Tooltip("Shortest corridor in cells.")]
        public int minLength = 6;

        [Min(2), Tooltip("Longest corridor in cells.")]
        public int maxLength = 14;

        public override void Apply(ChunkLayout layout, ref DetRandom rng)
        {
            int n = layout.Size;
            int hi = Mathf.Clamp(maxLength, 2, n);
            int lo = Mathf.Clamp(minLength, 2, hi);

            for (int i = 0; i < maxCorridors; i++)
            {
                if (!rng.Chance(chance)) continue;

                bool alongX = rng.Chance(0.5f);
                int length = rng.Range(lo, hi + 1);
                int row = rng.Range(1, n - 1);
                int start = rng.Range(0, n - length + 1);

                for (int a = start; a < start + length; a++)
                {
                    int x = alongX ? a : row;
                    int y = alongX ? row : a;
                    if (layout.IsReserved(x, y)) continue;

                    layout.SetCell(x, y, CellKind.Open);
                    layout.SetReserved(x, y, true);

                    if (alongX)
                    {
                        layout.SetWall(x, y, Side.South, true);
                        layout.SetWall(x, y, Side.North, true);
                        if (a > start) layout.SetWall(x, y, Side.West, false);
                    }
                    else
                    {
                        layout.SetWall(x, y, Side.West, true);
                        layout.SetWall(x, y, Side.East, true);
                        if (a > start) layout.SetWall(x, y, Side.South, false);
                    }
                }
            }
        }
    }
}
