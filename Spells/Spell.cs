using System;
using System.Diagnostics;
using Aiv.Engine;
using OpenTK;

namespace Futuridium.Spells
{
    public class Spell : GameObject
    {
        // energy usage per cast

        // helper variables, used to move the object with precision

        // is either the point where the spell is casted
        // or the Direction where the spell should go

        // speed per second

        public int EnergyUsage { get; set; }

        public int EnergyUsagePerSecond { get; set; }

        private float vx;
        private float vy;

        protected float Vx
        {
            get { return vx; }
            set
            {
                if (Math.Abs(value) > 1)
                {
                    X += (int)value;
                    value -= (int)value;
                }
                vx = value;
            }
        }

        protected float Vy
        {
            get { return vy; }
            set
            {
                if (Math.Abs(value) > 1)
                {
                    Y += (int)value;
                    value -= (int)value;
                }
                vy = value;
            }
        }

        private int rangeToGo;
        private int range = 500;

        public virtual int X { get; set; }
        public virtual int Y { get; set; }

        public Vector2 Direction { get; set; }

        public float Speed { get; protected set; }

        public float StartingSpeed { get; set; } = 250f;

        public Character Owner
        {
            get { return owner; }
            set
            {
                owner = value;

                StartingSpeed = owner.Level.shotSpeed;
                Range = owner.Level.shotRange;
            }
        }

        public float CdTimer { get; set; }

        public bool OnCd => CdTimer > 0;

        public float LifeSpan
        {
            get { return lifeSpan; }
            private set
            {
                lifeSpan = value;
                if (value <= 0)
                    Destroy();
            }
        }

        protected int RangeToGo
        {
            get { return rangeToGo; }

            set
            {
                if (value <= 0)
                    Destroy();
                rangeToGo = value;
            }
        }

        protected int Range
        {
            get { return range; }

            set
            {
                range = value;
                RangeToGo = range;
            }
        }

        public string SpellName;
        private Character owner;
        private float lifeSpan;

        public override void Start()
        {
            OnAfterUpdate += AfterUpdate;
            Owner.Level.energy -= EnergyUsage;
            LifeSpan = RangeToGo/Speed; // used in case the spell's speed is decreased
            Speed = StartingSpeed;
        }

        private void AfterUpdate(Object sender)
        {
            Owner.Level.energy -= (int) (EnergyUsagePerSecond * deltaTime);
        }

        public override void Update()
        {
            base.Update();

            LifeSpan -= deltaTime;

            if (OnCd) { 
                CdTimer -= deltaTime;
                Debug.WriteLine(CdTimer);
            }

            NextMove();
        }

        // to use for moving spells (ex. Bullet)
        protected void NextMove()
        {
            Vector2 nextStep = Direction * (Speed * deltaTime);
            Vx += nextStep.X;
            Vy += nextStep.Y;

            RangeToGo -= (int)(Speed * deltaTime);
        }
    }
}
