using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Aiv.Engine;
using Aiv.Fast2D;
using Futuridium.Characters;
using Futuridium.World;
using OpenTK;

namespace Futuridium.Spells
{
    internal sealed class DriveX : Spell
    {
        public new static string spellName = "Drive-X";
        // starts from Owner.x/y and goes to x,y
        private readonly DriveXRayObject laserLine;

        public DriveX(SpellManager spellManager, Character owner) : base(spellManager, owner)
        {
            BaseEnergyUsage = (int) (3*Owner.Level.SpellEnergyModifier);
            BaseEnergyUsagePerSecond = (int) (5*Owner.Level.SpellEnergyModifier);
            KnockBack = Owner.Level.SpellKnockBack;
            HitsDelay = Owner.Level.SpellCd*0.4f; // :>
            UpdateDirection = true;
            ContinuousSpell = true;
            CastingSound = "sound_drivex";

            laserLine = new DriveXRayObject();

            OnStart += StartEvent;
            OnUpdate += UpdateEvent;
            OnCastingDone += CastingDoneEvent;
            OnDestroy += DestroyEvent;
            OnStartCollisionCheck += StartCollisionEvent;
        }

        public override float X
        {
            get { return base.X; }
            set
            {
                base.X = value;
                laserLine.X = value;
            }
        }

        public override float Y
        {
            get { return base.Y; }
            set
            {
                base.Y = value;
                laserLine.Y = value;
            }
        }

        public override int Order
        {
            get { return base.Order; }
            set
            {
                base.Order = value;
                laserLine.Order = value;
            }
        }

        public int StepSize { get; set; } = 15;

        public float DamageModifer { get; } = 0.66f;

        private void DestroyEvent(object sender)
        {
            laserLine.Destroy();
        }

        private void StartCollisionEvent(object sender)
        {
            var hitbox = HitBoxes["mass"];
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
            laserLine.Size = new Vector2(StepSize, 4f);
            laserLine.Color = Color.WhiteSmoke;
            laserLine.SecondaryColor = DamageColor;
            Engine.SpawnObject($"{Name}_laserLine", laserLine);
            // generic hitbox to check if the tip of the laser is hitting something
            AddHitBox("mass", 0, 0, StepSize, StepSize);
        }

        private void UpdateEvent(object sender)
        {
            if (Game.Game.Instance.MainWindow != "game")
                return;

            if (Timer.Get($"{Name}_laserUpdateTimer") <= 0f)
            {
                Timer.Set($"{Name}_laserUpdateTimer", 0.033f);
                UpdateLaserPoints();
            }
        }

        private void UpdateLaserPoints()
        {
            // TODO: perlin noise invece che random, lighter
            // the higher the lower precision
            laserLine.Points.Clear();
            var lastPoint = new Vector2();
            if (laserLine.Points.Count > 0)
            {
                var lastPointTuple = laserLine.Points.Last();
                lastPoint = new Vector2(lastPointTuple.X, lastPointTuple.Y);
            }
            var vect = new Vector2(Direction.X, Direction.Y);
            vect.Normalize();
            var step = vect*StepSize;

            var rnd = new Random((int) DateTime.Now.Ticks);
            var halfStepSize = StepSize/2;
            var stepModifier = (int) (StepSize*0.66);
            var newPoint = new Vector2(X, Y);
            while (laserLine.Points.Count == 0 || !ManageCollisions())
            {
                newPoint = new Vector2(lastPoint.X, lastPoint.Y);
                newPoint += step;
                newPoint.X += rnd.Next(-stepModifier, stepModifier + 1);
                newPoint.Y += rnd.Next(-stepModifier, stepModifier + 1);
                if (newPoint != lastPoint)
                {
                    laserLine.Points.Add(new Vector2((int) newPoint.X, (int) newPoint.Y));
                    lastPoint = newPoint;
                }
                HitBoxes["mass"].X = (int) newPoint.X - halfStepSize;
                HitBoxes["mass"].Y = (int) newPoint.Y - halfStepSize;
            }
        }

        public override void NextMove()
        {
            // the laser doesn't move
            X = Owner.X + XOffset;
            Y = Owner.Y + YOffset;
        }

        public override float CalculateDamage(Character enemy, float baseModifier)
        {
            return Owner.Level.Attack*DamageModifer*baseModifier;
        }
    }

    public sealed class DriveXRayObject : GameObject
    {
        private Box box;

        public DriveXRayObject()
        {
            Points = new List<Vector2>();
        }

        public int Waves { get; set; } = 6;

        public Color Color { get; set; }
        public Color SecondaryColor { get; set; }

        public List<Vector2> Points { get; set; }

        public Vector2 Size { get; set; }

        public override void Start()
        {
            box = new Box((int) (Size.X*1.5f), (int) Size.Y);
            box.Fill = true;
            box.Color = Color;
        }

        public override void Draw()
        {
            base.Draw();
            for (var i = 1; i < Points.Count; i++)
            {
                //var deltaX = Points[i - 1].X - Points[i].X;
                //var deltaY = Points[i - 1].Y - Points[i].Y;
                var delta = Points[i] - Points[i - 1];
                delta.Normalize();
                for (var s = 1; s <= Waves && (s == 1 || i > 5); s++)
                {
                    box.Rotation = (float) Math.Atan2(delta.Y, delta.X)*s;
                    box.position = new Vector2(
                        DrawX + Points[i - 1].X + delta.X,
                        DrawY + Points[i - 1].Y + delta.Y);
                    box.Draw();
                    //Engine.workingGraphics.DrawLine(
                    //    s == 0 ? pen : secondaryPen, x + points[i - 1].Item1 + deltaX * s - engine.Camera.X,
                    //    y + points[i - 1].Item2 + deltaY * s - engine.Camera.Y,
                    //    x + points[i].Item1 + deltaX * s - engine.Camera.X, y + points[i].Item2 + deltaY * s - engine.Camera.Y);
                }
            }
        }
    }
}