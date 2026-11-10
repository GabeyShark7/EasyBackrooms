using UnityEngine;

namespace EasyBackrooms
{
    [CreateAssetMenu(menuName = "EasyBackrooms/Flicker/Random Flicker", fileName = "RandomFlicker")]
    public sealed class RandomFlicker : FlickerPattern
    {
        [Range(0f, 1f), Tooltip("Share of lights that are faulty. The rest stay steady.")]
        public float faultyShare = 0.03f;

        [Range(0f, 1f), Tooltip("How much of the time a faulty light is acting up.")]
        public float activity = 0.2f;

        [Min(0.1f), Tooltip("How fast it stutters while acting up.")]
        public float speed = 9f;

        [Range(0f, 1f), Tooltip("How bright the light is when it dips.")]
        public float lowLevel = 0.05f;

        public override float Evaluate(Vector3 position, float time)
        {
            // Same spot always acts the same
            uint h = Hash.Of(7331, Mathf.RoundToInt(position.x * 10f), Mathf.RoundToInt(position.y * 10f),
                Mathf.RoundToInt(position.z * 10f));

            if ((h & 0xFFFF) / 65535f > faultyShare) return 1f;

            float seed = (h >> 16) * 0.0137f;
            if (Mathf.PerlinNoise(seed, time * 0.4f) < 1f - activity) return 1f;

            return Mathf.PerlinNoise(seed + 31.7f, time * speed) > 0.45f ? 1f : lowLevel;
        }
    }
}
