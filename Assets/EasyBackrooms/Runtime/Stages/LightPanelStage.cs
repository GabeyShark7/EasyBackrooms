using UnityEngine;

namespace EasyBackrooms
{
    [CreateAssetMenu(menuName = "EasyBackrooms/Stages/Light Panels", fileName = "LightPanels")]
    public sealed class LightPanelStage : LayoutStage
    {
        [Min(1), Tooltip("Cells between light panels.")]
        public int spacing = 1;

        [Min(0), Tooltip("Shifts the light grid when Spacing is above 1.")]
        public int offset = 1;

        [Range(0f, 1f), Tooltip("Chance a grid spot actually gets a panel.")]
        public float chance = 0.85f;

        [Range(0f, 1f), Tooltip("Chance a panel is burnt out and dark.")]
        public float deadChance = 0.1f;

        public override void Apply(ChunkLayout layout, ref DetRandom rng)
        {
            int n = layout.Size;
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    if ((x + offset) % spacing != 0 || (y + offset) % spacing != 0) continue;
                    if (layout.GetCell(x, y) != CellKind.Open) continue;
                    if (!rng.Chance(chance)) continue;

                    var flag = rng.Chance(deadChance) ? CellFlags.DeadLight : CellFlags.Light;
                    layout.SetFlag(x, y, flag, true);
                }
            }
        }
    }
}
