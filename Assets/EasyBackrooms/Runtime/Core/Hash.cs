namespace EasyBackrooms
{
    public static class Hash
    {
        public static uint Mix(uint h)
        {
            h ^= h >> 16;
            h *= 0x7feb352dU;
            h ^= h >> 15;
            h *= 0x846ca68bU;
            h ^= h >> 16;
            return h;
        }

        public static uint Combine(uint a, int b)
        {
            return Mix(a ^ (Mix((uint)b) + 0x9e3779b9U + (a << 6) + (a >> 2)));
        }

        public static uint Of(int seed, int a, int b)
        {
            return Combine(Combine(Mix((uint)seed), a), b);
        }

        public static uint Of(int seed, int a, int b, int c)
        {
            return Combine(Of(seed, a, b), c);
        }

        public static int FromString(string s)
        {
            uint h = 2166136261U;
            foreach (char ch in s)
            {
                h ^= ch;
                h *= 16777619U;
            }
            return (int)h;
        }
    }
}
