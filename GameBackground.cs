using System;
using System.Collections.Generic;
using Aiv.Engine;

namespace StupidAivGame
{
    public class GameBackground : Background
    {
        public int backgroundChosen;
        public SpriteAsset doorAsset;
        public Room room;
        public bool spawnSmallObj = true;

        public GameBackground(int backgroundChosen, Room room)
        {
            name = room.name + "_game_background";
            this.room = room;

            order = 1;

            this.backgroundChosen = backgroundChosen;
        }

        public void SpawnBackgroundPart(int x, int y, SpriteObject background, int order = 0, int width = -1,
            int height = -1, int paddingx = 0, int paddingy = 0)
        {
            background = (SpriteObject) background.Clone();
            width = width == -1 ? background.currentSprite.sprite.Width : width;
            height = height == -1 ? background.currentSprite.sprite.Height : height;
            background.x = blockW + width*x + paddingx;
            background.y = blockH + height*y + paddingy;
            background.order = order;
            engine.SpawnObject(string.Format("{2}_bgblock_{0}.{1}_{3}", x, y, name, background.currentSprite.fileName),
                background);
        }

        public override void Start()
        {
            base.Start();
            // TODO: random (with seed) inside game
            var rnd = ((Game) engine.objects["game"]).random.GetRandom(name);
            blockAsset = (SpriteAsset) engine.GetAsset("block");
            blockAsset.name = "block";
            blockW = blockAsset.sprite.Width; //(blockAsset).sprite.Width;
            blockH = blockAsset.sprite.Height; //(blockAsset).sprite.Height;
            doorAsset = (SpriteAsset) engine.GetAsset("door");

            // not block*2 because blocks could go outside the screen area
            var gameWidth = engine.width - blockW;
            var gameHeight = engine.width - blockH;
            SpriteObject background;
            if (backgroundChosen == 0)
            {
                background = (SpriteObject) engine.objects["cache_background_0"];
                for (var x = 0; x <= gameWidth/background.currentSprite.sprite.Width; x++)
                    for (var y = 0; y <= gameHeight/background.currentSprite.sprite.Height; y++)
                    {
                        SpawnBackgroundPart(x, y, background);
                    }
            }
            else if (backgroundChosen == 1 || backgroundChosen == 2)
            {
                var backgroundParts =
                    ((Game) engine.objects["game"]).spritesAnimations["background_" + backgroundChosen];
                SpriteAsset backgroundSprite = ((SpriteObject)engine.objects[$"cache_background_{backgroundChosen}_0"]).currentSprite;
                for (var x = 0; x < gameWidth/ backgroundSprite.sprite.Width; x++)
                    for (var y = 0; y < gameHeight/ backgroundSprite.sprite.Height; y++)
                    {
                        background = (SpriteObject)engine.objects[$"cache_background_{backgroundChosen}_{rnd.Next(0, backgroundParts.Count)}"];
                        SpawnBackgroundPart(x, y, background);
                    }
            }

            if (spawnSmallObj)
            {
                var bloodAsset = (SpriteObject) engine.objects["cache_blood"];
                var skullAsset = (SpriteObject) engine.objects["cache_skull"];
                var sadSkullAsset = (SpriteObject) engine.objects["cache_sadskull"];
                for (var x = 0; x < gameWidth/blockW; x++)
                    for (var y = 0; y < gameHeight/blockH; y++)
                    {
                        var chosen = rnd.Next(0, 50*(room.roomType == 0 ? 5 : 1));
                        var paddingx = rnd.Next(0, 16);
                        var paddingy = rnd.Next(0, 16);
                        if (chosen == 0)
                            SpawnBackgroundPart(x, y, bloodAsset, 1, blockW, blockH, paddingx, paddingy);
                        else if (chosen == 1)
                            SpawnBackgroundPart(x, y, sadSkullAsset, 1, blockW, blockH, paddingx, paddingy);
                        else if (chosen == 2)
                            SpawnBackgroundPart(x, y, skullAsset, 1, blockW, blockH, paddingx, paddingy);
                    }
            }

            // other rooms blocks
            SpawnBlock(engine.width/2/blockW, 0, new Door(), name + "_top_door");
            SpawnBlock(engine.width/2/blockW, (engine.height - 1)/blockH, new Door(), name + "_bottom_door");
            SpawnBlock(0, engine.height/2/blockH, new Door(), name + "_left_door");
            SpawnBlock((engine.width - 1)/blockW, engine.height/2/blockH, new Door(), name + "_right_door");

            SpawnBorders();
        }

        internal static void Initialize(Engine engine)
        {
            // 0 = sprite, 1 = "animation"
            Dictionary<string, int> toGo = new Dictionary<string, int>
            {
                { "background_0", 0 },
                { "background_1", 1 },
                { "background_2", 1 },
                { "blood", 0 },
                { "skull", 0 },
                { "sadskull", 0 },
                { "block", 0 }
            };
            Game game = (Game) engine.objects["game"];
            foreach (KeyValuePair<string, int> pair in toGo)
            {
                if (pair.Value == 0)
                {
                    SpriteObject obj = new SpriteObject
                    {
                        currentSprite = (SpriteAsset) engine.GetAsset(pair.Key),
                        x = -999, 
                        name = "cache_" + pair.Key
                    };
                    engine.SpawnObject(obj);
                }
                else
                {
                    foreach (string assetName in game.spritesAnimations[pair.Key])
                    {
                        SpriteObject obj = new SpriteObject
                        {
                            currentSprite = (SpriteAsset) engine.GetAsset(assetName),
                            x = -999,
                            name = "cache_" + assetName
                        };
                        engine.SpawnObject(obj);
                    }
                }
            }
        }

        public void SetupDoorsForRoom(Room room)
        {
            ((SpriteObject) engine.objects[name + "_left_door"]).currentSprite = (room.left != null)
                ? doorAsset
                : blockAsset;
            //((SpriteObject)engine.objects ["left_door"]).enabled = room.left != null;
            ((SpriteObject) engine.objects[name + "_top_door"]).currentSprite = (room.top != null)
                ? doorAsset
                : blockAsset;
            //((SpriteObject)engine.objects ["top_door"]).enabled = room.top != null;
            ((SpriteObject) engine.objects[name + "_right_door"]).currentSprite = (room.right != null)
                ? doorAsset
                : blockAsset;
            //((SpriteObject)engine.objects ["right_door"]).enabled = room.right != null;
            ((SpriteObject) engine.objects[name + "_bottom_door"]).currentSprite = (room.bottom != null)
                ? doorAsset
                : blockAsset;
            //((SpriteObject)engine.objects ["bottom_door"]).enabled = room.bottom != null;
        }
    }
}