using System;
using Aiv.Engine;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;

namespace StupidAivGame
{
	public class FloatingText : TextObject
	{
		private Character owner;
		private float paddingStep = 2.5f;
		private float padding = 10f;
		private float xPadding = 0;
		private int lifeSpawn = 3500;
		public FloatingText (Character owner, string text, string color) : base("Arial", 14, color)
		{
			this.owner = owner;
			this.text = text;
			xPadding = (float)(new Random ((int)DateTime.Now.Ticks).NextDouble ());
		}

		public override void Update ()
		{
			Game.NormalizeTicks (ref this.deltaTicks);
			lifeSpawn -= this.deltaTicks;
			if (lifeSpawn < 0)
				this.Destroy ();
			// cos(x) => [0, 1] / 4 => [0, 0.25] + 0.25 => [0.25, 0.5]
			this.x = owner.x + (int)(owner.width * (0.25 + Math.Cos(xPadding)/4));
			this.y = owner.y - (int)padding;
			padding += (this.deltaTicks / 100f) * paddingStep;
			xPadding = (xPadding + (this.deltaTicks / 100f) * 0.4f);
		}
	}

}

