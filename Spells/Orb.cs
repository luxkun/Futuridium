using System;
using Aiv.Engine;
using Futuridium.Characters;
using OpenTK;

namespace Futuridium.Spells
{
    public sealed class Orb : Spell
    {
        public new static string spellName = "Energy Orb";
        private float angleTick;

        private SpriteObject body;

        private bool orbStretching; // true: decrease ; false: increase
        private float orbStretchStep;


        public Orb(SpellManager spellManager, Character owner) : base(spellManager, owner)
        {
            ActivatedSpell = true;
            BaseEnergyUsage = (int) (1*Owner.Level.SpellEnergyModifier);
            BaseEnergyUsagePerSecond = (int) (2*Owner.Level.SpellEnergyModifier);
            HitsDelay = Owner.Level.SpellCd*0.05f; // :>
            KnockBack = Owner.Level.SpellKnockBack*0.1f;
            ContinuousSpell = true;
            RoomConstricted = false;
            
            OnDestroy += DestroyEvent;
            //OnStart += StartEvent;
            OnUpdate += UpdateEvent;
        }

        public int OrbRange { get; set; } = 175;

        public float OrbSpeed { get; set; } = 0.9f;

        // % of range to stretch
        public float OrbStretch { get; set; } = 0.4f;

        // in how many seconds should do a full stretch
        public float OrbStretchTime { get; set; } = 0.6f;

        // can activate/disactivate every StartingCd seconds
        public override float StartingCd => 0.5f;

        public override int Order
        {
            get { return base.Order; }
            set
            {
                base.Order = value;
                body.Order = base.Order;
            }
        }

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

        public float DamageModifer { get; } = 0.75f;

        private void DestroyEvent(object sender)
        {
            body.Destroy();
        }

        public override void Init()
        {
            base.Init();
            // randomize start
            angleTick = (float) (new Random(Guid.NewGuid().GetHashCode()).NextDouble()*2*Math.PI);

            lastPoint = new Vector2(X, Y);
        }

        public override Vector2 Scale
        {
            get { return body.Scale; }
            set { body.Scale = value; }
        }

        public override void Start()
        {
            var orbSprite = (SpriteAsset)Engine.GetAsset("orb");
            body = new SpriteObject(orbSprite.Width, orbSprite.Height)
            {
                Name = Name + "_body",
                CurrentSprite = orbSprite
            };
            float scaleX = Owner.Level.SpellSize / body.Width + 0.4f;
            Scale = new Vector2(scaleX, scaleX);
            Engine.SpawnObject(body);

            AddHitBox("mass", 0, 0, (int)body.BaseWidth, (int)body.BaseHeight);

            base.Start();
        }

        public override void NextMove()
        {
            // rotate
            X = Owner.X;
            Y = Owner.Y;
            angleTick += OrbSpeed*DeltaTime;
            var points = new Vector2((int) (Math.Cos(angleTick)*OrbRange*(1 - orbStretchStep)),
                (int) (Math.Sin(angleTick)*OrbRange*(1 - orbStretchStep)));

            X += (int) points.X;
            Y += (int) points.Y;
        }

        public override float CalculateDamage(Character enemy, float baseModifier)
        {
            return Owner.Level.Attack*DamageModifer*baseModifier;
        }

        private void ManageStretch()
        {
            if (orbStretchStep <= 0.0)
            {
                orbStretching = true;
            }
            else if (orbStretchStep >= OrbStretch)
            {
                orbStretching = false;
            }
            orbStretchStep += OrbStretch/OrbStretchTime*(orbStretching ? 1 : -1)*DeltaTime;
        }

        private void UpdateEvent(object sender)
        {
            if (Game.Game.Instance.MainWindow != "game") return;
            ManageStretch();
            ManageCollisions();
        }
    }
}