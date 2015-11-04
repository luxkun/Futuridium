using System;
using Aiv.Engine;

namespace StupidAivGame
{
	public class Background : SpriteObject
	{
		public Background ()
		{
			this.order = -1;
		}

		public override void Start () 
		{
			this.x = 0;
			this.y = 0;
		}
	}
}

