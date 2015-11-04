using System;
using System.Collections.Generic;

namespace StupidAivGame
{
	class CharactersInfo {
		Dictionary<Enemy, double> enemies; // character, spawn modifier (1 is base)
		double rndRange = 0f;
		string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		public CharactersInfo ()
		{
			//Character ogre = new Character ("Ogre", 150, 120, 20, 0, 10, game);
			//Character troll = new Character ("Troll", 200, 75, 35, 0, 15, game);
			Enemy goblin = new Enemy ("enemy_goblin", "Goblin", "goblin");
			goblin.level0.maxHP = 70;
			goblin.level0.attack = 40;
			goblin.level0.xpReward = 5;
			goblin.level0.speed = 5;

			Enemy undeadGoblin = new Enemy ("enemy_undeadgoblin", "Undead Goblin", "undeadgoblin");
			undeadGoblin.level0.maxHP = 80;
			undeadGoblin.level0.attack = 100;
			undeadGoblin.level0.xpReward = 10;
			undeadGoblin.level0.speed = 2;
			undeadGoblin.useAnimations = true;

			Enemy drowner = new Enemy ("enemy_drowner", "Drowner", "undeadgoblin");
			drowner.level0.maxHP = 80;
			drowner.level0.attack = 60;
			drowner.level0.xpReward = 8;
			drowner.level0.speed = 6;

			Enemy floppyWiener = new Enemy ("enemy_floppywiener", "Floppy Wiener", "undeadgoblin");
			floppyWiener.level0.maxHP = 400;
			floppyWiener.level0.attack = 60;
			floppyWiener.level0.xpReward = 200;
			floppyWiener.level0.speed = 1;


			enemies = new Dictionary<Enemy, double> ();
			enemies[goblin] = 1.0;
			enemies[undeadGoblin] = 0.5;
			enemies[drowner] = 1.0;
			enemies[floppyWiener] = 0.1;
			foreach (KeyValuePair<Enemy, double> pair in enemies) {
				rndRange += pair.Value;
			}
		}

		public Enemy randomEnemy (int counter)
		{
			Random rnd = new Random((int) DateTime.Now.Ticks);

			// enemy.randomMod: probability to spawn
			// range = SUM(randomMods) 
			// sottraendo da un random(0f, range) ogni singolo randomMod
			//  e smettendo di sottrarre quando si arriva ad un numero negativo (o 0)
			//  si sceglie un nemico random
			// graficamente: |----Goblin----|-uGobl-|----Drowner---|F|
			double range = rnd.NextDouble () * rndRange;
			Dictionary<Enemy, double>.Enumerator enemiesList = enemies.GetEnumerator ();
			Enemy enemyInfo = null;
			for (int i = 0; range > 0.0 && i < enemies.Count; i++) {
				range -= enemiesList.Current.Value;
				enemyInfo = enemiesList.Current.Key;
				if (i + 1 < enemies.Count)
					enemiesList.MoveNext ();
			}

			//Character enemyInfo = enemies [rnd.Next (0, enemies.Length)];
			Enemy result = new Enemy(enemyInfo.name + letters[counter - 1 % letters.Length], enemyInfo.formattedName, enemyInfo.characterName);
			Level level0 = enemyInfo.level0.Clone ();
			result.useAnimations = enemyInfo.useAnimations;
			result.level0 = level0;
			return result;
		}
	}
}

