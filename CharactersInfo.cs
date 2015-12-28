using Aiv.Engine;
using Futuridium.Spells;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Futuridium
{
    public class CharactersInfo : GameObject
    {
        private readonly List<Dictionary<Enemy, float>> enemiesSpawnRate;
        private readonly Dictionary<string, Enemy> enemiesList;
        private readonly string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        // character, spawn modifier (1 is base)
        private readonly List<float> rndRanges;

        public CharactersInfo()
        {
            name = "charactersInfo";

            rndRanges = new List<float>(2);
            enemiesSpawnRate = new List<Dictionary<Enemy, float>>(2);
            enemiesList = new Dictionary<string, Enemy>();

            // ROOM TYPE: 0
            var mageGoblin = new Enemy("enemy_magegoblin", "Mage Goblin", "magegoblin")
            {
                Level0 =
                {
                    MaxHp = 60,
                    Attack = 30,
                    XpReward = 35,
                    Speed = 100f,
                    SpellCd = 0.75f,
                    SpellSpeed = 100f,
                    SpellRange = 600,
                    SpellSize = 10,
                    spellList = new List<Type>
                    {typeof (Bullet)}
                }
            };

            var goblin = new Enemy("enemy_goblin", "Goblin", "goblin")
            {
                Level0 =
                {
                    MaxHp = 60,
                    Attack = 20,
                    XpReward = 18,
                    Speed = 140f
                }
            };

            var undeadGoblin = new Enemy("enemy_undeadgoblin", "Undead Goblin", "undeadgoblin")
            {
                Level0 =
                {
                    MaxHp = 80,
                    Attack = 22,
                    XpReward = 20,
                    Speed = 130f
                }
            };

            var mummyGoblin = new Enemy("enemy_mummygoblin", "Mummy Goblin", "mummygoblin")
            {
                Level0 =
                {
                    MaxHp = 105,
                    Attack = 25,
                    XpReward = 20,
                    Speed = 120f
                }
            };

            var warriorGoblin = new Enemy("enemy_warriorgoblin", "Warrior Goblin", "warriorgoblin")
            {
                Level0 =
                {
                    MaxHp = 80,
                    Attack = 40,
                    XpReward = 30,
                    Speed = 130f,
                    SpellCd = 0.7f,
                    SpellSpeed = 110f,
                    SpellRange = 400,
                    SpellSize = 10,
                    spellList = new List<Type>
                    {typeof (Bullet)}
                }
            };

            var captainGoblin = new Enemy("enemy_captaingoblin", "Captain Goblin", "captaingoblin")
            {
                Level0 =
                {
                    MaxHp = 100,
                    Attack = 45,
                    XpReward = 30,
                    Speed = 130f,
                    SpellCd = 0.8f,
                    SpellSpeed = 120f,
                    SpellRange = 500,
                    SpellSize = 12,
                    spellList = new List<Type>
                    {typeof (Bullet)}
                }
            };

            var scorpion = new Enemy("enemy_scorpion", "Scorpion", "scorpion")
            {
                Level0 =
                {
                    MaxHp = 50,
                    Attack = 20,
                    XpReward = 15,
                    Speed = 130f
                }
            };

            var snake = new Enemy("enemy_snake", "Snake", "snake")
            {
                Level0 =
                {
                    MaxHp = 40,
                    Attack = 25,
                    XpReward = 15,
                    Speed = 130f
                }
            };

            enemiesSpawnRate.Add(new Dictionary<Enemy, float>(3));
            enemiesSpawnRate[0][mageGoblin] = 0.8f;
            enemiesSpawnRate[0][mummyGoblin] = 0.8f;
            enemiesSpawnRate[0][warriorGoblin] = 0.8f;
            enemiesSpawnRate[0][goblin] = 1.0f;
            enemiesSpawnRate[0][undeadGoblin] = 0.9f;
            enemiesSpawnRate[0][captainGoblin] = 0.75f;
            enemiesSpawnRate[0][scorpion] = 1.66f;
            enemiesSpawnRate[0][snake] = 1.66f;

            // ROOM TYPE: 1
            var kingGoblin = new Enemy("enemy_kinggoblin", "King Goblin", "kinggoblin")
            {
                Level0 =
                {
                    MaxHp = 500,
                    Attack = 70,
                    XpReward = 220,
                    Speed = 140,
                    SpellCd = 0.8f,
                    SpellSpeed = 200f,
                    SpellRange = 600,
                    SpellSize = 12,
                    spellList = new List<Type>
                    {typeof (Bullet)}
                }
            };

            var ogre = new Enemy("enemy_ogre", "Ogre", "ogre")
            {
                Level0 =
                {
                    MaxHp = 600,
                    Attack = 200,
                    XpReward = 200,
                    Speed = 110,
                    SpellCd = 5f,
                    SpellSpeed = 200f,
                    SpellRange = 2200, // :)
                    SpellSize = 65,
                    spellList = new List<Type>
                    {typeof (Bullet)}
                }
            };

            enemiesSpawnRate.Add(new Dictionary<Enemy, float>(2));
            enemiesSpawnRate[1][kingGoblin] = 1f;
            enemiesSpawnRate[1][ogre] = 1f;

            var count = 0;
            foreach (var list in enemiesSpawnRate)
            {
                rndRanges.Add(0);
                foreach (var pair in list)
                {
                    rndRanges[count] += pair.Value;
                    enemiesList[pair.Key.CharacterName] = pair.Key;
                }
                count++;
            }
        }

        public override void Start()
        {
            base.Start();
            LoadEnemiesAnimations();

            foreach (var list in enemiesSpawnRate)
            {
                foreach (var pair in list)
                {
                    var enemy = pair.Key;
                    if (!enemy.UseAnimations) // else the Character class handles it
                    {
                        enemy.currentSprite = (SpriteAsset)engine.GetAsset(enemy.CharacterName); //enemy.name);
                    }
                    else
                    {
                        enemy.currentSprite = enemy.animationsInfo[Character.MovingState.Idle][0];
                    }
                }
            }
        }

        private void LoadEnemiesAnimations()
        {
            // TODO: fix scorpion hitbox
            var scorpion = enemiesList["scorpion"];
            var scorpionSpriteName = $"scorpion";
            scorpion.AnimationFrequency = 0.2f;
            scorpion.animationsInfo[Character.MovingState.Idle] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{scorpionSpriteName}_0_0")
            };
            scorpion.animationsInfo[Character.MovingState.MovingRight] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{scorpionSpriteName}_3_0"),
                (SpriteAsset) engine.GetAsset($"{scorpionSpriteName}_3_1"),
                (SpriteAsset) engine.GetAsset($"{scorpionSpriteName}_3_2"),
                (SpriteAsset) engine.GetAsset($"{scorpionSpriteName}_3_3"),
                (SpriteAsset) engine.GetAsset($"{scorpionSpriteName}_3_4"),
                (SpriteAsset) engine.GetAsset($"{scorpionSpriteName}_3_5")
            };
            scorpion.animationsInfo[Character.MovingState.MovingLeft] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{scorpionSpriteName}_2_0"),
                (SpriteAsset) engine.GetAsset($"{scorpionSpriteName}_2_1"),
                (SpriteAsset) engine.GetAsset($"{scorpionSpriteName}_2_2"),
                (SpriteAsset) engine.GetAsset($"{scorpionSpriteName}_2_3"),
                (SpriteAsset) engine.GetAsset($"{scorpionSpriteName}_2_4"),
                (SpriteAsset) engine.GetAsset($"{scorpionSpriteName}_2_5")
            };
            scorpion.animationsInfo[Character.MovingState.MovingUp] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{scorpionSpriteName}_1_0"),
                (SpriteAsset) engine.GetAsset($"{scorpionSpriteName}_1_1"),
                (SpriteAsset) engine.GetAsset($"{scorpionSpriteName}_1_2"),
                (SpriteAsset) engine.GetAsset($"{scorpionSpriteName}_1_3"),
                (SpriteAsset) engine.GetAsset($"{scorpionSpriteName}_1_4"),
                (SpriteAsset) engine.GetAsset($"{scorpionSpriteName}_1_5")
            };
            scorpion.animationsInfo[Character.MovingState.MovingDown] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{scorpionSpriteName}_0_0"),
                (SpriteAsset) engine.GetAsset($"{scorpionSpriteName}_0_1"),
                (SpriteAsset) engine.GetAsset($"{scorpionSpriteName}_0_2"),
                (SpriteAsset) engine.GetAsset($"{scorpionSpriteName}_0_3"),
                (SpriteAsset) engine.GetAsset($"{scorpionSpriteName}_0_4"),
                (SpriteAsset) engine.GetAsset($"{scorpionSpriteName}_0_5")
            };
            scorpion.CalculateRealSpriteHitBoxes();

            var snake = enemiesList["snake"];
            var snakeSpriteName = $"snake";
            snake.AnimationFrequency = 0.2f;
            snake.animationsInfo[Character.MovingState.Idle] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{snakeSpriteName}_0_0")
            };
            snake.animationsInfo[Character.MovingState.MovingRight] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{snakeSpriteName}_2_0"),
                (SpriteAsset) engine.GetAsset($"{snakeSpriteName}_2_1"),
                (SpriteAsset) engine.GetAsset($"{snakeSpriteName}_2_2"),
                (SpriteAsset) engine.GetAsset($"{snakeSpriteName}_2_3")
            };
            snake.animationsInfo[Character.MovingState.MovingLeft] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{snakeSpriteName}_1_0"),
                (SpriteAsset) engine.GetAsset($"{snakeSpriteName}_1_1"),
                (SpriteAsset) engine.GetAsset($"{snakeSpriteName}_1_2"),
                (SpriteAsset) engine.GetAsset($"{snakeSpriteName}_1_3")
            };
            snake.animationsInfo[Character.MovingState.MovingUp] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{snakeSpriteName}_3_0"),
                (SpriteAsset) engine.GetAsset($"{snakeSpriteName}_3_1"),
                (SpriteAsset) engine.GetAsset($"{snakeSpriteName}_3_2"),
                (SpriteAsset) engine.GetAsset($"{snakeSpriteName}_3_3")
            };
            snake.animationsInfo[Character.MovingState.MovingDown] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{snakeSpriteName}_0_0"),
                (SpriteAsset) engine.GetAsset($"{snakeSpriteName}_0_1"),
                (SpriteAsset) engine.GetAsset($"{snakeSpriteName}_0_2"),
                (SpriteAsset) engine.GetAsset($"{snakeSpriteName}_0_3")
            };
            snake.CalculateRealSpriteHitBoxes();

            var goblin = enemiesList["goblin"];
            var goblinSpriteName = $"goblins"; // same for others
            goblin.AnimationFrequency = 0.2f;
            goblin.animationsInfo[Character.MovingState.Idle] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_0_4")
            };
            goblin.animationsInfo[Character.MovingState.MovingRight] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_2_3"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_2_4"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_2_5")
            };
            goblin.animationsInfo[Character.MovingState.MovingLeft] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_1_3"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_1_4"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_1_5")
            };
            goblin.animationsInfo[Character.MovingState.MovingUp] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_3_3"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_3_4"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_3_5")
            };
            goblin.animationsInfo[Character.MovingState.MovingDown] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_0_3"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_0_4"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_0_5")
            };
            goblin.CalculateRealSpriteHitBoxes();

            var undeadGoblin = enemiesList["undeadgoblin"];
            undeadGoblin.AnimationFrequency = 0.2f;
            undeadGoblin.animationsInfo[Character.MovingState.Idle] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_0_1")
            };
            undeadGoblin.animationsInfo[Character.MovingState.MovingRight] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_2_0"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_2_1"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_2_2")
            };
            undeadGoblin.animationsInfo[Character.MovingState.MovingLeft] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_1_0"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_1_1"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_1_2")
            };
            undeadGoblin.animationsInfo[Character.MovingState.MovingUp] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_3_0"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_3_1"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_3_2")
            };
            undeadGoblin.animationsInfo[Character.MovingState.MovingDown] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_0_0"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_0_1"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_0_2")
            };
            undeadGoblin.CalculateRealSpriteHitBoxes();

            var mageGoblin = enemiesList["magegoblin"];
            mageGoblin.AnimationFrequency = 0.2f;
            mageGoblin.animationsInfo[Character.MovingState.Idle] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_4_1")
            };
            mageGoblin.animationsInfo[Character.MovingState.MovingRight] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_6_0"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_6_1"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_6_2")
            };
            mageGoblin.animationsInfo[Character.MovingState.MovingLeft] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_5_0"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_5_1"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_5_2")
            };
            mageGoblin.animationsInfo[Character.MovingState.MovingUp] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_7_0"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_7_1"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_7_2")
            };
            mageGoblin.animationsInfo[Character.MovingState.MovingDown] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_4_0"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_4_1"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_4_2")
            };
            mageGoblin.CalculateRealSpriteHitBoxes();

            var mummyGoblin = enemiesList["mummygoblin"];
            mummyGoblin.AnimationFrequency = 0.2f;
            mummyGoblin.animationsInfo[Character.MovingState.Idle] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_4_7")
            };
            mummyGoblin.animationsInfo[Character.MovingState.MovingRight] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_6_6"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_6_7"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_6_8")
            };
            mummyGoblin.animationsInfo[Character.MovingState.MovingLeft] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_5_6"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_5_7"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_5_8")
            };
            mummyGoblin.animationsInfo[Character.MovingState.MovingUp] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_7_6"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_7_7"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_7_8")
            };
            mummyGoblin.animationsInfo[Character.MovingState.MovingDown] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_4_6"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_4_7"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_4_8")
            };
            mummyGoblin.CalculateRealSpriteHitBoxes();

            var captainGoblin = enemiesList["captaingoblin"];
            captainGoblin.AnimationFrequency = 0.2f;
            captainGoblin.animationsInfo[Character.MovingState.Idle] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_0_7")
            };
            captainGoblin.animationsInfo[Character.MovingState.MovingRight] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_2_6"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_2_7"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_2_8")
            };
            captainGoblin.animationsInfo[Character.MovingState.MovingLeft] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_1_6"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_1_7"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_1_8")
            };
            captainGoblin.animationsInfo[Character.MovingState.MovingUp] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_3_6"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_3_7"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_3_8")
            };
            captainGoblin.animationsInfo[Character.MovingState.MovingDown] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_0_6"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_0_7"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_0_8")
            };
            captainGoblin.CalculateRealSpriteHitBoxes();

            var warriorGoblin = enemiesList["warriorgoblin"];
            warriorGoblin.AnimationFrequency = 0.2f;
            warriorGoblin.animationsInfo[Character.MovingState.Idle] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_0_10")
            };
            warriorGoblin.animationsInfo[Character.MovingState.MovingRight] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_2_9"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_2_10"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_2_11")
            };
            warriorGoblin.animationsInfo[Character.MovingState.MovingLeft] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_1_9"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_1_10"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_1_11")
            };
            warriorGoblin.animationsInfo[Character.MovingState.MovingUp] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_3_9"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_3_10"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_3_11")
            };
            warriorGoblin.animationsInfo[Character.MovingState.MovingDown] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_0_9"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_0_10"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_0_11")
            };
            warriorGoblin.CalculateRealSpriteHitBoxes();

            // BOSS
            var ogre = enemiesList["ogre"];
            var ogreSpriteName = "ogre";
            ogre.AnimationFrequency = 0.3f;
            ogre.animationsInfo[Character.MovingState.Idle] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{ogreSpriteName}_0_0")
            };
            ogre.animationsInfo[Character.MovingState.MovingRight] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{ogreSpriteName}_2_0"),
                (SpriteAsset) engine.GetAsset($"{ogreSpriteName}_2_1"),
                (SpriteAsset) engine.GetAsset($"{ogreSpriteName}_2_2"),
                (SpriteAsset) engine.GetAsset($"{ogreSpriteName}_2_3")
            };
            ogre.animationsInfo[Character.MovingState.MovingLeft] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{ogreSpriteName}_1_0"),
                (SpriteAsset) engine.GetAsset($"{ogreSpriteName}_1_1"),
                (SpriteAsset) engine.GetAsset($"{ogreSpriteName}_1_2"),
                (SpriteAsset) engine.GetAsset($"{ogreSpriteName}_1_3")
            };
            ogre.animationsInfo[Character.MovingState.MovingUp] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{ogreSpriteName}_3_0"),
                (SpriteAsset) engine.GetAsset($"{ogreSpriteName}_3_1"),
                (SpriteAsset) engine.GetAsset($"{ogreSpriteName}_3_2"),
                (SpriteAsset) engine.GetAsset($"{ogreSpriteName}_3_3")
            };
            ogre.animationsInfo[Character.MovingState.MovingDown] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{ogreSpriteName}_0_0"),
                (SpriteAsset) engine.GetAsset($"{ogreSpriteName}_0_1"),
                (SpriteAsset) engine.GetAsset($"{ogreSpriteName}_0_2"),
                (SpriteAsset) engine.GetAsset($"{ogreSpriteName}_0_3")
            };
            ogre.CalculateRealSpriteHitBoxes();

            var kingGoblin = enemiesList["kinggoblin"];
            kingGoblin.AnimationFrequency = 0.2f;
            kingGoblin.animationsInfo[Character.MovingState.Idle] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_4_4")
            };
            kingGoblin.animationsInfo[Character.MovingState.MovingRight] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_6_3"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_6_4"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_6_5")
            };
            kingGoblin.animationsInfo[Character.MovingState.MovingLeft] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_5_3"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_5_4"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_5_5")
            };
            kingGoblin.animationsInfo[Character.MovingState.MovingUp] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_7_3"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_7_4"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_7_5")
            };
            kingGoblin.animationsInfo[Character.MovingState.MovingDown] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_4_3"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_4_4"),
                (SpriteAsset) engine.GetAsset($"{goblinSpriteName}_4_5")
            };
            kingGoblin.CalculateRealSpriteHitBoxes();
        }

        public Enemy RandomEnemy(int counter, int level, int roomType, Random rnd)
        {
            // enemy.randomMod: probability to spawn
            // range = SUM(randomMods)
            // sottraendo da un random(0f, range) ogni singolo randomMod
            //  e smettendo di sottrarre quando si arriva ad un numero negativo (o 0)
            //  si sceglie un nemico random
            // graficamente: |----Goblin----|-uGobl-|----Drowner---|F|
            var range = (float)rnd.NextDouble() * rndRanges[roomType];
            var srange = range;
            var currentEnemiesList = enemiesSpawnRate[roomType].GetEnumerator();
            Enemy enemyInfo = null;
            for (var i = 0; range >= 0f && i <= enemiesSpawnRate[roomType].Count; i++)
            {
                currentEnemiesList.MoveNext();
                range -= currentEnemiesList.Current.Value;
                enemyInfo = currentEnemiesList.Current.Key;
            }

            Debug.WriteLine($"Random enemy: {srange} to {range}, {rndRanges[roomType]} => {enemyInfo.CharacterName}");
            var result = (Enemy)enemyInfo.Clone();
            result.name += letters[counter - 1 % letters.Length];
            result.Xp = result.LevelManager.levelUpTable[level].NeededXp;
            result.LevelCheck();
            return result;
        }
    }
}