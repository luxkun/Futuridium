using System;
using System.Collections.Generic;
using System.Diagnostics;
using Aiv.Engine;

namespace Futuridium
{
    public class CharactersInfo : GameObject
    {
        private readonly List<Dictionary<Enemy, double>> enemies;
        private readonly string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        // character, spawn modifier (1 is base)
        private readonly List<double> rndRanges;

        public CharactersInfo()
        {
            name = "charactersInfo";

            rndRanges = new List<double>(2);
            enemies = new List<Dictionary<Enemy, double>>(2);

            // ROOM TYPE: 0
            var bigMonkey = new Enemy("enemy_bigmonkey", "Big Monkey", "bigmonkey")
            {
                Level0 =
                {
                    MaxHp = 60,
                    Attack = 40,
                    XpReward = 12,
                    Speed = 100f
                }
            };
            //bigMonkey.useAnimations = true;

            var monkey = new Enemy("enemy_monkey", "Monkey", "monkey")
            {
                Level0 =
                {
                    MaxHp = 40,
                    Attack = 20,
                    XpReward = 8,
                    Speed = 120f
                }
            };

            var bear = new Enemy("enemy_bear", "Bear", "bear")
            {
                Level0 =
                {
                    MaxHp = 200,
                    Attack = 50,
                    XpReward = 25,
                    Speed = 70f
                }
            };


            enemies.Add(new Dictionary<Enemy, double>(3));
            enemies[0][bear] = 0.2;
            enemies[0][bigMonkey] = 0.6;
            enemies[0][monkey] = 1.0;


            // ROOM TYPE: 1
            var mino = new Enemy("enemy_mino", "Mino", "mino")
            {
                Level0 =
                {
                    MaxHp = 600,
                    Attack = 100,
                    XpReward = 100,
                    Speed = 100f
                }
            };

            var megaMonkey = new Enemy("enemy_megamonkey", "Mega Monkey", "megamonkey")
            {
                Level0 =
                {
                    MaxHp = 500,
                    Attack = 70,
                    XpReward = 50,
                    Speed = 150
                }
            };

            enemies.Add(new Dictionary<Enemy, double>(2));
            enemies[1][mino] = 1;
            enemies[1][megaMonkey] = 1;


            var count = 0;
            foreach (var enemiesList in enemies)
            {
                rndRanges.Add(0);
                foreach (var pair in enemiesList)
                {
                    rndRanges[count] += pair.Value;
                }
                count++;
            }
        }

        public override void Start()
        {
            foreach (var enemiesList in enemies)
            {
                foreach (var pair in enemiesList)
                {
                    var enemy = pair.Key;
                    if (enemy.UseAnimations)
                    {
                        enemy.currentSprite =
                            (SpriteAsset) engine.GetAsset(Game.Instance.SpritesAnimations[enemy.CharacterName][0]);
                    }
                    else
                    {
                        enemy.currentSprite = (SpriteAsset) engine.GetAsset(enemy.CharacterName); //enemy.name);
                    }
                }
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
            var enemiesList = enemies[roomType].GetEnumerator();
            Enemy enemyInfo = null;
            for (var i = 0; range >= 0f && i <= enemies.Count; i++)
            {
                enemiesList.MoveNext();
                range -= enemiesList.Current.Value;
                enemyInfo = enemiesList.Current.Key;
            }

            Debug.WriteLine($"Random enemy: {srange} to {range}, {rndRanges[roomType]} => {enemyInfo.CharacterName}");
            var result = (Enemy) enemyInfo.Clone();
            result.name += letters[counter - 1%letters.Length];
            result.Xp = result.LevelManager.levelUpTable[level].NeededXp;
            result.LevelCheck();
            return result;
        }
    }
}