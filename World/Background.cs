using System;
using System.Diagnostics;
using Aiv.Engine;

namespace Futuridium.World
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
            base.Start();
            Blocks =
                new GameObject[Game.Game.Instance.CurrentFloor.CurrentRoom.Width/BlockW + 1,
                    Game.Game.Instance.CurrentFloor.CurrentRoom.Height/BlockH + 1];
        }

        private void SpawnBlock(int bx, int by)
        {
            var blockName = $"{Name}_{bx}_{@by}_block";
            GameObject block;
            if (BlockObject != null)
            {
                block = BlockObject();
            }
            else if (BlockAsset != null)
            {
                var blockSprite = (SpriteObject) ((SpriteObject) Engine.Objects[$"cache_{BlockAsset.Name}"]).Clone();
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
                Blocks[bx, by].Name = blockName;
                //blocks [x, y].currentSprite = (SpriteAsset) engine.GetAsset ("block");
                Blocks[bx, by].X = bx*BlockW;
                Blocks[bx, by].Y = by*BlockH;
                Blocks[bx, by].Order = Order;
                if (BlocksWithHitBox)
                    Blocks[bx, by].AddHitBox(blockName, 0, 0, BlockW, BlockH);
                Debug.WriteLine("Spawned block {0}.{1} at {2}.{3}", bx, by, Blocks[bx, by].X, Blocks[bx, by].Y);
                Engine.SpawnObject(blockName, Blocks[bx, by]);
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
            var width = Game.Game.Instance.CurrentFloor.CurrentRoom.Width;
            var height = Game.Game.Instance.CurrentFloor.CurrentRoom.Height;
            for (bx = 0; bx < width/BlockW; bx++)
            {
                SpawnBlock(bx, by);
            }
            by = (height - 1)/BlockH;
            for (bx = 0; bx < width/BlockW; bx++)
            {
                SpawnBlock(bx, by);
            }
            bx = 0;
            for (by = 0; by < height/BlockH; by++)
            {
                SpawnBlock(bx, by);
            }
            bx = (width - 1)/BlockW;
            for (by = 0; by < height/BlockH; by++)
            {
                SpawnBlock(bx, by);
            }
        }
    }
}