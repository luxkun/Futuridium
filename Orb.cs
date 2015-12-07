using System;
using System.Diagnostics;
using Aiv.Engine;
using OpenTK;

namespace Futuridium
{
    public class Orb : CircleObject
    {
        private readonly Character owner;
        private double orbStretchStep;

        private double angleTick;

        private bool orbStretching; // true: decrease ; false: increase

        private Vector2 virtPos;

        public int OrbRange { get; set; } = 150;

        public double OrbSpeed { get; set; } = 0.8;

        public double OrbStretch { get; set; } = 0.25;

        public int OrbStretchSteps { get; set; } = 20;

        public Orb(Character owner)
        {
            order = 5;
            this.owner = owner;
            fill = true;
        }

        public override void Start()
        {
            x = owner.x + OrbRange;
            y = owner.y;
            AddHitBox("mass", 0, 0, radius*2, radius*2);
        }

        public Vector2 GetNextStep(double angle)
        {
            return new Vector2((int) (Math.Cos(angle)*OrbRange*(1 - orbStretchStep)),
                (int) (Math.Sin(angle)*OrbRange*(1 - orbStretchStep)));
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
            orbStretchStep += OrbStretch/OrbStretchSteps*(orbStretching ? 1 : -1);
        }

        public override void Update()
        {
            if (((Game) engine.objects["game"]).MainWindow == "game")
            {
                ManageStretch();
                // rotate
                x = owner.x;
                y = owner.y;
                angleTick += OrbSpeed*deltaTime;
                var points = GetNextStep(angleTick);

                virtPos.X += (int) points.X;
                virtPos.Y += (int) points.Y;
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

                var collisions = CheckCollisions();
                foreach (var collision in collisions)
                {
                    var other = collision.other as Character;
                    if (other != null)
                    {
                        Debug.WriteLine("Orb hits enemy: " + other.name);

                        owner.DoDamage(other);

                        break;
                    }
                }
            }
        }
    }
}