using System;
using Aiv.Engine;

namespace StupidAivGame
{
    public class FloatingText : TextObject
    {
        private readonly Character owner;
        private readonly float paddingStep = 2.5f;
        private int lifeSpawn = 3500;
        private float padding = 10f;
        private float xPadding;

        public FloatingText(Character owner, string text, string color) : base("Arial", 14, color)
        {
            this.owner = owner;
            name = string.Format("{0}_floatingtext_{1}",
                ((Game) owner.engine.objects["game"]).currentFloor.currentRoom.name, Guid.NewGuid());
            order = owner.order - 1;
            this.text = text;
            xPadding = (float) new Random((int) DateTime.Now.Ticks).NextDouble();
        }

        public override void Update()
        {
            lifeSpawn -= deltaTicks;
            if (lifeSpawn < 0)
                Destroy();
            // cos(x) => [0, 1] / 4 => [0, 0.25] + 0.25 => [0.25, 0.5]
            x = owner.x + (int) (owner.width*(0.25 + Math.Cos(xPadding)/4));
            y = owner.y - (int) padding;
            padding += (deltaTicks / 1000f) * paddingStep;
            xPadding = xPadding + (deltaTicks / 1000f ) * 0.4f;
        }
    }
}