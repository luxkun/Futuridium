using System;
using OpenTK;

namespace Futuridium
{
    public class Damage
    {
        private Vector2 direction;

        public Damage(Character character, Character enemy)
        {
            Character = character;
            Enemy = enemy;
        }

        public Vector2 Direction
        {
            get { return direction; }
            set
            {
                direction = value;
                InverseDirection = value*-1;
            }
        }

        public Vector2 InverseDirection { get; private set; }

        public Func<Character, Character, int> DamageFunc { private get; set; }

        public Character Enemy { get; set; }

        public bool IsCloseCombat { get; set; }

        public Character Character { get; set; }

        public int Caculate(Character character, Character enemy)
        {
            return DamageFunc(character, enemy);
        }
    }
}