using System.Collections.Generic;
using UnityEngine;

namespace EasyBackrooms
{
    [CreateAssetMenu(menuName = "EasyBackrooms/Trackers/Square", fileName = "SquareTracker")]
    public sealed class SquareTracker : ChunkTracker
    {
        public override void GetWanted(Vector2Int center, int radius, List<Vector2Int> result)
        {
            Fill(center, radius, result);
        }

        public static void Fill(Vector2Int center, int radius, List<Vector2Int> result)
        {
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    result.Add(new Vector2Int(center.x + x, center.y + y));
                }
            }
        }
    }
}
