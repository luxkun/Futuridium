using System;
using System.Diagnostics;
using System.Drawing;
using Aiv.Engine;
using OpenTK;

namespace Futuridium.Spells
{
    public class Spell : GameObject
    {
        public delegate void CastingDoneEventHandler(object sender);

        public delegate void CollisionEventHandler(object sender, Collision collision);

        public delegate void EndCollisionCheckEventHandler(object sender);

        public delegate void StartCollisionCheckEventHandler(object sender);

        public static string spellName = "";
        public static bool roomConstricted = true;

        private Func<bool> castCheck;

        // resetted to 0 everytime a collision check is started
        // - when setted to 1 the collision check returns false
        // - when setted to 2 the collision check returns true
        protected int collisionCheckResult;
        private float hitDelayTimer;
        private bool isCasting;

        protected Vector2 lastPoint;
        private float lifeSpan;
        private int range;

        private int rangeToGo;

        private float vx;
        private float vy;
        public int xOffset;
        public int yOffset;

        public string SpellName => spellName;
        public bool RoomConstricted => roomConstricted;

        // an activated spell is a spell that's activated first time it's casted
        // the next time it's casted it's disactivated
        public virtual bool ActivatedSpell { get; protected set; }

        public virtual int EnergyUsage { get; set; }

        public virtual int EnergyUsagePerSecond { get; set; }

        protected virtual float Vx
        {
            get { return vx; }
            set
            {
                if (Math.Abs(value) > 1)
                {
                    X += (int) value;
                    value -= (int) value;
                }
                vx = value;
            }
        }

        protected virtual float Vy
        {
            get { return vy; }
            set
            {
                if (Math.Abs(value) > 1)
                {
                    Y += (int) value;
                    value -= (int) value;
                }
                vy = value;
            }
        }

        public virtual int X
        {
            get { return x; }
            set { x = value; }
        }

        public virtual int Y
        {
            get { return y; }
            set { y = value; }
        }

        public virtual float KnockBack { get; set; }

        // is either the point where the spell is casted
        // or the Direction where the spell should go
        // some spells (like Orb, or a passive spell) can have no direction
        public virtual Vector2 Direction { get; set; }

        public virtual float Speed { get; protected set; }
        public virtual float StartingSpeed { get; set; } = 250f;

        public virtual Color DamageColor
        {
            get
            {
                // by default 500 is considered "max attack"
                var maxAttack = 500f;
                var minAttackForColorChange = 25f;
                float colorMod;
                if (Owner.Level.Attack > maxAttack)
                    colorMod = 0f;
                else if (Owner.Level.Attack < minAttackForColorChange)
                    colorMod = 1f;
                else
                    colorMod = 1f - (Owner.Level.Attack - minAttackForColorChange)/(maxAttack - minAttackForColorChange);
                return Color.FromArgb((int) (255f*colorMod*0.5f + 125), (int) (255f*colorMod), (int) (255f*colorMod));
            }
        }
        
        public virtual Character Owner { get; set; }

        public virtual float LifeSpan
        {
            get { return lifeSpan; }
            private set
            {
                lifeSpan = value;
                if (value <= 0)
                    Destroy();
            }
        }

        protected virtual int RangeToGo
        {
            get { return rangeToGo; }

            set
            {
                if (value <= 0)
                    Destroy();
                rangeToGo = value;
            }
        }

        protected virtual int Range
        {
            get { return range; }

            set
            {
                range = value;
                RangeToGo = range;
            }
        }

        public virtual Func<bool> CastCheck
        {
            get { return castCheck; }
            set
            {
                castCheck = value;
                IsCasting = true;
            }
        }

        public virtual bool IsCasting
        {
            get { return enabled && isCasting; }
            set
            {
                isCasting = value;
                if (!value)
                    OnCastingDone?.Invoke(this);
            }
        }

        public virtual float HitsDelay { get; set; } = 0.5f;

        public virtual float StartingCd => Owner.Level.SpellCd;

        public bool MovingSpell { get; protected set; } = true;

        public event CastingDoneEventHandler OnCastingDone;
        public event StartCollisionCheckEventHandler OnStartCollisionCheck;
        public event EndCollisionCheckEventHandler OnEndCollisionCheck;
        public event CollisionEventHandler OnCollision;

        public override void Start()
        {
            X = Owner.x + xOffset;
            Y = Owner.y + yOffset;
            OnBeforeUpdate += BeforeUpdate;
            Owner.Level.Energy -= EnergyUsage;
            Speed = StartingSpeed;
            order = Owner.order;
            StartingSpeed = Owner.Level.SpellSpeed;
            if (!ActivatedSpell) { 
                Range = Owner.Level.SpellRange;
                LifeSpan = RangeToGo/Speed; // used in case the spell's speed is decreased
            }
        }

        private void BeforeUpdate(object sender)
        {
            if (EnergyUsagePerSecond != 0)
            {
                var deltaEnergy = EnergyUsagePerSecond*deltaTime;
                if (deltaEnergy > Owner.Level.Energy)
                    Destroy();
                else
                    Owner.Level.Energy -= deltaEnergy;
            }
        }

        private bool EndCollisionCheck(bool result)
        {
            collisionCheckResult = result ? 2 : 1;
            OnEndCollisionCheck?.Invoke(this);
            collisionCheckResult = 0;
            return result;
        }

        protected bool ManageCollisions()
        {
            OnStartCollisionCheck?.Invoke(this);
            if (!enabled)
                return EndCollisionCheck(true);
            if (collisionCheckResult != 0)
                return EndCollisionCheck(collisionCheckResult != 1);
            var collisions = CheckCollisions();
            if (collisions.Count > 0)
                Debug.WriteLine($"{name} {spellName} collides with n. {collisions.Count}");
            var collides = false;
            foreach (var collision in collisions)
            {
                if (collision.other.name == Owner.name || collision.other.name.Contains("spell"))
                    continue;
                Debug.WriteLine($"{name} {spellName} hits enemy: {collision.other.name} ({collision.otherHitBox})");
                collides = true;
                OnCollision?.Invoke(this, collision);
                var other = collision.other as Character;
                if (other != null && hitDelayTimer <= 0)
                {
                    Owner.DoDamage(this, other, collision);
                    hitDelayTimer = HitsDelay;
                    break;
                }

                if (!enabled)
                    break;
                if (collisionCheckResult != 0)
                    return EndCollisionCheck(collisionCheckResult != 1);
            }
            return EndCollisionCheck(collides);
        }

        public override void Update()
        {
            if (Game.Instance.MainWindow != "game") return;
            lastPoint = new Vector2(X, Y);
            base.Update();

            if (hitDelayTimer > 0)
                hitDelayTimer -= deltaTime;

            if (IsCasting && !CastCheck())
            {
                IsCasting = false;
            }

            if (RangeToGo > 0 && LifeSpan > 0)
                LifeSpan -= deltaTime;

            if (MovingSpell)
                NextMove();
        }

        // to use for moving spells (ex. Bullet)
        protected virtual void NextMove()
        {
            var nextStep = Direction*(Speed*deltaTime);
            Vx += nextStep.X;
            Vy += nextStep.Y;

            RangeToGo -= (int) (Speed*deltaTime);
        }

        public virtual float CalculateDamage(Character enemy, float baseModifier)
        {
            return Owner.Level.Attack*(Speed/StartingSpeed)*baseModifier;
        }
    }
}