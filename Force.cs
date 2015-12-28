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
            base.Update();
            DestroyTimer -= deltaTime;
            if (DestroyTimer <= 0)
                Destroy();
            var direction = Direction * Step * deltaTime;
            var lastPoint = new Vector2(Owner.x, Owner.y);
            var lastVirtPoint = new Vector2(Owner.Vx, Owner.Vy);
            Owner.Vx += direction.X;
            Owner.Vy += direction.Y;
            // TODO: AnyCollision(delegate) in aiv-engine
            if (Owner.CheckCollisions().Count > 0)
            {
                Owner.x = (int)lastPoint.X;
                Owner.y = (int)lastPoint.Y;
                Owner.Vx = lastVirtPoint.X;
                Owner.Vy = lastVirtPoint.Y;
            }
        }
    }
}