using System;
using System.Diagnostics;
using System.Drawing;
using Aiv.Engine;
using Futuridium.Characters;
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

        private readonly float levelMod = 0.09f;

        private readonly SpellManager spellManager;

        private Func<bool> castCheck;

        // resetted to 0 everytime a collision check is started
        // - when setted to 1 the collision check returns false
        // - when setted to 2 the collision check returns true
        protected int collisionCheckResult;

        private bool isCasting;

        protected Vector2 lastPoint;
        private float lifeSpan;
        private float range;

        private float rangeToGo;

        public Spell(SpellManager spellManager, Character owner)
        {
            this.spellManager = spellManager;
            Owner = owner;

            OnCastingDone += CastingDoneEvent;
            OnDestroy += DestroyEvent;
        }

        public bool RoomConstricted { get; protected set; } = true;

        public float XOffset { get; set; }
        public float YOffset { get; set; }

        public string SpellName => spellName;
        public bool UpdateDirection { get; set; } = false;

        // an activated spell is a spell that's activated first time it's casted
        // the next time it's casted it's disactivated
        public virtual bool ActivatedSpell { get; protected set; } = false;

        public virtual int BaseEnergyUsage { get; set; }

        public virtual int BaseEnergyUsagePerSecond { get; set; }

        // when casted
        public virtual string CastSound { get; set; }
        // while alive
        public virtual string CastingSound { get; set; }

        public virtual int EnergyUsage
        {
            get { return (int) (BaseEnergyUsage*(1 + (Owner.Level.level - 1)*levelMod)); }
        }

        public virtual int EnergyUsagePerSecond
        {
            get { return (int) (BaseEnergyUsagePerSecond*(1 + (Owner.Level.level - 1)*levelMod)); }
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

        protected virtual float RangeToGo
        {
            get { return rangeToGo; }

            set
            {
                if (value <= 0)
                    Destroy();
                rangeToGo = value;
            }
        }

        protected virtual float Range
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
            get { return Enabled && isCasting; }
            set
            {
                isCasting = value;
                if (!value)
                    OnCastingDone?.Invoke(this);
            }
        }

        // such as drivex
        public virtual bool ContinuousSpell { get; protected set; }

        public virtual float HitsDelay { get; set; } = 0.5f;

        public virtual float StartingCd => Owner.Level.SpellCd;

        public bool MovingSpell { get; protected set; } = true;

        private void DestroyEvent(object sender)
        {
            AudioSource.Stop();
        }

        private void CastingDoneEvent(object sender)
        {
            AudioSource.Pause();
        }

        public event CastingDoneEventHandler OnCastingDone;

        public event StartCollisionCheckEventHandler OnStartCollisionCheck;

        public event EndCollisionCheckEventHandler OnEndCollisionCheck;

        public event CollisionEventHandler OnCollision;

        public override void Start()
        {
            base.Start();
            Init();
            if (CastSound != null)
                AudioSource.Play(((AudioAsset)Engine.GetAsset(CastSound)).Clip);
            //Engine.PlaySound(CastSound);
            if (CastingSound != null)
                AudioSource.Play(((AudioAsset)Engine.GetAsset(CastingSound)).Clip, true);
            //CastingSoundObject = Engine.PlaySoundLoop(CastingSound);
        }

        private void BeforeUpdate(object sender)
        {
            if (EnergyUsagePerSecond != 0)
            {
                var deltaEnergy = EnergyUsagePerSecond*DeltaTime;
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
            if (!Enabled)
                return EndCollisionCheck(true);
            if (collisionCheckResult != 0)
                return EndCollisionCheck(collisionCheckResult != 1);
            var collisions = CheckCollisions();
            if (collisions.Count > 0)
                Debug.WriteLine($"{Name} {spellName} collides with n. {collisions.Count}");
            var collides = false;
            foreach (var collision in collisions)
            {
                if (collision.Other.Name == Owner.Name || collision.Other.Name.Contains("spell"))
                    continue;
                if (spellManager.Mask != null && !spellManager.Mask(collision.Other) &&
                    !collision.OtherHitBox.StartsWith("wall", StringComparison.Ordinal))
                    continue;
                Debug.WriteLine($"{Name} {spellName} hits enemy: {collision.Other.Name} ({collision.OtherHitBox})");
                collides = true;
                OnCollision?.Invoke(this, collision);
                var other = collision.Other as Character;
                if (other != null && Timer.Get("hitDelayTimer") <= 0)
                {
                    Owner.DoDamage(this, other, collision);
                    Timer.Set("hitDelayTimer", HitsDelay);
                    break;
                }

                if (!Enabled)
                    break;
                if (collisionCheckResult != 0)
                    return EndCollisionCheck(collisionCheckResult != 1);
            }
            return EndCollisionCheck(collides);
        }

        public override void Update()
        {
            base.Update();
            if (Game.Game.Instance.MainWindow != "game") return;
            lastPoint = new Vector2(X, Y);

            if (IsCasting && (CastCheck == null || !CastCheck()))
            {
                IsCasting = false;
            }

            if (RangeToGo > 0 && LifeSpan > 0)
                LifeSpan -= DeltaTime;

            if (MovingSpell)
                NextMove();
        }

        public virtual void Init()
        {
            X = Owner.X + XOffset;
            Y = Owner.Y + YOffset;
            OnBeforeUpdate += BeforeUpdate;
            Owner.Level.Energy -= EnergyUsage;
            Speed = StartingSpeed;
            Order = Owner.Order - 2;
            StartingSpeed = Owner.Level.SpellSpeed;
            if (!ActivatedSpell)
            {
                Range = Owner.Level.SpellRange;
                LifeSpan = RangeToGo/Speed; // used in case the spell's speed is decreased
            }
        }

        // to use for moving spells (ex. Bullet)
        public virtual void NextMove()
        {
            var nextStep = Direction*(Speed*DeltaTime);
            X += nextStep.X;
            Y += nextStep.Y;

            RangeToGo -= (int) (Speed*DeltaTime);
        }

        public virtual float CalculateDamage(Character enemy, float baseModifier)
        {
            return Owner.Level.Attack*(Speed/StartingSpeed)*baseModifier;
        }
    }
}