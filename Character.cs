using System;
using Aiv.Engine;
using System.Collections.Generic;

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
		public Character (string name, string formattedName, string characterName)
		{
			level0 = new Level ();
			this.name = name;
			this.formattedName = formattedName;
			this.characterName = characterName;
		}

		// not started before update??
		public override void Start ()
		{
		}

		public override void Update ()
		{
			if (levelManager == null)
				levelManager = new LevelManager (this, level0);
			levelManager.CheckLevelUp ();
		}

		public int doDamage (Character enemy)
		{
			level.hp -= enemy.level.attack;
			return level.hp;
		}

		public bool isAlive 
		{
			get { return level.hp > 0; }
		}
	}
}

