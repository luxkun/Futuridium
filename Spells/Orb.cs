using Aiv.Engine;
using OpenTK;
using System;

namespace Futuridium.Spells
{
    public sealed class Orb : Spell
    {
        private double angleTick;

        private readonly CircleObject body;

        private bool orbStretching; // true: decrease ; false: increase
        private double orbStretchStep;

        public new static string spellName = "Energy Orb";

        public Orb(SpellManager spellManager, Character owner) : base(spellManager, owner)
        {
            ActivatedSpell = true;
            BaseEnergyUsage = 0;
            BaseEnergyUsagePerSecond = 1;
            HitsDelay = 0.075f; // :>
            KnockBack = 0f;

            body = new CircleObject();

            OnDestroy += DestroyEvent;
            OnStart += StartEvent;
            OnUpdate += UpdateEvent;
        }

        public int OrbRange { get; set; } = 150;

        public double OrbSpeed { get; set; } = 0.8;

        public double OrbStretch { get; set; } = 0.25;

        public int OrbStretchSteps { get; set; } = 20;

        // can activate/disactivate every StartingCd seconds
        public override float StartingCd => 0.5f;

        public bool Fill
        {
            get { return body.fill; }
            set { body.fill = value; }
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

        public int Radius
        {
            get { return body.radius; }
            set { body.radius = value; }
        }

        public float DamageModifer { get; private set; } = 0.75f;

        private void DestroyEvent(object sender)
        {
            body.Destroy();
        }

        private void StartEvent(object sender)
        {
            // TODO: random startx
            body.name = $"{name}_body";
            Fill = true;
            body.color = DamageColor;
            Radius = Owner.Level.SpellSize;
            engine.SpawnObject(body);

            AddHitBox("mass", 0, 0, Radius * 2, Radius * 2);
        }

        public override void NextMove()
        {
            // rotate
            X = Owner.x;
            Y = Owner.y;
            angleTick += OrbSpeed * deltaTime;
            var points = new Vector2((int)(Math.Cos(angleTick) * OrbRange * (1 - orbStretchStep)),
                (int)(Math.Sin(angleTick) * OrbRange * (1 - orbStretchStep)));

            Vx += (int)points.X;
            Vy += (int)points.Y;
        }

        public override float CalculateDamage(Character enemy, float baseModifier)
        {
            return Owner.Level.Attack * DamageModifer * baseModifier;
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
            orbStretchStep += OrbStretch / OrbStretchSteps * (orbStretching ? 1 : -1);
        }

        private void UpdateEvent(object sender)
        {
            if (Game.Instance.MainWindow != "game") return;
            ManageStretch();
            ManageCollisions();
        }
    }
}