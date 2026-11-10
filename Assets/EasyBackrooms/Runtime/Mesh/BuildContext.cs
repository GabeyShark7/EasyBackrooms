using UnityEngine;

namespace EasyBackrooms
{
    public readonly struct BuildContext
    {
        public readonly ChunkLayout Layout;
        public readonly float CellSize;
        public readonly float WallHeight;
        public readonly Vector3 WorldOrigin;

        // True if no neighbor builds the edge
        public readonly bool OwnsEastEdge;
        public readonly bool OwnsNorthEdge;

        public BuildContext(ChunkLayout layout, float cellSize, float wallHeight, Vector3 worldOrigin, bool ownsEastEdge, bool ownsNorthEdge)
        {
            Layout = layout;
            CellSize = cellSize;
            WallHeight = wallHeight;
            WorldOrigin = worldOrigin;
            OwnsEastEdge = ownsEastEdge;
            OwnsNorthEdge = ownsNorthEdge;
        }

        public Vector3 CellCenter(int x, int y)
        {
            return new Vector3((x + 0.5f) * CellSize, 0f, (y + 0.5f) * CellSize);
        }
    }
}
