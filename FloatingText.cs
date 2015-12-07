using System;
using Aiv.Engine;

namespace Futuridium
{
    public class FloatingText : TextObject
    {
        private readonly Character owner;
        private readonly float paddingStep = 25f;
        private float lifeSpawn = 3.5f;
        private float padding = 10f;
        private float xPadding;

        public FloatingText(Character owner, string text, string color) : base("Arial", 14, color)
        {
            this.owner = owner;
            name =
                $"{((Game) owner.engine.objects["game"]).CurrentFloor.CurrentRoom.name}_floatingtext_{Guid.NewGuid()}";
            order = owner.order - 1;
            this.text = text;
            xPadding = (float) new Random((int) DateTime.Now.Ticks).NextDouble();
        }

        public override void Update()
        {
            lifeSpawn -= deltaTime;
            if (lifeSpawn < 0)
                Destroy();
            // cos(x) => [0, 1] / 4 => [0, 0.25] + 0.25 => [0.25, 0.5]
            x = owner.x + (int) (owner.width*(0.25 + Math.Cos(xPadding)/4));
            y = owner.y - (int) padding;
            padding += deltaTime*paddingStep;
            xPadding = xPadding + deltaTime*0.4f;
        }
    }
}