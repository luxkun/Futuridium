using System;
using System.IO;
using Aiv.Engine;
using Futuridium.Characters;
using OpenTK;
using Utils = Futuridium.Game.Utils;

namespace Futuridium.Items
{
    public static class BasicItems
    {
        public static Item MinorHpPotion { get; private set; }
        public static Item MinorEnergyPotion { get; private set; }
        public static Item EnergyPotion { get; private set; }
        public static Item MediumEnergyPotion { get; private set; }
        public static Item BigEnergyPotion { get; private set; }
        public static Item HpPotion { get; private set; }
        public static Item MediumHpPotion { get; private set; }
        public static Item BigHpPotion { get; private set; }
        public static Item BlackRock { get; private set; }
        public static Item UsainBolt { get; private set; }

        public static Item GrowthHormone { get; private set; }

        public static Item JohnnysMind { get; private set; }

        public static void LoadAssets(Engine engine)
        {
            // items
            //Utils.LoadAnimation(engine, "potions", Path.Combine("items", "potions.png"), 17, 11);
            Utils.LoadAnimation(engine, "items", Path.Combine("items", "items.png"), 14, 30);
            // audio
            engine.LoadAsset("sound_drink", new AudioAsset(Path.Combine("sound", "inventory", "bottle.ogg")));
            engine.LoadAsset("sound_powerup", new AudioAsset(Path.Combine("sound", "inventory", "powerup.ogg")));
        }

        public static void Initialize(Engine engine)
        {
            var potionScale = new Vector2(1.33f, 1.33f);

            // HP
            var minorHpPotionAsset = (SpriteAsset) engine.GetAsset(Utils.GetAssetName("items", 0, 3)[0]);
            MinorHpPotion = new Item(minorHpPotionAsset.Width, minorHpPotionAsset.Height)
            {
                Effects =
                {
                    Tuple.Create<Action<Character>, Item.EffectType>(
                        (Character owner) => owner.Level.Hp += owner.Level.MaxHp*0.07f, Item.EffectType.Instant)
                },
                ItemName = "Minor HP Potion",
                Description = "Phew there, some health points for you.",
                ActivateSound = "drink",
                CurrentSprite = minorHpPotionAsset,
                Scale = potionScale
            };
            //MinorHpPotion.AddAnimation("base", Utils.GetAssetName("potions", 0, 1), 7, engine);
            //MinorHpPotion.CurrentAnimation = "base";

            var hpPotionAsset = (SpriteAsset) engine.GetAsset(Utils.GetAssetName("items", 7, 2)[0]);
            HpPotion = new Item(hpPotionAsset.Width, hpPotionAsset.Height)
            {
                Effects =
                {
                    Tuple.Create<Action<Character>, Item.EffectType>(
                        (Character owner) => owner.Level.Hp += owner.Level.MaxHp*0.14f, Item.EffectType.Instant)
                },
                ItemName = "HP Potion",
                Description = "Feels good!",
                ActivateSound = "drink",
                CurrentSprite = hpPotionAsset,
                Scale = potionScale
            };

            var mediumHpPotionAsset = (SpriteAsset) engine.GetAsset(Utils.GetAssetName("items", 7, 3)[0]);
            MediumHpPotion = new Item(mediumHpPotionAsset.Width, mediumHpPotionAsset.Height)
            {
                Effects =
                {
                    Tuple.Create<Action<Character>, Item.EffectType>(
                        (Character owner) => owner.Level.Hp += owner.Level.MaxHp*0.24f, Item.EffectType.Instant)
                },
                ItemName = "Medium HP Potion",
                Description = "+24% hp",
                ActivateSound = "drink",
                CurrentSprite = mediumHpPotionAsset,
                Scale = potionScale
            };

            var bigHpPotionAsset = (SpriteAsset) engine.GetAsset(Utils.GetAssetName("items", 0, 2)[0]);
            BigHpPotion = new Item(bigHpPotionAsset.Width, bigHpPotionAsset.Height)
            {
                Effects =
                {
                    Tuple.Create<Action<Character>, Item.EffectType>(
                        (Character owner) => owner.Level.Hp += owner.Level.MaxHp*0.4f, Item.EffectType.Instant)
                },
                ItemName = "Big HP Potion",
                Description = "Gnom gnom gnom gnom",
                ActivateSound = "drink",
                CurrentSprite = bigHpPotionAsset,
                Scale = potionScale
            };

            // ENERGY
            var minorEnergyPotionAsset = (SpriteAsset)engine.GetAsset(Utils.GetAssetName("items", 3, 3)[0]);
            MinorEnergyPotion = new Item(minorEnergyPotionAsset.Width, minorEnergyPotionAsset.Height)
            {
                Effects =
                {
                    Tuple.Create<Action<Character>, Item.EffectType>(
                        (Character owner) => owner.Level.Energy += owner.Level.MaxEnergy*0.07f, Item.EffectType.Instant)
                },
                ItemName = "Minor Energy Potion",
                Description = "Spam time?",
                ActivateSound = "drink",
                CurrentSprite = minorEnergyPotionAsset,
                Scale = potionScale
            };

            var energyPotionAsset = (SpriteAsset) engine.GetAsset(Utils.GetAssetName("items", 10, 2)[0]);
            EnergyPotion = new Item(energyPotionAsset.Width, energyPotionAsset.Height)
            {
                Effects =
                {
                    Tuple.Create<Action<Character>, Item.EffectType>(
                        (Character owner) => owner.Level.Energy += owner.Level.MaxEnergy*0.14f, Item.EffectType.Instant)
                },
                ItemName = "Energy Potion",
                Description = "More energy for you",
                ActivateSound = "drink",
                CurrentSprite = energyPotionAsset,
                Scale = potionScale
            };

            var mediumEnergyPotionAsset = (SpriteAsset) engine.GetAsset(Utils.GetAssetName("items", 10, 3)[0]);
            MediumEnergyPotion = new Item(mediumEnergyPotionAsset.Width, mediumEnergyPotionAsset.Height)
            {
                Effects =
                {
                    Tuple.Create<Action<Character>, Item.EffectType>(
                        (Character owner) => owner.Level.Energy += owner.Level.MaxEnergy*0.24f, Item.EffectType.Instant)
                },
                ItemName = "Medium Energy Potion",
                Description = "Mmmhana",
                ActivateSound = "drink",
                CurrentSprite = mediumEnergyPotionAsset,
                Scale = potionScale
            };

            var bigEnergyPotionAsset = (SpriteAsset) engine.GetAsset(Utils.GetAssetName("items", 3, 2)[0]);
            BigEnergyPotion = new Item(bigEnergyPotionAsset.Width, bigEnergyPotionAsset.Height)
            {
                Effects =
                {
                    Tuple.Create<Action<Character>, Item.EffectType>(
                        (Character owner) => owner.Level.Energy += owner.Level.MaxEnergy*0.4f, Item.EffectType.Instant)
                },
                ItemName = "Big Energy Potion",
                Description = "Eeeeeeeeeeeeeeeeeeeeeeeenergy!!!",
                ActivateSound = "drink",
                CurrentSprite = bigEnergyPotionAsset,
                Scale = potionScale
            };

            // POWERUPS
            var blackRockAsset = (SpriteAsset) engine.GetAsset(Utils.GetAssetName("items", 13, 15)[0]);
            BlackRock = new Item(blackRockAsset.Width, blackRockAsset.Height)
            {
                Effects =
                {
                    Tuple.Create<Action<Character>, Item.EffectType>((Character owner) =>
                    {
                        owner.Level.Attack *= 1.15f;
                        owner.Level.Luck *= 1.05f;
                        owner.Level.MaxEnergy *= 1.05f;
                        owner.Level.SpellCd *= 0.95f;
                    }, Item.EffectType.Persistent),
                    Tuple.Create<Action<Character>, Item.EffectType>((Character owner) =>
                    {
                        owner.Level.Hp += owner.Level.MaxHp*0.33f;
                        owner.Level.Energy += owner.Level.MaxEnergy*0.33f;
                    }, Item.EffectType.Instant)
                },
                ItemName = "Black Rock",
                Description = "Glows with energy... emits a weird obscure light",
                ActivateSound = "powerup",
                CurrentSprite = blackRockAsset,
                Scale = potionScale
            };

            var usainBoltAsset = (SpriteAsset) engine.GetAsset(Utils.GetAssetName("items", 4, 14)[0]);
            UsainBolt = new Item(usainBoltAsset.Width, usainBoltAsset.Height)
            {
                Effects =
                {
                    Tuple.Create<Action<Character>, Item.EffectType>(
                        (Character owner) => { owner.Level.Speed *= 1.2f; }, Item.EffectType.Persistent),
                    Tuple.Create<Action<Character>, Item.EffectType>(
                        (Character owner) => { owner.Level.Hp += owner.Level.MaxHp*0.1f; }, Item.EffectType.Instant)
                },
                ItemName = "Usain Bolt",
                Description = "100m in 9.58s",
                ActivateSound = "powerup",
                CurrentSprite = usainBoltAsset,
                Scale = potionScale
            };

            var johnnysMindAsset = (SpriteAsset) engine.GetAsset(Utils.GetAssetName("items", 9, 18)[0]);
            JohnnysMind = new Item(johnnysMindAsset.Width, johnnysMindAsset.Height)
            {
                Effects =
                {
                    Tuple.Create<Action<Character>, Item.EffectType>((Character owner) =>
                    {
                        var mod = 0.2f;
                        var choose = new Random(Guid.NewGuid().GetHashCode()).Next(6);
                        var buff = new Buff
                        {
                            Type = Buff.BuffType.PerRoom
                        };
                        switch (choose)
                        {
                            case 0:
                                buff.Stat = "attack";
                                buff.Value = owner.Level.Attack*mod;
                                break;
                            case 1:
                                buff.Stat = "speed";
                                buff.Value = owner.Level.Speed*mod;
                                break;
                            case 2:
                                buff.Stat = "spellCd";
                                buff.Value = -owner.Level.SpellCd*mod;
                                break;
                            case 3:
                                buff.Stat = "spellRange";
                                buff.Value = owner.Level.SpellRange*mod;
                                break;
                            case 4:
                                buff.Stat = "spellSize";
                                buff.Value = owner.Level.SpellSize*mod;
                                break;
                            case 5:
                                buff.Stat = "spellSpeed";
                                buff.Value = owner.Level.SpellSpeed*mod;
                                break;
                        }
                        owner.LevelManager.AddBuff(buff);
                    }, Item.EffectType.PerRoom),
                    Tuple.Create<Action<Character>, Item.EffectType>(
                        (Character owner) => { owner.Level.Luck *= 1.1f; }, Item.EffectType.Persistent)
                },
                ItemName = "Johnny's mind",
                Description = "The random one",
                ActivateSound = "powerup",
                CurrentSprite = johnnysMindAsset,
                Scale = potionScale
            };

            var growthHormoneAsset = (SpriteAsset) engine.GetAsset(Utils.GetAssetName("items", 8, 4)[0]);
            GrowthHormone = new Item(growthHormoneAsset.Width, growthHormoneAsset.Height)
            {
                Effects =
                {
                    Tuple.Create<Action<Character>, Item.EffectType>((Character owner) =>
                    {
                        owner.Level.Attack *= 1.22f;
                        owner.Level.Size += 0.20f;
                        owner.Level.Speed *= 0.95f;
                        owner.Level.SpellSize *= 1.25f;
                    }, Item.EffectType.Persistent),
                    Tuple.Create<Action<Character>, Item.EffectType>(
                        (Character owner) => { owner.Level.Hp -= owner.Level.MaxHp*0.2f; }, Item.EffectType.Instant),
                    Tuple.Create<Action<Character>, Item.EffectType>(
                        (Character owner) => { owner.Level.Hp += owner.Level.MaxHp*0.01f; }, Item.EffectType.PerRoom)
                },
                ItemName = "Growth Hormone",
                Description = "Stimulates growth, cell reproduction, and cell regeneration.",
                ActivateSound = "powerup",
                CurrentSprite = growthHormoneAsset,
                Scale = potionScale * 2f
            };

            var drGregoryHouseAsset = (SpriteAsset) engine.GetAsset(Utils.GetAssetName("items", 7, 22)[0]);
            DrGregoryHouse = new Item(drGregoryHouseAsset.Width, drGregoryHouseAsset.Height)
            {
                Effects =
                {
                    Tuple.Create<Action<Character>, Item.EffectType>((Character owner) =>
                    {
                        owner.Level.MaxHp *= 1.1f;
                    }, Item.EffectType.Persistent),
                    Tuple.Create<Action<Character>, Item.EffectType>(
                        (Character owner) => { owner.Level.Hp = owner.Level.MaxHp; }, Item.EffectType.Instant),
                    Tuple.Create<Action<Character>, Item.EffectType>(
                        (Character owner) => { owner.Level.Hp += owner.Level.MaxHp*0.03f; }, Item.EffectType.PerRoom)
                },
                ItemName = "Dr. Gregory House",
                Description = "Want some vicodin?",
                ActivateSound = "powerup",
                CurrentSprite = drGregoryHouseAsset,
                Scale = potionScale
            };

            var energyAmuletAsset = (SpriteAsset) engine.GetAsset(Utils.GetAssetName("items", 8, 19)[0]);
            EnergyAmulet = new Item(energyAmuletAsset.Width, energyAmuletAsset.Height)
            {
                Effects =
                {
                    Tuple.Create<Action<Character>, Item.EffectType>((Character owner) =>
                    {
                        owner.Level.MaxEnergy *= 1.1f;
                    }, Item.EffectType.Persistent),
                    Tuple.Create<Action<Character>, Item.EffectType>(
                        (Character owner) => { owner.Level.Energy = owner.Level.MaxEnergy; }, Item.EffectType.Instant),
                    Tuple.Create<Action<Character>, Item.EffectType>(
                        (Character owner) => { owner.Level.Energy += owner.Level.MaxEnergy*0.03f; }, Item.EffectType.PerRoom)
                },
                ItemName = "Energy Amulet",
                Description = "You feel full of energy!",
                ActivateSound = "powerup",
                CurrentSprite = energyAmuletAsset,
                Scale = potionScale
            };

            var manaStoneAsset = (SpriteAsset) engine.GetAsset(Utils.GetAssetName("items", 2, 15)[0]);
            ManaStone = new Item(manaStoneAsset.Width, manaStoneAsset.Height)
            {
                Effects =
                {
                    Tuple.Create<Action<Character>, Item.EffectType>((Character owner) =>
                    {
                        owner.Level.Attack *= 1.05f;
                        owner.Level.SpellEnergyModifier *= 0.9f;
                    }, Item.EffectType.Persistent),
                    Tuple.Create<Action<Character>, Item.EffectType>(
                        (Character owner) => { owner.Level.Energy -= owner.Level.MaxEnergy*0.03f; }, Item.EffectType.PerRoom)
                },
                ItemName = "Mana Stone",
                Description = "Mana? What's 'mana'? Why is this thing leeching my energy??",
                ActivateSound = "powerup",
                CurrentSprite = manaStoneAsset,
                Scale = potionScale
            };
        }

        public static Item ManaStone { get; private set; }

        public static Item EnergyAmulet { get; private set; }

        public static Item DrGregoryHouse { get; private set; }
    }
}