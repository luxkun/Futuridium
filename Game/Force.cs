using Aiv.Engine;
using Futuridium.Characters;
using OpenTK;

namespace Futuridium.Game
{
    internal class Force : GameObject
    {
        public float DestroyTimer { get; set; }

        public Vector2 Direction { get; set; }
        public Character Owner { get; set; }
        public float Step { get; set; }

        public override void Update()
        {
            base.Update();
            DestroyTimer -= DeltaTime;
            if (DestroyTimer <= 0)
                Destroy();
            var direction = Direction*Step*DeltaTime;
            var lastPoint = new Vector2(Owner.X, Owner.Y);
            Owner.X += direction.X;
            Owner.Y += direction.Y;
            if (Owner.HasCollisions())
            {
                Owner.X = (int) lastPoint.X;
                Owner.Y = (int) lastPoint.Y;
            }
        }
    }
}