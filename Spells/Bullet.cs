using System;
using System.Diagnostics;
using Aiv.Engine;
using Futuridium.Characters;
using OpenTK;

namespace Futuridium.Spells
{
    public sealed class Bullet : Spell
    {
        private const int MinSpeed = 2;

        public new static string spellName = "Energy Bullet";

        private SpriteObject body;
        private bool bounced;
        private Vector2 direction;

        private Vector2 fadeAwayStep = Vector2.Zero;
        private Vector2 startingScale;

        public Bullet(SpellManager spellManager, Character owner) : base(spellManager, owner)
        {
            BaseEnergyUsage = 0;
            BaseEnergyUsagePerSecond = 0;
            KnockBack = Owner.Level.SpellKnockBack;
            ContinuousSpell = false;
            CastSound = "sound_energy_bullet";

            OnDestroy += DestroyEvent;
            //OnStart += StartEvent;
            OnUpdate += UpdateEvent;
            OnStartCollisionCheck += StartCollisionEvent;
            OnEndCollisionCheck += EndCollisionEvent;
            OnCollision += CollisionEvent;
        }

        public override int Order
        {
            get { return base.Order; }
            set
            {
                base.Order = value;
                body.Order = base.Order;
            }
        }

        public float FadeAwayRange { get; set; } = 0.5f;

        public override float X
        {
            get { return base.X; }
            set
            {
                base.X = value;
                body.X = base.X;
            }
        }

        public override float Y
        {
            get { return base.Y; }
            set
            {
                base.Y = value;
                body.Y = base.Y;
            }
        }

        public bool SpawnParticleOnDestroy { get; set; }

        public bool BounceBullet { get; set; } = false;

        public float BounceDelay { get; set; } = 2.5f;

        public double BounceMod { get; set; } = 0.8;

        public override Vector2 Scale
        {
            get { return body.Scale; }
            set { body.Scale = value; }
        }

        private void DestroyEvent(object sender)
        {
            var roomName = Game.Game.Instance.CurrentFloor.CurrentRoom.Name;
            //if (SpawnParticleOnDestroy)
            //{
            //    var particleRadius = body.Width;
            //    if (particleRadius < 1)
            //        particleRadius = 1;
            //    var particleSystem = new ParticleSystem($"{roomName}_{Name}_psys", "homogeneous", 30, (int)particleRadius,
            //        DamageColor,
            //        4,
            //        Speed, 
            //        body.Width * 2)
            //    {
            //        Order = Order,
            //        X = X,
            //        Y = Y,
            //        fade = 200
            //    };
            //    Debug.WriteLine(particleSystem.Name);
            //    Engine.SpawnObject(particleSystem.Name, particleSystem);
            //}

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

        public override void Init()
        {
            base.Init();

            lastPoint = new Vector2(X, Y);
        }

        public override void Start()
        {
            var bulletSprite = (SpriteAsset)Engine.GetAsset("bullet");
            body = new SpriteObject(bulletSprite.Width, bulletSprite.Height)
            {
                Name = Name + "_body",
                CurrentSprite = bulletSprite
            };
            var scaleX = Owner.Level.SpellSize / body.Width;
            Scale = new Vector2(scaleX, scaleX);
            startingScale = new Vector2(scaleX, scaleX);
            Engine.SpawnObject(body);

            AddHitBox("mass", 0, 0, (int) body.BaseWidth, (int) body.BaseHeight);

            base.Start();
        }

        // simulate collision between two GameObject rectangles
        // returns 0: X collision ; 1: Y collision
        private int SimulateCollision(Collision collision)
        {
            var hitBox1 = HitBoxes[collision.HitBox]; // bullet
            var hitBox2 = collision.Other.HitBoxes[collision.OtherHitBox];

            var x2 = (int) (hitBox2.X + collision.Other.X);
            var y2 = (int) (hitBox2.Y + collision.Other.Y);
            var w2 = hitBox2.Width;
            var h2 = hitBox2.Height;
            var w1 = hitBox1.Width;
            var h1 = hitBox1.Height;

            // should have same abs value
            var diffX = (int) (X - lastPoint.X);
            var diffY = (int) (Y - lastPoint.Y);
            Debug.Assert(Math.Abs(diffX) == Math.Abs(diffY));
            // ignores first Step
            // could optimize by starting near second hitbox
            var xCollisions = 0;
            var yCollisions = 0;
            var steps = Math.Max(Math.Abs(diffX), Math.Abs(diffY));
            for (var step = steps; step >= 0; step--)
            {
                var x1 = (int) (hitBox1.X + X - Math.Sign(diffX)*step);
                var y1 = (int) (hitBox1.Y + Y - Math.Sign(diffY)*step);

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
                var otherHitBox = collision.Other.HitBoxes[collision.OtherHitBox];
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
                }
                else if (collisionDirection == 1)
                {
                    direction.Y *= -1;
                }
                else
                {
                    // if the collission simulation failed (ex. reason: multiple moving stuff?)
                    //  then bounce back
                    direction.X *= -1;
                    direction.Y *= -1;
                }
                Speed = (int) (Speed*BounceMod);
                if (Speed <= MinSpeed)
                    Speed = MinSpeed;
                else
                    RangeToGo = (int) (RangeToGo*BounceMod);
                Timer.Set("lastBounce", BounceDelay);
                X = (int) lastPoint.X;
                Y = (int) lastPoint.Y;
                return true;
            }
            Destroy();
            return false;
        }

        private void UpdateEvent(object sender)
        {
            if (Game.Game.Instance.MainWindow != "game")
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
            if (RangeToGo > FadeAwayRange*Range) return;
            if (fadeAwayStep == Vector2.Zero)
            {
                fadeAwayStep = body.Scale/LifeSpan;
            }
            if (body.Scale.Length > 0)
            {
                var startingSize = new Vector2(body.Width, body.Height);
                body.Scale -= fadeAwayStep*DeltaTime;
                var newSize = new Vector2(body.Width, body.Height);
                var diffSize = startingSize - newSize;
                // move the object to keep the same center of position
                body.X += diffSize.X/2;
                body.Y += diffSize.Y/2;
                //HitBoxes["mass"].Height = (int) (body.Width*body.Scale.X);
                //HitBoxes["mass"].Width = (int) (body.Height*body.Scale.Y);
            }
        }

        public override float CalculateDamage(Character enemy, float baseModifier)
        {
            return Owner.Level.Attack*baseModifier*(body.Scale.X / startingScale.X); //((body.Scale.X * 2 + 1) / 3);
        }
    }
}