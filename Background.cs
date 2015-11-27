using System;
using System.Diagnostics;
using Aiv.Engine;

namespace StupidAivGame
{
    public class Background : GameObject
    {
        // use blockasset, if not available use blockobject
        public SpriteAsset blockAsset = null;
        public int blockH;
        public Func<GameObject> blockObject = null;
        public GameObject[,] blocks;

        public bool blocksWithHitBox = true;
        public int blockW;

        public Background()
        {
            blockW = 32;
            blockH = 32;
        }

        public override void Start()
        {
            blocks = new GameObject[engine.width/blockW + 1, engine.height/blockH + 1];
        }

        protected void SpawnBlock(int bx, int by)
        {
            var blockName = string.Format("{0}_{1}_{2}_block", name, bx, by);
            GameObject block;
            if (blockObject != null)
            {
                block = blockObject();
            }
            else if (blockAsset != null)
            {
                var blockSprite = (SpriteObject) ((SpriteObject) engine.objects[$"cache_{blockAsset.name}"]).Clone();
                block = blockSprite;
            }
            else
            {
                return;
            }
            SpawnBlock(bx, by, block, blockName);
        }

        public void SpawnBlock(int bx, int by, GameObject spriteObj, string blockName)
        {
            Debug.WriteLine("bx: {0}, by: {1}, {2} {3}", bx, by, blocks.GetLength(0), blocks.Length);
            if (blocks[bx, by] == null)
            {
                blocks[bx, by] = spriteObj;
                blocks[bx, by].name = blockName;
                //blocks [x, y].currentSprite = (SpriteAsset) engine.GetAsset ("block");
                blocks[bx, by].x = bx*blockW;
                blocks[bx, by].y = by*blockH;
                blocks[bx, by].order = order;
                if (blocksWithHitBox)
                    blocks[bx, by].AddHitBox(blockName, 0, 0, blockW, blockH);
                Debug.WriteLine("Spawned block {0}.{1} at {2}.{3}", bx, by, blocks[bx, by].x, blocks[bx, by].y);
                engine.SpawnObject(blockName, blocks[bx, by]);
            }
        }

        public void DestroyBlock(int bx, int by)
        {
            if (blocks[bx, by] != null)
            {
                blocks[bx, by].Destroy();
                blocks[bx, by] = null;
            }
        }

        public void SpawnBorders()
        {
            Debug.WriteLine("Spawning borders.");
            var by = 0;
            var bx = 0;
            for (bx = 0; bx < engine.width/blockW; bx++)
            {
                SpawnBlock(bx, by);
            }
            by = (engine.height - 1)/blockH;
            for (bx = 0; bx < engine.width/blockW; bx++)
            {
                SpawnBlock(bx, by);
            }
            bx = 0;
            for (by = 0; by < engine.height/blockH; by++)
            {
                SpawnBlock(bx, by);
            }
            bx = (engine.width - 1)/blockW;
            ;
            for (by = 0; by < engine.height/blockH; by++)
            {
                SpawnBlock(bx, by);
            }
        }
    }
}