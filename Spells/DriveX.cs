using Aiv.Engine;
using OpenTK;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace Futuridium.Spells
{
    internal sealed class DriveX : Spell
    {
        // starts from Owner.x/y and goes to x,y
        private readonly DriveXRayObject laserLine;

        public DriveX(SpellManager spellManager, Character owner) : base(spellManager, owner)
        {
            BaseEnergyUsage = 3;
            BaseEnergyUsagePerSecond = 5;
            laserLine = new DriveXRayObject();
            laserLine.width = 3;
            KnockBack = 1.33f;
            UpdateDirection = true;

            OnStart += StartEvent;
            OnUpdate += UpdateEvent;
            OnCastingDone += CastingDoneEvent;
            OnDestroy += DestroyEvent;
            OnStartCollisionCheck += StartCollisionEvent;
        }

        public new static string spellName = "Drive-X";

        public override int X
        {
            get { return base.X; }
            set
            {
                base.X = value;
                laserLine.x = value;
            }
        }

        public override int Y
        {
            get { return base.Y; }
            set
            {
                base.Y = value;
                laserLine.y = value;
            }
        }

        public override int order
        {
            get { return base.order; }
            set
            {
                base.order = value;
                laserLine.order = value;
            }
        }

        public int StepSize { get; set; } = 5;

        public float DamageModifer { get; } = 0.66f;

        public override float HitsDelay
        {
            get { return base.HitsDelay * 0.66f; }
            set { base.HitsDelay = value; }
        }

        private void DestroyEvent(object sender)
        {
            laserLine.Destroy();
        }

        private void StartCollisionEvent(object sender)
        {
            var hitbox = hitBoxes["mass"];
            var wallSize = GameBackground.WallWidth;
            // why is this needed?
            //if (hitbox.x + X < wallSize || hitbox.y + Y < wallSize ||
            //    hitbox.x + X > engine.width - wallSize || hitbox.y + Y > engine.height - wallSize)
            //    collisionCheckResult = 2;
        }

        // laser is killed when the casting key is unpressed
        private void CastingDoneEvent(object sender)
        {
            Destroy();
        }

        private void StartEvent(object sender)
        {
            laserLine.color = Color.WhiteSmoke;
            laserLine.SecondaryColor = DamageColor;
            engine.SpawnObject($"{name}_laserLine", laserLine);
            // generic hitbox to check if the tip of the laser is hitting something
            AddHitBox("mass", 0, 0, StepSize, StepSize);
        }

        private void UpdateEvent(object sender)
        {
            if (Game.Instance.MainWindow != "game")
                return;

            laserLine.points.Clear();
            UpdateLaserPoints();
        }

        private void UpdateLaserPoints()
        {
            // TODO: perlin noise invece che random, lighter
            // the higher the lower precision
            var lastPoint = new Vector2();
            if (laserLine.points.Count > 0)
            {
                var lastPointTuple = laserLine.points.Last();
                lastPoint = new Vector2(lastPointTuple.Item1, lastPointTuple.Item2);
            }
            var step = new Vector2(Direction.X, Direction.Y).Normalized() * StepSize;

            var rnd = new Random((int)DateTime.Now.Ticks);
            var halfStepSize = StepSize / 2;
            var stepModifier = (int)(StepSize * 0.66);
            while (laserLine.points.Count == 0 || !ManageCollisions())
            {
                var newPoint = new Vector2(lastPoint.X, lastPoint.Y);
                newPoint += step;
                newPoint.X += rnd.Next(-stepModifier, stepModifier + 1);
                newPoint.Y += rnd.Next(-stepModifier, stepModifier + 1);
                if (newPoint != lastPoint)
                {
                    laserLine.points.Add(Tuple.Create((int)newPoint.X, (int)newPoint.Y));
                    lastPoint = newPoint;
                }
                hitBoxes["mass"].x = (int)newPoint.X - halfStepSize;
                hitBoxes["mass"].y = (int)newPoint.Y - halfStepSize;
            }
        }

        public override void NextMove()
        {
            // the laser doesn't move
            X = Owner.x + xOffset;
            Y = Owner.y + yOffset;
        }

        public override float CalculateDamage(Character enemy, float baseModifier)
        {
            return Owner.Level.Attack * DamageModifer * baseModifier;
        }
    }

    public sealed class DriveXRayObject : MultipleRayObject
    {
        private Pen secondaryPen;

        public int Waves { get; set; } = 5;

        public Color SecondaryColor { get; set; }

        public override void Draw()
        {
            base.Draw();
            if (secondaryPen == null)
            {
                pen.Width = 4;
                secondaryPen = new Pen(SecondaryColor, pen.Width / 2) { DashCap = DashCap.Round };
                pen.DashCap = DashCap.Round;
            }
            for (var i = 1; i < points.Count; i++)
            {
                // since engine is going to use only opentk we are going to use opentk
                var deltaX = points[i - 1].Item1 - points[i].Item1;
                var deltaY = points[i - 1].Item2 - points[i].Item2;
                for (var s = 0; s < Waves && (s == 0 || i > 5); s++)
                {
                    engine.workingGraphics.DrawLine(
                        s == 0 ? pen : secondaryPen, x + points[i - 1].Item1 + deltaX * s,
                        y + points[i - 1].Item2 + deltaY * s,
                        x + points[i].Item1 + deltaX * s, y + points[i].Item2 + deltaY * s);
                }
            }
        }
    }
}