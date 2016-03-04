using System;
using System.IO;
using System.Windows;
using Aiv.Engine;
using Futuridium.Characters;
using Futuridium.Items;
using Futuridium.World;
using Utils = Futuridium.Game.Utils;

namespace Futuridium
{
    public static class MainClass
    {
        public static void Main(string[] args)
        {
            var size = new Size(1280, 720);
            Run(size);
            Environment.Exit(0);
        }

        private static void Run(Size size)
        {
            var engine = new Engine("Futuridium", (int) size.Width, (int) size.Height, 60);
#if DEBUG
            engine.debugCollisions = true;
#else
            //engine.FullScreen();
#endif
            //Input.Initialize(engine);


            engine.SpawnObject("game", Game.Game.Instance);

            engine.ClearEveryFrame = false;
            engine.Run();
        }

        public static void LoadAssets(Engine engine)
        {
            // set the base path for assets
            Asset.BasePath = "..\\..\\Assets";
            // misc music
            engine.LoadAsset("sound_soundtrack", new AudioAsset(Path.Combine("sound", "misc", "soundtrack.ogg"), true));
            engine.LoadAsset("sound_levelup", new AudioAsset(Path.Combine("sound", "misc", "levelup.ogg")));
            // base
            engine.LoadAsset("logo", new SpriteAsset("Futuridium.png"));
            engine.LoadAsset("sound_death", new AudioAsset(Path.Combine("sound", "NPC", "death.ogg")));
            // animated player
            Utils.LoadAnimation(engine, "player_animated", Path.Combine("characters", "player.png"), 3, 4);
            Player.Init(engine);
            // enemies
            Utils.LoadAnimation(engine, "scorpion", Path.Combine("characters", "scorpion.png"), 6, 4);
            Utils.LoadAnimation(engine, "goblins", Path.Combine("characters", "goblins.png"), 12, 8);
            Utils.LoadAnimation(engine, "ogre", Path.Combine("characters", "ogre.png"), 4, 4);
            Utils.LoadAnimation(engine, "snake", Path.Combine("characters", "snake.png"), 4, 4);
            // bosses
            // spells
            engine.LoadAsset("bullet", new SpriteAsset(Path.Combine("spells", "singleBullet.png")));
            engine.LoadAsset("sound_energy_bullet", new AudioAsset(Path.Combine("sound", "battle", "energy_bullet.ogg")));
            engine.LoadAsset("orb", new SpriteAsset(Path.Combine("spells", "bullets.png"), 436, 327, 64, 64));
            engine.LoadAsset("sound_energy_orb", new AudioAsset(Path.Combine("sound", "battle", "energy_bullet.ogg")));
            engine.LoadAsset("sound_drivex", new AudioAsset(Path.Combine("sound", "battle", "drivex.ogg")));
            // decorations
            engine.LoadAsset("blood", new SpriteAsset(Path.Combine("background", "blood.png")));
            engine.LoadAsset("skull", new SpriteAsset(Path.Combine("background", "skull.png")));
            engine.LoadAsset("sadskull", new SpriteAsset(Path.Combine("background", "sadskull.png")));
            // background
            LoadBackground(engine);
            // doors
            engine.LoadAsset("top_door", new SpriteAsset(Path.Combine("background", "top_door.png"), 0, 0, 45, 70));
            engine.LoadAsset("bottom_door",
                new SpriteAsset(Path.Combine("background", "bottom_door.png"), 51*4 + 1, 0, 45, 70));
            engine.LoadAsset("left_door",
                new SpriteAsset(Path.Combine("background", "left_door.png"), 0, 51*4 + 1, 70, 45));
            engine.LoadAsset("right_door", new SpriteAsset(Path.Combine("background", "right_door.png"), 0, 0, 70, 45));
            engine.LoadAsset("escape_floor", new SpriteAsset(Path.Combine("background", "escape_floor.png")));
            engine.LoadAsset("sound_door_open", new AudioAsset(Path.Combine("sound", "world", "door_open.ogg")));
            engine.LoadAsset("sound_door_close", new AudioAsset(Path.Combine("sound", "world", "door_close.ogg")));
        }

        private static void LoadBackground(Engine engine)
        {
            var backgroundPath = Path.Combine("background", "static_background.jpg");
            var staticBackgroundAsset = new SpriteAsset(backgroundPath);
            engine.LoadAsset("static_background", staticBackgroundAsset);
            // TOP
            engine.LoadAsset("background_topleft",
                new SpriteAsset(backgroundPath, 0, 0, GameBackground.WallWidth, GameBackground.WallHeight));
            engine.LoadAsset("background_topright",
                new SpriteAsset(
                    backgroundPath, staticBackgroundAsset.Width - GameBackground.WallWidth,
                    0, GameBackground.WallWidth, GameBackground.WallHeight));
            engine.LoadAsset("background_topstart",
                new SpriteAsset(backgroundPath, GameBackground.WallWidth, 0,
                    staticBackgroundAsset.Width/2 - GameBackground.WallWidth, GameBackground.WallHeight));
            engine.LoadAsset("background_topend",
                new SpriteAsset(
                    backgroundPath, staticBackgroundAsset.Width/2, 0,
                    staticBackgroundAsset.Width/2 - GameBackground.WallWidth, GameBackground.WallHeight));
            engine.LoadAsset("background_topcenter",
                new SpriteAsset(
                    backgroundPath, staticBackgroundAsset.Width/2 - GameBackground.WallWidth/2, 0,
                    GameBackground.WallWidth, GameBackground.WallHeight));
            // LEFT
            engine.LoadAsset("background_leftstart",
                new SpriteAsset(
                    backgroundPath, 0, GameBackground.WallHeight, GameBackground.WallWidth,
                    staticBackgroundAsset.Height/2 - GameBackground.WallHeight));
            engine.LoadAsset("background_leftend",
                new SpriteAsset(
                    backgroundPath, 0, staticBackgroundAsset.Height/2 - GameBackground.WallHeight,
                    GameBackground.WallWidth, staticBackgroundAsset.Height/2 - GameBackground.WallHeight));
            engine.LoadAsset("background_leftcenter",
                new SpriteAsset(
                    backgroundPath, 0, staticBackgroundAsset.Height/2 - GameBackground.WallHeight/2,
                    GameBackground.WallWidth, GameBackground.WallHeight));
            // RIGHT
            engine.LoadAsset("background_rightstart",
                new SpriteAsset(
                    backgroundPath, staticBackgroundAsset.Width - GameBackground.WallWidth,
                    GameBackground.WallHeight, GameBackground.WallWidth,
                    staticBackgroundAsset.Height/2 - GameBackground.WallHeight));
            engine.LoadAsset("background_rightend",
                new SpriteAsset(
                    backgroundPath, staticBackgroundAsset.Width - GameBackground.WallWidth,
                    staticBackgroundAsset.Height/2 - GameBackground.WallHeight,
                    GameBackground.WallWidth, staticBackgroundAsset.Height/2 - GameBackground.WallHeight));
            engine.LoadAsset("background_rightcenter",
                new SpriteAsset(
                    backgroundPath, staticBackgroundAsset.Width - GameBackground.WallWidth,
                    GameBackground.WallHeight, GameBackground.WallWidth,
                    staticBackgroundAsset.Height/2 + GameBackground.WallHeight/2));
            // BOTTOM
            engine.LoadAsset("background_bottomleft",
                new SpriteAsset(
                    backgroundPath, 0,
                    staticBackgroundAsset.Height - GameBackground.WallHeight, GameBackground.WallWidth,
                    GameBackground.WallHeight));
            engine.LoadAsset("background_bottomright",
                new SpriteAsset(
                    backgroundPath, staticBackgroundAsset.Width - GameBackground.WallWidth,
                    staticBackgroundAsset.Height - GameBackground.WallHeight, GameBackground.WallWidth,
                    GameBackground.WallHeight));
            engine.LoadAsset("background_bottomstart",
                new SpriteAsset(
                    backgroundPath, GameBackground.WallWidth,
                    staticBackgroundAsset.Height - GameBackground.WallHeight + GameBackground.BottomBlockDiff,
                    staticBackgroundAsset.Width/2 - GameBackground.WallWidth,
                    GameBackground.WallHeight - GameBackground.BottomBlockDiff));
            engine.LoadAsset("background_bottomend",
                new SpriteAsset(
                    backgroundPath, staticBackgroundAsset.Width/2,
                    staticBackgroundAsset.Height - GameBackground.WallHeight + GameBackground.BottomBlockDiff,
                    staticBackgroundAsset.Width/2 - GameBackground.WallWidth,
                    GameBackground.WallHeight - GameBackground.BottomBlockDiff));
            engine.LoadAsset("background_bottomcenter",
                new SpriteAsset(
                    backgroundPath, staticBackgroundAsset.Width/2 - GameBackground.WallWidth/2,
                    staticBackgroundAsset.Height - GameBackground.WallHeight + GameBackground.BottomBlockDiff,
                    GameBackground.WallWidth, GameBackground.WallHeight - GameBackground.BottomBlockDiff));
            // BLOCKS
            var startX = 205;
            var startY = 211;
            engine.LoadAsset("background_blocks0",
                new SpriteAsset(
                    backgroundPath, startX,
                    startY, GameBackground.BlockSizeX*2,
                    GameBackground.BlockSizeY*2));
            engine.LoadAsset("background_blocks1",
                new SpriteAsset(
                    backgroundPath, startX + GameBackground.BlockSizeX*2 - 2,
                    startY, GameBackground.BlockSizeX*2,
                    GameBackground.BlockSizeY*2));
            engine.LoadAsset("background_blocks2",
                new SpriteAsset(
                    backgroundPath, startX + GameBackground.BlockSizeX*4 - 4,
                    startY, GameBackground.BlockSizeX*2,
                    GameBackground.BlockSizeY*2));
            engine.LoadAsset("background_blocks3",
                new SpriteAsset(
                    backgroundPath, startX + GameBackground.BlockSizeX*6 - 5,
                    startY, GameBackground.BlockSizeX*2,
                    GameBackground.BlockSizeY*2));
            engine.LoadAsset("background_blocks4",
                new SpriteAsset(
                    backgroundPath, startX,
                    startY + GameBackground.BlockSizeX*2 + 3, GameBackground.BlockSizeX*2,
                    GameBackground.BlockSizeY*2));
            engine.LoadAsset("background_blocks5",
                new SpriteAsset(
                    backgroundPath, startX + GameBackground.BlockSizeX*2 - 2,
                    startY + GameBackground.BlockSizeX*2 + 3, GameBackground.BlockSizeX*2,
                    GameBackground.BlockSizeY*2));
            engine.LoadAsset("background_blocks6",
                new SpriteAsset(
                    backgroundPath, startX + GameBackground.BlockSizeX*4 - 4,
                    startY + GameBackground.BlockSizeX*2 + 3, GameBackground.BlockSizeX*2,
                    GameBackground.BlockSizeY*2));
            engine.LoadAsset("background_blocks7",
                new SpriteAsset(
                    backgroundPath, startX + GameBackground.BlockSizeX*6 - 5,
                    startY + GameBackground.BlockSizeX*2 + 3, GameBackground.BlockSizeX*2,
                    GameBackground.BlockSizeY*2));
            // SHADOW
            engine.LoadAsset("background_shadow_top", new SpriteAsset(Path.Combine("background", "shadow_top.png")));
            engine.LoadAsset("background_shadow_left", new SpriteAsset(Path.Combine("background", "shadow_left.png")));
            engine.LoadAsset("background_shadow_bottom",
                new SpriteAsset(Path.Combine("background", "shadow_bottom.png")));
            engine.LoadAsset("background_shadow_right", new SpriteAsset(Path.Combine("background", "shadow_right.png")));

            BasicItems.LoadAssets(engine);
        }
    }
}