using System;
using System.Linq;
using Aiv.Engine;
using OpenTK;

namespace Futuridium.Spells
{
    class DriveX : Spell
    {
        // starts from Owner.x/y and goes to x,y
        private MultipleRayObject laserLine;

        // resets the laserPoints when ticks >= resetPointsTimer
        private float resetPointsTimer;
        private float resetPointsDelay = 0.2f;

        public DriveX ()
        {
            EnergyUsage = 5;
            EnergyUsagePerSecond = 25;
        }

        public override void Start()
        {
            base.Start();
            laserLine = new MultipleRayObject();
        }

        public override void Update()
        {
            base.Update();

            // this.x and this.y are relative to Owner.x and Owner.y
            NextMove();

            if (resetPointsTimer < 0)
            {
                resetPointsTimer = resetPointsDelay;
                laserLine.points.Clear();
            } else
            {
                resetPointsTimer -= deltaTime;
            }
            UpdateLaserPoints();
        }

        private void UpdateLaserPoints()
        {
            // the higher the lower precision
            Tuple<int, int> lastPointTuple = laserLine.points.Last();
            Vector2 lastPoint = new Vector2(lastPointTuple.Item1, lastPointTuple.Item2);
            Vector2 objective = new Vector2(x, y);
            Vector2 step = new Vector2(Direction.X, Direction.Y).Normalized();

            while ((lastPoint - objective).LengthFast > 1)
            {
                Vector2 newPoint = new Vector2(lastPoint.X, lastPoint.Y);
                newPoint += step;
                if (newPoint != lastPoint) {
                    laserLine.points.Add(Tuple.Create((int)newPoint.X, (int)newPoint.Y));
                    lastPoint = newPoint;
                }
            }
        }
    }
}
