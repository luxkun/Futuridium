using System;
using Aiv.Engine;

namespace Futuridium
{
    public sealed class FloatingText : TextObject
    {
        private readonly Character owner;
        private readonly float paddingStepX = 5f;
        private readonly float paddingStepY = 25f;
        private float lifeSpawn = 1.5f;
        private float padding = 10f;
        private float xPadding;
        private readonly int startingX;
        private readonly int startingY;

        public FloatingText(Character owner, string text, string color, int size) : base("Arial", size, color)
        {
            this.owner = owner;
            name =
                $"{Game.Instance.CurrentFloor.CurrentRoom.name}_floatingtext_{Guid.NewGuid()}";
            order = owner.order - 1;
            this.text = text;
            xPadding = (float) new Random((int) DateTime.Now.Ticks).NextDouble();

            startingX = owner.x;
            startingY = owner.y;
        }

        public override void Update()
        {
            lifeSpawn -= deltaTime;
            if (lifeSpawn < 0)
                Destroy();
            // cos(x) => [0, 1] * 0.33 => [0, 0.33] + 0.33 => [0.33, 0.66]
            x = startingX + (int) (owner.width*(0.33f + Math.Cos(xPadding) * 0.33f));
            y = startingY - (int)padding;
            padding += deltaTime * paddingStepY;
            xPadding += deltaTime * paddingStepX;
        }
    }
}