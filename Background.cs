using System;
using Aiv.Engine;

namespace StupidAivGame
{
	public class Background : SpriteObject
	{
		private SpriteObject[,] blocks;
		public int blockW;
		public int blockH;

		public Background ()
		{
			this.order = -1;
			blockW = 32;
			blockH = 32;
		}

		public override void Start () 
		{
			this.x = 0;
			this.y = 0;
			blocks = new SpriteObject[engine.width / blockW, engine.height / blockH];
			SpawnBorders ();
			blockW = 32;//((SpriteAsset)engine.GetAsset ("block")).sprite.Width;
			blockH = 32;//((SpriteAsset)engine.GetAsset ("block")).sprite.Height;
		}

		private void SpawnBlock (int x, int y)
		{
			if (blocks [x, y] == null) {
				string blockName = string.Format ("block_{0}_{1}", x, y);
				blocks [x, y] = new SpriteObject ();
				blocks [x, y].currentSprite = (SpriteAsset) engine.GetAsset ("block");
				blocks [x, y].x = x * blockW;
				blocks [x, y].y = y * blockH;
				blocks [x, y].AddHitBox (blockName, 0, 0, blockW, blockH);
				Console.WriteLine ("Spawned block {0}.{1} at {2}.{3}", x, y, blocks [x, y].x, blocks [x, y].y);
				engine.SpawnObject (blockName, blocks [x, y]);
			}
		}

		private void DestroyBlock (int x, int y)
		{
			if (blocks [x, y] != null) {
				blocks [x, y].Destroy ();
				blocks [x, y] = null;
			}
		}

		public void SpawnBorders () 
		{
			Console.WriteLine ("Spawning borders.");
			int y = 0;
			int x = 0;
			for (x = 0; x < (engine.width / blockW); x++) {
				SpawnBlock (x, y);
			}
			y = (engine.height - 1) / blockH;
			for (x = 0; x < (engine.width / blockW); x++) {
				SpawnBlock (x, y);
			}
			x = 0;
			for (y = 0; y < (engine.height / blockH); y++) {
				SpawnBlock (x, y);
			}
			x = (engine.width - 1) / blockW;;
			for (y = 0; y < (engine.height / blockH); y++) {
				SpawnBlock (x, y);
			}
		}
	}
}

