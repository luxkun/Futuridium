using System;

namespace Futuridium.Game
{
    public class RandomSeed
    {
        public RandomSeed(string seed)
        {
            Seed = seed.GetHashCode();
        }

        public int Seed { get; }

        public Random GetRandom(string name)
        {
            return new Random(Seed + name.GetHashCode());
        }
    }
}