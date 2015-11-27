using System;
using System.Diagnostics;
using Aiv.Engine;
using OpenTK;

namespace StupidAivGame
{
    public class Bullet : CircleObject
    {
        private const int MINSPEED = 2;

        private const float fadeAwayRange = 0.2f;
        private const float fadeAwayMod = 0.8f;
        private const int minRadius = 1;

        private const int maxLifeSpan = 5000; // dies after 5s
        private readonly int bounceDelay = 250;
        private readonly double bounceMod = 0.8; // speed = bounceMod * speed
        private readonly int range = 500;

        public bool bounceBullet = false;
        private Vector2 direction;

        private int lastBounce;

        private Vector2 lastPoint;
        private int lifeSpan;
        public GameObject owner;

        private int rangeToGo;
        public int speed;
        public int startingSpeed = 25;
        private Vector2 virtPos;
        private double virtRadius;

        public Bullet(GameObject owner, Vector2 direction)
        {
            this.owner = owner;
            var ownerCharacter = owner as Character;
            if (owner != null)
            {
                startingSpeed = ownerCharacter.level.shotSpeed;
                range = ownerCharacter.level.shotRange;
            }
            speed = startingSpeed;
            this.direction = direction;

            rangeToGo = range;
            fill = true;
            order = 6;

            OnDestroy += DestroyEvent;
        }

        private void DestroyEvent(object sender)
        {
            var roomName = ((Game) engine.objects["game"]).currentFloor.currentRoom.name;
            var particleSystem = new ParticleSystem($"{roomName}_{name}_psys", "homogeneous", 80, 800, color, radius/2,
                20, radius*2)
            {
                order = order,
                x = x,
                y = y,
                fade = 200
            };
            Debug.WriteLine(particleSystem.name);
            engine.SpawnObject(particleSystem.name, particleSystem);
        }

        public override void Start()
        {
            AddHitBox("mass", 0, 0, radius*2, radius*2);
            lastPoint = new Vector2(x, y);
        }

        // simulate collision between two GameObject rectangles
        // returns 0: X collision ; 1: Y collision
        public int SimulateCollision(Collision collision)
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
            var diffX = x - (int) lastPoint.X;
            var diffY = y - (int) lastPoint.Y;
            Debug.Assert(Math.Abs(diffX) == Math.Abs(diffY));
            // ignores first step
            // could optimize by starting near second hitbox
            var xCollisions = 0;
            var yCollisions = 0;
            var steps = Math.Max(Math.Abs(diffX), Math.Abs(diffY));
            for (var step = steps; step >= 0; step--)
            {
                var x1 = hitBox1.x + x - Math.Sign(diffX)*step;
                var y1 = hitBox1.y + y - Math.Sign(diffY)*step;

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
            if (bounceBullet)
            {
                var collisionDirection = -1;
                var hitBox = hitBoxes[collision.hitBox];
                var otherHitBox = collision.other.hitBoxes[collision.otherHitBox];
                if (lastBounce > 0)
                    return false;
                if (otherHitBox == null)
                {
                    Debug.WriteLine("Collission with non-character objects not supported.");
                }
                else
                {
                    collisionDirection = SimulateCollision(collision);

                    return BounceOrDie(collisionDirection, otherHitBox);
                    /*this.x = (int)lastPoint.X;
					this.y = (int)lastPoint.Y;*/
                }
                return false;
            }
            Destroy();
            return false;
        }

        // collisionDirection: 0: X collision ; 1: Y collision
        private bool BounceOrDie(int collisionDirection, HitBox colliderHitBox)
        {
            if (bounceBullet)
            {
                Debug.WriteLine("Collision direction:" + collisionDirection);
                if (collisionDirection == 0)
                {
                    direction.X *= -1;
                    virtPos.X = 0;
                }
                else if (collisionDirection == 1)
                {
                    direction.Y *= -1;
                    virtPos.Y = 0;
                }
                else
                {
                    // if the collission simulation failed (ex. reason: multiple moving stuff?)
                    //  then bounce back
                    virtPos.X = 0;
                    virtPos.Y = 0;
                    direction.X *= -1;
                    direction.Y *= -1;
                }
                speed = (int) (speed*bounceMod);
                if (speed <= MINSPEED)
                    speed = MINSPEED;
                else
                    rangeToGo = (int) (rangeToGo*bounceMod);
                lastBounce = bounceDelay;
                x = (int) lastPoint.X;
                y = (int) lastPoint.Y;
                return true;
            }
            Destroy();
            return false;
        }

        public void NextMove()
        {
            if (rangeToGo <= 0)
            {
                Destroy();
            }
            virtPos.X += (int) (speed*direction.X*(deltaTicks/100.0));
            virtPos.Y += (int) (speed*direction.Y*(deltaTicks/100.0));
            if (Math.Abs(virtPos.X) > 1)
            {
                x += (int) virtPos.X;
                virtPos.X -= (int) virtPos.X;
            }
            if (Math.Abs(virtPos.Y) > 1)
            {
                y += (int) virtPos.Y;
                virtPos.Y -= (int) virtPos.Y;
            }
            rangeToGo -= (int) (speed*(deltaTicks/100.0));
        }

        public override void Update()
        {
            // the opposite of the usual solution
            lifeSpan += deltaTicks;
            if (lifeSpan > maxLifeSpan)
                Destroy();
            if (((Game) engine.objects["game"]).mainWindow == "game")
            {
                if (lastBounce > 0)
                    lastBounce -= deltaTicks;
                if (rangeToGo <= fadeAwayRange*range)
                {
                    var deltaRadius = (radius - radius*fadeAwayMod)*(deltaTicks/100.0);
                    if (deltaRadius > 0)
                    {
                        virtRadius += deltaRadius;
                        if (virtRadius > 1.0)
                        {
                            x += (int) virtRadius/2;
                            y += (int) virtRadius/2;
                            radius -= (int) virtRadius;
                            virtRadius -= (int) virtRadius;
                            hitBoxes["mass"].height = radius*2;
                            hitBoxes["mass"].width = radius*2;
                        }
                    }
                }
                // 0 left; 1 top; 2 right; 3 bottom; 4: top-left; 5: top-right; 6: bottom-left; 7: bottom-right
                NextMove();

                var collisions = CheckCollisions();
                if (collisions.Count > 0)
                    Debug.WriteLine("Bullet collides with n." + collisions.Count);
                var bounced = false;
                foreach (var collision in collisions)
                {
                    if (collision.other.name == owner.name || collision.other.name.StartsWith("bullet") ||
                        collision.other.name.StartsWith("orb"))
                        continue;
                    Debug.WriteLine("Bullet hits enemy: " + collision.other.name);
                    if (BounceOrDie(collision))
                        bounced = true;
                    if (collision.other.name.StartsWith("enemy"))
                    {
                        var game = (Game) engine.objects["game"];

                        var enemy = collision.other as Enemy;
                        game.Hits(this, enemy, collision);

                        break;
                    }
                }
                if (bounced)
                    NextMove();
                lastPoint = new Vector2(x, y);
            }
        }
    }
}