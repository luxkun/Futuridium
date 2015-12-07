using System;

namespace Futuridium
{
    public class RandomSeed
    {
        public RandomSeed(string seed)
        {
            Seed = seed.GetHashCode();
        }

        public int Seed { get; private set; }

        public Random GetRandom(string name)
        {
            return new Random(Seed + name.GetHashCode());
        }
    }
}