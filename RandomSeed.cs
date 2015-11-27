using System;

namespace StupidAivGame
{
    public class RandomSeed
    {
        public int seed;

        public RandomSeed(string seed)
        {
            this.seed = seed.GetHashCode();
        }

        public Random GetRandom(string name)
        {
            return new Random(seed + name.GetHashCode());
        }
    }
}