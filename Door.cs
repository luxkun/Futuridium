using Aiv.Engine;

namespace StupidAivGame
{
    public class Door : SpriteObject
    {
        public override void Start()
        {
            AddHitBox(name, -5, -5, 42, 42);
        }
    }
}