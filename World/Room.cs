using System;
using System.Collections.Generic;
using System.Diagnostics;
using Aiv.Engine;
using Futuridium.Characters;

namespace Futuridium.World
{
    public class Room : GameObject
    {
        private readonly int spawnPadding = 150;

        public Room(List<Enemy> enemies, Tuple<int, int> roomIndex, Floor floor)
        {
            Name = $"Room_{floor.FloorIndex}_{roomIndex.Item1}.{roomIndex.Item2}";
            Enemies = enemies;
            RoomIndex = roomIndex;
            Floor = floor;

            RoomObjects = new List<GameObject>();

            GameBackground = new GameBackground(Floor.FloorBackgroundType, this);

            OnDestroy += DestroyEvent;
        }

        public bool FirstRoom { get; set; }

        public Room Bottom { get; set; }

        public Room Left { get; set; }

        public Room Right { get; set; }

        public Room Top { get; set; }

        public GameBackground GameBackground { get; }

        public Tuple<int, int> RoomIndex { get; private set; }

        public int RoomType { get; set; }

        public Floor Floor { get; }

        public List<Enemy> Enemies { get; private set; }

        // usually items dropped on the room
        public List<GameObject> RoomObjects { get; }

        public int Width { get; set; }
        public int Height { get; set; }

        public override void Start()
        {
            base.Start();

            // randomize size
            var rnd = Game.Game.Instance.Random.GetRandom(Name + "_sizes");
            if (FirstRoom)
            {
                Width = Engine.Width;
                Height = Engine.Height;
            }
            else
            {
                var rndW = rnd.NextDouble()*0.66;
                Width = (int) (Engine.Width*(1 + (rndW < 0.33 ? 0 : rndW * 0.66f)));
                var rndH = rnd.NextDouble()*0.66;
                Height = (int) (Engine.Height*(1 + (rndH < 0.33 ? 0 : rndH * 0.66f)));
            }
        }

        private void DestroyEvent(object sender)
        {
            foreach (var obj in RoomObjects)
                obj.Destroy();
        }

        public void RandomizeRoom(int minEnemies, int maxEnemies, int level, Random rnd, CharactersInfo charactersInfo)
        {
            var numberOfEnemies = rnd.Next(minEnemies, maxEnemies);
            var randomEnemies = new List<Enemy>();
            for (var i = 0; i < numberOfEnemies; i++)
            {
                var enemy = charactersInfo.RandomEnemy(i + 1, level, RoomType, rnd);
                enemy.OnDestroy += (object sender) => Game.Game.Instance.CharacterDied((Character) sender);
                randomEnemies.Add(enemy);
            }
            Enemies = randomEnemies;
        }

        public void RemoveEnemy(Enemy enemy)
        {
            Enemies.Remove(enemy);
        }

        // disable all enemies and objects
        public void CloseRoom()
        {
            // can't happen right now, could happen with a room teleport or something similiar
            foreach (var enemy in Enemies)
                enemy.Enabled = false;
            foreach (var obj in RoomObjects)
                obj.Enabled = false;
        }

        public void OpenRoom()
        {
            Engine.SpawnObject(GameBackground.Name, GameBackground);
        }

        // enemies and items spawn
        public void SpawnRoomObjects()
        {
            // spawn room enemies in random spots
            var rnd = Game.Game.Instance.Random.GetRandom(Name + "_spawn");
            var count = 0;
            foreach (var enemy in Enemies)
            {
                Debug.WriteLine("Spawning enemy: {0} n.{1}", enemy.Name, count);
                Engine.SpawnObject(enemy.Name + count++, enemy);
                enemy.AddHitBox("tmp_enemy_" + Name, 0, 0, (int) enemy.BaseWidth, (int) enemy.BaseHeight);
                do
                {
                    enemy.X = rnd.Next(GameBackground.WallWidth + spawnPadding,
                        (int) (Width - enemy.Width - spawnPadding));
                    enemy.Y = rnd.Next(GameBackground.WallHeight + spawnPadding,
                        (int) (Height - enemy.Height - spawnPadding));
                } while (enemy.CheckCollisions().Count > 0);
                enemy.HitBoxes.Remove("tmp_enemy_" + Name);
            }

            foreach (var obj in RoomObjects)
            {
                if (Engine.Objects.ContainsKey(obj.Name))
                    obj.Enabled = true;
                else
                    Engine.SpawnObject(obj);
            }
        }
    }
}