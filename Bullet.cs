using System;
using Aiv.Engine;
using System.Collections.Generic;

namespace StupidAivGame
{
	public class Bullet : CircleObject
	{
		private int range = 500;
		private int speed = 25;
		public GameObject owner;
		private int direction;

		private int rangeToGo;

		public bool bounceBullet = true;

		private const int fadeAwayRange = 10;

		private int lastBounce = 0;
		private int bounceDelay = 100;

		private Dictionary<int, int> bounceMap = new Dictionary<int, int> {{0, 2}, {1, 3}, {2, 0}, {3, 1}};
		//{4, 7}, {5, 6}, {6, 5}, {7, 4}

		public Bullet (GameObject owner, int direction)
		{
			this.owner = owner;
			Character ownerCharacter = owner as Character;
			if (owner != null) {
				this.speed = ownerCharacter.level.shotSpeed;
				this.range = ownerCharacter.level.shotRange;
			}
			this.order = 2;
			this.direction = direction;

			this.rangeToGo = this.range;
		}

		public override void Start ()
		{
			this.AddHitBox ("mass", 0, 0, 10, 10);
		}

		private void BounceOrDie (Collision collision)
		{
			// TODO: calcolate collisionDirection with Collision, for now static X collision
			BounceOrDie(0);
		}
		// collisionDirection: 0: X collision ; 1: Y collision
		private void BounceOrDie (int collisionDirection) 
		{
			if (bounceBullet) {
				if (lastBounce > 0)
					lastBounce -= this.deltaTicks;
				if (lastBounce > 0)
					return;
				if (bounceMap.ContainsKey (direction))
					direction = bounceMap [direction];
				else {
					// 4: top-left; 5: top-right; 6: bottom-left; 7: bottom-right
					if (direction == 4) {
						if (collisionDirection == 1)
							direction = 6;
						else if (collisionDirection == 0)
							direction = 5;
					} else if (direction == 5) {
						if (collisionDirection == 1)
							direction = 7;
						else if (collisionDirection == 0)
							direction = 4;
					} else if (direction == 6) {
						if (collisionDirection == 1)
							direction = 4;
						else if (collisionDirection == 0)
							direction = 7;
					} else if (direction == 7) {
						if (collisionDirection == 1)
							direction = 5;
						else if (collisionDirection == 0)
							direction = 6;
					}
				}
				lastBounce = bounceDelay;
			} else {
				this.Destroy ();
			}
		}

		public override void Update ()
		{
			if (rangeToGo <= 0) {
				this.Destroy ();
			}
			if (rangeToGo <= fadeAwayRange) {
				// resize...
			}
			// 0 left; 1 top; 2 right; 3 bottom; 4: top-left; 5: top-right; 6: bottom-left; 7: bottom-right
			if (direction == 7) {
				this.x += speed / 2;
				this.y += speed / 2;
			} else if (direction == 6) {
				this.x -= speed / 2;
				this.y += speed / 2;
			} else if (direction == 5) {
				this.x += speed / 2;
				this.y -= speed / 2;
			} else if (direction == 4) {
				this.x -= speed / 2;
				this.y -= speed / 2;
			} else if (direction == 3)
				this.y += speed;
			else if (direction == 2)
				this.x += speed;
			else if (direction == 1)
				this.y -= speed;
			else if (direction == 0)
				this.x -= speed;
			rangeToGo -= speed;
			if (this.x > this.engine.width || this.x < 0) {
				BounceOrDie (0);
			} else if (this.y > this.engine.height || this.y < 0) {
				BounceOrDie (1);
			}

			List<Collision> collisions = this.CheckCollisions ();
			if (collisions.Count > 0)
				Console.WriteLine ("Bullet collides with n." + collisions.Count);
			foreach (Collision collision in collisions) {
				Console.WriteLine ("Bullet hits enemy: " + collision.other.name);
				if (collision.other.name.StartsWith ("enemy")) {
					Game game = (Game) this.engine.objects ["game"];

					Enemy enemy = collision.other as Enemy;
					game.Hits (this, enemy, collision);

					BounceOrDie (collision);

					break;
				}
			}
		}
	}
}

