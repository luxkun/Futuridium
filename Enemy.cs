using System;
using OpenTK;

namespace StupidAivGame
{
    public class Enemy : Character
    {
        private const int delayBeforeActivation = 500;
        private const double MINBESTDELTA = 0.01;
        private bool activated;
        private double lastMove;
        private Vector2 nextStep;
        private int timeBeforeActivation;
        private Vector2 virtPos;

        public Enemy(string name, string formattedName, string characterName) : base(name, formattedName, characterName)
        {
        }

        public override void Start()
        {
        }

        // TEMP
        // TODO: A* algorithm if there will ever be obstacles 
        // TODO: (futuro) algoritmo intelligente che mette in conto dove sta andando il player
        private void Follow(Player player)
        {
            // regga tangente per due punti (x - player.x) / (this.x - player.x) = (y - player.y) / (this.y - player.y)

            var playerV = new Vector2(player.x, player.y);
            var agentV = new Vector2(x, y);
            var paddingV = new Vector2(10, 10);
            //List<Vector> points = new List<Vector> ();
            var distance = (int) ((playerV - agentV).Length*2); // sucks
            double bestDelta = engine.width; // flag?
            nextStep = new Vector2();
            for (var i = 0; i <= distance; i++)
            {
                var newPoint = (playerV - agentV)*((float) i/distance) + agentV;
                //newPoint.X = (int)newPoint.X;
                //newPoint.Y = (int)newPoint.Y;
                //if (!points.Contains(newPoint)) // sucks
                //	points.Add (newPoint);
                double pointDelta = Math.Abs(level.speed - (newPoint - agentV).Length);
                // tries to get point closer to character's speed, usually is perfect or close to
                if (bestDelta > pointDelta)
                {
                    bestDelta = pointDelta;
                    nextStep = newPoint;
                    if (bestDelta <= MINBESTDELTA)
                    {
                        break;
                    }
                }
            }
            if (distance > 0)
            {
                //Console.WriteLine("{0} {1} {2} {3} {4}", playerV, agentV, nextStep, bestDelta, level.speed);
                var utopiaX = (nextStep.X - x);
                var utopiaY = (nextStep.Y - y);
                //if (utopiaX > (playerV.X - paddingV) && this.x < (playerV.X - paddingV))
                //	utopiaX = playerV.X - paddingV;
                virtPos.X = utopiaX*(deltaTicks/100f);
                virtPos.Y = utopiaY*(deltaTicks/100f);

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
            }
        }

        public override void Update()
        {
            base.Update();
            if (((Game) engine.objects["game"]).mainWindow == "game")
            {
                if (!activated)
                {
                    if (timeBeforeActivation == 0)
                        timeBeforeActivation = delayBeforeActivation;
                    else
                    {
                        if (timeBeforeActivation > 0)
                            timeBeforeActivation -= deltaTicks;
                        if (timeBeforeActivation < 0)
                        {
                            activated = true;
                            AddHitBox("enemy_" + name, 0, 0, width, height);
                        }
                    }
                }
                if (activated)
                {
                    //Shot(0);
                    if (lastMove > 0)
                        lastMove -= deltaTicks;
                    if (lastMove <= 0)
                    {
                        Follow(((Game) engine.objects["game"]).player);
                        lastMove = 5; // move every 5ms
                    }
                    //Shot (1);
                }
            }
        }
    }
}