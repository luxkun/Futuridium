using System;
using Aiv.Engine;
using System.Collections.Generic;
using System.Text;

// this is a simple game written for learning that uses AivEngine
// the game doesn't and won't have any story since the only purpose of the game itself is to try new algorithm and gaming paradigms 
namespace StupidAivGame
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Engine engine = new Engine ("StupidAivGame", 1024, 768, 30);
			//engine.debugCollisions = true;

			Game game = new Game (engine);

			// set the base path for assets
			Asset.basePath = "../../Assets";
			// load assets
			engine.LoadAsset ("player", new SpriteAsset ("player.png"));
			engine.LoadAsset ("goblin", new SpriteAsset ("goblin.png"));
			// TODO: use animations in Character class, standard animations file or hardcoded for every file?
			game.spritesAnimations ["undeadgoblin"] = new List<string> ();
			for (int y = 0; y < 6; y++) {
				for (int x = 0; x < 10; x++) {
					string key = "undeadgoblin_" + game.spritesAnimations ["undeadgoblin"].Count;
					engine.LoadAsset(key, new SpriteAsset("" +
						"undeadgoblin.png", x * 42, y * 81, 42, 81));
					game.spritesAnimations["undeadgoblin"].Add(key);
				}
			}

			engine.LoadAsset ("background", new SpriteAsset ("background.jpg"));
			Background background = new Background ();
			background.currentSprite = (SpriteAsset) engine.GetAsset ("background");
			engine.SpawnObject ("background", background);

			engine.SpawnObject ("game", game);

			engine.Run ();
		}
	}
}
