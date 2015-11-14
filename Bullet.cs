using System;
using Aiv.Engine;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK;

namespace StupidAivGame
{
	public class Bullet : CircleObject
	{
		private int range = 500;
		public int startingSpeed = 25;
		public int speed;
		private const int MINSPEED = 2;
		public GameObject owner;
		private Vector2 direction;

		private int rangeToGo;

		public bool bounceBullet = false;

		private const float fadeAwayRange = 0.2f;
		private const float fadeAwayMod = 0.8f;
		private const int minRadius = 1;
		private double virtRadius;
		private Vector2 virtPos;

		private int lastBounce = 0;
		private int bounceDelay = 250;
		private double bounceMod = 0.8; // speed = bounceMod * speed

		private const int maxLifeSpan = 5000; // dies after 5s
		private int lifeSpan = 0;

		private Vector2 lastPoint;

		public Bullet (GameObject owner, Vector2 direction)
		{
			this.owner = owner;
			Character ownerCharacter = owner as Character;
			if (owner != null) {
				this.startingSpeed = ownerCharacter.level.shotSpeed;
				this.range = ownerCharacter.level.shotRange;
			}
			this.speed = this.startingSpeed;
			this.direction = direction;

			this.rangeToGo = this.range;
			this.fill = true;
			this.order = 6;
		}

		public override void Start ()
		{
			this.AddHitBox ("mass", 0, 0, this.radius * 2, this.radius * 2);
			lastPoint = new Vector2 (this.x, this.y);
		}

		// simulate collision between two GameObject rectangles
		// returns 0: X collision ; 1: Y collision
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
			int diffX = this.x - (int)lastPoint.X;
			int diffY = this.y - (int)lastPoint.Y;
			Debug.Assert (Math.Abs(diffX) == Math.Abs(diffY));
			// ignores first step
			// could optimize by starting near second hitbox
			int xCollisions = 0;
			int yCollisions = 0;
			int steps = Math.Max (Math.Abs (diffX), Math.Abs (diffY));
			for (int step = steps; step >= 0; step--) {
				int x1 = hitBox1.x + this.x - Math.Sign(diffX) * step;
				int y1 = hitBox1.y + this.y - Math.Sign(diffY) * step;

				int tempxCollisions = Math.Min(x2+w2, x1+w1) - Math.Max(x2, x1);
				if (y1 != y2 && (y1 + h1) != (y2 + h2) && y1 != (y2 + h2) && (y1 + h1) != y2)
					tempxCollisions = 0;
				int tempyCollisions = Math.Min(y2+h2, y1+h1) - Math.Max(y1, y2);
				if (x1 != x2 && (x1 + w1) != (x2 + w2) && x1 != (x2 + w2) && (x1 + w1) != x2)
					tempyCollisions = 0;
				if (tempxCollisions > xCollisions)
					xCollisions = tempxCollisions;
				if (tempyCollisions > yCollisions)
					yCollisions = tempyCollisions;
				// keeping tempcollisions for now
				if (yCollisions > 0 || xCollisions > 0)
					break;
			}
			int result = -1;
			if (yCollisions < xCollisions)
				result = 1;
			else if (yCollisions > xCollisions || (yCollisions == xCollisions && yCollisions > 0))
				result = 0;
			Console.WriteLine ("Collision Simulation result: {0} ({1} vs {2})", result, xCollisions, yCollisions);

			return result;
		}

		// doesn't and won't handle collision between two moving objects
		private bool BounceOrDie (Collision collision)
		{
			if (bounceBullet) {
				int collisionDirection = -1;
				HitBox hitBox = this.hitBoxes [collision.hitBox];
				HitBox otherHitBox = collision.other.hitBoxes [collision.otherHitBox];
				if (lastBounce > 0)
					return false;
				if (otherHitBox == null) {
					Console.WriteLine ("Collission with non-character objects not supported.");
				} else {
					collisionDirection = SimulateCollision (collision);

					return BounceOrDie (collisionDirection, otherHitBox);
					/*this.x = (int)lastPoint.X;
					this.y = (int)lastPoint.Y;*/
				}
				return false;
			} else {
				this.Destroy ();
				return false;
			}
		}
		// collisionDirection: 0: X collision ; 1: Y collision
		private bool BounceOrDie (int collisionDirection, HitBox colliderHitBox) 
		{
			if (bounceBullet) {
				Console.WriteLine ("Collision direction:" + collisionDirection);
				if (collisionDirection == 0) {
					direction.X *= -1;
					virtPos.X = 0;
				} else if (collisionDirection == 1) {
					direction.Y *= -1;
					virtPos.Y = 0;
				} else {
					// if the collission simulation failed (ex. reason: multiple moving stuff?)
					//  then bounce back
					virtPos.X = 0;
					virtPos.Y = 0;
					direction.X *= -1;
					direction.Y *= -1;
				}
				speed = (int) (speed * bounceMod);
				if (speed <= MINSPEED)
					speed = MINSPEED;
				else
					rangeToGo = (int) (rangeToGo * bounceMod);
				lastBounce = bounceDelay;
				this.x = (int)lastPoint.X;
				this.y = (int)lastPoint.Y;
				return true;
			} else {
				this.Destroy ();
				return false;
			}
		}

		public void NextMove ()
		{
			if (rangeToGo <= 0) {
				this.Destroy ();
			}
			this.virtPos.X += (int)(speed * direction.X * (this.deltaTicks/100.0));
			this.virtPos.Y += (int)(speed * direction.Y * (this.deltaTicks/100.0));
			if (Math.Abs(this.virtPos.X) > 1) {
				this.x += (int)this.virtPos.X;
				this.virtPos.X -= (int)this.virtPos.X;
			}
			if (Math.Abs(this.virtPos.Y) > 1) {
				this.y += (int)this.virtPos.Y;
				this.virtPos.Y -= (int)this.virtPos.Y;
			}
			rangeToGo -= (int)(speed * (this.deltaTicks/100.0));
		}

		public override void Update ()
		{
			Game.NormalizeTicks (ref this.deltaTicks);
			// the opposite of the usual solution
			this.lifeSpan += this.deltaTicks;
			if (this.lifeSpan > maxLifeSpan)
				this.Destroy ();
			if (((Game)engine.objects ["game"]).mainWindow == "game") {
				if (lastBounce > 0)
					lastBounce -= this.deltaTicks;
				if (rangeToGo <= (fadeAwayRange * range)) {
					double deltaRadius = (this.radius - (this.radius * fadeAwayMod)) * (this.deltaTicks/100.0);
					if (deltaRadius > 0) {
						this.virtRadius += deltaRadius;
						if (virtRadius > 1.0) {
							this.x += (int)((int)virtRadius / 2);
							this.y += (int)((int)virtRadius / 2);
							this.radius -= (int)virtRadius;
							this.virtRadius -= (int)virtRadius;
							this.hitBoxes ["mass"].height = this.radius * 2;
							this.hitBoxes ["mass"].width = this.radius * 2;
						}
					}
				}
				// 0 left; 1 top; 2 right; 3 bottom; 4: top-left; 5: top-right; 6: bottom-left; 7: bottom-right
				NextMove();

				List<Collision> collisions = this.CheckCollisions ();
				if (collisions.Count > 0)
					Console.WriteLine ("Bullet collides with n." + collisions.Count);
				bool bounced = false;
				foreach (Collision collision in collisions) {
					if (collision.other.name == owner.name || collision.other.name.StartsWith ("bullet") || collision.other.name.StartsWith ("orb"))
						continue;
					Console.WriteLine ("Bullet hits enemy: " + collision.other.name);
					if (BounceOrDie (collision))
						bounced = true;
					if (collision.other.name.StartsWith ("enemy")) {
						Game game = (Game)this.engine.objects ["game"];

						Enemy enemy = collision.other as Enemy;
						game.Hits (this, enemy, collision);

						break;
					}
				}
				if (bounced)
					NextMove ();
				lastPoint = new Vector2 (this.x, this.y);
			}
		}
	}
}

