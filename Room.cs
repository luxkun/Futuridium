using System;
using System.Collections.Generic;
using Aiv.Engine;

namespace StupidAivGame
{
	public class Room : GameObject {
		public List<Enemy> enemies;

		public Game game;

		public Room left;
		public Room top;
		public Room bottom;
		public Room right;

		public Tuple<int, int> roomIndex;

		public GameBackground gameBackground;
		public Floor floor; 

		// TODO: roomType as class
		public int roomType = 0; // 0 normal room ; 1 boss room

		public Room (string name, List<Enemy> enemies, Tuple<int, int> roomIndex, Floor floor) 
		{
			this.name = name;
			this.enemies = enemies;
			this.roomIndex = roomIndex;
			this.floor = floor;
		}

		public override void Start ()
		{
			//engine.SpawnObject(string.Format("room_{0}", name), background);
			gameBackground = new GameBackground (floor.floorBackgroundType, this);
			engine.SpawnObject (gameBackground.name, gameBackground);
		}

		public static Room RandomRoom (int counter, int minEnemies, int maxEnemies, Floor floor, int level, Tuple<int, int> roomIndex, Random rnd) 
		{ // random room
			string randomName = string.Format("Room_{0}_{0}", floor.floorIndex, counter);
			//Random rnd = game.random.GetRandom(randomName);
			CharactersInfo charactersInfo = new CharactersInfo (rnd);
			int numberOfEnemies = rnd.Next(minEnemies, maxEnemies);
			List<Enemy> randomEnemies = new List<Enemy>();
			for (int i = 0; i < numberOfEnemies; i++) {
				randomEnemies.Add(charactersInfo.randomEnemy (i + 1, level));
			}
			//string[] availableBackgroundAssets = new string[] { "background_0", "background_1" };
			//GameBackground gameBackground = new GameBackground (availableBackgroundAssets[rnd.Next(0, availableBackgroundAssets.Length)]);
			Room room = new Room (randomName, randomEnemies, roomIndex, floor);
			return room;
		}

		public void RemoveEnemy (Enemy enemy)
		{
			/*Character[] newEnemies = new Character[enemies.Length - 1];
			int y = 0;
			for (int i = 0; i < enemies.Length; i++) {
				if (enemies[i] != enemy)
					newEnemies [y++] = enemies [i];
			}
			enemies = newEnemies;*/
			enemies.Remove (enemy);
		}

		public void SpawnEnemies ()
		{
			Game game = (Game)engine.objects ["game"];
			Random rnd = game.random.GetRandom (this.name + "_spawn");
			int count = 0;
			foreach (Enemy enemy in enemies) {
				Console.WriteLine ("Spawning enemy: {0} n.{1}", enemy.name, count);
				if (enemy.useAnimations) {
					// TODO: use animations... 
					enemy.currentSprite = (SpriteAsset)engine.GetAsset(game.spritesAnimations [enemy.characterName] [0]);
				} else {
					// TODO: add all sprites
					enemy.currentSprite = (SpriteAsset)engine.GetAsset (enemy.characterName);//enemy.name);
				}
				engine.SpawnObject (enemy.name + count++, enemy);
				enemy.AddHitBox ("tmp_enemy_" + name, 0, 0, enemy.width, enemy.height);
				do {
					enemy.x = rnd.Next (50, engine.width - enemy.width - 5);
					enemy.y = rnd.Next (0, engine.height - enemy.height - 5);
				} while (enemy.CheckCollisions ().Count > 0);
				enemy.hitBoxes.Clear ();
			}
		}
	}
}

