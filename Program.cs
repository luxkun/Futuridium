using System;
using Aiv.Engine;
using System.Collections.Generic;
using System.Text;

// this is a simple game written for learning that uses AivEngine
// the game doesn't and won't have any story and game balance since the only purpose of the game itself is to try new algorithm and gaming paradigms 
using System.Drawing;
using OpenTK.Input;


namespace StupidAivGame
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			FastEngine engine = new FastEngine ("Futuridium", 1920, 1080, 60);
			//engine.debugCollisions = true;

			Game game = new Game ();


			// set the base path for assets
			Asset.basePath = "../../Assets";
			// load assets
			engine.LoadAsset ("levelup_sound", new Asset ("Music/levelup.wav"));

			engine.LoadAsset ("logo", new SpriteAsset ("Futuridium.png"));
			engine.LoadAsset ("player", new SpriteAsset ("player.png"));
			engine.LoadAsset ("monkey", new SpriteAsset ("monkey.png"));
			engine.LoadAsset ("bigmonkey", new SpriteAsset ("bigmonkey.png"));
			engine.LoadAsset ("megamonkey", new SpriteAsset ("megamonkey.png"));
			engine.LoadAsset ("bear", new SpriteAsset ("pedobear.png"));

			// not real animation..
			game.spritesAnimations ["background_1"] = new List<string> ();
			for (int y = 0; y < 4; y++) {
				for (int x = 0; x < 4; x++) {
					string key = "background_1_" + game.spritesAnimations ["background_1"].Count;
					engine.LoadAsset(key, new SpriteAsset(
						"background_1.png", x * 50, y * 50, 50, 50));
					game.spritesAnimations["background_1"].Add(key);
				}
			}
			game.spritesAnimations ["background_2"] = new List<string> ();
			for (int y = 0; y < 4; y++) {
				for (int x = 0; x < 4; x++) {
					string key = "background_2_" + game.spritesAnimations ["background_2"].Count;
					engine.LoadAsset(key, new SpriteAsset(
						"background_2.png", x * 50, y * 50, 50, 50)); // TODO: fix
					game.spritesAnimations["background_2"].Add(key);
				}
			}

			engine.LoadAsset ("background_0", new SpriteAsset ("background_0.gif"));
			engine.LoadAsset ("block", new SpriteAsset ("block.png"));
			engine.LoadAsset ("door", new SpriteAsset ("door.png"));
			engine.LoadAsset ("escape_floor", new SpriteAsset ("escape_floor.png"));

			engine.SpawnObject ("game", game);

			engine.Run ();
		}
	}
}
