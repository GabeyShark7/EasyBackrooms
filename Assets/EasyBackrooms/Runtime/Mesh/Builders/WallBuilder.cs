using UnityEngine;

namespace EasyBackrooms
{
    [CreateAssetMenu(menuName = "EasyBackrooms/Surfaces/Walls", fileName = "Walls")]
    public sealed class WallBuilder : SurfaceBuilder
    {
        [Min(0.01f), Tooltip("How thick walls are in meters.")]
        public float thickness = 0.15f;

        [Tooltip("Also fill Solid cells with a full block of wall.")]
        public bool buildSolidCells = true;

        public override void Build(in BuildContext context, MeshData mesh)
        {
            var layout = context.Layout;
            int n = layout.Size;
            float c = context.CellSize;
            float h = context.WallHeight;
            float t = thickness * 0.5f;
            float full = n * c;

            // Neighbors build our east and north
            int lastV = context.Owns(ChunkEdges.East) ? n : n - 1;
            int lastH = context.Owns(ChunkEdges.North) ? n : n - 1;

            for (int line = 0; line <= lastV; line++)
            {
                float x = line * c;

                if ((line == 0 && context.Seals(ChunkEdges.West)) || (line == n && context.Seals(ChunkEdges.East)))
                {
                    mesh.AddBox(new Vector3(x - t, 0f, -t), new Vector3(x + t, h, full + t), false, false);
                    continue;
                }

                int y = 0;
                while (y < n)
                {
                    if (!layout.HasVWall(line, y)) { y++; continue; }
                    int start = y;
                    while (y < n && layout.HasVWall(line, y)) y++;

                    mesh.AddBox(new Vector3(x - t, 0f, start * c - t), new Vector3(x + t, h, y * c + t), false, false);
                }
            }

            for (int line = 0; line <= lastH; line++)
            {
                float z = line * c;

                if ((line == 0 && context.Seals(ChunkEdges.South)) || (line == n && context.Seals(ChunkEdges.North)))
                {
                    mesh.AddBox(new Vector3(-t, 0f, z - t), new Vector3(full + t, h, z + t), false, false);
                    continue;
                }

                int x = 0;
                while (x < n)
                {
                    if (!layout.HasHWall(x, line)) { x++; continue; }
                    int start = x;
                    while (x < n && layout.HasHWall(x, line)) x++;

                    mesh.AddBox(new Vector3(start * c - t, 0f, z - t), new Vector3(x * c + t, h, z + t), false, false);
                }
            }

            if (!buildSolidCells) return;

            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    if (layout.GetCell(x, y) != CellKind.Solid) continue;
                    mesh.AddBox(new Vector3(x * c, 0f, y * c), new Vector3((x + 1) * c, h, (y + 1) * c), false, false);
                }
            }
        }
    }
}
