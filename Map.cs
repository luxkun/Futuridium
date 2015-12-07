using System.Diagnostics;
using System.Drawing;
using Aiv.Engine;

namespace Futuridium
{
    public class Map : GameObject
    {
        private MapBorders borders;

        public Map()
        {
            name = "map";
            order = 100;
        }

        public override void Start()
        {
            var floor = ((Game) engine.objects["game"]).CurrentFloor;
            Debug.WriteLine("Map size: {0}.{1}", floor.MapWidth, floor.MapHeight);

            /*RectangleObject background = new RectangleObject ();
			background.width = engine.width;
			background.height = engine.height;
			background.color = Color.Black;
			background.fill = true;
			background.order = order;
			engine.SpawnObject ("map_background", background);*/

            borders = new MapBorders();
            engine.SpawnObject("map_borders", borders);
        }

        public override void Update()
        {
        }
    }

    public class MapBorders : Background
    {
        public MapBorders()
        {
            order = 10;
        }

        public override void Start()
        {
            name = "map_borders";
            BlocksWithHitBox = false;

            BlockW = 32; //(blockAsset).sprite.Width;
            BlockH = 32; //(blockAsset).sprite.Height;

            base.Start();

            BlockObject = () =>
            {
                var rectangleBlock = new RectangleObject();
                rectangleBlock.color = Color.SandyBrown;
                rectangleBlock.width = BlockW;
                rectangleBlock.height = BlockH;
                rectangleBlock.fill = true;
                rectangleBlock.order = 10;
                return rectangleBlock;
            };

            SpawnBorders();

            var game = (Game) engine.objects["game"];
            var roomWidth = (engine.width - BlockW*2)/game.CurrentFloor.MapWidth;
            var roomHeight = (engine.height - BlockH*2)/game.CurrentFloor.MapHeight;
            if (game.CurrentFloor.MapWidth >= game.CurrentFloor.MapHeight)
            {
                roomHeight = (int) (roomWidth/1.77);
            }
            else if (game.CurrentFloor.MapHeight > game.CurrentFloor.MapWidth)
            {
                roomWidth = (int) (roomHeight*1.77);
            }

            var paddingX = (engine.width - roomWidth*game.CurrentFloor.MapWidth - BlockW*2)/2 + BlockW;
            var paddingY = (engine.height - roomHeight*game.CurrentFloor.MapHeight - BlockH*2)/2 + BlockH;

            for (var bx = 0; bx < game.CurrentFloor.MapWidth; bx++)
            {
                for (var by = 0; by < game.CurrentFloor.MapHeight; by++)
                {
                    if (game.CurrentFloor.Rooms[bx, by] != null)
                    {
                        //Room room = game.currentFloor.rooms [x, y];
                        var mapObj = new RectangleObject();
                        mapObj.color = game.CurrentFloor.Rooms[bx, by] == game.CurrentFloor.CurrentRoom
                            ? Color.Green
                            : (game.CurrentFloor.Rooms[bx, by].enemies.Count > 0 ? Color.SaddleBrown : Color.Wheat);
                        mapObj.fill = true;
                        mapObj.x = paddingX + roomWidth*bx + roomHeight/10;
                        mapObj.y = paddingY + roomHeight*by + roomWidth/10;
                        mapObj.width = roomWidth - roomWidth/5;
                        mapObj.height = roomHeight - roomHeight/5;
                        mapObj.order = 10;
                        Debug.WriteLine("Spawning map block at: {0}.{1}", mapObj.x, mapObj.y);
                        engine.SpawnObject(string.Format("map_border_{0}_{1}", bx, by), mapObj);
                    }
                }
            }
        }
    }
}