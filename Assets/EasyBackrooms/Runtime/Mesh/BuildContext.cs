using UnityEngine;

namespace EasyBackrooms
{
    [System.Flags]
    public enum ChunkEdges : byte
    {
        None = 0,
        West = 1,
        East = 2,
        South = 4,
        North = 8,
        All = West | East | South | North
    }

    public readonly struct BuildContext
    {
        public readonly ChunkLayout Layout;
        public readonly float CellSize;
        public readonly float WallHeight;
        public readonly Vector3 WorldOrigin;

        // Edges this chunk builds walls on
        public readonly ChunkEdges OwnedEdges;

        // Edges that get a full solid wall
        public readonly ChunkEdges SealedEdges;

        public BuildContext(ChunkLayout layout, float cellSize, float wallHeight, Vector3 worldOrigin,
            ChunkEdges ownedEdges, ChunkEdges sealedEdges)
        {
            Layout = layout;
            CellSize = cellSize;
            WallHeight = wallHeight;
            WorldOrigin = worldOrigin;
            OwnedEdges = ownedEdges;
            SealedEdges = sealedEdges;
        }

        public bool Owns(ChunkEdges edge) => (OwnedEdges & edge) != 0;
        public bool Seals(ChunkEdges edge) => (SealedEdges & edge) != 0;

        public Vector3 CellCenter(int x, int y)
        {
            return new Vector3((x + 0.5f) * CellSize, 0f, (y + 0.5f) * CellSize);
        }
    }
}
