using System;
using System.Collections.Generic;
using Aiv.Engine;

namespace StupidAivGame
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
            name = $"Room_{floor.floorIndex}_{roomIndex.Item1}.{roomIndex.Item2}";
            this.enemies = enemies;
            this.roomIndex = roomIndex;
            this.floor = floor;
        }

        public override void Start()
        {
            //engine.SpawnObject(string.Format("room_{0}", name), background);
            gameBackground = new GameBackground(floor.floorBackgroundType, this);
            engine.SpawnObject(gameBackground.name, gameBackground);
        }

        public static Room RandomRoom(int counter, int minEnemies, int maxEnemies, Floor floor, int level,
            Tuple<int, int> roomIndex, Random rnd, int roomType)
        {
            // random room
            //string randomName = string.Format("Room_{0}_{0}", floor.floorIndex, counter);
            //Random rnd = game.random.GetRandom(randomName);
            var charactersInfo = new CharactersInfo(rnd);
            var numberOfEnemies = rnd.Next(minEnemies, maxEnemies);
            var randomEnemies = new List<Enemy>();
            for (var i = 0; i < numberOfEnemies; i++)
            {
                randomEnemies.Add(charactersInfo.randomEnemy(i + 1, level, roomType));
            }
            //string[] availableBackgroundAssets = new string[] { "background_0", "background_1" };
            //GameBackground gameBackground = new GameBackground (availableBackgroundAssets[rnd.Next(0, availableBackgroundAssets.Length)]);
            var room = new Room(randomEnemies, roomIndex, floor);
            room.roomType = roomType;
            return room;
        }

        public void RemoveEnemy(Enemy enemy)
        {
            /*Character[] newEnemies = new Character[enemies.Length - 1];
			int y = 0;
			for (int i = 0; i < enemies.Length; i++) {
				if (enemies[i] != enemy)
					newEnemies [y++] = enemies [i];
			}
			enemies = newEnemies;*/
            enemies.Remove(enemy);
        }

        public void SpawnEnemies()
        {
            var game = (Game) engine.objects["game"];
            var rnd = game.random.GetRandom(name + "_spawn");
            var count = 0;
            foreach (var enemy in enemies)
            {
                Console.WriteLine("Spawning enemy: {0} n.{1}", enemy.name, count);
                if (enemy.useAnimations)
                {
                    // TODO: use animations... 
                    enemy.currentSprite = (SpriteAsset) engine.GetAsset(game.spritesAnimations[enemy.characterName][0]);
                }
                else
                {
                    // TODO: add all sprites
                    enemy.currentSprite = (SpriteAsset) engine.GetAsset(enemy.characterName); //enemy.name);
                }
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