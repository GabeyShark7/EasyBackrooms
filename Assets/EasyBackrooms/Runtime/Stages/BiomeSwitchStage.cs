using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyBackrooms
{
    [Serializable]
    public sealed class Biome
    {
        public string name = "Biome";

        [Min(0f), Tooltip("How common this biome is compared to the others.")]
        public float weight = 1f;

        [Tooltip("Stages that only run in this biome.")]
        public List<LayoutStage> stages = new List<LayoutStage>();
    }

    // Different stage mixes per area
    [CreateAssetMenu(menuName = "EasyBackrooms/Stages/Biome Switch", fileName = "BiomeSwitch")]
    public sealed class BiomeSwitchStage : LayoutStage
    {
        [Min(0.5f), Tooltip("Rough size of a biome in chunks.")]
        public float biomeSize = 4f;

        public List<Biome> biomes = new List<Biome>();

        public Biome Pick(ChunkLayout layout)
        {
            float total = 0f;
            foreach (var b in biomes) total += Mathf.Max(0f, b.weight);
            if (total <= 0f) return null;

            uint h = Hash.Of(layout.WorldSeed, Salt, 91);
            float ox = (h & 0xFFFF) * 0.0137f;
            float oy = (h >> 16) * 0.0137f;
            float n = Mathf.Clamp01(Mathf.PerlinNoise(ox + layout.Coord.x / biomeSize, oy + layout.Coord.y / biomeSize));

            float pick = n * total;
            foreach (var b in biomes)
            {
                pick -= Mathf.Max(0f, b.weight);
                if (pick <= 0f) return b;
            }
            return biomes[biomes.Count - 1];
        }

        public override void Apply(ChunkLayout layout, ref DetRandom rng)
        {
            var biome = Pick(layout);
            if (biome == null) return;

            foreach (var stage in biome.stages)
            {
                if (stage == null || !stage.Active || stage == this) continue;
                var stageRng = new DetRandom(Hash.Combine(layout.Seed, stage.Salt));
                stage.Apply(layout, ref stageRng);
            }
        }
    }
}
