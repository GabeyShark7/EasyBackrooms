using UnityEngine;

namespace EasyBackrooms
{
    [CreateAssetMenu(menuName = "EasyBackrooms/Surfaces/Ceiling", fileName = "Ceiling")]
    public sealed class CeilingBuilder : SurfaceBuilder
    {
        public override void Build(in BuildContext context, MeshData mesh)
        {
            var layout = context.Layout;
            float c = context.CellSize;
            float h = context.WallHeight;

            for (int y = 0; y < layout.Size; y++)
            {
                for (int x = 0; x < layout.Size; x++)
                {
                    if (!layout.IsWalkable(x, y)) continue;

                    float x0 = x * c, x1 = x0 + c;
                    float z0 = y * c, z1 = z0 + c;
                    mesh.AddQuad(new Vector3(x0, h, z0), new Vector3(x0, h, z1),
                                 new Vector3(x1, h, z1), new Vector3(x1, h, z0), Vector3.down);
                }
            }
        }
    }
}
