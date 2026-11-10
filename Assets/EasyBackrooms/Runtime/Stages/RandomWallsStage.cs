using UnityEngine;

namespace EasyBackrooms
{
    [CreateAssetMenu(menuName = "EasyBackrooms/Stages/Random Walls", fileName = "RandomWalls")]
    public sealed class RandomWallsStage : LayoutStage
    {
        [Min(0), Tooltip("How many wall runs to place per chunk.")]
        public int runs = 28;

        [Min(1), Tooltip("Shortest wall run in cells.")]
        public int minLength = 2;

        [Min(1), Tooltip("Longest wall run in cells.")]
        public int maxLength = 6;

        public override void Apply(ChunkLayout layout, ref DetRandom rng)
        {
            int n = layout.Size;
            int maxLen = Mathf.Max(minLength, maxLength);

            for (int r = 0; r < runs; r++)
            {
                bool vertical = rng.Chance(0.5f);
                int length = rng.Range(minLength, maxLen + 1);
                int line = rng.Range(1, n);
                int start = rng.Range(0, n);

                for (int i = 0; i < length; i++)
                {
                    int along = start + i;
                    if (along >= n) break;

                    if (vertical)
                    {
                        if (layout.IsReserved(line - 1, along) || layout.IsReserved(line, along)) continue;
                        layout.SetVWall(line, along, true);
                    }
                    else
                    {
                        if (layout.IsReserved(along, line - 1) || layout.IsReserved(along, line)) continue;
                        layout.SetHWall(along, line, true);
                    }
                }
            }
        }
    }
}
