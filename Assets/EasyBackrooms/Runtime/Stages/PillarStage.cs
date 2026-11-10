using UnityEngine;

namespace EasyBackrooms
{
    public enum PillarPattern
    {
        Random,
        Grid
    }

    [CreateAssetMenu(menuName = "EasyBackrooms/Stages/Pillars", fileName = "Pillars")]
    public sealed class PillarStage : LayoutStage
    {
        [Tooltip("Random scatters pillars. Grid lines them up like a pillar hall.")]
        public PillarPattern pattern = PillarPattern.Grid;

        [Range(0f, 1f), Tooltip("Chance per cell when the pattern is Random.")]
        public float chance = 0.12f;

        [Min(2), Tooltip("Cells between pillars when the pattern is Grid.")]
        public int spacing = 2;

        [Tooltip("Only put pillars inside open rooms.")]
        public bool onlyInRooms = true;

        public override void Apply(ChunkLayout layout, ref DetRandom rng)
        {
            int n = layout.Size;
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    if (layout.GetCell(x, y) != CellKind.Open) continue;
                    if (onlyInRooms && !layout.IsReserved(x, y)) continue;

                    bool place = pattern == PillarPattern.Grid
                        ? x % spacing == 0 && y % spacing == 0
                        : rng.Chance(chance);

                    if (place) layout.SetCell(x, y, CellKind.Pillar);
                }
            }
        }
    }
}
