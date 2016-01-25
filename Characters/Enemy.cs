using System;
using System.Collections.Generic;
using System.Linq;
using Aiv.Engine;
using Futuridium.Game;
using Futuridium.Spells;
using Futuridium.World;
using OpenTK;

namespace Futuridium.Characters
{
    public class Enemy : Character
    {
        private readonly int maxPointDistance = 600;
        private readonly int minDistance = 10; // calculate this somehow

        // virtual circular radar
        private readonly int radarRadius = 175;
        private readonly bool useAI = false; // TESTING

        private bool isShotting;

        private Vector2 rndPoint = new Vector2(-1, -1);
        private State state = State.Normal;

        public Enemy(string name, string formattedName, string characterName, int width, int height) : base(name, formattedName, characterName, width, height)
        {
            SpawnParticleOnDestroy = false;

            OnStart += StartEvent;
            OnAfterUpdate += UpdateEvent;
            OnDestroy += DestroyEvent;
        }

        private void DestroyEvent(object sender)
        {
            //AudioSource.Play(((AudioAsset) Engine.GetAsset("sound_death")).Clip);
            //Engine.PlaySound("sound_death");
        }

        private void StartEvent(object sender)
        {
            SpellManager.Mask = (GameObject enemy) => enemy is Player;
        }

        private void RandomizeNextObjective()
        {
            var agentV = new Vector2(X, Y);
            if (rndPoint.X != -1 && (rndPoint - agentV).Length > minDistance)
            {
                return;
            }
            var rnd = new Random((int) DateTime.Now.Ticks);
            int distance;
            var roomWidth = Game.Game.Instance.CurrentFloor.CurrentRoom.Width;
            var roomHeight = Game.Game.Instance.CurrentFloor.CurrentRoom.Height;
            do
            {
                // fix
                var deltaX = rnd.Next(-1*maxPointDistance, maxPointDistance);
                var deltaY = rnd.Next(-1*maxPointDistance, maxPointDistance);
                rndPoint = new Vector2(
                    X + deltaX + 100*Math.Sign(deltaX),
                    Y + deltaY + 100*Math.Sign(deltaX));
                distance = Math.Abs(deltaX) + Math.Abs(deltaY);
            } while (rndPoint.X < GameBackground.WallWidth || rndPoint.Y < GameBackground.WallHeight ||
                     rndPoint.X + Width > roomWidth - GameBackground.WallWidth ||
                     rndPoint.Y + Height > roomHeight - GameBackground.WallHeight || distance < minDistance);
        }

        private void RandomMove()
        {
            RandomizeNextObjective();
            RealSpeed = Level.Speed*0.5f;
            MoveTo(rndPoint, RealSpeed);
        }

        // TODO: A* algorithm if there will ever be obstacles
        private void SearchAndDestroy(Character character)
        {
            // center of enemy body
            var objectiveV = character.GetHitCenter();
            var agentV = new Vector2(X, Y);
            //var paddingV = new Vector2(10, 10);
            var diffV = objectiveV - agentV;
            var distance = (int) diffV.Length;

            // if has been hitted the radarRadius is doubled
            if (state >= State.Normal && Level.Hp < Level.MaxHp)
                state = State.Damaged;

            if (distance > radarRadius*(1 + (int) state))
            {
                RandomMove();
                return;
            }

            // check if the the enemy is in spell range when is warned
            isShotting = state >= State.Warned && Level.SpellList.Contains(typeof (Bullet)) &&
                         Level.SpellRange >= distance;
            if (isShotting)
            {
                // predict enemy position when the shot should hit the enemy
                //diffV = Bullet.PredictDirection(this, character);
                SpellManager.ChangeSpell(typeof (Bullet));
                var spell = Shot(
                    diffV,
                    recalculateDirection: (Spell s) => objectiveV - new Vector2(X + s.XOffset, Y + s.YOffset)
                    );
            }

            if (state == State.Normal)
                state = State.Warned;
            if (rndPoint.X != -1)
                rndPoint = new Vector2(-1, -1);
            // moves slightly slower if is shotting
            RealSpeed = Level.Speed*(isShotting ? 0.75f : 1f);
            MoveTo(objectiveV, RealSpeed);
        }

        // FIX: stop when near objective
        private void MoveTo(Vector2 objectiveV, float speed)
        {
            var movingDirection = new Vector2();
            var agentV = new Vector2(X, Y);
            //var distance = Math.Abs(agentV.X - objectiveV.X) + Math.Abs(agentV.Y + objectiveV.Y);
            // just touching the objective is enough
            if ((Math.Abs(agentV.X - objectiveV.X) > Width || Math.Abs(agentV.Y + objectiveV.Y) > Height) &&
                !HitBoxes.FirstOrDefault().Value.CollideWith(Player.Instance.HitBoxes.FirstOrDefault().Value))
                // first or value dependant key?
            {
                //movingDirection = objectiveV - agentV;
                //movingDirection.Normalize();
                ////Debug.WriteLine("{0} {1} {2} {3} {4}", playerV, agentV, movingDirection, bestDelta, level.speed);
                //Vx += movingDirection.X * deltaTime * speed;
                //Vy += movingDirection.Y * deltaTime * speed;
                if (useAI)
                {
                    var path = AI.CalculatePath(this, objectiveV);
                    if (path == null) // temp workaround test
                        return;
                    movingDirection = path[0];
                }
                else
                {
                    movingDirection = objectiveV - agentV;
                }
                movingDirection.Normalize();
                X += movingDirection.X*DeltaTime*speed;
                Y += movingDirection.Y*DeltaTime*speed;

                CalculateMovingState(movingDirection);
                MovingDirection = movingDirection;
            }
        }

        private void UpdateEvent(object sender)
        {
            if (Game.Game.Instance.MainWindow != "game") return;
            if (movingState >= MovingState.Idle)
            {
                if (Timer.Get("lastMove") <= 0)
                {
                    SearchAndDestroy(Player.Instance);
                    Timer.Set("lastMove", 0.005f); // move every 5ms
                }
            }
        }

        public override GameObject Clone()
        {
            var go = new Enemy(Name, FormattedName, CharacterName, (int) BaseWidth, (int) BaseHeight)
            {
                CurrentSprite = CurrentSprite,
                Name = Name,
                X = X,
                Y = Y,
                FormattedName = FormattedName,
                CharacterName = CharacterName,
                Level0 = Level0.Clone(),
                BounceTime = BounceTime,
                BounceSpeed = BounceSpeed,
                HitBoxOffSet = HitBoxOffSet.ToDictionary(x => x.Key, x => x.Value),
                HitBoxSize = HitBoxSize.ToDictionary(x => x.Key, x => x.Value)
            };
            if (Animations != null)
            {
                go.Animations = new Dictionary<string, Animation>(Animations.Count);
                foreach (var animKey in Animations.Keys)
                {
                    go.Animations[animKey] = Animations[animKey].Clone();
                    go.Animations[animKey].Owner = go;
                }
            }
            go.CurrentAnimation = CurrentAnimation;
            go.LevelCheck();
            return go;
        }

        private enum State
        {
            Normal = 1,
            Warned = 3,
            Damaged = 6
        }
    }
}