using System.Collections.Generic;
using System.IO;
using Aiv.Engine;

namespace StupidAivGame
{
    internal class MainClass
    {
        public static void Main(string[] args)
        {
            var usingOpenTK = true;
            Engine engine;
            if (usingOpenTK)
                engine = new FastEngine("Futuridium", 1280, 720, 60);
            else
                engine = new Engine("Futuridium", 1280, 720, 60);
#if DEBUG
            engine.debugCollisions = true;
#else
            engine.FullScreen();
#endif
            Input.Initialize(engine);

            var game = new Game {usingOpenTK = usingOpenTK};

            // set the base path for assets
            Asset.basePath = "../../Assets";
            // music
            engine.LoadAsset("levelup_sound", new Asset("levelup.ogg"));
            // base
            engine.LoadAsset("logo", new SpriteAsset("Futuridium.png"));
            engine.LoadAsset("player", new SpriteAsset("player.png"));
            // enemies
            engine.LoadAsset("monkey", new SpriteAsset("monkey.png"));
            engine.LoadAsset("bigmonkey", new SpriteAsset("bigmonkey.png"));
            engine.LoadAsset("bear", new SpriteAsset("pedobear.png"));
            // bosses
            engine.LoadAsset("mino", new SpriteAsset("minotaur.gif"));
            engine.LoadAsset("megamonkey", new SpriteAsset("megamonkey.png"));
            // decorations
            engine.LoadAsset("blood", new SpriteAsset(Path.Combine("background", "blood.png")));
            engine.LoadAsset("skull", new SpriteAsset(Path.Combine("background", "skull.png")));
            engine.LoadAsset("sadskull", new SpriteAsset(Path.Combine("background", "sadskull.png")));
            // background
            engine.LoadAsset("static_background", new SpriteAsset(Path.Combine("background", "static_background.jpg")));
            // portals
            engine.LoadAsset("top_door", new SpriteAsset(Path.Combine("background", "top_door.png"), 0, 0, 62, 85));
            engine.LoadAsset("bottom_door", new SpriteAsset(Path.Combine("background", "bottom_door.png"), 62 * 4, 0, 62, 85));
            engine.LoadAsset("left_door", new SpriteAsset(Path.Combine("background", "left_door.png"), 0, 62 * 4, 85, 62));
            engine.LoadAsset("right_door", new SpriteAsset(Path.Combine("background", "right_door.png"), 0, 0, 85, 62));
            engine.LoadAsset("escape_floor", new SpriteAsset(Path.Combine("background", "escape_floor.png")));

            engine.SpawnObject("game", game);

            engine.Run();
        }
    }
}