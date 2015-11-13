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
			Enemy bigMonkey = new Enemy ("enemy_bigmonkey", "Big Monkey", "bigmonkey");
			bigMonkey.level0.maxHP = 120;
			bigMonkey.level0.attack = 100;
			bigMonkey.level0.xpReward = 12;
			bigMonkey.level0.speed = 4;
			//bigMonkey.useAnimations = true;

			Enemy monkey = new Enemy ("enemy_monkey", "Monkey", "monkey");
			monkey.level0.maxHP = 80;
			monkey.level0.attack = 60;
			monkey.level0.xpReward = 8;
			monkey.level0.speed = 6;

			Enemy bear = new Enemy ("enemy_bear", "Bear", "bear");
			bear.level0.maxHP = 400;
			bear.level0.attack = 120;
			bear.level0.xpReward = 25;
			bear.level0.speed = 5;

			// TODO: boss
			Enemy megaMonkey = new Enemy ("enemy_megamonkey", "Mega Monkey", "megamonkey");
			megaMonkey.level0.maxHP = 999;
			megaMonkey.level0.attack = 180;
			megaMonkey.level0.xpReward = 50;
			megaMonkey.level0.speed = 8;


			enemies = new Dictionary<Enemy, double> ();
			enemies[megaMonkey] = 0.15;
			enemies[bear] = 0.2;
			enemies[bigMonkey] = 0.6;
			enemies[monkey] = 1.0;
			foreach (KeyValuePair<Enemy, double> pair in enemies) {
				rndRange += pair.Value;
			}
		}

		public Enemy randomEnemy (int counter, int level)
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
				if ((i + 1) < enemies.Count)
					enemiesList.MoveNext ();
			}

			//Character enemyInfo = enemies [rnd.Next (0, enemies.Length)];
			Enemy result = new Enemy(enemyInfo.name + letters[counter - 1 % letters.Length], enemyInfo.formattedName, enemyInfo.characterName);
			Level level0 = enemyInfo.level0.Clone ();
			result.useAnimations = enemyInfo.useAnimations;
			result.level0 = level0;
			result.LevelCheck ();
			result.xp = result.levelManager.levelUpTable [level].neededXP;
			result.LevelCheck ();
			return result;
		}
	}
}

