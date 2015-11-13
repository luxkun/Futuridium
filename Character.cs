using System;
using Aiv.Engine;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;

namespace StupidAivGame
{
	public class Character : SpriteObject
	{
		public Level level0;
		public LevelManager levelManager;

		public bool useAnimations = false;
		protected int bulletCounter = 0;
		protected int lastShot = 0;
		protected bool isCloseCombat = true;

		public Hud hud = null;

		protected long _xp;
		public long xp {
			get {
				return _xp;
			}
			set {
				_xp = value;
				if (hud != null) {
					hud.UpdateXPBar ();
				}
				LevelCheck ();
			}
		}
		protected Level _level;
		public Level level {
			get {
				return _level;
			}
			set {
				_level = value;
				if (hud != null) {
					hud.UpdateXPBar ();
					hud.UpdateHPBar ();
				}
			}
		}

		public string formattedName;
		public string characterName;

		private Color shotColor = Color.GhostWhite;
		public Character (string name, string formattedName, string characterName)
		{
			this.order = 6;

			level0 = new Level ();
			this.name = name;
			this.formattedName = formattedName;
			this.characterName = characterName;
		}

		public void LevelCheck ()
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


		public void Shot (Vector2 direction)
		{
			direction.Normalize ();
			Console.WriteLine ("{0} is shotting to direction: {1}", this.name, direction);
			// 0 left; 1 top; 2 right; 3 bottom; 4: top-left; 5: top-right; 6: bottom-left; 7: bottom-right
			Bullet bullet = new Bullet (this, direction);
			float xMod = direction.X + 0.5f;
			float yMod = direction.Y + 0.5f;
			bullet.x = this.x + (int)(this.width * xMod);
			bullet.y = this.y + (int)(this.height * yMod);

			bullet.radius = level.shotRadius;
			bullet.color = shotColor;
			this.engine.SpawnObject (string.Format("bullet_{0}_{1}", this.name, bulletCounter), bullet);
			bulletCounter++;
		}

		public virtual int GetDamage (Character enemy, Func<Character, Character, int> damageFunc)
		{
			LevelCheck (); // could happen that the player kills the enemy before he fully spawn (before Start "starts")
			enemy.LevelCheck();
			level.hp -= damageFunc (this, enemy);//enemy.level.attack;
			if (hud != null)
				hud.UpdateHPBar ();
			return level.hp;
		}

		public bool isAlive 
		{
			get { return level.hp > 0; }
		}
	}
}

