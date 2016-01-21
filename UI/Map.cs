using System.Diagnostics;
using System.Drawing;
using Aiv.Engine;
using Futuridium.World;

namespace Futuridium.UI
{
    public class Map : GameObject
    {
        private MapBorders borders;

        public Map()
        {
            Name = "map";
            Order = 100;

            OnDestroy += DestroyEvent;
        }

        private void DestroyEvent(object sender)
        {
            Engine.TimeModifier = 1f;
        }

        public override void Start()
        {
            base.Start();
            Engine.TimeModifier = 0f;

            var floor = Game.Game.Instance.CurrentFloor;
            Debug.WriteLine("Map size: {0}.{1}", floor.MapWidth, floor.MapHeight);

            /*RectangleObject background = new RectangleObject ();
			background.width = engine.width;
			background.height = engine.height;
			background.color = Color.Black;
			background.Fill = true;
			background.order = order;
			engine.SpawnObject ("map_background", background);*/

            borders = new MapBorders();
            Engine.SpawnObject("map_borders", borders);
        }
    }

    public class MapBorders : Background
    {
        public MapBorders()
        {
            Order = 10;
        }

        public override void Start()
        {
            base.Start();
            Name = "map_borders";
            BlocksWithHitBox = false;

            BlockW = 32; //(blockAsset).sprite.Width;
            BlockH = 32; //(blockAsset).sprite.Height;

            base.Start();

            BlockObject = () =>
            {
                var rectangleBlock = new RectangleObject(BlockW, BlockH)
                {
                    Color = Color.SandyBrown,
                    Fill = true,
                    Order = 10,
                    IgnoreCamera = true
                };
                return rectangleBlock;
            };

            SpawnBorders();

            var roomWidth = (Engine.Width - BlockW*2)/Game.Game.Instance.CurrentFloor.MapWidth;
            var roomHeight = (Engine.Height - BlockH*2)/Game.Game.Instance.CurrentFloor.MapHeight;
            if (Game.Game.Instance.CurrentFloor.MapWidth >= Game.Game.Instance.CurrentFloor.MapHeight)
            {
                roomHeight = (int) (roomWidth/1.77);
            }
            else if (Game.Game.Instance.CurrentFloor.MapHeight > Game.Game.Instance.CurrentFloor.MapWidth)
            {
                roomWidth = (int) (roomHeight*1.77);
            }

            var paddingX = (Engine.Width - roomWidth*Game.Game.Instance.CurrentFloor.MapWidth - BlockW*2)/2 + BlockW;
            var paddingY = (Engine.Height - roomHeight*Game.Game.Instance.CurrentFloor.MapHeight - BlockH*2)/2 + BlockH;

            for (var bx = 0; bx < Game.Game.Instance.CurrentFloor.MapWidth; bx++)
            {
                for (var by = 0; by < Game.Game.Instance.CurrentFloor.MapHeight; by++)
                {
                    if (Game.Game.Instance.CurrentFloor.Rooms[bx, by] != null)
                    {
                        //Room room = Game.Instance.currentFloor.rooms [x, y];
                        var mapObj = new RectangleObject(roomWidth - roomWidth/5, roomHeight - roomHeight/5)
                        {
                            Color =
                                Game.Game.Instance.CurrentFloor.Rooms[bx, by] ==
                                Game.Game.Instance.CurrentFloor.CurrentRoom
                                    ? Color.Green
                                    : (Game.Game.Instance.CurrentFloor.Rooms[bx, by].Enemies.Count > 0
                                        ? Color.SaddleBrown
                                        : Color.Wheat),
                            Fill = true,
                            X = paddingX + roomWidth*bx + roomHeight/10,
                            Y = paddingY + roomHeight*by + roomWidth/10,
                            Order = 10,
                            IgnoreCamera = true
                        };
                        Debug.WriteLine("Spawning map block at: {0}.{1}", mapObj.X, mapObj.Y);
                        Engine.SpawnObject($"map_border_{bx}_{by}", mapObj);
                    }
                }
            }
        }
    }
}