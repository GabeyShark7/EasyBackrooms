using System.Collections.Generic;
using UnityEngine;

namespace EasyBackrooms
{
    // Inherit this to pick what loads
    public abstract class ChunkTracker : ScriptableObject
    {
        public abstract void GetWanted(Vector2Int center, int radius, List<Vector2Int> result);
    }
}
