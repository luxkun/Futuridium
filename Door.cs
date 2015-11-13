using System;
using Aiv.Engine;

namespace StupidAivGame
{
	public class Door : SpriteObject
	{
		public override void Start ()
		{
			this.AddHitBox (this.name, 0, 0, 32, 32);
		}
	}
}

