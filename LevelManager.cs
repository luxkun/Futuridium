using System;

namespace StupidAivGame
{
	public class Level 
	{
		public int hp;
		public int maxHP;
		public long xpReward;
		public int attack;
		public int level;
		public long neededXP = 30;
		public int speed;
		public int shotDelay;
		public int shotSpeed;
		public int shotRange;
		public int shotRadius;

		public void LevelUp (Level oldLevel)
		{
			if (oldLevel != null) {
				hp = oldLevel.hp;
				hp += (int)(maxHP * 0.25);
				if (hp > maxHP)
					hp = maxHP;
			} else {
				hp = maxHP;
			}
		}

		public Level Clone () 
		{
			Level clone = (Level) this.MemberwiseClone();
			return clone;
		}
	}
	public class LevelManager
	{
		Character character;
		public Level[] levelUpTable;
		public LevelManager (Character character, Level level0)
		{
			this.character = character;
			CreateLevelUpTable (level0);
		}

		private void CreateLevelUpTable (Level level0)
		{
			levelUpTable = new Level[100];
			Level lvl;
			levelUpTable [0] = level0;
			for (int level = 1; level < 100; level++) {
				lvl = new Level ();
				lvl.level = level;
				lvl.maxHP = (int) (level0.maxHP * (1 + level / 6.0));
				lvl.hp = lvl.maxHP;
				lvl.neededXP = (long) (level0.neededXP * level*level*level); // x^3
				lvl.xpReward = (long) (level0.xpReward * level*level); // x^2
				lvl.attack = (int) (level0.attack * (1 + level / 4.0));
				lvl.speed = (int) (level0.speed * (1 + level / 15.0));
				lvl.shotDelay = (int) (level0.shotDelay * (1 - level / 100.0));
				lvl.shotSpeed = (int) (level0.shotSpeed * (1 + level / 6.0));
				lvl.shotRange = (int) (level0.shotRange * (1 + level / 10.0));
				lvl.shotRadius = (int) (level0.shotRadius * (1 + level / 10.0));
				levelUpTable [level] = lvl;
			}
		}

		public bool CheckLevelUp ()
		{
			// base case
			if (character.level == null) {
				character.level = levelUpTable [0];
				character.level.LevelUp (null);
				CheckLevelUp ();
				return true;
			}
			if (character.level.level == (levelUpTable.Length - 1))
				return false;
			Level nextLevel = levelUpTable [character.level.level + 1];
			if (character.xp >= nextLevel.neededXP) {
				nextLevel.LevelUp (character.level);
				character.level = nextLevel;
				CheckLevelUp ();
				return true;
			}
			return false;
		}
	}
}

