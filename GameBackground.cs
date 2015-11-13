using System;
using Aiv.Engine;
using System.Collections.Generic;

namespace StupidAivGame
{
	public class GameBackground : Background
	{
		public SpriteAsset doorAsset;

		public int backgroundChosen;
		public Room room;

		public GameBackground (int backgroundChosen, Room room)
		{
			this.name = room.name + "_game_background";
			this.room = room;

			this.backgroundChosen = backgroundChosen;
		}

		public void SpawnBackgroundPart (int x, int y, SpriteAsset backgroundAsset) 
		{
			SpriteObject background = new SpriteObject ();
			background.x = blockW + backgroundAsset.sprite.Width * x;
			background.y = blockH + backgroundAsset.sprite.Height * y;
			background.currentSprite = backgroundAsset;
			background.order = 0;
			engine.SpawnObject (string.Format ("{2}_bgblock_{0}.{1}", x, y, name), background);
		}

		public override void Start () 
		{
			base.Start ();
			// TODO: random (with seed) inside game
			Random rnd = new Random ((int)DateTime.Now.Ticks);
			blockAsset = (SpriteAsset)engine.GetAsset ("block");
			blockW = blockAsset.sprite.Width;//(blockAsset).sprite.Width;
			blockH = blockAsset.sprite.Height;//(blockAsset).sprite.Height;
			doorAsset = (SpriteAsset)engine.GetAsset ("door");

			SpriteAsset backgroundAsset;
			if (backgroundChosen == 0) {
				backgroundAsset = (SpriteAsset)engine.GetAsset ("background_0");
				for (int x = 0; x <= engine.width / backgroundAsset.sprite.Width; x++)
					for (int y = 0; y <= engine.height / backgroundAsset.sprite.Height; y++) {
						SpawnBackgroundPart (x, y, backgroundAsset);
					}
			} else if (backgroundChosen == 1 || backgroundChosen == 2) {
				List<string> backgroundParts = ((Game)engine.objects["game"]).spritesAnimations ["background_" + backgroundChosen];
				backgroundAsset = (SpriteAsset)engine.GetAsset(backgroundParts [0]);
				for (int x = 0; x < engine.width / backgroundAsset.sprite.Width; x++)
					for (int y = 0; y < engine.height / backgroundAsset.sprite.Height; y++) {
						backgroundAsset = (SpriteAsset)engine.GetAsset(backgroundParts [rnd.Next (0, backgroundParts.Count)]);
						SpawnBackgroundPart (x, y, backgroundAsset);
					}
			}

			// other rooms blocks
			SpawnBlock(engine.width / 2 / blockW, 0, new Door(), name + "_top_door");
			SpawnBlock(engine.width / 2 / blockW, (engine.height - 1) / blockH, new Door(), name + "_bottom_door");
			SpawnBlock(0, engine.height / 2 / blockH, new Door(), name + "_left_door");
			SpawnBlock((engine.width - 1) / blockW, engine.height / 2 / blockH, new Door(), name + "_right_door");

			SpawnBorders ();
		}

		public void SetupDoorsForRoom (Room room)
		{
			((SpriteObject)engine.objects [name + "_left_door"]).currentSprite = (room.left != null) ? doorAsset : blockAsset;
			//((SpriteObject)engine.objects ["left_door"]).enabled = room.left != null;
			((SpriteObject)engine.objects [name + "_top_door"]).currentSprite = (room.top != null) ? doorAsset : blockAsset;
			//((SpriteObject)engine.objects ["top_door"]).enabled = room.top != null;
			((SpriteObject)engine.objects [name + "_right_door"]).currentSprite = (room.right != null) ? doorAsset : blockAsset;
			//((SpriteObject)engine.objects ["right_door"]).enabled = room.right != null;
			((SpriteObject)engine.objects [name + "_bottom_door"]).currentSprite = (room.bottom != null) ? doorAsset : blockAsset;
			//((SpriteObject)engine.objects ["bottom_door"]).enabled = room.bottom != null;
		}
	}
}

