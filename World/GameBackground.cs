using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Aiv.Engine;
using OpenTK;

namespace Futuridium.World
{
    public class GameBackground : Background
    {
        private const int ExtraHitBoxSize = 5;
        private const int PaddingFromEnd = 15;

        public static readonly int AvailableBackgrounds = 1;
        public static readonly int BlockSizeX = 49; // 49
        public static readonly int BlockSizeY = 51; // 49
        public static readonly int BottomBlockDiff = 0; // 4
        public static readonly int WallHeight = 109;
        public static readonly int WallWidth = 109;
        private readonly Vector2 doorScale = new Vector2(1.33f, 1.33f);

        public GameBackground(int backgroundChosen, Room room)
        {
            Name = room.Name + "_game_background";
            Room = room;

            Order = 1;

            BackgroundChosen = backgroundChosen;
        }

        public int BackgroundChosen { get; set; }

        public SpriteAsset BottomDoorAsset { get; set; }

        public SpriteAsset LeftDoorAsset { get; set; }

        public SpriteAsset RightDoorAsset { get; set; }

        public Room Room { get; set; }

        public int SpawnOnDoorPadding { get; set; } = (ExtraHitBoxSize + PaddingFromEnd)*3;

        public bool SpawnSmallObj { get; set; } = true;

        public SpriteAsset TopDoorAsset { get; set; }

        private SpriteObject SpawnBackgroundPart(float x, float y, SpriteObject background, int partOrder = 0)
        {
            background = (SpriteObject) background.Clone();
            background.X = x;
            background.Y = y;
            background.Order = partOrder;
            Engine.SpawnObject(
                $"{Name}_bgblock_{x}.{y}_{Path.GetFileNameWithoutExtension(background.CurrentSprite.FileName)}",
                background
                );
            return background;
        }

        // not much userful but handy
        internal static void Initialize(Engine engine)
        {
            // 0 = sprite, 1 = "animation"
            var toGo = new Dictionary<string, int>
            {
                {"background_topleft", 0},
                {"background_topstart", 0},
                {"background_topcenter", 0},
                {"background_topend", 0},
                {"background_topright", 0},
                {"background_leftstart", 0},
                {"background_leftcenter", 0},
                {"background_leftend", 0},
                {"background_rightstart", 0},
                {"background_rightcenter", 0},
                {"background_rightend", 0},
                {"background_bottomleft", 0},
                {"background_bottomstart", 0},
                {"background_bottomcenter", 0},
                {"background_bottomend", 0},
                {"background_bottomright", 0},
                {"background_blocks0", 0},
                {"background_blocks1", 0},
                {"background_blocks2", 0},
                {"background_blocks3", 0},
                {"background_blocks4", 0},
                {"background_blocks5", 0},
                {"background_blocks6", 0},
                {"background_blocks7", 0},
                {"background_shadow_top", 0},
                {"background_shadow_left", 0},
                {"background_shadow_bottom", 0},
                {"background_shadow_right", 0},
                {"blood", 0},
                {"skull", 0},
                {"sadskull", 0}
            };
            foreach (var pair in toGo)
            {
                if (pair.Value == 0)
                {
                    var spriteAsset = (SpriteAsset) engine.GetAsset(pair.Key);
                    var obj = new SpriteObject(spriteAsset.Width, spriteAsset.Height)
                    {
                        CurrentSprite = spriteAsset,
                        X = -1*(spriteAsset.Width + 10)*2,
                        Name = "cache_" + pair.Key,
                        Enabled = false
                    };
                    engine.SpawnObject(obj);
                }
                //else
                //{
                //    foreach (var assetName in Game.Instance.SpritesAnimations[pair.Key])
                //    {
                //        var spriteAsset = (SpriteAsset) Engine.GetAsset(assetName);
                //        var obj = new SpriteObject
                //        {
                //            currentSprite = spriteAsset,
                //            x = -1*(spriteAsset.sprite.Width + 10),
                //            name = "cache_" + assetName
                //        };
                //        Engine.SpawnObject(obj);
                //    }
                //}
            }
        }

        public void InitDoors()
        {
            // other rooms blocks
            if (Room.Top != null)
            {
                var topDoor = new SpriteObject(TopDoorAsset.Width, TopDoorAsset.Height)
                {
                    Name = Name + "_top_door",
                    Order = Order
                };
                Engine.SpawnObject(topDoor);
                topDoor.CurrentSprite = TopDoorAsset;
                topDoor.Scale = doorScale;
                topDoor.X = Room.Width/2 - topDoor.Width/2;
                topDoor.Y = PaddingFromEnd;
            }
            if (Room.Left != null)
            {
                var leftDoor = new SpriteObject(LeftDoorAsset.Width, LeftDoorAsset.Height)
                {
                    Name = Name + "_left_door", Order = Order
                };
                Engine.SpawnObject(leftDoor);
                leftDoor.CurrentSprite = LeftDoorAsset;
                leftDoor.Scale = doorScale;
                leftDoor.X = PaddingFromEnd;
                leftDoor.Y = Room.Height/2 - leftDoor.Height/2;
            }
            if (Room.Bottom != null)
            {
                var bottomDoor = new SpriteObject(BottomDoorAsset.Width, BottomDoorAsset.Height)
                {
                    Name = Name + "_bottom_door", Order = Order
                };
                Engine.SpawnObject(bottomDoor);
                bottomDoor.CurrentSprite = BottomDoorAsset;
                bottomDoor.Scale = doorScale;
                bottomDoor.X = Room.Width/2 - bottomDoor.Width/2;
                bottomDoor.Y = Room.Height - bottomDoor.Height - PaddingFromEnd;
            }
            if (Room.Right != null)
            {
                var rightDoor = new SpriteObject(RightDoorAsset.Width, RightDoorAsset.Height)
                {
                    Name= Name + "_right_door",Order = Order
                };
                Engine.SpawnObject(rightDoor);
                rightDoor.CurrentSprite = RightDoorAsset;
                rightDoor.Scale = doorScale;
                rightDoor.X = Room.Width - rightDoor.Width - PaddingFromEnd;
                rightDoor.Y = Room.Height/2 - rightDoor.Height/2;
            }
        }

        public void OpenDoors()
        {
            if (Room.Left != null)
            {
                var leftDoor = (SpriteObject) Engine.Objects[Name + "_left_door"];
                leftDoor.AddHitBox(leftDoor.Name, 0, 0, (int) (leftDoor.BaseWidth + ExtraHitBoxSize),
                    (int) leftDoor.BaseHeight);
            }
            if (Room.Top != null)
            {
                var topDoor = (SpriteObject) Engine.Objects[Name + "_top_door"];
                topDoor.AddHitBox(topDoor.Name, 0, 0, (int) topDoor.BaseWidth,
                    (int) (topDoor.BaseHeight + ExtraHitBoxSize));
            }
            if (Room.Right != null)
            {
                var rightDoor = (SpriteObject) Engine.Objects[Name + "_right_door"];
                rightDoor.AddHitBox(rightDoor.Name, -1*ExtraHitBoxSize, 0,
                    (int) (rightDoor.BaseWidth + ExtraHitBoxSize), (int) rightDoor.BaseHeight);
            }
            if (Room.Bottom != null)
            {
                var bottomDoor = (SpriteObject) Engine.Objects[Name + "_bottom_door"];
                bottomDoor.AddHitBox(bottomDoor.Name, 0, -1*ExtraHitBoxSize,
                    (int) bottomDoor.BaseWidth, (int) (bottomDoor.BaseHeight + ExtraHitBoxSize));
            }
            if (Game.Game.Instance.CurrentFloor.FirstRoom != Room)
                AudioSource.Play(((AudioAsset)Engine.GetAsset("sound_door_open")).Clip);
        }

        public override void Start()
        {
            base.Start();

            var extraHitBoxSize = 100;
            AddHitBox("wallLeft", -extraHitBoxSize, 0, WallWidth + extraHitBoxSize, Room.Height);
            AddHitBox("wallTop", WallWidth, -extraHitBoxSize, Room.Width - WallWidth*2, WallHeight + extraHitBoxSize);
            AddHitBox("wallRight", Room.Width - WallWidth, 0, WallWidth + extraHitBoxSize, Room.Height);
            AddHitBox("wallBottom", WallWidth, Room.Height - WallHeight, Room.Width - WallWidth*2,
                WallHeight + extraHitBoxSize);

            var rnd = Game.Game.Instance.Random.GetRandom(Name);
            var genericRnd = new Random();

            TopDoorAsset = (SpriteAsset) Engine.GetAsset("top_door");
            LeftDoorAsset = (SpriteAsset) Engine.GetAsset("left_door");
            RightDoorAsset = (SpriteAsset) Engine.GetAsset("right_door");
            BottomDoorAsset = (SpriteAsset) Engine.GetAsset("bottom_door");

            if (BackgroundChosen == 0)
            {
                var background = (SpriteAsset) Engine.GetAsset("static_background");
                //if (Room.Width == Engine.Width && Room.Height == Engine.Height) // disabled
                //    SpawnBackgroundPart(0, 0, (SpriteObject) Engine.Objects["cache_static_background"]);
                // BLOCKS
                var blocks = new List<SpriteObject>
                {
                    (SpriteObject) Engine.Objects["cache_background_blocks0"],
                    (SpriteObject) Engine.Objects["cache_background_blocks1"],
                    (SpriteObject) Engine.Objects["cache_background_blocks2"],
                    (SpriteObject) Engine.Objects["cache_background_blocks3"],
                    (SpriteObject) Engine.Objects["cache_background_blocks4"],
                    (SpriteObject) Engine.Objects["cache_background_blocks5"],
                    (SpriteObject) Engine.Objects["cache_background_blocks6"],
                    (SpriteObject) Engine.Objects["cache_background_blocks7"]
                };
                var sizeX = blocks[0].Width;
                var sizeY = blocks[0].Height;
                for (var pY = 0; pY < (Room.Height - WallHeight*2)/sizeY + 1; pY++)
                {
                    for (var pX = 0; pX < (Room.Width - WallWidth*2)/sizeX + 1; pX++)
                    {
                        SpawnBackgroundPart(
                            WallWidth + sizeX*pX, WallHeight + sizeY*pY, blocks[genericRnd.Next(0, blocks.Count)]
                            );
                    }
                }
                // TOP
                var topEnd = (SpriteObject) Engine.Objects["cache_background_topend"];
                var topCenter = (SpriteObject) Engine.Objects["cache_background_topcenter"];
                SpawnBackgroundPart(0, 0, (SpriteObject) Engine.Objects["cache_background_topleft"]);
                SpawnBackgroundPart(WallWidth, 0, (SpriteObject) Engine.Objects["cache_background_topstart"]);
                var diffHorizontal = Room.Width - topEnd.Width*2 - WallWidth*2;
                if (diffHorizontal > 0)
                {
                    for (var p = 0; p < diffHorizontal/topCenter.Width + 1; p++)
                    {
                        SpawnBackgroundPart(WallWidth + topEnd.Width + topCenter.Width*p, 0, topCenter);
                    }
                }
                SpawnBackgroundPart(Room.Width - WallWidth - topEnd.Width, 0, topEnd);
                SpawnBackgroundPart(Room.Width - WallWidth, 0,
                    (SpriteObject) Engine.Objects["cache_background_topright"]);
                // SHADOW
                var shadowTop = (SpriteObject) Engine.Objects["cache_background_shadow_top"];
                for (var p = 0; p < (Room.Width - WallWidth*2)/shadowTop.Width + 1; p++)
                    SpawnBackgroundPart(WallWidth + shadowTop.Width*p, WallHeight, shadowTop);
                var shadowLeft = (SpriteObject) Engine.Objects["cache_background_shadow_left"];
                for (var p = 0; p < (Room.Height - WallHeight*2)/shadowLeft.Height + 1; p++)
                    SpawnBackgroundPart(WallWidth, WallHeight + shadowLeft.Height*p, shadowLeft);
                var shadowBottom = (SpriteObject) Engine.Objects["cache_background_shadow_bottom"];
                for (var p = 0; p < (Room.Width - WallWidth*2)/shadowBottom.Width + 1; p++)
                    SpawnBackgroundPart(WallWidth + shadowTop.Width*p, Room.Height - WallHeight - shadowBottom.Height,
                        shadowBottom);
                var shadowRight = (SpriteObject) Engine.Objects["cache_background_shadow_right"];
                for (var p = 0; p < (Room.Height - WallHeight*2)/shadowLeft.Height + 1; p++)
                    SpawnBackgroundPart(Room.Width - WallWidth - shadowRight.Width, WallHeight + shadowLeft.Height*p,
                        shadowRight);
                // LEFT
                var leftEnd = (SpriteObject) Engine.Objects["cache_background_leftend"];
                var leftCenter = (SpriteObject) Engine.Objects["cache_background_leftcenter"];
                SpawnBackgroundPart(0, WallHeight, (SpriteObject) Engine.Objects["cache_background_leftstart"]);
                var diffVertical = Room.Height - leftEnd.Height*2 - WallHeight*2;
                if (diffVertical > 0)
                {
                    for (var p = 0; p < diffVertical/leftCenter.Height + 1; p++)
                    {
                        SpawnBackgroundPart(0, WallHeight + leftEnd.Height + leftCenter.Height*p, leftCenter);
                    }
                }
                SpawnBackgroundPart(0, Room.Height - WallHeight - leftEnd.Height, leftEnd);
                // RIGHT
                var rightEnd = (SpriteObject) Engine.Objects["cache_background_rightend"];
                var rightCenter = (SpriteObject) Engine.Objects["cache_background_rightcenter"];
                SpawnBackgroundPart(Room.Width - WallWidth, WallHeight,
                    (SpriteObject) Engine.Objects["cache_background_rightstart"]);
                if (diffVertical > 0)
                {
                    for (var p = 0; p < diffVertical/rightCenter.Width + 1; p++)
                    {
                        SpawnBackgroundPart(Room.Width - WallWidth, WallHeight + rightEnd.Height + rightCenter.Height*p,
                            rightCenter);
                    }
                }
                SpawnBackgroundPart(Room.Width - WallWidth, Room.Height - WallHeight - rightEnd.Height, rightEnd);
                // BOTTOM
                var bottomEnd = (SpriteObject) Engine.Objects["cache_background_bottomend"];
                var bottomCenter = (SpriteObject) Engine.Objects["cache_background_bottomcenter"];
                SpawnBackgroundPart(0, Room.Height - WallHeight,
                    (SpriteObject) Engine.Objects["cache_background_bottomleft"]);
                SpawnBackgroundPart(WallWidth, Room.Height - WallHeight + BottomBlockDiff,
                    (SpriteObject) Engine.Objects["cache_background_bottomstart"]);
                if (diffHorizontal > 0)
                {
                    for (var p = 0; p < diffHorizontal/bottomCenter.Width + 1; p++)
                    {
                        SpawnBackgroundPart(
                            WallWidth + bottomEnd.Width + bottomCenter.Width*p,
                            Room.Height - WallHeight + BottomBlockDiff, bottomCenter);
                    }
                }
                SpawnBackgroundPart(Room.Width - WallWidth - topEnd.Width, Room.Height - WallHeight + BottomBlockDiff,
                    bottomEnd);
                SpawnBackgroundPart(Room.Width - WallWidth, Room.Height - WallHeight,
                    (SpriteObject) Engine.Objects["cache_background_bottomright"]);
            }

            if (SpawnSmallObj)
            {
                var bloodAsset = (SpriteObject) Engine.Objects["cache_blood"];
                var skullAsset = (SpriteObject) Engine.Objects["cache_skull"];
                var sadSkullAsset = (SpriteObject) Engine.Objects["cache_sadskull"];
                var maxStepX = (Room.Width - WallWidth*2)/BlockW - 2;
                var maxStepY = (Room.Height - WallHeight*2)/BlockH - 2;
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
                            var background = SpawnBackgroundPart(partX*BlockW + paddingx, partY*BlockW + paddingy, chosenAsset, Order);
                            float rndRadian = (float) (rnd.NextDouble()*2*Math.PI);
                            background.Rotation = rndRadian;
                            spawnedCount++;
                        }
                    }
                Debug.WriteLine(spawnedCount);
            }

            InitDoors();
        }
    }
}