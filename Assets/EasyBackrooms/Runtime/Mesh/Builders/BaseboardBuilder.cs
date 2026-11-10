using UnityEngine;

namespace EasyBackrooms
{
    // Trim strip along the bottom of walls
    [CreateAssetMenu(menuName = "EasyBackrooms/Surfaces/Baseboard", fileName = "Baseboard")]
    public sealed class BaseboardBuilder : SurfaceBuilder
    {
        [Min(0.01f), Tooltip("Match this to the Walls thickness.")]
        public float wallThickness = 0.2f;

        [Min(0.01f), Tooltip("How tall the trim is in meters.")]
        public float height = 0.1f;

        [Min(0.001f), Tooltip("How far the trim sticks out from the wall.")]
        public float depth = 0.015f;

        public override void Build(in BuildContext context, MeshData mesh)
        {
            float d = depth;
            float e = depth * 0.5f;
            float top = height;

            // Ends stick out so faces dont overlap
            WallRuns.ForEach(context, wallThickness, (min, max, alongZ) =>
            {
                var lo = new Vector3(min.x - (alongZ ? d : e), 0f, min.z - (alongZ ? e : d));
                var hi = new Vector3(max.x + (alongZ ? d : e), top, max.z + (alongZ ? e : d));
                mesh.AddBox(lo, hi, true, false);
            });
        }
    }
}
