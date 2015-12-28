using Aiv.Engine;
using OpenTK;
using System;
using System.Diagnostics;

namespace Futuridium.Spells
{
    public sealed class Bullet : Spell
    {
        private const int MinSpeed = 2;

        private readonly CircleObject body;
        private bool bounced;
        private Vector2 direction;

        private float fadeAwayStep;

        private float virtRadius;

        public new static string spellName = "Energy Bullet";

        public Bullet(SpellManager spellManager, Character owner) : base(spellManager, owner)
        {
            BaseEnergyUsage = 0;
            BaseEnergyUsagePerSecond = 0;
            KnockBack = 1f;

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
                body.order = base.order;
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
                virtRadius = value;
                if (virtRadius >= 1f && Radius >= 1 + (int)virtRadius)
                {
                    var deltaPos = value / 2f;
                    Vx += deltaPos;
                    Vy += deltaPos;
                    Radius -= (int)virtRadius;
                    virtRadius -= (int)virtRadius;
                    hitBoxes["mass"].height = Radius * 2;
                    hitBoxes["mass"].width = Radius * 2;
                }
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
                body.x = base.X;
            }
        }

        public override int Y
        {
            get { return base.Y; }
            set
            {
                base.Y = value;
                body.y = base.Y;
            }
        }

        public bool SpawnParticleOnDestroy { get; set; }

        public bool BounceBullet { get; set; } = false;

        public float BounceDelay { get; set; } = 2.5f;

        public double BounceMod { get; set; } = 0.8;

        private void DestroyEvent(object sender)
        {
            var roomName = Game.Instance.CurrentFloor.CurrentRoom.name;
            if (SpawnParticleOnDestroy)
            {
                var particleRadius = Radius / 2;
                if (particleRadius < 1)
                    particleRadius = 1;
                var particleSystem = new ParticleSystem($"{roomName}_{name}_psys", "homogeneous", 30, particleRadius,
                    DamageColor,
                    400,
                    (int)Speed, Radius)
                {
                    order = order,
                    x = X,
                    y = Y,
                    fade = 200
                };
                Debug.WriteLine(particleSystem.name);
                engine.SpawnObject(particleSystem.name, particleSystem);
            }

            body.Destroy();
        }

        //public static Vector2 PredictDirection(Character from, Character to)
        //{
        //    var agentV = new Vector2(from.x, from.y);
        //    var spellSpeed = from.Level.SpellSpeed;
        //    var timeStep = 0.2f;
        //    var startingPosition = new Vector2(to.x, to.y);
        //    var lastPos = new Vector2();
        //    for (float deltaTime = 0f; deltaTime < 100f; deltaTime += timeStep)
        //    {
        //        var predictedPosition = new Vector2(
        //            to.x + to.MovingDirection.X * to.RealSpeed * deltaTime,
        //            to.y + to.MovingDirection.Y * to.RealSpeed * deltaTime
        //            );
        //        //if ((int)lastPos.X == (int)predictedPosition.X &&
        //        //    (int)lastPos.Y == (int)predictedPosition.Y)
        //        //    continue;
        //        lastPos = predictedPosition;
        //        to.x = (int)predictedPosition.X;
        //        to.y = (int)predictedPosition.Y;
        //        var predictedSpell = (Bullet)from.Shot(to.GetHitCenter() - agentV, simulate: true);
        //        predictedSpell.Init();
        //        predictedSpell.Start();
        //        predictedSpell.deltaTime = deltaTime;
        //        predictedSpell.NextMove();
        //        if (predictedSpell.hitBoxes["mass"].CollideWith(to.hitBoxes["mass"]))
        //            return predictedSpell.Direction;
        //    }
        //    to.x = (int)startingPosition.X;
        //    to.y = (int)startingPosition.Y;
        //    return new Vector2(-1, -1);
        //}

        public override void Init ()
        {
            base.Init();
            Fill = true;
            body.name = name + "_body";
            body.color = DamageColor;

            lastPoint = new Vector2(X, Y);
            Radius = Owner.Level.SpellSize / 2;
            AddHitBox("mass", 0, 0, Radius * 2, Radius * 2);
        }
        private void StartEvent(object sender)
        {
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
            var diffX = X - (int)lastPoint.X;
            var diffY = Y - (int)lastPoint.Y;
            Debug.Assert(Math.Abs(diffX) == Math.Abs(diffY));
            // ignores first Step
            // could optimize by starting near second hitbox
            var xCollisions = 0;
            var yCollisions = 0;
            var steps = Math.Max(Math.Abs(diffX), Math.Abs(diffY));
            for (var step = steps; step >= 0; step--)
            {
                var x1 = hitBox1.x + X - Math.Sign(diffX) * step;
                var y1 = hitBox1.y + Y - Math.Sign(diffY) * step;

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
                if (Timer.Get("lastBounce") > 0)
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
                Speed = (int)(Speed * BounceMod);
                if (Speed <= MinSpeed)
                    Speed = MinSpeed;
                else
                    RangeToGo = (int)(RangeToGo * BounceMod);
                Timer.Set("lastBounce", BounceDelay);
                X = (int)lastPoint.X;
                Y = (int)lastPoint.Y;
                return true;
            }
            Destroy();
            return false;
        }

        private void UpdateEvent(object sender)
        {
            if (Game.Instance.MainWindow != "game")
                return;
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

        private void ManageFade()
        {
            if (RangeToGo > FadeAwayRange * Range) return;
            if (fadeAwayStep == 0)
            {
                fadeAwayStep = (Radius - 1) / LifeSpan;
            }
            var deltaRadius = fadeAwayStep * deltaTime;
            if (deltaRadius > 0)
            {
                VirtRadius += deltaRadius;
            }
        }
    }
}