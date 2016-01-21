using System;
using System.Drawing;
using Aiv.Engine;
using Futuridium.Characters;

namespace Futuridium.UI
{
    public sealed class FloatingText : TextObject
    {
        private readonly Character owner;
        private readonly float paddingStepX = 6f;
        private readonly float paddingStepY = 25f;
        private readonly float startingX;
        private readonly float startingY;
        private float padding = 10f;
        private float xPadding;

        public FloatingText(Character owner, string text, Color color, float scale) : base(scale, color)
        {
            this.owner = owner;
            Name =
                $"{Game.Game.Instance.CurrentFloor.CurrentRoom.Name}_floatingtext_{Guid.NewGuid()}";
            Order = owner.Order;
            Text = text;
            xPadding = (float) new Random((int) DateTime.Now.Ticks).NextDouble()*3;

            startingX = owner.X;
            startingY = owner.Y;

            IgnoreCamera = false;
            //Font = new System.Drawing.Font("arial", size);
            // TODO: COLOR
        }

        public override void Start()
        {
            base.Start();
            Timer.Set("lifeSpan", 1.5f);
        }

        public override void Update()
        {
            base.Update();
            if (Timer.Get("lifeSpan") < 0)
                Destroy();
            // cos(x) => [0, 1] * 0.33 => [0, 0.33] + 0.33 => [0.33, 0.66]
            X = startingX + (int) (owner.Width*(0.33f + Math.Cos(xPadding)*0.33f));
            Y = startingY - (int) padding;
            padding += DeltaTime*paddingStepY;
            xPadding += DeltaTime*paddingStepX;
        }
    }
}