using UnityEngine;

namespace EasyBackrooms
{
    [CreateAssetMenu(menuName = "EasyBackrooms/Surfaces/Walls", fileName = "Walls")]
    public sealed class WallBuilder : SurfaceBuilder
    {
        [Min(0.01f), Tooltip("How thick walls are in meters.")]
        public float thickness = 0.2f;

        [Tooltip("Also fill Solid cells with a full block of wall.")]
        public bool buildSolidCells = true;

        public override void Build(in BuildContext context, MeshData mesh)
        {
            WallRuns.ForEach(context, thickness, (min, max, _) => mesh.AddBox(min, max, false, false));

            if (!buildSolidCells) return;

            var layout = context.Layout;
            float c = context.CellSize;
            float h = context.WallHeight;

            for (int y = 0; y < layout.Size; y++)
            {
                for (int x = 0; x < layout.Size; x++)
                {
                    if (layout.GetCell(x, y) != CellKind.Solid) continue;
                    mesh.AddBox(new Vector3(x * c, 0f, y * c), new Vector3((x + 1) * c, h, (y + 1) * c), false, false);
                }
            }
        }
    }
}
