namespace LegacyThroughTime.Narrative.Ink
{
    /// <summary>
    /// Knuth's subtractive generator, bit-for-bit identical to seeded <c>System.Random</c>
    /// on .NET Framework / Mono / .NET (the "Net5CompatSeed" algorithm). ink's RANDOM,
    /// shuffle sequences and LIST_RANDOM are defined in terms of it, so owning the
    /// implementation keeps outcomes identical to inklecate and inky on every platform,
    /// IL2CPP included.
    /// </summary>
    struct InkRandom
    {
        const int MBig = int.MaxValue;
        const int MSeed = 161803398;

        readonly int[] _seedArray;
        int _inext, _inextp;

        public InkRandom(int seed)
        {
            _seedArray = new int[56];
            int subtraction = seed == int.MinValue ? int.MaxValue : System.Math.Abs(seed);
            int mj = MSeed - subtraction;
            _seedArray[55] = mj;
            int mk = 1;
            for (int i = 1; i < 55; i++)
            {
                int ii = 21 * i % 55;
                _seedArray[ii] = mk;
                mk = mj - mk;
                if (mk < 0) mk += MBig;
                mj = _seedArray[ii];
            }
            for (int k = 1; k < 5; k++)
            {
                for (int i = 1; i < 56; i++)
                {
                    _seedArray[i] -= _seedArray[1 + (i + 30) % 55];
                    if (_seedArray[i] < 0) _seedArray[i] += MBig;
                }
            }
            _inext = 0;
            _inextp = 21;
        }

        /// <summary>Uniform int in [0, int.MaxValue).</summary>
        public int Next()
        {
            if (++_inext >= 56) _inext = 1;
            if (++_inextp >= 56) _inextp = 1;
            int ret = _seedArray[_inext] - _seedArray[_inextp];
            if (ret == MBig) ret--;
            if (ret < 0) ret += MBig;
            _seedArray[_inext] = ret;
            return ret;
        }

        /// <summary>First output of a generator seeded with <paramref name="seed"/>.</summary>
        public static int First(int seed) => new InkRandom(seed).Next();
    }
}
