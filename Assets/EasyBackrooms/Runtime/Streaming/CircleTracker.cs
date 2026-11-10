using System.Collections.Generic;
using UnityEngine;

namespace EasyBackrooms
{
    [CreateAssetMenu(menuName = "EasyBackrooms/Trackers/Circle", fileName = "CircleTracker")]
    public sealed class CircleTracker : ChunkTracker
    {
        public override void GetWanted(Vector2Int center, int radius, List<Vector2Int> result)
        {
            int limit = radius * radius + radius;
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    if (x * x + y * y > limit) continue;
                    result.Add(new Vector2Int(center.x + x, center.y + y));
                }
            }
        }
    }
}
