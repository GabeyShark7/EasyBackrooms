using UnityEngine;

namespace EasyBackrooms
{
    [CreateAssetMenu(menuName = "EasyBackrooms/Borders/Doorway Border", fileName = "DoorwayBorder")]
    public sealed class DoorwayBorderStage : BorderStage
    {
        [Range(0f, 1f), Tooltip("How much of each chunk edge is wall. Higher means fewer ways through.")]
        public float wallChance = 0.6f;

        [Min(1), Tooltip("Every chunk edge gets at least this many gaps so you never get stuck.")]
        public int minOpenings = 2;

        public override void BuildSide(bool[] walls, ref DetRandom rng)
        {
            int open = 0;
            for (int i = 0; i < walls.Length; i++)
            {
                walls[i] = rng.Chance(wallChance);
                if (!walls[i]) open++;
            }

            int want = Mathf.Min(minOpenings, walls.Length);
            while (open < want)
            {
                int i = rng.Range(0, walls.Length);
                if (!walls[i]) continue;
                walls[i] = false;
                open++;
            }
        }
    }
}
