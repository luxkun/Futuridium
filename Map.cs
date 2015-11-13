using System;
using Aiv.Engine;
using System.Drawing;

namespace StupidAivGame
{
	public class Map : GameObject
	{
		MapBorders borders;
		public Map ()
		{
			name = "map";
			order = 100;
		}

		public override void Start ()
		{
			Floor floor = ((Game)engine.objects ["game"]).currentFloor;
			Console.WriteLine ("Map size: {0}.{1}", floor.mapWidth, floor.mapHeight);

			/*RectangleObject background = new RectangleObject ();
			background.width = engine.width;
			background.height = engine.height;
			background.color = Color.Black;
			background.fill = true;
			background.order = order;
			engine.SpawnObject ("map_background", background);*/

			borders = new MapBorders ();
			engine.SpawnObject ("map_borders", borders);
		}

		public override void Update ()
		{
		}
	}

	public class MapBorders : Background
	{
		public override void Start ()
		{
			name = "map_borders";
			blocksWithHitBox = false;

			blockW = 32;//(blockAsset).sprite.Width;
			blockH = 32;//(blockAsset).sprite.Height;

			base.Start ();

			blockObject = () => {
				RectangleObject rectangleBlock = new RectangleObject ();
				rectangleBlock.color = Color.SandyBrown;
				rectangleBlock.width = blockW;
				rectangleBlock.height = blockH;
				rectangleBlock.fill = true;
				rectangleBlock.order = 10;
				return rectangleBlock;
			};

			SpawnBorders ();

			Game game = (Game)engine.objects ["game"];
			int roomWidth = ((engine.width - blockW * 2) / (game.currentFloor.mapWidth));
			int roomHeight = ((engine.height - blockH * 2) / (game.currentFloor.mapHeight));
			if (game.currentFloor.mapWidth >= game.currentFloor.mapHeight) {
				roomHeight = (int)(roomWidth / 1.77);
			} else if (game.currentFloor.mapHeight > game.currentFloor.mapWidth) {
				roomWidth = (int)(roomHeight * 1.77);
			}

			int paddingX = (engine.width - roomWidth * game.currentFloor.mapWidth - blockW * 2) / 2 + blockW;
			int paddingY = (engine.height - roomHeight * game.currentFloor.mapHeight - blockH * 2) / 2 + blockH;

			for (int bx = 0; bx < game.currentFloor.mapWidth; bx++) {
				for (int by = 0; by < game.currentFloor.mapHeight; by++) {
					if (game.currentFloor.rooms [bx, by] != null) {
						//Room room = game.currentFloor.rooms [x, y];
						RectangleObject mapObj = new RectangleObject();
						mapObj.color = game.currentFloor.rooms [bx, by] == game.currentFloor.currentRoom ? Color.Green : (game.currentFloor.rooms [bx, by].enemies.Count > 0 ? Color.SaddleBrown : Color.Wheat);
						mapObj.fill = true;
						mapObj.x = paddingX + roomWidth * bx + roomHeight / 10;
						mapObj.y = paddingY + roomHeight * by + roomWidth / 10;
						mapObj.width = roomWidth - roomWidth / 5;
						mapObj.height = roomHeight - roomHeight / 5;
						mapObj.order = 10;
						Console.WriteLine ("Spawning map block at: {0}.{1}", mapObj.x, mapObj.y);
						engine.SpawnObject(string.Format("map_border_{0}_{1}", bx, by), mapObj);
					}
				}
			}
		}
	}
}

