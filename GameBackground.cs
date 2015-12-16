using System;
using System.Collections.Generic;
using System.Drawing;
using Aiv.Engine;

namespace Futuridium
{
    public class GameBackground : Background
    {
        private const int ExtraHitBoxSize = 5;
        private const int PaddingFromEnd = 15;
        public static readonly int AvailableBackgrounds = 1;

        public static readonly int WallWidth = 109;
        public static readonly int WallHeight = 109;


        public GameBackground(int backgroundChosen, Room room)
        {
            name = room.name + "_game_background";
            Room = room;

            order = 1;

            BackgroundChosen = backgroundChosen;
        }

        public int BackgroundChosen { get; set; }

        public SpriteAsset BottomDoorAsset { get; set; }

        public SpriteAsset LeftDoorAsset { get; set; }

        public SpriteAsset RightDoorAsset { get; set; }

        public SpriteAsset TopDoorAsset { get; set; }

        public Room Room { get; set; }

        public int SpawnOnDoorPadding { get; set; } = (ExtraHitBoxSize + PaddingFromEnd)*3;

        public bool SpawnSmallObj { get; set; } = true;

        private void SpawnBackgroundPart(int partX, int partY, SpriteObject background, int partOrder = 0,
            int width = -1,
            int height = -1, int paddingx = 0, int paddingy = 0)
        {
            background = (SpriteObject) background.Clone();
            width = width == -1 ? background.currentSprite.sprite.Width : width;
            height = height == -1 ? background.currentSprite.sprite.Height : height;
            background.x = width*partX + paddingx;
            background.y = height*partY + paddingy;
            background.order = partOrder;
            engine.SpawnObject(
                string.Format("{2}_bgblock_{0}.{1}_{3}", partX, partY, name, background.currentSprite.fileName),
                background);
        }

        public override void Start()
        {
            base.Start();

            var extraHitBoxSize = 100;
            AddHitBox("wallLeft", -extraHitBoxSize, 0, WallWidth + extraHitBoxSize, engine.height);
            AddHitBox("wallTop", WallWidth, -extraHitBoxSize, engine.width - WallWidth*2, WallHeight + extraHitBoxSize);
            AddHitBox("wallRight", engine.width - WallWidth, 0, WallWidth + extraHitBoxSize, engine.height);
            AddHitBox("wallBottom", WallWidth, engine.height - WallHeight, engine.width - WallWidth*2,
                WallHeight + extraHitBoxSize);

            var rnd = Game.Instance.Random.GetRandom(name);

            TopDoorAsset = (SpriteAsset) engine.GetAsset("top_door");
            LeftDoorAsset = (SpriteAsset) engine.GetAsset("left_door");
            RightDoorAsset = (SpriteAsset) engine.GetAsset("right_door");
            BottomDoorAsset = (SpriteAsset) engine.GetAsset("bottom_door");

            var gameWidth = engine.width;
            var gameHeight = engine.width;
            if (BackgroundChosen == 0)
            {
                var background = (SpriteObject) engine.objects["cache_static_background"];
                SpawnBackgroundPart(0, 0, background);
            }

            if (SpawnSmallObj)
            {
                var bloodAsset = (SpriteObject) engine.objects["cache_blood"];
                var skullAsset = (SpriteObject) engine.objects["cache_skull"];
                var sadSkullAsset = (SpriteObject) engine.objects["cache_sadskull"];
                var maxStepX = (gameWidth - WallWidth*2)/BlockW - 1;
                var maxStepY = (gameHeight - WallHeight*2)/BlockH - 1;
                var spawnedCount = 0;
                for (var partX = 0; partX < maxStepX; partX++)
                    for (var partY = 0; partY < maxStepY; partY++)
                    {
                        var chosen = rnd.Next(0, 50*(Room.RoomType == 0 ? 5 : 1));
                        var paddingx = rnd.Next(0, 16) + WallWidth;
                        var paddingy = rnd.Next(0, 16) + WallHeight;
                        SpriteObject chosenAsset = null;
                        if (chosen == 0)
                            chosenAsset = bloodAsset;
                        else if (chosen == 1)
                            chosenAsset = sadSkullAsset;
                        else if (chosen == 2)
                            chosenAsset = skullAsset;
                        if (chosenAsset != null)
                        {
                            SpawnBackgroundPart(partX, partY, chosenAsset, order, BlockW, BlockH, paddingx, paddingy);
                            spawnedCount++;
                        }
                    }
                Console.WriteLine(spawnedCount);
            }

            InitDoors();
        }

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
            foreach (var pair in toGo)
            {
                if (pair.Value == 0)
                {
                    var spriteAsset = (SpriteAsset) engine.GetAsset(pair.Key);
                    var obj = new SpriteObject
                    {
                        currentSprite = spriteAsset,
                        x = -1*(spriteAsset.sprite.Width + 10),
                        name = "cache_" + pair.Key
                    };
                    engine.SpawnObject(obj);
                }
                else
                {
                    foreach (var assetName in Game.Instance.SpritesAnimations[pair.Key])
                    {
                        var spriteAsset = (SpriteAsset) engine.GetAsset(assetName);
                        var obj = new SpriteObject
                        {
                            currentSprite = spriteAsset,
                            x = -1*(spriteAsset.sprite.Width + 10),
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
            if (Room.Top != null)
            {
                var topDoor = new Door(name + "_top_door") {order = order};
                engine.SpawnObject(topDoor);
                topDoor.currentSprite = TopDoorAsset;
                var topDoorWidth = Utils.FixBoxValue(topDoor.width);
                var topDoorHeight = Utils.FixBoxValue(topDoor.height);
                topDoor.x = engine.width/2 - topDoorWidth/2;
                topDoor.y = PaddingFromEnd;
            }
            if (Room.Left != null)
            {
                var leftDoor = new Door(name + "_left_door") {order = order};
                engine.SpawnObject(leftDoor);
                leftDoor.currentSprite = LeftDoorAsset;
                var leftDoorWidth = Utils.FixBoxValue(leftDoor.width);
                var leftDoorHeight = Utils.FixBoxValue(leftDoor.height);
                leftDoor.x = PaddingFromEnd;
                leftDoor.y = engine.height/2 - leftDoorHeight/2;
            }
            if (Room.Bottom != null)
            {
                var bottomDoor = new Door(name + "_bottom_door") {order = order};
                engine.SpawnObject(bottomDoor);
                bottomDoor.currentSprite = BottomDoorAsset;
                var bottomDoorWidth = Utils.FixBoxValue(bottomDoor.width);
                var bottomDoorHeight = Utils.FixBoxValue(bottomDoor.height);
                bottomDoor.x = engine.width/2 - bottomDoorWidth/2;
                bottomDoor.y = engine.height - bottomDoorHeight - PaddingFromEnd;
            }
            if (Room.Right != null)
            {
                var rightDoor = new Door(name + "_right_door") {order = order};
                engine.SpawnObject(rightDoor);
                rightDoor.currentSprite = RightDoorAsset;
                var rightDoorWidth = Utils.FixBoxValue(rightDoor.width);
                var rightDoorHeight = Utils.FixBoxValue(rightDoor.height);
                rightDoor.x = engine.width - rightDoorWidth - PaddingFromEnd;
                rightDoor.y = engine.height/2 - rightDoorHeight/2;
            }
        }

        public void OpenDoors()
        {
            if (Room.Left != null)
            {
                var leftDoor = (Door) engine.objects[name + "_left_door"];
                leftDoor.AddHitBox(leftDoor.name, 0, 0, Utils.FixBoxValue(leftDoor.width) + ExtraHitBoxSize,
                    Utils.FixBoxValue(leftDoor.height));
            }
            if (Room.Top != null)
            {
                var topDoor = (Door) engine.objects[name + "_top_door"];
                topDoor.AddHitBox(topDoor.name, 0, 0, Utils.FixBoxValue(topDoor.width),
                    Utils.FixBoxValue(topDoor.height) + ExtraHitBoxSize);
            }
            if (Room.Right != null)
            {
                var rightDoor = (Door) engine.objects[name + "_right_door"];
                rightDoor.AddHitBox(rightDoor.name, -1*ExtraHitBoxSize, 0,
                    Utils.FixBoxValue(rightDoor.width) + ExtraHitBoxSize, Utils.FixBoxValue(rightDoor.height));
            }
            if (Room.Bottom == null) return;
            var bottomDoor = (Door) engine.objects[name + "_bottom_door"];
            bottomDoor.AddHitBox(bottomDoor.name, 0, -1*ExtraHitBoxSize,
                Utils.FixBoxValue(bottomDoor.width), Utils.FixBoxValue(bottomDoor.height) + ExtraHitBoxSize);
        }
    }
}