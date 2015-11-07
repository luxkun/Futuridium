using System;
using System.Collections.Generic;
using Aiv.Engine;

namespace StupidAivGame
{
	public class Room {
		public string name;
		public List<Enemy> enemies;

		public Game game;

		public Room (string name, Game game, List<Enemy> enemies) 
		{
			this.name = name;
			this.game = game;
			this.enemies = enemies;
		}

		public static Room randomRoom (int counter, Game game, int minEnemies, int maxEnemies, int level) 
		{ // random room
			//name = Tools.RandomString(5);
			// TODO: levels
			string randomName = "Room" + counter;
			Random rnd = new Random((int) DateTime.Now.Ticks);
			CharactersInfo charactersInfo = new CharactersInfo ();
			int numberOfEnemies = rnd.Next(minEnemies, maxEnemies);
			List<Enemy> randomEnemies = new List<Enemy>();
			for (int i = 0; i < numberOfEnemies; i++) {
				randomEnemies.Add(charactersInfo.randomEnemy (i + 1, level));
			}
			Room room = new Room (randomName, game, randomEnemies);
			return room;
		}

		public void removeEnemy (Enemy enemy)
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
			Random rnd = new Random((int) DateTime.Now.Ticks);;
			int count = 0;
			foreach (Enemy enemy in enemies) {
				Console.WriteLine ("Spawning enemy: {0} n.{1}", enemy.name, count);
				if (enemy.useAnimations) {
					// TODO: use animations... 
					enemy.currentSprite = (SpriteAsset)game.engine.GetAsset(game.spritesAnimations [enemy.characterName] [0]);
				} else {
					// TODO: add all sprites
					enemy.currentSprite = (SpriteAsset)game.engine.GetAsset ("goblin");//enemy.name);
				}
				game.engine.SpawnObject (enemy.name + count++, enemy);
				do {
					enemy.x = rnd.Next (50, game.engine.width - enemy.width - 5);
					enemy.y = rnd.Next (0, game.engine.height - enemy.height - 5);
				} while (enemy.CheckCollisions ().Count > 0);
			}
		}
	}
}

