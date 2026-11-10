using System;
using UnityEngine;

namespace EasyBackrooms
{
    // Shared by walls and trim
    public static class WallRuns
    {
        // Gives min and max of each wall box
        public static void ForEach(BuildContext context, float thickness, Action<Vector3, Vector3, bool> onRun)
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
                    onRun(new Vector3(x - t, 0f, -t), new Vector3(x + t, h, full + t), true);
                    continue;
                }

                int y = 0;
                while (y < n)
                {
                    if (!layout.HasVWall(line, y)) { y++; continue; }
                    int start = y;
                    while (y < n && layout.HasVWall(line, y)) y++;
                    onRun(new Vector3(x - t, 0f, start * c - t), new Vector3(x + t, h, y * c + t), true);
                }
            }

            for (int line = 0; line <= lastH; line++)
            {
                float z = line * c;

                if ((line == 0 && context.Seals(ChunkEdges.South)) || (line == n && context.Seals(ChunkEdges.North)))
                {
                    onRun(new Vector3(-t, 0f, z - t), new Vector3(full + t, h, z + t), false);
                    continue;
                }

                int x = 0;
                while (x < n)
                {
                    if (!layout.HasHWall(x, line)) { x++; continue; }
                    int start = x;
                    while (x < n && layout.HasHWall(x, line)) x++;
                    onRun(new Vector3(start * c - t, 0f, z - t), new Vector3(x * c + t, h, z + t), false);
                }
            }
        }
    }
}
