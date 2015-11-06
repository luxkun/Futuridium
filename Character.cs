using System;
using Aiv.Engine;
using System.Collections.Generic;
using System.Drawing;

namespace StupidAivGame
{
	public class Character : SpriteObject
	{
		public Level level;
		public Level level0;
		public LevelManager levelManager;

		public bool useAnimations = false;
		protected int bulletCounter = 0;
		protected int lastShot = 0;
		protected bool isCloseCombat = true;

		public long xp = 0;

		public string formattedName;
		public string characterName;

		private Color shotColor = Color.White;
		public Character (string name, string formattedName, string characterName)
		{
			level0 = new Level ();
			this.name = name;
			this.formattedName = formattedName;
			this.characterName = characterName;
		}

		private void LevelCheck ()
		{
			if (levelManager == null)
				levelManager = new LevelManager (this, level0);
			levelManager.CheckLevelUp ();
		}

		// not started before update??
		public override void Start ()
		{
			LevelCheck ();
		}

		public override void Update ()
		{
			LevelCheck ();
		}


		public void Shot (int direction)
		{
			Console.WriteLine ("Shotting to direction: " + direction);
			// 0 left; 1 top; 2 right; 3 bottom; 4: top-left; 5: top-right; 6: bottom-left; 7: bottom-right
			Bullet bullet = new Bullet (this, direction);
			if (direction == 7) {
				bullet.x = this.x + this.width;
				bullet.y = this.y + this.height;
			} else if (direction == 6) {
				bullet.x = this.x;
				bullet.y = this.y + this.height;
			} else if (direction == 5) {
				bullet.x = this.x + this.width;
				bullet.y = this.y;
			} else if (direction == 4) {
				bullet.x = this.x;
				bullet.y = this.y;
			} else if (direction == 3) {
				bullet.x = this.x + (this.width / 2);
				bullet.y = this.y + this.height;
			} else if (direction == 2) {
				bullet.x = this.x + this.width;
				bullet.y = this.y + (this.height / 2);
			} else if (direction == 1) {
				bullet.x = this.x + (this.width / 2);
				bullet.y = this.y;// - this.height;
			} else if (direction == 0) {
				bullet.x = this.x;// - this.width;
				bullet.y = this.y + (this.height / 2);
			}

			bullet.radius = level.shotRadius;
			bullet.color = shotColor;
			this.engine.SpawnObject (string.Format("bullet_{0}_{1}", this.name, bulletCounter), bullet);
			bulletCounter++;
		}

		public int DoDamage (Character enemy)
		{
			LevelCheck (); // could happen that the player kills the enemy before he fully spawn (before Start "starts")
			enemy.LevelCheck();
			level.hp -= enemy.level.attack;
			return level.hp;
		}

		public bool isAlive 
		{
			get { return level.hp > 0; }
		}
	}
}

