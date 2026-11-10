using UnityEngine;

namespace EasyBackrooms
{
    public enum CellKind : byte
    {
        Open,
        Pillar,
        Solid
    }

    // User bits are free for your stages
    [System.Flags]
    public enum CellFlags : ushort
    {
        None = 0,
        Light = 1,
        DeadLight = 2,
        User1 = 256,
        User2 = 512,
        User3 = 1024,
        User4 = 2048
    }

    public enum Side : byte
    {
        West,
        East,
        South,
        North
    }

    public sealed class ChunkLayout
    {
        public int Size { get; }
        public Vector2Int Coord { get; }
        public uint Seed { get; }
        public int WorldSeed { get; }

        readonly CellKind[] cells;
        readonly bool[] reserved;
        readonly CellFlags[] flags;
        readonly bool[] vWalls;
        readonly bool[] hWalls;

        // Edges are locked so chunks line up
        internal bool BorderLocked = true;

        public ChunkLayout(int size, Vector2Int coord, uint seed, int worldSeed = 0)
        {
            Size = size;
            Coord = coord;
            Seed = seed;
            WorldSeed = worldSeed;
            cells = new CellKind[size * size];
            reserved = new bool[size * size];
            flags = new CellFlags[size * size];
            vWalls = new bool[(size + 1) * size];
            hWalls = new bool[size * (size + 1)];
        }

        // Cell position across the whole world
        public Vector2Int WorldCell(int x, int y) => new Vector2Int(Coord.x * Size + x, Coord.y * Size + y);

        public bool InBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Size && y < Size;
        }

        public CellKind GetCell(int x, int y) => cells[x + y * Size];
        public void SetCell(int x, int y, CellKind kind) => cells[x + y * Size] = kind;
        public bool IsWalkable(int x, int y) => GetCell(x, y) != CellKind.Solid;

        public bool IsReserved(int x, int y) => reserved[x + y * Size];
        public void SetReserved(int x, int y, bool value) => reserved[x + y * Size] = value;

        public CellFlags GetFlags(int x, int y) => flags[x + y * Size];
        public bool HasFlag(int x, int y, CellFlags flag) => (flags[x + y * Size] & flag) != 0;

        public void SetFlag(int x, int y, CellFlags flag, bool on)
        {
            int i = x + y * Size;
            flags[i] = on ? flags[i] | flag : flags[i] & ~flag;
        }

        public bool HasVWall(int line, int row) => vWalls[line + row * (Size + 1)];
        public bool HasHWall(int column, int line) => hWalls[column + line * Size];

        public bool IsBorderLine(int line) => line == 0 || line == Size;

        public bool SetVWall(int line, int row, bool on)
        {
            if (BorderLocked && IsBorderLine(line)) return false;
            vWalls[line + row * (Size + 1)] = on;
            return true;
        }

        public bool SetHWall(int column, int line, bool on)
        {
            if (BorderLocked && IsBorderLine(line)) return false;
            hWalls[column + line * Size] = on;
            return true;
        }

        public bool HasWall(int x, int y, Side side)
        {
            return side switch
            {
                Side.West => HasVWall(x, y),
                Side.East => HasVWall(x + 1, y),
                Side.South => HasHWall(x, y),
                _ => HasHWall(x, y + 1)
            };
        }

        public bool SetWall(int x, int y, Side side, bool on)
        {
            return side switch
            {
                Side.West => SetVWall(x, y, on),
                Side.East => SetVWall(x + 1, y, on),
                Side.South => SetHWall(x, y, on),
                _ => SetHWall(x, y + 1, on)
            };
        }
    }
}
