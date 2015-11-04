using System;
using Aiv.Engine;
using System.Collections.Generic;

namespace StupidAivGame
{
	public class Orb : CircleObject
	{
		private Character owner;

		public int orbRange = 100;
		private double _orbStretch = 0.0;
		private bool orbStretching; // true: decrease ; false: increase
		public int orbStretchSteps = 50;
		public double orbStretch = 0.5; // orbRange goes from orbRange * orbStretch to orbRange
		public double orbSpeed = 0.04;

		private double angleTick = 0;

		public Orb (Character owner)
		{
			this.owner = owner;
			this.fill = true;
		}

		public override void Start ()
		{
			this.x = owner.x + orbRange;
			this.y = owner.y;
			this.AddHitBox ("mass", 0, 0, 10, 10);
		}

		public Tuple<int, int> getPoints (double angle)
		{
			return Tuple.Create((int) (Math.Cos(angle) * orbRange * (1 - _orbStretch)), (int) (Math.Sin(angle) * orbRange * (1 - _orbStretch)));
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
			ManageStretch ();
			// rotate
			angleTick += orbSpeed;
			Tuple<int, int> points = getPoints (angleTick);
			this.x = owner.x + points.Item1;
			this.y = owner.y + points.Item2;

			List<Collision> collisions = this.CheckCollisions ();
			if (collisions.Count > 0)
				Console.WriteLine ("Orb collides with n." + collisions.Count);
			foreach (Collision collision in collisions) {
				Console.WriteLine ("Orb hits enemy: " + collision.other.name);
				if (collision.other.name.StartsWith ("enemy")) {
					Game game = (Game) this.engine.objects ["game"];

					Enemy enemy = collision.other as Enemy;
					// broken, deliberately
					game.Hits (owner, enemy, collision);

					break;
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

