using Aiv.Engine;
using OpenTK;

namespace Futuridium
{
    internal class Force : GameObject
    {
        // if true this force ignore other forces
        public float DestroyTimer { get; set; }
        public Vector2 Direction { get; set; }
        public Character Owner { get; set; }
        public float Step { get; set; }

        public override void Update()
        {
            DestroyTimer -= deltaTime;
            if (DestroyTimer <= 0)
                Destroy();
            var direction = Direction*Step*deltaTime;
            Owner.Vx += direction.X;
            Owner.Vy += direction.Y;
        }
    }
}