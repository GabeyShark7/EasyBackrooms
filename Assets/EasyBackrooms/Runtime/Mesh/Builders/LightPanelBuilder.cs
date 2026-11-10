using System.Collections.Generic;
using UnityEngine;

namespace EasyBackrooms
{
    public enum PanelKind
    {
        Lit,
        Dead
    }

    // Use one for lit and one for dead
    [CreateAssetMenu(menuName = "EasyBackrooms/Surfaces/Light Panels", fileName = "LightPanels")]
    public sealed class LightPanelBuilder : SurfaceBuilder
    {
        [Tooltip("Lit panels glow and get real lights. Dead panels are the burnt out ones.")]
        public PanelKind kind = PanelKind.Lit;

        [Header("Tile Snapping")]
        [Min(0f), Tooltip("Size of one ceiling tile in meters. Set it to half your ceiling Tile Size if your texture has 2x2 tiles. 0 turns snapping off.")]
        public float tileGrid = 0.8f;

        [Tooltip("Panel size counted in ceiling tiles.")]
        public Vector2Int panelTiles = new Vector2Int(2, 1);

        [Tooltip("Nudge this if your tile lines dont sit where the panels snap.")]
        public Vector2 gridOffset = Vector2.zero;

        [Header("No Snapping")]
        [Tooltip("Panel size in meters when Tile Grid is 0.")]
        public Vector2 panelSize = new Vector2(0.6f, 1.2f);

        [Header("Placement")]
        [Min(0.001f), Tooltip("How far the panel hangs below the ceiling.")]
        public float drop = 0.01f;

        [Min(0f), Tooltip("How far below the panel the real light sits.")]
        public float lightDrop = 0.15f;

        void Reset()
        {
            addCollider = false;
            shadows = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        public override void Build(in BuildContext context, MeshData mesh)
        {
            var layout = context.Layout;
            var flag = kind == PanelKind.Lit ? CellFlags.Light : CellFlags.DeadLight;
            float y = context.WallHeight - drop;

            for (int cy = 0; cy < layout.Size; cy++)
            {
                for (int cx = 0; cx < layout.Size; cx++)
                {
                    if (!layout.HasFlag(cx, cy, flag)) continue;

                    GetPanel(context, cx, cy, out var c, out var half);
                    mesh.AddQuad(
                        new Vector3(c.x - half.x, y, c.z - half.y), new Vector3(c.x - half.x, y, c.z + half.y),
                        new Vector3(c.x + half.x, y, c.z + half.y), new Vector3(c.x + half.x, y, c.z - half.y),
                        Vector3.down,
                        new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f));
                }
            }
        }

        public override void CollectLightPoints(in BuildContext context, List<Vector3> points)
        {
            if (kind != PanelKind.Lit) return;

            var layout = context.Layout;
            float y = context.WallHeight - drop - lightDrop;

            for (int cy = 0; cy < layout.Size; cy++)
            {
                for (int cx = 0; cx < layout.Size; cx++)
                {
                    if (!layout.HasFlag(cx, cy, CellFlags.Light)) continue;
                    GetPanel(context, cx, cy, out var c, out _);
                    points.Add(new Vector3(c.x, y, c.z));
                }
            }
        }

        void GetPanel(in BuildContext context, int cx, int cy, out Vector3 center, out Vector2 half)
        {
            var local = context.CellCenter(cx, cy);

            if (tileGrid <= 0f)
            {
                center = local;
                half = panelSize * 0.5f;
                return;
            }

            // Snap in world space to match the UVs
            var origin = context.WorldOrigin;
            float wx = Snap(origin.x + local.x - gridOffset.x, tileGrid, panelTiles.x) + gridOffset.x;
            float wz = Snap(origin.z + local.z - gridOffset.y, tileGrid, panelTiles.y) + gridOffset.y;

            center = new Vector3(wx - origin.x, 0f, wz - origin.z);
            half = new Vector2(Mathf.Max(1, panelTiles.x) * tileGrid * 0.5f, Mathf.Max(1, panelTiles.y) * tileGrid * 0.5f);
        }

        static float Snap(float value, float grid, int tiles)
        {
            return tiles % 2 == 0
                ? Mathf.Round(value / grid) * grid
                : (Mathf.Floor(value / grid) + 0.5f) * grid;
        }
    }
}
