using System;
using System.Collections.Generic;
using System.Diagnostics;
using Aiv.Engine;

namespace Futuridium
{
    public class Room : GameObject
    {
        public Room bottom;
        public List<Enemy> enemies;
        public Floor floor;
        public Game game;
        public GameBackground gameBackground;
        public Room left;
        public Room right;
        public Tuple<int, int> roomIndex;
        // TODO: roomType as class
        public int roomType; // 0 normal room ; 1 boss room
        public Room top;

        public Room(List<Enemy> enemies, Tuple<int, int> roomIndex, Floor floor)
        {
            name = $"Room_{floor.FloorIndex}_{roomIndex.Item1}.{roomIndex.Item2}";
            this.enemies = enemies;
            this.roomIndex = roomIndex;
            this.floor = floor;
        }

        public override void Start()
        {
            //engine.SpawnObject(string.Format("room_{0}", name), background);
            gameBackground = new GameBackground(floor.FloorBackgroundType, this);
            engine.SpawnObject(gameBackground.name, gameBackground);
        }

        public void RandomizeRoom(int minEnemies, int maxEnemies, int level, Random rnd, CharactersInfo charactersInfo)
        {
            var numberOfEnemies = rnd.Next(minEnemies, maxEnemies);
            var randomEnemies = new List<Enemy>();
            for (var i = 0; i < numberOfEnemies; i++)
            {
                Enemy enemy = charactersInfo.RandomEnemy(i + 1, level, roomType, rnd);
                enemy.OnDestroy += (Object sender) => ((Game)engine.objects["game"]).CharacterDied((Character) sender);
                randomEnemies.Add(enemy);
            }
            enemies = randomEnemies;
        }

        public void RemoveEnemy(Enemy enemy)
        {
            enemies.Remove(enemy);
        }

        public void SpawnEnemies()
        {
            var game = (Game) engine.objects["game"];
            var rnd = game.Random.GetRandom(name + "_spawn");
            var count = 0;
            foreach (var enemy in enemies)
            {
                Debug.WriteLine("Spawning enemy: {0} n.{1}", enemy.name, count);
                /*if (enemy.useAnimations)
                {
                    // TODO: use animations... 
                    enemy.currentSprite = (SpriteAsset) engine.GetAsset(game.spritesAnimations[enemy.characterName][0]);
                }
                else
                {
                    // TODO: add all sprites
                    enemy.currentSprite = (SpriteAsset) engine.GetAsset(enemy.characterName); //enemy.name);
                }*/
                engine.SpawnObject(enemy.name + count++, enemy);
                enemy.AddHitBox("tmp_enemy_" + name, 0, 0, enemy.width, enemy.height);
                do
                {
                    enemy.x = rnd.Next(50, engine.width - enemy.width - 5);
                    enemy.y = rnd.Next(0, engine.height - enemy.height - 5);
                } while (enemy.CheckCollisions().Count > 0);
                enemy.hitBoxes.Clear();
            }
        }
    }
}