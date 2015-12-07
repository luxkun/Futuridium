using OpenTK;

namespace Futuridium
{
    internal class Force
    {
        // if true this force ignore other forces
        public bool BreakChain { get; set; }
        public float DestroyTimer { get; set; }
        public Vector2 Direction { get; set; }
        public Character Owner { get; set; }
        public float Step { get; set; }
    }
}