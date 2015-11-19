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

        public void SpawnBackgroundPart(int x, int y, SpriteAsset backgroundAsset, int order = 0, int width = -1,
            int height = -1, int paddingx = 0, int paddingy = 0)
        {
            width = width == -1 ? backgroundAsset.sprite.Width : width;
            height = height == -1 ? backgroundAsset.sprite.Height : height;
            var background = new SpriteObject();
            background.x = blockW + width*x + paddingx;
            background.y = blockH + height*y + paddingy;
            background.currentSprite = backgroundAsset;
            background.order = order;
            engine.SpawnObject(string.Format("{2}_bgblock_{0}.{1}_{3}", x, y, name, backgroundAsset.fileName),
                background);
        }

        public override void Start()
        {
            base.Start();
            // TODO: random (with seed) inside game
            var rnd = ((Game) engine.objects["game"]).random.GetRandom(name);
            blockAsset = (SpriteAsset) engine.GetAsset("block");
            blockW = blockAsset.sprite.Width; //(blockAsset).sprite.Width;
            blockH = blockAsset.sprite.Height; //(blockAsset).sprite.Height;
            doorAsset = (SpriteAsset) engine.GetAsset("door");

            // not block*2 because blocks could go outside the screen area
            var gameWidth = engine.width - blockW;
            var gameHeight = engine.width - blockH;
            SpriteAsset backgroundAsset;
            if (backgroundChosen == 0)
            {
                backgroundAsset = (SpriteAsset) engine.GetAsset("background_0");
                for (var x = 0; x <= gameWidth/backgroundAsset.sprite.Width; x++)
                    for (var y = 0; y <= gameHeight/backgroundAsset.sprite.Height; y++)
                    {
                        SpawnBackgroundPart(x, y, backgroundAsset);
                    }
            }
            else if (backgroundChosen == 1 || backgroundChosen == 2)
            {
                var backgroundParts =
                    ((Game) engine.objects["game"]).spritesAnimations["background_" + backgroundChosen];
                backgroundAsset = (SpriteAsset) engine.GetAsset(backgroundParts[0]);
                for (var x = 0; x < gameWidth/backgroundAsset.sprite.Width; x++)
                    for (var y = 0; y < gameHeight/backgroundAsset.sprite.Height; y++)
                    {
                        backgroundAsset =
                            (SpriteAsset) engine.GetAsset(backgroundParts[rnd.Next(0, backgroundParts.Count)]);
                        SpawnBackgroundPart(x, y, backgroundAsset);
                    }
            }

            // boss
            if (spawnSmallObj)
            {
                var bloodAsset = (SpriteAsset) engine.GetAsset("blood");
                var skullAsset = (SpriteAsset) engine.GetAsset("skull");
                var sadSkullAsset = (SpriteAsset) engine.GetAsset("sadskull");
                for (var x = 0; x < gameWidth/blockW; x++)
                    for (var y = 0; y < gameHeight/blockH; y++)
                    {
                        var chosen = rnd.Next(0, 6*(room.roomType == 1 ? 1 : 20));
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