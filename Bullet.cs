using System;
using Aiv.Engine;
using System.Collections.Generic;
using System.Diagnostics;

namespace StupidAivGame
{
	public class Bullet : CircleObject
	{
		private int range = 500;
		private int speed = 25;
		private const int MINSPEED = 2;
		public GameObject owner;
		private int direction;

		private int rangeToGo;

		public bool bounceBullet = true;

		private const int fadeAwayRange = 10;

		private int lastBounce = 0;
		// the same collider can collide once every bounceDelay 
		private int bounceDelay = 50;
		private HitBox lastColliderHitBox;
		private double bounceMod = 0.66; // speed = bounceMod * speed

		private Tuple<int, int> lastPoint;

		// 0 left; 1 top; 2 right; 3: bottom; 4: top-left; 5: top-right; 6: bottom-left; 7: bottom-right
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
			this.AddHitBox ("mass", 0, 0, this.radius * 2, this.radius * 2);
			lastPoint = Tuple.Create (this.x, this.y);
		}

		// simulate collision between two GameObject rectangles
		// returns 0: X collision ; 1: Y collision - defaults to 0
		public int SimulateCollision (Collision collision)
		{
			HitBox hitBox1 = this.hitBoxes [collision.hitBox]; // bullet
			HitBox hitBox2 = collision.other.hitBoxes [collision.otherHitBox];

			int x2 = hitBox2.x + collision.other.x;
			int y2 = hitBox2.y + collision.other.y;
			int w2 = hitBox2.width;
			int h2 = hitBox2.height;
			int w1 = hitBox1.width;
			int h1 = hitBox1.height;

			// should have same abs value
			int diffX = this.x - lastPoint.Item1;
			int diffY = this.y - lastPoint.Item2;
			Debug.Assert (Math.Abs(diffX) == Math.Abs(diffY));
			// ignores first step
			// could optimize by starting near second hitbox
			int xCollisions = 0;
			int yCollisions = 0;
			for (int step = Math.Abs(diffX); step >= 0; step--) {
				int x1 = hitBox1.x + this.x - Math.Sign(diffX) * step;
				int y1 = hitBox1.y + this.y - Math.Sign(diffY) * step;

				xCollisions = Math.Min(x2+w2, x1+w1) - Math.Max(x2, x1);
				if (y1 != y2 && (y1 + h1) != (y2 + h2) && y1 != (y2 + h2) && (y1 + h1) != y2)
					xCollisions = 0;
				yCollisions = Math.Min(y2+h2, y1+h1) - Math.Max(y1, y2);
				if (x1 != x2 && (x1 + w1) != (x2 + w2) && x1 != (x2 + w2) && (x1 + w1) != x2)
					yCollisions = 0;
				if (yCollisions > 0 || xCollisions > 0)
					break;
			}
			int result = (yCollisions < xCollisions) ? 1 : 0;
			Console.WriteLine ("Collision Simulation result: {0} ({1} vs {2})", result, xCollisions, yCollisions);

			return result;
		}

		// doesn't and won't handle collision between two moving objects
		private void BounceOrDie (Collision collision)
		{
			// TODO: FIX THIS
			int collisionDirection = -1;
			HitBox hitBox = this.hitBoxes [collision.hitBox];
			HitBox otherHitBox =  collision.other.hitBoxes [collision.otherHitBox];
			if (otherHitBox == null) {
				Console.WriteLine ("Collission with non-character objects not supported.");
			} else {
				int D = hitBox.width;//this.radius * 2; square hitbox
				int x1 = otherHitBox.x + collision.other.x;
				int y1 = otherHitBox.y + collision.other.y;
				int w1 = otherHitBox.width;
				int h1 = otherHitBox.height;

				int x2 = hitBox.x + this.x;
				int y2 = hitBox.y + this.y;
				int w2 = D;
				int h2 = D;
				// ball going left collides on the left
				if (direction == 0 && x2 < x1 && y2 < (y1 + h1) && y2 > y1)
					collisionDirection = 0;
				// ball going top collides on top
				else if (direction == 1 && y2 < y1 && (x2 + w2) > x1 && x2 < (x1 + w1))
					collisionDirection = 1;
				// ball going right collides on the right
				else if (direction == 2 && (x2 + w2) > x1 && y2 > y1 && y2 < (y1 + h1))
					collisionDirection = 0;
				// ball going bottom collides on the bottom
				else if (direction == 3 && (y2 + h2) > y1 && x2 < (x1 + w1) && (x2 + w2) > x1)
					collisionDirection = 1;
				else { // could replace all the other if, but is more costy... 
					collisionDirection = SimulateCollision(collision);
				}

				Console.WriteLine ("Collision direction:" + collisionDirection);
				if (collisionDirection != -1)
					BounceOrDie (collisionDirection, otherHitBox);
			}
		}
		// collisionDirection: 0: X collision ; 1: Y collision
		private void BounceOrDie (int collisionDirection, HitBox colliderHitBox) 
		{
			if (bounceBullet) {
				if (lastBounce > 0)
					lastBounce -= this.deltaTicks;
				if (lastBounce > 0 && (colliderHitBox == null || lastColliderHitBox == colliderHitBox))
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
				speed = (int) (speed * bounceMod);
				if (speed <= MINSPEED)
					speed = MINSPEED;
				lastColliderHitBox = colliderHitBox;
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

			int blockW = ((Background) engine.objects["background"]).blockW;
			int blockH = ((Background) engine.objects["background"]).blockH;
			if (this.x > (this.engine.width - blockW - this.radius*2)) {
				BounceOrDie (0, null);
				this.x = this.engine.width - blockW - this.radius * 2 - 1;
			} else if (this.x < blockW) {
				BounceOrDie (0, null);
				this.x = blockW + 1;
			} else if (this.y > (this.engine.height - blockH - this.radius * 2)) {
				BounceOrDie (1, null);
				this.y = this.engine.height - blockH - this.radius * 2 - 1;
			} else if (this.y < blockH) {
				BounceOrDie (1, null);
				this.y = blockH + 1;
			}

			List<Collision> collisions = this.CheckCollisions ();
			if (collisions.Count > 0)
				Console.WriteLine ("Bullet collides with n." + collisions.Count);
			foreach (Collision collision in collisions) {
				if (collision.other.name == owner.name || collision.other.name.StartsWith("bullet") || collision.other.name.StartsWith("orb"))
					continue;
				Console.WriteLine ("Bullet hits enemy: " + collision.other.name);
				BounceOrDie (collision);
				if (collision.other.name.StartsWith ("enemy")) {
					Game game = (Game) this.engine.objects ["game"];

					Enemy enemy = collision.other as Enemy;
					game.Hits (this, enemy, collision);

					break;
				}
			}
			lastPoint = Tuple.Create (this.x, this.y);
		}
	}
}

