using System;
using System.Collections.Generic;
using Futuridium.Characters;
using OpenTK;

namespace Futuridium.Items
{
    public class DropManager
    {
        private int globalItemCount;

        public DropManager(Character owner)
        {
            Owner = owner;
            // automatically add base global item drops
            DropTable = new DropTableType
            {
                Tuple.Create(BasicItems.MinorHpPotion, 2, 0.035f),
                Tuple.Create(BasicItems.HpPotion, 2, 0.015f),
                Tuple.Create(BasicItems.MediumHpPotion, 2, 0.01f),
                Tuple.Create(BasicItems.BigHpPotion, 2, 0.005f),
                Tuple.Create(BasicItems.MinorEnergyPotion, 2, 0.035f),
                Tuple.Create(BasicItems.EnergyPotion, 2, 0.015f),
                Tuple.Create(BasicItems.MediumEnergyPotion, 2, 0.01f),
                Tuple.Create(BasicItems.BigEnergyPotion, 2, 0.005f),
                Tuple.Create(BasicItems.BlackRock, 1, 0.02f),
                Tuple.Create(BasicItems.UsainBolt, 1, 0.025f),
                Tuple.Create(BasicItems.JohnnysMind, 1, 0.025f),
                Tuple.Create(BasicItems.GrowthHormone, 1, 0.02f),
                Tuple.Create(BasicItems.DrGregoryHouse, 1, 0.025f),
                Tuple.Create(BasicItems.EnergyAmulet, 1, 0.025f),
                Tuple.Create(BasicItems.ManaStone, 1, 0.02f)
            };
        }

        public DropTableType DropTable { get; set; }
        public Character Owner { get; }

        public List<Item> DropAndSpawn(Character enemy)
        {
            if (DropTable == null) return null;

            var padding = 2;
            Vector2 maxItemSize;
            var droppedItems = Drop(out maxItemSize, enemy);
            maxItemSize.X += padding;
            maxItemSize.Y += padding;
            var rows = (int) Math.Sqrt(droppedItems.Count);
            if (rows == 0)
                rows = 1;
            var columns = droppedItems.Count/rows;
            var itemPos = Owner.GetHitCenter();
            itemPos.X -= maxItemSize.X*(columns/2);
            itemPos.Y -= maxItemSize.X*(rows/2);
            var row = 0;
            var column = 0;
            for (var i = 0; i < droppedItems.Count; i++)
            {
                var item = droppedItems[i];
                var newItem = (Item) item.Clone();
                newItem.Name = $"{Owner.Name}_{newItem.ItemName}_{globalItemCount++}";
                newItem.Room = Game.Game.Instance.CurrentFloor.CurrentRoom;
                newItem.X = itemPos.X + maxItemSize.X*column;
                newItem.Y = itemPos.Y;
                Game.Game.Instance.CurrentFloor.CurrentRoom.RoomObjects.Add(newItem);
                Owner.Engine.SpawnObject(newItem);
                // update grid
                column++;
                if (column == columns)
                {
                    column = 0;
                    row++;
                    itemPos.Y += maxItemSize.Y;
                }
            }
            return droppedItems;
        }

        public List<Item> Drop(out Vector2 maxSize, Character enemy)
        {
            var results = new List<Item>();
            var rnd = Game.Game.Instance.Random.GetRandom(Owner.Name + "_dropm");
            var max = new Vector2();
            foreach (var tuple in DropTable)
            {
                var item = tuple.Item1;
                var P = tuple.Item3 * Owner.Level.DropModifier * enemy.Level.Luck * GlobalDropModifier;
                for (var i = 0; i < tuple.Item2; i++)
                {
                    if (rnd.NextDouble() <= P)
                    {
                        var sprite = item.CurrentSprite;
                        if (sprite == null)
                        {
                            sprite = item.Animations["base"].Sprites[0];
                        }
                        var sWidth = sprite.Width*item.Scale.X;
                        var sHeight = sprite.Height*item.Scale.Y;
                        if (max.X < sWidth)
                            max.X = sWidth;
                        if (max.Y < sHeight)
                            max.Y = sHeight;
                        results.Add(item);
                    }
                }
            }
            maxSize = max;
            return results;
        }

        public double GlobalDropModifier { get; private set; } = 2f;

        // (item, max_num_of_drops, perc_of_dropping)
        public class DropTableType : List<Tuple<Item, int, float>>
        {
        }
    }
}