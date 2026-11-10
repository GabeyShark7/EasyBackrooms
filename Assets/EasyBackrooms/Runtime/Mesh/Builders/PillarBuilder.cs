using UnityEngine;

namespace EasyBackrooms
{
    [CreateAssetMenu(menuName = "EasyBackrooms/Surfaces/Pillars", fileName = "Pillars")]
    public sealed class PillarBuilder : SurfaceBuilder
    {
        [Min(0.05f), Tooltip("Width of each square pillar in meters.")]
        public float width = 1.6f;

        public override void Build(in BuildContext context, MeshData mesh)
        {
            var layout = context.Layout;
            float half = width * 0.5f;
            float h = context.WallHeight;

            for (int y = 0; y < layout.Size; y++)
            {
                for (int x = 0; x < layout.Size; x++)
                {
                    if (layout.GetCell(x, y) != CellKind.Pillar) continue;

                    var center = context.CellCenter(x, y);
                    mesh.AddBox(new Vector3(center.x - half, 0f, center.z - half),
                                new Vector3(center.x + half, h, center.z + half), false, false);
                }
            }
        }
    }
}
