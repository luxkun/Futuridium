using System;
using Aiv.Engine;
using System.Collections.Generic;
using OpenTK;

namespace StupidAivGame
{
	public class Orb : CircleObject
	{
		private Character owner;

		public int orbRange = 150;
		private double _orbStretch = 0.0;
		private bool orbStretching; // true: decrease ; false: increase
		public int orbStretchSteps = 50;
		public double orbStretch = 0.4; // orbRange goes from orbRange * orbStretch to orbRange
		public double orbSpeed = 0.08;

		private Vector2 virtPos = new Vector2 ();

		private double angleTick = 0;

		public Orb (Character owner)
		{
			this.order = 5;
			this.owner = owner;
			this.fill = true;
		}

		public override void Start ()
		{
			this.x = owner.x + orbRange;
			this.y = owner.y;
			this.AddHitBox ("mass", 0, 0, this.radius * 2, this.radius * 2);
		}

		public Vector2 GetNextStep (double angle)
		{
			return new Vector2 ((int)(Math.Cos(angle) * orbRange * (1 - _orbStretch)), (int)(Math.Sin(angle) * orbRange * (1 - _orbStretch)));
		}

		private void ManageStretch () 
		{
			if (_orbStretch <= 0.0) {
				orbStretching = true;
			} else if (_orbStretch >= orbStretch) {
				orbStretching = false;
			}
			_orbStretch += orbStretch / orbStretchSteps * (orbStretching ? 1 : -1);
		}

		public override void Update ()
		{
			if (((Game)engine.objects ["game"]).mainWindow == "game") {
				ManageStretch ();
				// rotate
				this.x = owner.x;
				this.y = owner.y;
				angleTick += orbSpeed * (this.deltaTicks / 100.0);
				Vector2 points = GetNextStep (angleTick);

				this.virtPos.X += (int)(points.X);
				this.virtPos.Y += (int)(points.Y);
				if (Math.Abs(this.virtPos.X) > 1) {
					this.x += (int)this.virtPos.X;
					this.virtPos.X -= (int)this.virtPos.X;
				}
				if (Math.Abs(this.virtPos.Y) > 1) {
					this.y += (int)this.virtPos.Y;
					this.virtPos.Y -= (int)this.virtPos.Y;
				}

				List<Collision> collisions = this.CheckCollisions ();
				foreach (Collision collision in collisions) {
					if (collision.other.name.StartsWith ("enemy")) {
						Console.WriteLine ("Orb hits enemy: " + collision.other.name);
						Game game = (Game)this.engine.objects ["game"];

						Enemy enemy = collision.other as Enemy;
						// broken, deliberately
						game.Hits (owner, enemy, collision, null);

						break;
					}
				}
			}
		}

		static int Randomize (int min, int max)
		{
			Random random = new Random (Environment.TickCount);
			return random.Next (min, max);
		}
	}
}

