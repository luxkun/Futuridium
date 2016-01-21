using System;
using System.Collections.Generic;
using System.Diagnostics;
using Aiv.Engine;
using Futuridium.Spells;
using Utils = Futuridium.Game.Utils;

namespace Futuridium.Characters
{
    public class CharactersInfo : GameObject
    {
        private readonly Dictionary<string, Enemy> enemiesList;
        private readonly List<Dictionary<Enemy, float>> enemiesSpawnRate;
        private readonly string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        // character, spawn modifier (1 is base)
        private readonly List<float> rndRanges;

        public CharactersInfo()
        {
            Name = "charactersInfo";

            rndRanges = new List<float>(2);
            enemiesSpawnRate = new List<Dictionary<Enemy, float>>(2);
            enemiesList = new Dictionary<string, Enemy>();

        }

        public override void Start()
        {
            base.Start();
            LoadEnemies();

            foreach (var list in enemiesSpawnRate)
            {
                foreach (var pair in list)
                {
                    var enemy = pair.Key;
                    if (enemy.Animations == null)
                    {
                        enemy.CurrentSprite = (SpriteAsset) Engine.GetAsset(enemy.CharacterName); //enemy.name);
                    }
                }
            }
        }

        private void LoadEnemies()
        {
            var idleName = Character.GetMovingStateString(Character.MovingState.Idle);
            var movingLeftName = Character.GetMovingStateString(Character.MovingState.MovingLeft);
            var movingRightName = Character.GetMovingStateString(Character.MovingState.MovingRight);
            var movingDownName = Character.GetMovingStateString(Character.MovingState.MovingDown);
            var movingUpName = Character.GetMovingStateString(Character.MovingState.MovingUp);

            var goblinSpriteName = $"goblins"; // same for others
            // ROOM TYPE: 0
            var mageGoblinIdleSprite = (SpriteAsset) Engine.GetAsset(Utils.GetAssetName(goblinSpriteName, 1, 4)[0]);
            var mageGoblin = new Enemy("enemy_magegoblin", "Mage Goblin", "magegoblin", mageGoblinIdleSprite.Width, mageGoblinIdleSprite.Height)
            {
                Level0 =
                {
                    MaxHp = 100,
                    Attack = 30,
                    XpReward = 35,
                    Speed = 110f,
                    SpellCd = 0.65f,
                    SpellSpeed = 120f,
                    SpellRange = 600,
                    SpellSize = 12,
                    SpellList = new List<Type>
                    {typeof (Bullet)},
                    Luck = 1f
                }
            };
            mageGoblin.AddAnimation(idleName, Utils.GetAssetName(goblinSpriteName, 1, 4), 5, Engine).Loop = false;
            mageGoblin.AddAnimation(movingRightName, Utils.GetAssetName(goblinSpriteName, 0, 6, 3), 5, Engine);
            mageGoblin.AddAnimation(movingLeftName, Utils.GetAssetName(goblinSpriteName, 0, 5, 3), 5, Engine);
            mageGoblin.AddAnimation(movingUpName, Utils.GetAssetName(goblinSpriteName, 0, 7, 3), 5, Engine);
            mageGoblin.AddAnimation(movingDownName, Utils.GetAssetName(goblinSpriteName, 0, 4, 3), 5, Engine);
            mageGoblin.CalculateAnimationHitBoxes();

            var goblinIdleSprite = (SpriteAsset) Engine.GetAsset(Utils.GetAssetName(goblinSpriteName, 4, 0)[0]);
            var goblin = new Enemy("enemy_goblin", "Goblin", "goblin", goblinIdleSprite.Width, goblinIdleSprite.Height)
            {
                Level0 =
                {
                    MaxHp = 70,
                    Attack = 40,
                    XpReward = 18,
                    Speed = 150f,
                    Luck = 0.55f
                }
            };
            goblin.AddAnimation(idleName, Utils.GetAssetName(goblinSpriteName, 4, 0), 5, Engine).Loop = false;
            goblin.AddAnimation(movingRightName, Utils.GetAssetName(goblinSpriteName, 3, 2, 3), 5, Engine);
            goblin.AddAnimation(movingLeftName, Utils.GetAssetName(goblinSpriteName, 3, 1, 3), 5, Engine);
            goblin.AddAnimation(movingUpName, Utils.GetAssetName(goblinSpriteName, 3, 3, 3), 5, Engine);
            goblin.AddAnimation(movingDownName, Utils.GetAssetName(goblinSpriteName, 3, 0, 3), 5, Engine);
            goblin.CalculateAnimationHitBoxes();

            var undeadGoblinIdleSprite = (SpriteAsset) Engine.GetAsset(Utils.GetAssetName(goblinSpriteName, 1, 0)[0]);
            var undeadGoblin = new Enemy("enemy_undeadgoblin", "Undead Goblin", "undeadgoblin", undeadGoblinIdleSprite.Width, undeadGoblinIdleSprite.Height)
            {
                Level0 =
                {
                    MaxHp = 100,
                    Attack = 60,
                    XpReward = 20,
                    Speed = 140f,
                    Luck = 0.65f
                }
            };
            undeadGoblin.AddAnimation(idleName, Utils.GetAssetName(goblinSpriteName, 1, 0), 5, Engine).Loop = false;
            undeadGoblin.AddAnimation(movingRightName, Utils.GetAssetName(goblinSpriteName, 0, 2, 3), 5, Engine);
            undeadGoblin.AddAnimation(movingLeftName, Utils.GetAssetName(goblinSpriteName, 0, 1, 3), 5, Engine);
            undeadGoblin.AddAnimation(movingUpName, Utils.GetAssetName(goblinSpriteName, 0, 3, 3), 5, Engine);
            undeadGoblin.AddAnimation(movingDownName, Utils.GetAssetName(goblinSpriteName, 0, 0, 3), 5, Engine);
            undeadGoblin.CalculateAnimationHitBoxes();

            var mummyGoblinIdleSprite = (SpriteAsset) Engine.GetAsset(Utils.GetAssetName(goblinSpriteName, 7, 4)[0]);
            var mummyGoblin = new Enemy("enemy_mummygoblin", "Mummy Goblin", "mummygoblin", mummyGoblinIdleSprite.Width, mummyGoblinIdleSprite.Height)
            {
                Level0 =
                {
                    MaxHp = 115,
                    Attack = 70,
                    XpReward = 20,
                    Speed = 130f,
                    Luck = 0.75f
                }
            };
            mummyGoblin.AddAnimation(idleName, Utils.GetAssetName(goblinSpriteName, 7, 4), 5, Engine).Loop = false;
            mummyGoblin.AddAnimation(movingRightName, Utils.GetAssetName(goblinSpriteName, 6, 6, 3), 5, Engine);
            mummyGoblin.AddAnimation(movingLeftName, Utils.GetAssetName(goblinSpriteName, 6, 5, 3), 5, Engine);
            mummyGoblin.AddAnimation(movingUpName, Utils.GetAssetName(goblinSpriteName, 6, 7, 3), 5, Engine);
            mummyGoblin.AddAnimation(movingDownName, Utils.GetAssetName(goblinSpriteName, 6, 4, 3), 5, Engine);
            mummyGoblin.CalculateAnimationHitBoxes();

            var warriorGoblinIdleSprite = (SpriteAsset)Engine.GetAsset(Utils.GetAssetName(goblinSpriteName, 10, 0)[0]);
            var warriorGoblin = new Enemy("enemy_warriorgoblin", "Warrior Goblin", "warriorgoblin", warriorGoblinIdleSprite.Width, warriorGoblinIdleSprite.Height)
            {
                Level0 =
                {
                    MaxHp = 120,
                    Attack = 40,
                    XpReward = 30,
                    Speed = 140f,
                    SpellCd = 1f,
                    SpellSpeed = 110f,
                    SpellRange = 400,
                    SpellSize = 15,
                    SpellList = new List<Type>
                    {typeof (Bullet)},
                    Luck = 0.8f
                }
            };
            warriorGoblin.AddAnimation(idleName, Utils.GetAssetName(goblinSpriteName, 10, 0), 5, Engine).Loop = false;
            warriorGoblin.AddAnimation(movingRightName, Utils.GetAssetName(goblinSpriteName, 9, 2, 3), 5, Engine);
            warriorGoblin.AddAnimation(movingLeftName, Utils.GetAssetName(goblinSpriteName, 9, 1, 3), 5, Engine);
            warriorGoblin.AddAnimation(movingUpName, Utils.GetAssetName(goblinSpriteName, 9, 3, 3), 5, Engine);
            warriorGoblin.AddAnimation(movingDownName, Utils.GetAssetName(goblinSpriteName, 9, 0, 3), 5, Engine);
            warriorGoblin.CalculateAnimationHitBoxes();

            var captainGoblinIdleSprite = (SpriteAsset)Engine.GetAsset(Utils.GetAssetName(goblinSpriteName, 7, 0)[0]);
            var captainGoblin = new Enemy("enemy_captaingoblin", "Captain Goblin", "captaingoblin", captainGoblinIdleSprite.Width, captainGoblinIdleSprite.Height)
            {
                Level0 =
                {
                    MaxHp = 160,
                    Attack = 55,
                    XpReward = 35,
                    Speed = 160f,
                    SpellCd = 1.2f,
                    SpellSpeed = 120f,
                    SpellRange = 500,
                    SpellSize = 16,
                    SpellList = new List<Type>
                    {typeof (Bullet)},
                    Luck = 1f
                }
            };
            captainGoblin.AddAnimation(idleName, Utils.GetAssetName(goblinSpriteName, 7, 0), 5, Engine).Loop = false;
            captainGoblin.AddAnimation(movingRightName, Utils.GetAssetName(goblinSpriteName, 6, 2, 3), 5, Engine);
            captainGoblin.AddAnimation(movingLeftName, Utils.GetAssetName(goblinSpriteName, 6, 1, 3), 5, Engine);
            captainGoblin.AddAnimation(movingUpName, Utils.GetAssetName(goblinSpriteName, 6, 3, 3), 5, Engine);
            captainGoblin.AddAnimation(movingDownName, Utils.GetAssetName(goblinSpriteName, 6, 0, 3), 5, Engine);
            captainGoblin.CalculateAnimationHitBoxes();

            var scorpionSpriteName = $"scorpion";
            var scorpionIdleSprite = (SpriteAsset)Engine.GetAsset(Utils.GetAssetName(scorpionSpriteName, 0, 0)[0]);
            var scorpion = new Enemy("enemy_scorpion", "Scorpion", "scorpion", scorpionIdleSprite.Width, scorpionIdleSprite.Height)
            {
                Level0 =
                {
                    MaxHp = 60,
                    Attack = 40,
                    XpReward = 15,
                    Speed = 170f,
                    Luck = 0.5f
                }
            };
            scorpion.AddAnimation(idleName, Utils.GetAssetName(scorpionSpriteName, 0, 0), 5, Engine).Loop = false;
            scorpion.AddAnimation(movingRightName, Utils.GetAssetName(scorpionSpriteName, 0, 3, 6), 5, Engine);
            scorpion.AddAnimation(movingLeftName, Utils.GetAssetName(scorpionSpriteName, 0, 2, 6), 5, Engine);
            scorpion.AddAnimation(movingUpName, Utils.GetAssetName(scorpionSpriteName, 0, 1, 6), 5, Engine);
            scorpion.AddAnimation(movingDownName, Utils.GetAssetName(scorpionSpriteName, 0, 0, 6), 5, Engine);
            scorpion.CalculateAnimationHitBoxes();

            var snakeSpriteName = $"snake";
            var snakeIdleSprite = (SpriteAsset) Engine.GetAsset(Utils.GetAssetName(snakeSpriteName, 0, 0)[0]);
            var snake = new Enemy("enemy_snake", "Snake", "snake", snakeIdleSprite.Width, snakeIdleSprite.Height)
            {
                Level0 =
                {
                    MaxHp = 50,
                    Attack = 50,
                    XpReward = 15,
                    Speed = 160f,
                    Luck = 0.5f
                }
            };
            snake.AddAnimation(idleName, Utils.GetAssetName(snakeSpriteName, 0, 0), 5, Engine).Loop = false;
            snake.AddAnimation(movingRightName, Utils.GetAssetName(snakeSpriteName, 0, 2, 4), 5, Engine);
            snake.AddAnimation(movingLeftName, Utils.GetAssetName(snakeSpriteName, 0, 1, 4), 5, Engine);
            snake.AddAnimation(movingUpName, Utils.GetAssetName(snakeSpriteName, 0, 3, 4), 5, Engine);
            snake.AddAnimation(movingDownName, Utils.GetAssetName(snakeSpriteName, 0, 0, 4), 5, Engine);
            snake.CalculateAnimationHitBoxes();

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
            var kingGoblinSprite = (SpriteAsset) Engine.GetAsset(Utils.GetAssetName(goblinSpriteName, 4, 4)[0]);
            var kingGoblin = new Enemy("enemy_kinggoblin", "King Goblin", "kinggoblin", kingGoblinSprite.Width, kingGoblinSprite.Height)
            {
                Level0 =
                {
                    MaxHp = 500,
                    Attack = 140,
                    XpReward = 220,
                    Speed = 170,
                    SpellCd = 0.8f,
                    SpellSpeed = 300f,
                    SpellRange = 800,
                    SpellSize = 14,
                    SpellList = new List<Type>
                    {typeof (Bullet)},
                    Luck = 6f
                }
            };
            kingGoblin.AddAnimation(idleName, Utils.GetAssetName(goblinSpriteName, 4, 4), 5, Engine).Loop = false;
            kingGoblin.AddAnimation(movingRightName, Utils.GetAssetName(goblinSpriteName, 3, 6, 3), 5, Engine);
            kingGoblin.AddAnimation(movingLeftName, Utils.GetAssetName(goblinSpriteName, 3, 5, 3), 5, Engine);
            kingGoblin.AddAnimation(movingUpName, Utils.GetAssetName(goblinSpriteName, 3, 7, 3), 5, Engine);
            kingGoblin.AddAnimation(movingDownName, Utils.GetAssetName(goblinSpriteName, 3, 4, 3), 5, Engine);
            kingGoblin.CalculateAnimationHitBoxes();

            var ogreSpriteName = "ogre";
            var ogreIdleSprite = (SpriteAsset)Engine.GetAsset(Utils.GetAssetName(ogreSpriteName, 0, 0)[0]);
            var ogre = new Enemy("enemy_ogre", "Ogre", "ogre", ogreIdleSprite.Width, ogreIdleSprite.Height)
            {
                Level0 =
                {
                    MaxHp = 600,
                    Attack = 220,
                    XpReward = 200,
                    Speed = 130,
                    SpellCd = 2f,
                    SpellSpeed = 200f,
                    SpellRange = 2200, // :)
                    SpellSize = 35,
                    SpellList = new List<Type>
                    {typeof (Bullet)},
                    Luck = 5f
                }
            };
            ogre.AddAnimation(idleName, Utils.GetAssetName(ogreSpriteName, 0, 0), 5, Engine).Loop = false;
            ogre.AddAnimation(movingRightName, Utils.GetAssetName(ogreSpriteName, 0, 2, 4), 5, Engine);
            ogre.AddAnimation(movingLeftName, Utils.GetAssetName(ogreSpriteName, 0, 1, 4), 5, Engine);
            ogre.AddAnimation(movingUpName, Utils.GetAssetName(ogreSpriteName, 0, 3, 4), 5, Engine);
            ogre.AddAnimation(movingDownName, Utils.GetAssetName(ogreSpriteName, 0, 0, 4), 5, Engine);
            ogre.CalculateAnimationHitBoxes();

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

        public Enemy RandomEnemy(int counter, int level, int roomType, Random rnd)
        {
            // enemy.randomMod: probability to spawn
            // range = SUM(randomMods)
            // sottraendo da un random(0f, range) ogni singolo randomMod
            //  e smettendo di sottrarre quando si arriva ad un numero negativo (o 0)
            //  si sceglie un nemico random
            // graficamente: |----Goblin----|-uGobl-|----Drowner---|F|
            var range = (float) rnd.NextDouble()*rndRanges[roomType];
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
            var result = (Enemy) enemyInfo.Clone();
            result.Name += letters[counter - 1%letters.Length];
            result.Xp = result.LevelManager.levelUpTable[level].NeededXp;
            result.LevelCheck();
            return result;
        }
    }
}