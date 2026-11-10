using System.Collections.Generic;
using UnityEngine;

namespace EasyBackrooms
{
    // Keep this stage last in the list
    [CreateAssetMenu(menuName = "EasyBackrooms/Stages/Connectivity", fileName = "Connectivity")]
    public sealed class ConnectivityStage : LayoutStage
    {
        [Range(0f, 1f), Tooltip("Chance to knock out extra walls so there are loops, not just a maze.")]
        public float extraOpenings = 0.08f;

        struct Edge
        {
            public int X;
            public int Y;
            public bool Vertical;
        }

        public override void Apply(ChunkLayout layout, ref DetRandom rng)
        {
            int n = layout.Size;
            var parent = new int[n * n];
            for (int i = 0; i < parent.Length; i++) parent[i] = i;

            var walled = new List<Edge>();

            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    if (!layout.IsWalkable(x, y)) continue;

                    if (x + 1 < n && layout.IsWalkable(x + 1, y))
                    {
                        if (layout.HasVWall(x + 1, y)) walled.Add(new Edge { X = x, Y = y, Vertical = true });
                        else Union(parent, x + y * n, x + 1 + y * n);
                    }

                    if (y + 1 < n && layout.IsWalkable(x, y + 1))
                    {
                        if (layout.HasHWall(x, y + 1)) walled.Add(new Edge { X = x, Y = y, Vertical = false });
                        else Union(parent, x + y * n, x + (y + 1) * n);
                    }
                }
            }

            for (int i = walled.Count - 1; i > 0; i--)
            {
                int j = rng.Range(0, i + 1);
                (walled[i], walled[j]) = (walled[j], walled[i]);
            }

            foreach (var e in walled)
            {
                int a = e.X + e.Y * n;
                int b = e.Vertical ? a + 1 : a + n;

                if (Find(parent, a) != Find(parent, b))
                {
                    Open(layout, e);
                    Union(parent, a, b);
                }
                else if (rng.Chance(extraOpenings))
                {
                    Open(layout, e);
                }
            }
        }

        static void Open(ChunkLayout layout, Edge e)
        {
            if (e.Vertical) layout.SetVWall(e.X + 1, e.Y, false);
            else layout.SetHWall(e.X, e.Y + 1, false);
        }

        static int Find(int[] parent, int i)
        {
            while (parent[i] != i)
            {
                parent[i] = parent[parent[i]];
                i = parent[i];
            }
            return i;
        }

        static void Union(int[] parent, int a, int b)
        {
            parent[Find(parent, a)] = Find(parent, b);
        }
    }
}
