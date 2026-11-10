using UnityEngine;

namespace EasyBackrooms
{
    public static class LayoutGenerator
    {
        public static ChunkLayout Generate(BackroomsProfile profile, Vector2Int coord)
        {
            int size = Mathf.Max(4, profile.chunkCells);
            var layout = new ChunkLayout(size, coord, Hash.Of(profile.seed, coord.x, coord.y));

            if (profile.border != null) BuildBorders(profile, layout);

            // Your stages run top to bottom
            foreach (var stage in profile.stages)
            {
                if (stage == null || !stage.Active) continue;
                var rng = new DetRandom(Hash.Combine(layout.Seed, stage.Salt));
                stage.Apply(layout, ref rng);
            }

            return layout;
        }

        static void BuildBorders(BackroomsProfile profile, ChunkLayout layout)
        {
            int n = layout.Size;
            int cx = layout.Coord.x;
            int cy = layout.Coord.y;
            int s = profile.seed;
            var walls = new bool[n];

            layout.BorderLocked = false;

            BuildSide(profile.border, walls, Hash.Of(s, 1, cx, cy));
            for (int i = 0; i < n; i++) layout.SetVWall(0, i, walls[i]);

            BuildSide(profile.border, walls, Hash.Of(s, 1, cx + 1, cy));
            for (int i = 0; i < n; i++) layout.SetVWall(n, i, walls[i]);

            BuildSide(profile.border, walls, Hash.Of(s, 2, cx, cy));
            for (int i = 0; i < n; i++) layout.SetHWall(i, 0, walls[i]);

            BuildSide(profile.border, walls, Hash.Of(s, 2, cx, cy + 1));
            for (int i = 0; i < n; i++) layout.SetHWall(i, n, walls[i]);

            layout.BorderLocked = true;
        }

        static void BuildSide(BorderStage border, bool[] walls, uint seed)
        {
            System.Array.Clear(walls, 0, walls.Length);
            var rng = new DetRandom(seed);
            border.BuildSide(walls, ref rng);
        }
    }
}
