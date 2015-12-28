using Aiv.Engine;
using OpenTK;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Futuridium
{
    internal class MainClass
    {
        public static void Main(string[] args)
        {
            var size = new Size(1280, 720);
            Run(size);
        }

        private static void Run(Size size)
        {
            var usingOpenTK = true;
            Engine engine;
            if (usingOpenTK)
                engine = new FastEngine("Futuridium", (int)size.Width, (int)size.Height, 60);
            else
                engine = new Engine("Futuridium", (int)size.Width, (int)size.Height, 60);
#if DEBUG
            engine.debugCollisions = true;
#else
            //engine.FullScreen();
#endif
            Input.Initialize(engine);

            Game.Instance.UsingOpenTK = usingOpenTK;

            LoadAssets(engine);

            engine.SpawnObject("game", Game.Instance);

            engine.ClearEveryFrame = false;
            engine.Run();
        }

        private static void LoadAnimation(Engine engine, string name, string fileName, int xLen, int yLen)
        {
            var spriteAsset = new SpriteAsset(fileName);
            engine.LoadAsset("player_animated", spriteAsset);
            var blockSize = new Vector2(spriteAsset.sprite.Width / (float)xLen, spriteAsset.sprite.Height / (float)yLen);
            for (int posX = 0; posX < xLen; posX++)
                for (int posY = 0; posY < yLen; posY++)
                {
                    var animName = $"{name}_{posY}_{posX}";
                    Debug.WriteLine("Loaded animations: " + animName);
                    engine.LoadAsset(animName, new SpriteAsset(fileName, (int)(posX * blockSize.X), (int)(posY * blockSize.Y), (int)blockSize.X, (int)blockSize.Y));
                }
        }

        private static void LoadAssets(Engine engine)
        {
            // set the base path for assets
            Asset.basePath = "../../Assets";
            // music
            engine.LoadAsset("levelup_sound", new Asset("levelup.ogg"));
            // base
            engine.LoadAsset("logo", new SpriteAsset("Futuridium.png"));
            engine.LoadAsset("player", new SpriteAsset("player.png"));
            // animated player
            LoadAnimation(engine, "player_animated", "player_animated.png", 3, 4);
            // enemies
            LoadAnimation(engine, "scorpion", "scorpion.png", 6, 4);
            LoadAnimation(engine, "goblins", "goblins.png", 12, 8);
            LoadAnimation(engine, "ogre", "ogre.png", 4, 4);
            LoadAnimation(engine, "snake", "snake.png", 4, 4);
            // bosses
            // decorations
            engine.LoadAsset("blood", new SpriteAsset(Path.Combine("background", "blood.png")));
            engine.LoadAsset("skull", new SpriteAsset(Path.Combine("background", "skull.png")));
            engine.LoadAsset("sadskull", new SpriteAsset(Path.Combine("background", "sadskull.png")));
            // background
            engine.LoadAsset("static_background", new SpriteAsset(Path.Combine("background", "static_background.jpg")));
            // portals
            engine.LoadAsset("top_door", new SpriteAsset(Path.Combine("background", "top_door.png"), 0, 0, 45, 70));
            engine.LoadAsset("bottom_door",
                new SpriteAsset(Path.Combine("background", "bottom_door.png"), 51 * 4 + 1, 0, 45, 70));
            engine.LoadAsset("left_door",
                new SpriteAsset(Path.Combine("background", "left_door.png"), 0, 51 * 4 + 1, 70, 45));
            engine.LoadAsset("right_door", new SpriteAsset(Path.Combine("background", "right_door.png"), 0, 0, 70, 45));
            engine.LoadAsset("escape_floor", new SpriteAsset(Path.Combine("background", "escape_floor.png")));
        }
    }
}