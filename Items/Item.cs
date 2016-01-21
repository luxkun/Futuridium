using System;
using System.Collections.Generic;
using System.Drawing;
using Aiv.Engine;
using Futuridium.Characters;
using Futuridium.World;

namespace Futuridium.Items
{
    public class Item : SpriteObject
    {
        public enum EffectType
        {
            Instant,
            Persistent,
            PerRoom
        }

        public Item(int width, int height) : base(width, height)
        {
            Effects = new List<Tuple<Action<Character>, EffectType>>();
        }

        public List<Tuple<Action<Character>, EffectType>> Effects { get; private set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public string ActivateSound { get; set; }
        public Room Room { get; set; }

        public override void Start()
        {
            base.Start();

            AddHitBox("mass", 0, 0, (int)BaseWidth, (int)BaseHeight);
        }

        public override void Update()
        {
            base.Update();
            ManageCollisions();
        }

        private void SpawnInfoText()
        {
            if (Engine.Objects.ContainsKey("infoText"))
                Engine.Objects["infoText"].Destroy();
            var itemInfo = new ItemInfo(this);
            Engine.SpawnObject(itemInfo);
        }

        private void ManageCollisions()
        {
            var collisions = CheckCollisions();
            foreach (var collision in collisions)
            {
                var player = collision.Other as Player;
                if (player != null)
                {
                    foreach (var tuple in Effects)
                    {
                        player.ApplyEffect(tuple.Item1, tuple.Item2);
                    }
                    if (ActivateSound != null)
                        AudioSource.Play(((AudioAsset)Engine.GetAsset("sound_" + ActivateSound)).Clip);

                    //Engine.PlaySound("sound_" + ActivateSound);
                    Room.RoomObjects.Remove(this);
                    Destroy();

                    SpawnInfoText();
                }
            }
        }

        public override GameObject Clone()
        {
            var go = new Item((int)Width, (int)Height)
            {
                CurrentSprite = CurrentSprite,
                Name = Name,
                Room = Room,
                X = X,
                Y = Y,
                ItemName = ItemName,
                Description = Description,
                Effects = new List<Tuple<Action<Character>, EffectType>>(Effects),
                ActivateSound = ActivateSound
            };
            if (Animations != null)
            {
                go.Animations = new Dictionary<string, Animation>(Animations.Count);
                foreach (var animKey in Animations.Keys)
                {
                    go.Animations[animKey] = Animations[animKey].Clone();
                    go.Animations[animKey].owner = go;
                }
            }
            go.CurrentAnimation = CurrentAnimation;
            return go;
        }
    }

    public class ItemInfo : GameObject
    {
        private readonly Item item;
        private TextObject descriptionText;
        private TextObject nameText;

        public ItemInfo(Item item)
        {
            this.item = item;

            IgnoreCamera = true;
            Order = 9;

            Name = $"infoText";

            OnDestroy += DestroyEvent;
        }

        private void DestroyEvent(object sender)
        {
            nameText.Destroy();
            descriptionText.Destroy();
        }

        public override void Start()
        {
            base.Start();

            var padding = 5f;

            nameText = new TextObject(0.9f, Color.White, alpha: 0.66f)
            {
                Text = item.ItemName,
                IgnoreCamera = true,
                Order = 9
            };
            var nameTextMeasure = nameText.Measure();
            nameText.X = Engine.Width/2f - nameTextMeasure.X/2;
            nameText.Y = Engine.Height - nameTextMeasure.Y - padding;

            descriptionText = new TextObject(0.66f, Color.White, alpha: 0.66f)
            {
                Text = item.Description,
                IgnoreCamera = true,
                Order = 9
            };
            var descriptionTextMeasure = descriptionText.Measure();
            descriptionText.X = Engine.Width/2f - descriptionText.Measure().X/2;
            descriptionText.Y = nameText.Y - descriptionTextMeasure.Y - padding;

            Engine.SpawnObject($"infoText_name", nameText);
            Engine.SpawnObject($"infoText_description", descriptionText);

            Engine.Timer.Set("infoText_destroy", 3.33f,
                (GameObject obj, object[] extraArgs) => { ((ItemInfo) extraArgs[0]).Destroy(); },
                extraArgs: new object[] {this});
        }
    }
}