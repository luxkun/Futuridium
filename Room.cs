using Aiv.Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Futuridium
{
    public class Room : GameObject
    {
        private readonly int spawnPadding = 50;

        public Room(List<Enemy> enemies, Tuple<int, int> roomIndex, Floor floor)
        {
            name = $"Room_{floor.FloorIndex}_{roomIndex.Item1}.{roomIndex.Item2}";
            Enemies = enemies;
            RoomIndex = roomIndex;
            Floor = floor;
        }

        public Room Bottom { get; set; }

        public Room Left { get; set; }

        public Room Right { get; set; }

        public Room Top { get; set; }

        public GameBackground GameBackground { get; private set; }

        public Tuple<int, int> RoomIndex { get; private set; }

        public int RoomType { get; set; }

        public Floor Floor { get; }

        public List<Enemy> Enemies { get; private set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public override void Start()
        {
            base.Start();
            // TOOD: change
            Width = engine.width;
            Height = engine.height;
            //engine.SpawnObject(string.Format("room_{0}", name), background);
            GameBackground = new GameBackground(Floor.FloorBackgroundType, this);
            engine.SpawnObject(GameBackground.name, GameBackground);
        }

        public void RandomizeRoom(int minEnemies, int maxEnemies, int level, Random rnd, CharactersInfo charactersInfo)
        {
            var numberOfEnemies = rnd.Next(minEnemies, maxEnemies);
            var randomEnemies = new List<Enemy>();
            for (var i = 0; i < numberOfEnemies; i++)
            {
                var enemy = charactersInfo.RandomEnemy(i + 1, level, RoomType, rnd);
                enemy.OnDestroy += (object sender) => Game.Instance.CharacterDied((Character)sender);
                randomEnemies.Add(enemy);
            }
            Enemies = randomEnemies;
        }

        public void RemoveEnemy(Enemy enemy)
        {
            Enemies.Remove(enemy);
        }

        public void SpawnEnemies()
        {
            var rnd = Game.Instance.Random.GetRandom(name + "_spawn");
            var count = 0;
            foreach (var enemy in Enemies)
            {
                Debug.WriteLine("Spawning enemy: {0} n.{1}", enemy.name, count);
                /*if (enemy.useAnimations)
                {
                    enemy.currentSprite = (SpriteAsset) engine.GetAsset(game.spritesAnimations[enemy.characterName][0]);
                }
                else
                {
                    enemy.currentSprite = (SpriteAsset) engine.GetAsset(enemy.characterName); //enemy.name);
                }*/
                engine.SpawnObject(enemy.name + count++, enemy);
                enemy.AddHitBox("tmp_enemy_" + name, 0, 0, enemy.width, enemy.height);
                do
                {
                    enemy.x = rnd.Next(GameBackground.WallWidth + spawnPadding, Width - enemy.width - spawnPadding);
                    enemy.y = rnd.Next(GameBackground.WallHeight + spawnPadding, Height - enemy.height - spawnPadding);
                } while (enemy.CheckCollisions().Count > 0);
                enemy.hitBoxes.Clear();
            }
        }
    }
}