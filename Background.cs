using System;
using System.Diagnostics;
using Aiv.Engine;

namespace Futuridium
{
    public class Background : GameObject
    {
        public Background()
        {
            BlockW = 32;
            BlockH = 32;
        }

        // use blockasset, if not available use blockobject

        public SpriteAsset BlockAsset { get; set; } = null;

        public int BlockH { get; set; }

        public int BlockW { get; set; }

        public bool BlocksWithHitBox { get; set; } = true;

        public GameObject[,] Blocks { get; set; }

        public Func<GameObject> BlockObject { get; set; } = null;

        public override void Start()
        {
            Blocks = new GameObject[engine.width/BlockW + 1, engine.height/BlockH + 1];
        }

        private void SpawnBlock(int bx, int by)
        {
            var blockName = $"{name}_{bx}_{@by}_block";
            GameObject block;
            if (BlockObject != null)
            {
                block = BlockObject();
            }
            else if (BlockAsset != null)
            {
                var blockSprite = (SpriteObject) ((SpriteObject) engine.objects[$"cache_{BlockAsset.name}"]).Clone();
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
            Debug.WriteLine("bx: {0}, by: {1}, {2} {3}", bx, by, Blocks.GetLength(0), Blocks.Length);
            if (Blocks[bx, by] == null)
            {
                Blocks[bx, by] = spriteObj;
                Blocks[bx, by].name = blockName;
                //blocks [x, y].currentSprite = (SpriteAsset) engine.GetAsset ("block");
                Blocks[bx, by].x = bx*BlockW;
                Blocks[bx, by].y = by*BlockH;
                Blocks[bx, by].order = order;
                if (BlocksWithHitBox)
                    Blocks[bx, by].AddHitBox(blockName, 0, 0, BlockW, BlockH);
                Debug.WriteLine("Spawned block {0}.{1} at {2}.{3}", bx, by, Blocks[bx, by].x, Blocks[bx, by].y);
                engine.SpawnObject(blockName, Blocks[bx, by]);
            }
        }

        public void DestroyBlock(int bx, int by)
        {
            if (Blocks[bx, by] != null)
            {
                Blocks[bx, by].Destroy();
                Blocks[bx, by] = null;
            }
        }

        public void SpawnBorders()
        {
            Debug.WriteLine("Spawning borders.");
            var by = 0;
            int bx;
            for (bx = 0; bx < engine.width/BlockW; bx++)
            {
                SpawnBlock(bx, by);
            }
            by = (engine.height - 1)/BlockH;
            for (bx = 0; bx < engine.width/BlockW; bx++)
            {
                SpawnBlock(bx, by);
            }
            bx = 0;
            for (by = 0; by < engine.height/BlockH; by++)
            {
                SpawnBlock(bx, by);
            }
            bx = (engine.width - 1)/BlockW;
            for (by = 0; by < engine.height/BlockH; by++)
            {
                SpawnBlock(bx, by);
            }
        }
    }
}