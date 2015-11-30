using System.Collections.Generic;
using Aiv.Engine;

namespace StupidAivGame
{
    public class GameBackground : Background
    {
        public int backgroundChosen;
        public SpriteAsset topDoorAsset;
        public SpriteAsset leftDoorAsset;
        public SpriteAsset bottomDoorAsset;
        public SpriteAsset rightDoorAsset;
        private const int extraHitBoxSize = 5;
        private const int paddingFromEnd = 0;
        public int spawnOnDoorPadding = (extraHitBoxSize + paddingFromEnd) * 3;

        public Room room;
        public bool spawnSmallObj = true;
        public static int availableBackgrounds = 1;

        private int wallWidth = 109;
        private int wallHeight = 109;

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
            background.x = width*x + paddingx;
            background.y = height*y + paddingy;
            background.order = order;
            engine.SpawnObject(string.Format("{2}_bgblock_{0}.{1}_{3}", x, y, name, background.currentSprite.fileName),
                background);
        }

        public override void Start()
        {
            base.Start();

            AddHitBox("wallLeft", 0, 0, wallWidth, engine.height);
            AddHitBox("wallTop", wallWidth, 0, engine.width - wallWidth*2, wallHeight);
            AddHitBox("wallRight", engine.width - wallWidth, 0, wallWidth, engine.height);
            AddHitBox("wallBottom", wallWidth, engine.height - wallHeight, engine.width - wallWidth * 2, wallHeight);

            var rnd = ((Game) engine.objects["game"]).random.GetRandom(name);

            topDoorAsset = (SpriteAsset)engine.GetAsset("top_door");
            leftDoorAsset = (SpriteAsset)engine.GetAsset("left_door");
            rightDoorAsset = (SpriteAsset)engine.GetAsset("right_door");
            bottomDoorAsset = (SpriteAsset)engine.GetAsset("bottom_door");

            var gameWidth = engine.width;
            var gameHeight = engine.width;
            SpriteObject background;
            if (backgroundChosen == 0)
            {
                background = (SpriteObject) engine.objects["cache_static_background"];
                SpawnBackgroundPart(0, 0, background);
            }

            if (spawnSmallObj)
            {
                var bloodAsset = (SpriteObject) engine.objects["cache_blood"];
                var skullAsset = (SpriteObject) engine.objects["cache_skull"];
                var sadSkullAsset = (SpriteObject) engine.objects["cache_sadskull"];
                int maxStepX = (gameWidth - wallWidth*2) / blockW;
                int maxStepY = (gameHeight - wallHeight*2) / blockH;
                for (var x = 0; x < maxStepX; x++)
                    for (var y = 0; y < maxStepY; y++)
                    {
                        var chosen = rnd.Next(0, 50*(room.roomType == 0 ? 5 : 1));
                        var paddingx = rnd.Next(0, 16) + wallWidth;
                        var paddingy = rnd.Next(0, 16) + wallHeight;
                        if (chosen == 0)
                            SpawnBackgroundPart(x, y, bloodAsset, 1, blockW, blockH, paddingx, paddingy);
                        else if (chosen == 1)
                            SpawnBackgroundPart(x, y, sadSkullAsset, 1, blockW, blockH, paddingx, paddingy);
                        else if (chosen == 2)
                            SpawnBackgroundPart(x, y, skullAsset, 1, blockW, blockH, paddingx, paddingy);
                    }
            }

            InitDoors();

            //SpawnBorders();
        }

        /*public new void SpawnBorders()
        {
            blockObject = () =>
            {
                var rectangleBlock = new RectangleObject();
                rectangleBlock.color = Color.SandyBrown;
                rectangleBlock.width = blockW;
                rectangleBlock.height = blockH;
                rectangleBlock.fill = true;
                rectangleBlock.order = 10;
                return rectangleBlock;
            };
            base.SpawnBorders();
        }*/

        internal static void Initialize(Engine engine)
        {
            // 0 = sprite, 1 = "animation"
            var toGo = new Dictionary<string, int>
            {
                {"static_background", 0},
                {"blood", 0},
                {"skull", 0},
                {"sadskull", 0}
            };
            var game = (Game) engine.objects["game"];
            foreach (var pair in toGo)
            {
                if (pair.Value == 0)
                {
                    var spriteAsset = (SpriteAsset) engine.GetAsset(pair.Key);
                    var obj = new SpriteObject
                    {
                        currentSprite = spriteAsset,
                        x = -1 * (spriteAsset.sprite.Width + 10),
                        name = "cache_" + pair.Key
                    };
                    engine.SpawnObject(obj);
                }
                else
                {
                    foreach (var assetName in game.spritesAnimations[pair.Key])
                    {
                        var spriteAsset = (SpriteAsset) engine.GetAsset(assetName);
                        var obj = new SpriteObject
                        {
                            currentSprite = spriteAsset,
                            x = -1 * (spriteAsset.sprite.Width + 10),
                            name = "cache_" + assetName
                        };
                        engine.SpawnObject(obj);
                    }
                }
            }
        }

        public void InitDoors()
        {
            // other rooms blocks
            if (room.top != null)
            {
                var topDoor = new Door(name + "_top_door");
                topDoor.order = order;
                engine.SpawnObject(topDoor);
                topDoor.currentSprite = topDoorAsset;
                int topDoorWidth = Utils.FixBoxValue(topDoor.width);
                int topDoorHeight = Utils.FixBoxValue(topDoor.height);
                topDoor.x = engine.width / 2 - topDoorWidth / 2;
                topDoor.y = paddingFromEnd;
            }
            if (room.left != null)
            {
                var leftDoor = new Door(name + "_left_door");
                leftDoor.order = order;
                engine.SpawnObject(leftDoor);
                leftDoor.currentSprite = leftDoorAsset;
                int leftDoorWidth = Utils.FixBoxValue(leftDoor.width);
                int leftDoorHeight = Utils.FixBoxValue(leftDoor.height);
                leftDoor.x = paddingFromEnd;
                leftDoor.y = engine.height / 2 - leftDoorHeight / 2;
            }
            if (room.bottom != null)
            {
                var bottomDoor = new Door(name + "_bottom_door");
                bottomDoor.order = order;
                engine.SpawnObject(bottomDoor);
                bottomDoor.currentSprite = bottomDoorAsset;
                int bottomDoorWidth = Utils.FixBoxValue(bottomDoor.width);
                int bottomDoorHeight = Utils.FixBoxValue(bottomDoor.height);
                bottomDoor.x = engine.width / 2 - bottomDoorWidth / 2;
                bottomDoor.y = engine.height - bottomDoorHeight - paddingFromEnd;
            }
            if (room.right != null)
            {
                var rightDoor = new Door(name + "_right_door");
                rightDoor.order = order;
                engine.SpawnObject(rightDoor);
                rightDoor.currentSprite = rightDoorAsset;
                int rightDoorWidth = Utils.FixBoxValue(rightDoor.width);
                int rightDoorHeight = Utils.FixBoxValue(rightDoor.height);
                rightDoor.x = engine.width - rightDoorWidth - paddingFromEnd;
                rightDoor.y = engine.height / 2 - rightDoorHeight / 2;
            }

        }
        public void OpenDoors()
        {
            if (room.left != null)
            {
                Door leftDoor = ((Door) engine.objects[name + "_left_door"]);
                leftDoor.AddHitBox(leftDoor.name, 0, 0, Utils.FixBoxValue(leftDoor.width) + extraHitBoxSize, Utils.FixBoxValue(leftDoor.height));
            }
            if (room.top != null)
            {
                Door topDoor = ((Door)engine.objects[name + "_top_door"]);
                topDoor.AddHitBox(topDoor.name, 0, 0, Utils.FixBoxValue(topDoor.width), Utils.FixBoxValue(topDoor.height) + extraHitBoxSize);
            }
            if (room.right != null)
            {
                Door rightDoor = ((Door)engine.objects[name + "_right_door"]);
                rightDoor.AddHitBox(rightDoor.name, -1 * extraHitBoxSize, 0, 
                    Utils.FixBoxValue(rightDoor.width) + extraHitBoxSize, Utils.FixBoxValue(rightDoor.height));
            }
            if (room.bottom != null)
            {
                Door bottomDoor = ((Door)engine.objects[name + "_bottom_door"]);
                bottomDoor.AddHitBox(bottomDoor.name, 0, -1 * extraHitBoxSize, 
                    Utils.FixBoxValue(bottomDoor.width), Utils.FixBoxValue(bottomDoor.height) + extraHitBoxSize);
            }
        }
    }
}