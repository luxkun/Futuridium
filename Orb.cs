using System;
using System.Diagnostics;
using Aiv.Engine;
using OpenTK;

namespace StupidAivGame
{
    public class Orb : CircleObject
    {
        private double _orbStretch;

        private double angleTick;

        public int orbRange = 150;
        public double orbSpeed = 0.08;
        public double orbStretch = 0.25; // orbRange goes from orbRange * orbStretch to orbRange
        private bool orbStretching; // true: decrease ; false: increase
        public int orbStretchSteps = 50;
        private readonly Character owner;

        private Vector2 virtPos;

        public Orb(Character owner)
        {
            order = 5;
            this.owner = owner;
            fill = true;
        }

        public override void Start()
        {
            x = owner.x + orbRange;
            y = owner.y;
            AddHitBox("mass", 0, 0, radius*2, radius*2);
        }

        public Vector2 GetNextStep(double angle)
        {
            return new Vector2((int) (Math.Cos(angle)*orbRange*(1 - _orbStretch)),
                (int) (Math.Sin(angle)*orbRange*(1 - _orbStretch)));
        }

        private void ManageStretch()
        {
            if (_orbStretch <= 0.0)
            {
                orbStretching = true;
            }
            else if (_orbStretch >= orbStretch)
            {
                orbStretching = false;
            }
            _orbStretch += orbStretch/orbStretchSteps*(orbStretching ? 1 : -1);
        }

        public override void Update()
        {
            if (((Game) engine.objects["game"]).mainWindow == "game")
            {
                ManageStretch();
                // rotate
                x = owner.x;
                y = owner.y;
                angleTick += orbSpeed*(deltaTicks/100.0);
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
                    if (collision.other.name.StartsWith("enemy"))
                    {
                        Debug.WriteLine("Orb hits enemy: " + collision.other.name);
                        var game = (Game) engine.objects["game"];

                        var enemy = collision.other as Enemy;
                        // broken, deliberately
                        game.Hits(owner, enemy, collision, null);

                        break;
                    }
                }
            }
        }
    }
}