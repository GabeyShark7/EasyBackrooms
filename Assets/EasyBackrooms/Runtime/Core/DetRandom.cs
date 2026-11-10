namespace EasyBackrooms
{
    // Use this not UnityEngine Random
    public struct DetRandom
    {
        uint state;

        public DetRandom(uint seed)
        {
            state = Hash.Mix(seed) | 1U;
        }

        public uint NextUInt()
        {
            uint x = state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            state = x;
            return x;
        }

        public float NextFloat()
        {
            return (NextUInt() >> 8) * (1f / 16777216f);
        }

        public int Range(int min, int maxExclusive)
        {
            if (maxExclusive <= min) return min;
            return min + (int)(NextUInt() % (uint)(maxExclusive - min));
        }

        public bool Chance(float probability)
        {
            return NextFloat() < probability;
        }
    }
}
