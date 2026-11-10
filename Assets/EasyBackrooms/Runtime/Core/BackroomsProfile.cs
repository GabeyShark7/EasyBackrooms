using System.Collections.Generic;
using UnityEngine;

namespace EasyBackrooms
{
    [CreateAssetMenu(menuName = "EasyBackrooms/Empty Profile", fileName = "BackroomsProfile", order = 1)]
    public sealed class BackroomsProfile : ScriptableObject
    {
        [Header("World")]
        [Tooltip("Same seed gives the same backrooms every time.")]
        public int seed = 12345;

        [Min(4), Tooltip("How many cells across one chunk is.")]
        public int chunkCells = 16;

        [Min(0.5f), Tooltip("Width of one cell in meters.")]
        public float cellSize = 2f;

        [Min(1f), Tooltip("Floor to ceiling height in meters.")]
        public float wallHeight = 2.7f;

        [Header("Stages")]
        [Tooltip("Decides the walls on chunk edges so neighbors line up. Leave empty for open edges.")]
        public BorderStage border;

        [Tooltip("Runs top to bottom on every chunk. Add, remove or reorder freely.")]
        public List<LayoutStage> stages = new List<LayoutStage>();

        public float ChunkWorldSize => chunkCells * cellSize;
    }
}
