using System;
using Aiv.Engine;
using System.Collections.Generic;

namespace StupidAivGame
{
	public class Game : GameObject
	{
		public bool gameOver = false;
		private int gameOverTimer = 1000;

		public Player player;

		public Floor currentFloor;
		//public List<Floor> floors;

		public Dictionary<string, List<string>> spritesAnimations;
			
		public Game (Engine engine)
		{
			this.engine = engine;
			spritesAnimations = new Dictionary<string, List<string>> ();
		}

		public void initializeFloor ()
		{
			// TODO: when floor is cleared created a new random floor with higher level monsters
			currentFloor = Floor.randomFloor (this, 3, 6);
			currentFloor.OpenRoom (0);
		}

		public override void Start ()
		{
			player = new Player ();
			player.x = 40;
			player.y = 40;
			player.currentSprite = (SpriteAsset) engine.GetAsset ("player");
			engine.SpawnObject ("player", player);

			TextObject score = new TextObject ("Arial", 17, "red");
			score.text = "XP: 0 - Level: 1";
			engine.SpawnObject ("xp", score);

			TextObject statistics = new TextObject ("Arial", 17, "red");
			statistics.text = "Statistics...";
			engine.SpawnObject ("statistics", statistics);
			((TextObject) engine.objects ["statistics"]).y = 25;

			initializeFloor ();
		}

		// override Start vs constructor?

		// bullet hits enemy
		public bool Hits (Bullet bullet, Character enemy, Collision collision)
		{ 
			return this.Hits ((Character)bullet.owner, enemy, collision);
		}

		// character hits enemy
		public bool Hits (Character character, Character enemy, Collision collision)
		{ 
			enemy.DoDamage (character);

			if (!enemy.isAlive) {
				collision.other.Destroy ();

				character.xp += enemy.level.xpReward;

				Enemy enemyObj = enemy as Enemy;
				if (enemyObj != null) {
					currentFloor.currentRoom.removeEnemy (enemyObj);

					Console.WriteLine ("Enemies to go in current room: " + currentFloor.currentRoom.enemies.Count);
					foreach (Enemy en in currentFloor.currentRoom.enemies) {
						Console.Write("{0} - ", en.name);
					}

					if (currentFloor.currentRoom.enemies.Count == 0) {
						if ((currentFloor.currentRoomIndex + 1) < currentFloor.rooms.Count) { 
							currentFloor.OpenRoom (currentFloor.currentRoomIndex + 1);
						} else {
							currentFloor = Floor.randomFloor (this, 3, 6);
							currentFloor.OpenRoom (0);
						}
					}
				}
			}
			return enemy.isAlive;
		}

		public override void Update ()
		{
			if (player.level != null &&!player.isAlive)
				gameOver = true;
			// check for gameOver
			if (this.gameOver) {
				this.gameOverTimer -= this.deltaTicks;
				if (this.gameOverTimer <= 0) {
					this.engine.isGameRunning = false;
				}
			}
		}
	}
}

