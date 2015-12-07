using System;
using Aiv.Engine;
using OpenTK;

namespace Futuridium.Spells
{
    public class Spell : GameObject
    {
        // energy usage per cast

        // helper variables, used to move the object with precision

        // is either the point where the spell is casted
        // or the Direction where the spell should go

        // speed per second

        public int EnergyUsage { get; set; }

        public int EnergyUsagePerSecond { get; set; }

        public float VX { get; set; }

        public float VY { get; set; }

        public Vector2 Direction { get; set; }

        public int Speed { get; set; }

        public Character Owner { get; set; }

        public override void Start()
        {
            OnAfterUpdate += AfterUpdate;
        }

        public void AfterUpdate(Object sender)
        {
            if (Math.Abs(VX) > 1)
            {
                x += (int)VX;
                VX -= (int)VX;
            }
            if (Math.Abs(VY) > 1)
            {
                y += (int)VY;
                VY -= (int)VY;
            }
            Owner.Level.energy -= (int) (EnergyUsagePerSecond * deltaTime);
        }

        // to use for moving spells (ex. Bullet)
        public void NextMove()
        {
            Vector2 nextStep = Direction.Normalized() * (Speed * deltaTime);
            VX += nextStep.X;
            VY += nextStep.Y;
        }
    }
}
