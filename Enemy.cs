using System;
using System.Linq;
using Aiv.Engine;
using OpenTK;

namespace Futuridium
{
    public class Enemy : Character
    {
        private const float DelayBeforeActivation = 0.5f;
        private const float Minbestdelta = 0.01f;
        private readonly int maxPointDistance = 400;
        private readonly int minDistance = 10; // calculate this somehow
        // virtual circular radar
        private readonly int radarRadius = 150;
        private bool activated;
        private float lastMove;
        private Vector2 nextStep;

        private Vector2 rndPoint = new Vector2(-1, -1);
        private float timeBeforeActivation;

        public Enemy(string name, string formattedName, string characterName) : base(name, formattedName, characterName)
        {
        }

        // TEMP
        // TODO: A* algorithm if there will ever be obstacles 
        //      (futuro) algoritmo intelligente che mette in conto dove sta andando il player
        private void RandomizeNextObjective()
        {
            var agentV = new Vector2(x, y);
            if (rndPoint.X != -1 && (rndPoint - agentV).Length > minDistance)
            {
                return;
            }
            var rnd = new Random(new Guid().GetHashCode());
            int distance;
            do
            {
                // fix
                var deltaX = rnd.Next(-1*maxPointDistance, maxPointDistance);
                var deltaY = rnd.Next(-1*maxPointDistance, maxPointDistance);
                rndPoint = new Vector2(
                    x + deltaX + 100*Math.Sign(deltaX),
                    y + deltaY + 100*Math.Sign(deltaX));
                distance = Math.Abs(deltaX) + Math.Abs(deltaY);
            } while (rndPoint.X < GameBackground.WallWidth || rndPoint.Y < GameBackground.WallHeight ||
                     rndPoint.X + width > engine.width - GameBackground.WallWidth ||
                     rndPoint.Y + height > engine.height - GameBackground.WallHeight || distance < minDistance);
        }

        private void RandomMove()
        {
            RandomizeNextObjective();
            MoveTo(rndPoint, (int) (Level.Speed*0.5));
        }

        private void Move(Player player)
        {
            var objectiveV = new Vector2(player.x, player.y);
            var agentV = new Vector2(x, y);
            //var paddingV = new Vector2(10, 10);
            var distance = (int) (objectiveV - agentV).Length;
            // if has been hitted the radarRadius is doubled
            if (distance > radarRadius * (Level.Hp < Level.maxHp ? 4 : 2))
            {
                RandomMove();
                return;
            }
            if (rndPoint.X != -1)
                rndPoint = new Vector2(-1, -1);
            MoveTo(objectiveV, Level.Speed);
        }

        // FIX: stop when near objective
        private void MoveTo(Vector2 objectiveV, float speed)
        {
            nextStep = new Vector2();
            var agentV = new Vector2(x, y);
            var player = (Player) engine.objects["player"];
            //var distance = Math.Abs(agentV.X - objectiveV.X) + Math.Abs(agentV.Y + objectiveV.Y);
            // just touching the objective is enough
            if ((Math.Abs(agentV.X - objectiveV.X) > width || Math.Abs(agentV.Y + objectiveV.Y) > height) &&
                !hitBoxes.FirstOrDefault().Value.CollideWith(player.hitBoxes.FirstOrDefault().Value))
                // first or value dependant key?
            {
                nextStep = objectiveV - agentV;
                nextStep.Normalize();
                //Debug.WriteLine("{0} {1} {2} {3} {4}", playerV, agentV, nextStep, bestDelta, level.speed);
                Vx += nextStep.X*deltaTime*speed;
                Vy += nextStep.Y*deltaTime*speed;
            }
        }

        public override GameObject Clone()
        {
            var go = new Enemy(name, FormattedName, CharacterName)
            {
                currentSprite = currentSprite,
                name = name,
                x = x,
                y = y,
                FormattedName = FormattedName,
                CharacterName = CharacterName,
                Level0 = Level0.Clone(),
                UseAnimations = UseAnimations
            };
            /*if (animations != null)
            {
                result.animations = new Dictionary<string, Animation>();
                foreach (string animKey in animations.Keys)
                {
                    result.animations[animKey] = animations[animKey].Clone();
                    result.animations[animKey].Owner = result;
                }
            }*/
            //result.currentAnimation = currentAnimation;
            // ---
            go.LevelCheck();
            return go;
        }

        public override void Update()
        {
            base.Update();
            if (((Game) engine.objects["game"]).MainWindow != "game") return;
            if (!activated)
            {
                if (timeBeforeActivation == 0)
                    timeBeforeActivation = DelayBeforeActivation;
                else
                {
                    if (timeBeforeActivation > 0)
                        timeBeforeActivation -= deltaTime;
                    if (timeBeforeActivation < 0)
                    {
                        activated = true;
                        AddHitBox("enemy_" + name, 0, 0, width, height);
                    }
                }
            }
            else
            {
                if (lastMove > 0)
                    lastMove -= deltaTime;
                if (lastMove <= 0)
                {
                    Move(((Game) engine.objects["game"]).Player);
                    lastMove = 0.005f; // move every 5ms
                }
            }
        }
    }
}