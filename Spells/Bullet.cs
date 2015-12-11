using System;
using System.Diagnostics;
using System.Drawing;
using Aiv.Engine;
using Futuridium.Spells;
using OpenTK;

namespace Futuridium
{
    public sealed class Bullet : Spell
    {
        private const int MinSpeed = 2;

        private readonly CircleObject body;
        private bool bounced;
        private Vector2 direction;

        private float fadeAwayStep;

        private float lastBounce;

        private float virtRadius;

        public Bullet()
        {
            EnergyUsage = 0;
            EnergyUsagePerSecond = 0;
            SpellName = "Energy Bullet";
            RoomConstricted = true;

            body = new CircleObject();

            OnDestroy += DestroyEvent;
            OnStart += StartEvent;
            OnUpdate += UpdateEvent;
            OnStartCollisionCheck += StartCollisionEvent;
            OnEndCollisionCheck += EndCollisionEvent;
            OnCollision += CollisionEvent;
        }

        public override int order
        {
            get { return base.order; }
            set
            {
                base.order = value;
                body.order = value;
            }
        }

        public int Radius
        {
            get { return body.radius; }
            set { body.radius = value; }
        }

        private float VirtRadius
        {
            get { return virtRadius; }

            set
            {
                if (value >= 1f && Radius >= 1 + (int) value)
                {
                    var deltaPos = value/2;
                    X += (int) deltaPos;
                    Y += (int) deltaPos;
                    Radius -= (int) value;
                    value -= (int) value;
                    hitBoxes["mass"].height = Radius*2;
                    hitBoxes["mass"].width = Radius*2;
                    if (Radius <= 1)
                        Console.WriteLine(Radius);
                }
                virtRadius = value;
            }
        }

        public float FadeAwayRange { get; set; } = 0.33f;

        public bool Fill
        {
            get { return body.fill; }
            set { body.fill = value; }
        }

        public override int X
        {
            get { return base.X; }
            set
            {
                base.X = value;
                body.x = value;
            }
        }

        public override int Y
        {
            get { return base.Y; }
            set
            {
                base.Y = value;
                body.y = value;
            }
        }

        public Color Color
        {
            get { return body.color; }

            set { body.color = value; }
        }

        public bool SpawnParticleOnDestroy { get; set; }

        public bool BounceBullet { get; set; } = false;

        public float BounceDelay { get; set; } = 2.5f;

        public double BounceMod { get; set; } = 0.8;

        private void DestroyEvent(object sender)
        {
            var roomName = ((Game) engine.objects["game"]).CurrentFloor.CurrentRoom.name;
            if (SpawnParticleOnDestroy)
            {
                var particleRadius = Radius/2;
                if (particleRadius < 1)
                    particleRadius = 1;
                var particleSystem = new ParticleSystem($"{roomName}_{name}_psys", "homogeneous", 30, particleRadius,
                    Color,
                    400,
                    (int) Speed, Radius)
                {
                    order = order,
                    x = X,
                    y = Y,
                    fade = 200
                };
                Debug.WriteLine(particleSystem.name);
                engine.SpawnObject(particleSystem.name, particleSystem);
            }
            enabled = false;

            body.Destroy();
        }

        private void StartEvent(object sender)
        {
            AddHitBox("mass", 0, 0, Radius*2, Radius*2);
            lastPoint = new Vector2(X, Y);
            Fill = true;

            body.name = name + "_body";
            engine.SpawnObject(body);
        }

        // simulate collision between two GameObject rectangles
        // returns 0: X collision ; 1: Y collision
        private int SimulateCollision(Collision collision)
        {
            var hitBox1 = hitBoxes[collision.hitBox]; // bullet
            var hitBox2 = collision.other.hitBoxes[collision.otherHitBox];

            var x2 = hitBox2.x + collision.other.x;
            var y2 = hitBox2.y + collision.other.y;
            var w2 = hitBox2.width;
            var h2 = hitBox2.height;
            var w1 = hitBox1.width;
            var h1 = hitBox1.height;

            // should have same abs value
            var diffX = X - (int) lastPoint.X;
            var diffY = Y - (int) lastPoint.Y;
            Debug.Assert(Math.Abs(diffX) == Math.Abs(diffY));
            // ignores first Step
            // could optimize by starting near second hitbox
            var xCollisions = 0;
            var yCollisions = 0;
            var steps = Math.Max(Math.Abs(diffX), Math.Abs(diffY));
            for (var step = steps; step >= 0; step--)
            {
                var x1 = hitBox1.x + X - Math.Sign(diffX)*step;
                var y1 = hitBox1.y + Y - Math.Sign(diffY)*step;

                var tempxCollisions = Math.Min(x2 + w2, x1 + w1) - Math.Max(x2, x1);
                if (y1 != y2 && y1 + h1 != y2 + h2 && y1 != y2 + h2 && y1 + h1 != y2)
                    tempxCollisions = 0;
                var tempyCollisions = Math.Min(y2 + h2, y1 + h1) - Math.Max(y1, y2);
                if (x1 != x2 && x1 + w1 != x2 + w2 && x1 != x2 + w2 && x1 + w1 != x2)
                    tempyCollisions = 0;
                if (tempxCollisions > xCollisions)
                    xCollisions = tempxCollisions;
                if (tempyCollisions > yCollisions)
                    yCollisions = tempyCollisions;
                // keeping tempcollisions for now
                if (yCollisions > 0 || xCollisions > 0)
                    break;
            }
            var result = -1;
            if (yCollisions < xCollisions)
                result = 1;
            else if (yCollisions > xCollisions || (yCollisions == xCollisions && yCollisions > 0))
                result = 0;
            Debug.WriteLine("Collision Simulation result: {0} ({1} vs {2})", result, xCollisions, yCollisions);

            return result;
        }

        // doesn't and won't handle collision between two moving objects
        private bool BounceOrDie(Collision collision)
        {
            if (BounceBullet)
            {
                var otherHitBox = collision.other.hitBoxes[collision.otherHitBox];
                if (lastBounce > 0)
                    return false;
                if (otherHitBox == null)
                {
                    Debug.WriteLine("Collission with non-character objects not supported.");
                }
                else
                {
                    return BounceOrDie(SimulateCollision(collision));
                    /*this.x = (int)lastPoint.X;
					this.y = (int)lastPoint.Y;*/
                }
                return false;
            }
            Destroy();
            return false;
        }

        // collisionDirection: 0: X collision ; 1: Y collision
        private bool BounceOrDie(int collisionDirection)
        {
            if (BounceBullet)
            {
                Debug.WriteLine("Collision Direction:" + collisionDirection);
                if (collisionDirection == 0)
                {
                    direction.X *= -1;
                    Vx = 0;
                }
                else if (collisionDirection == 1)
                {
                    direction.Y *= -1;
                    Vy = 0;
                }
                else
                {
                    // if the collission simulation failed (ex. reason: multiple moving stuff?)
                    //  then bounce back
                    Vx = 0;
                    Vy = 0;
                    direction.X *= -1;
                    direction.Y *= -1;
                }
                Speed = (int) (Speed*BounceMod);
                if (Speed <= MinSpeed)
                    Speed = MinSpeed;
                else
                    RangeToGo = (int) (RangeToGo*BounceMod);
                lastBounce = BounceDelay;
                X = (int) lastPoint.X;
                Y = (int) lastPoint.Y;
                return true;
            }
            Destroy();
            return false;
        }

        private void UpdateEvent(object sender)
        {
            if (((Game) engine.objects["game"]).MainWindow != "game")
                return;
            if (lastBounce > 0)
                lastBounce -= deltaTime;
            ManageFade();

            ManageCollisions();
        }

        private void CollisionEvent(object sender, Collision collision)
        {
            bounced = BounceOrDie(collision) || bounced;
        }

        private void StartCollisionEvent(object sender)
        {
            bounced = false;
        }

        private void EndCollisionEvent(object sender)
        {
            if (bounced)
                NextMove();
        }

        // TODO: bullet doesn't fade
        private void ManageFade()
        {
            if (RangeToGo > FadeAwayRange*Range) return;
            if (fadeAwayStep == 0)
            {
                fadeAwayStep = (Radius - 1)/LifeSpan;
            }
            var deltaRadius = fadeAwayStep*deltaTime;
            if (deltaRadius > 0)
            {
                VirtRadius += deltaRadius;
            }
        }
    }
}