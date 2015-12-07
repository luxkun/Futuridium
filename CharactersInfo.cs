using System;
using System.Collections.Generic;
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
                    maxHP = 60,
                    attack = 40,
                    xpReward = 12,
                    speed = 200f
                }
            };
            //bigMonkey.useAnimations = true;

            var monkey = new Enemy("enemy_monkey", "Monkey", "monkey")
            {
                Level0 =
                {
                    maxHP = 40,
                    attack = 20,
                    xpReward = 8,
                    speed = 200f
                }
            };

            var bear = new Enemy("enemy_bear", "Bear", "bear")
            {
                Level0 =
                {
                    maxHP = 200,
                    attack = 50,
                    xpReward = 25,
                    speed = 120f
                }
            };


            enemies.Add(new Dictionary<Enemy, double>(3));
            enemies[0][bear] = 0.2;
            enemies[0][bigMonkey] = 0.6;
            enemies[0][monkey] = 1.0;


            // ROOM TYPE: 1
            // TODO: special attacks, charge for mino, bullets for megamonkey
            var mino = new Enemy("enemy_mino", "Mino", "mino")
            {
                Level0 =
                {
                    maxHP = 600,
                    attack = 100,
                    xpReward = 100,
                    speed = 200f
                }
            };

            var megaMonkey = new Enemy("enemy_megamonkey", "Mega Monkey", "megamonkey")
            {
                Level0 =
                {
                    maxHP = 500,
                    attack = 70,
                    xpReward = 50,
                    speed = 250f
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
            var game = (Game) engine.objects["game"];
            foreach (var enemiesList in enemies)
            {
                foreach (var pair in enemiesList)
                {
                    var enemy = pair.Key;
                    if (enemy.UseAnimations)
                    {
                        // TODO: use animations... 
                        enemy.currentSprite =
                            (SpriteAsset) engine.GetAsset(game.SpritesAnimations[enemy.CharacterName][0]);
                    }
                    else
                    {
                        // TODO: add all sprites
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
            var range = rnd.NextDouble()*rndRanges[roomType];
            var enemiesList = enemies[roomType].GetEnumerator();
            Enemy enemyInfo = null;
            enemiesList.MoveNext();
            for (var i = 0; range > 0.0 && i < enemies.Count; i++)
            {
                range -= enemiesList.Current.Value;
                enemyInfo = enemiesList.Current.Key;
                if (i + 1 < enemies.Count)
                    enemiesList.MoveNext();
            }

            //Character enemyInfo = enemies [rnd.Next (0, enemies.Length)];
            var result = (Enemy) enemyInfo.Clone();
            result.name += letters[counter - 1%letters.Length];
            result.Xp = result.LevelManager.levelUpTable[level].neededXP;
            result.LevelCheck();
            return result;
        }
    }
}