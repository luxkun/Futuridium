using System;
using Aiv.Engine;

namespace StupidAivGame
{
	public class Background : SpriteObject
	{
		public GameObject [,] blocks;
		public int blockW;
		public int blockH;

		// use blockasset, if not available use blockobject
		public SpriteAsset blockAsset = null;
		public Func<GameObject> blockObject = null;

		public bool blocksWithHitBox = true;

		public Background ()
		{
			blockW = 32;
			blockH = 32;
		}

		public override void Start () 
		{
			blocks = new GameObject[engine.width / blockW + 1, engine.height / blockH + 1];
		}

		protected void SpawnBlock (int bx, int by)
		{
			string blockName = string.Format ("{0}_{1}_{2}_block", name, bx, by);
			GameObject block;
			if (blockAsset != null) {
				SpriteObject blockSprite = new SpriteObject ();
				blockSprite.currentSprite = blockAsset;
				block = blockSprite;
			} else if (blockObject != null) {
				block = blockObject();
			} else {
				return;
			}
			SpawnBlock (bx, by, block, blockName);
		}

		public void SpawnBlock (int bx, int by, GameObject spriteObj, string blockName)
		{
			Console.WriteLine ("bx: {0}, by: {1}, {2} {3}", bx, by, blocks.GetLength (0), blocks.Length);
			if (blocks [bx, by] == null) {
				blocks [bx, by] = spriteObj;
				blocks [bx, by].name = blockName;
				//blocks [x, y].currentSprite = (SpriteAsset) engine.GetAsset ("block");
				blocks [bx, by].x = bx * blockW;
				blocks [bx, by].y = by * blockH;
				blocks [bx, by].order = 1;
				if (blocksWithHitBox)
					blocks [bx, by].AddHitBox (blockName, 0, 0, blockW, blockH);
				Console.WriteLine ("Spawned block {0}.{1} at {2}.{3}", bx, by, blocks [bx, by].x, blocks [bx, by].y);
				engine.SpawnObject (blockName, blocks [bx, by]);
			}
		}

		public void DestroyBlock (int bx, int by)
		{
			if (blocks [bx, by] != null) {
				blocks [bx, by].Destroy ();
				blocks [bx, by] = null;
			}
		}

		public void SpawnBorders () 
		{
			Console.WriteLine ("Spawning borders.");
			int by = 0;
			int bx = 0;
			for (bx = 0; bx < (engine.width / blockW); bx++) {
				SpawnBlock (bx, by);
			}
			by = (engine.height - 1) / blockH;
			for (bx = 0; bx < (engine.width / blockW); bx++) {
				SpawnBlock (bx, by);
			}
			bx = 0;
			for (by = 0; by < (engine.height / blockH); by++) {
				SpawnBlock (bx, by);
			}
			bx = (engine.width - 1) / blockW;;
			for (by = 0; by < (engine.height / blockH); by++) {
				SpawnBlock (bx, by);
			}
		}
	}
}

