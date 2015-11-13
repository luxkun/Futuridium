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

		public bool bounceBullet = true;

		private const float fadeAwayRange = 0.2f;
		private const float fadeAwayMod = 0.8f;
		private const int minRadius = 1;
		private double virtRadius;
		private Vector2 virtPos;

		private int lastBounce = 0;
		// the same collider can collide once every bounceDelay 
		private int bounceDelay = 750;
		private HitBox lastColliderHitBox;
		private double bounceMod = 0.8; // speed = bounceMod * speed

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
			int diffX = this.x - (int)lastPoint.X;
			int diffY = this.y - (int)lastPoint.Y;
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
		private bool BounceOrDie (Collision collision)
		{
			if (bounceBullet) {
				int collisionDirection = -1;
				HitBox hitBox = this.hitBoxes [collision.hitBox];
				HitBox otherHitBox = collision.other.hitBoxes [collision.otherHitBox];
				if (otherHitBox == null) {
					Console.WriteLine ("Collission with non-character objects not supported.");
				} else {
					collisionDirection = SimulateCollision (collision);

					Console.WriteLine ("Collision direction:" + collisionDirection);
					if (collisionDirection != -1) {
						BounceOrDie (collisionDirection, otherHitBox);
						/*this.x = (int)lastPoint.X;
						this.y = (int)lastPoint.Y;*/
						return true;
					}
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
				if (lastBounce > 0 && colliderHitBox != null && lastColliderHitBox == colliderHitBox)
					return false;
				if (collisionDirection == 0) {
					direction.X *= -1;
					virtPos.X = 0;
				}
				if (collisionDirection == 1) {
					virtPos.Y = 0;
					direction.Y *= -1;
				}
				speed = (int) (speed * bounceMod);
				if (speed <= MINSPEED)
					speed = MINSPEED;
				else
					rangeToGo = (int) (rangeToGo * bounceMod);
				if (colliderHitBox != null) {
					lastBounce = bounceDelay;
					lastColliderHitBox = colliderHitBox;
				}
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
			if (this.deltaTicks > 100) // super lag or bug? bug!
				this.deltaTicks = 0;
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
						}
					}
				}
				// 0 left; 1 top; 2 right; 3 bottom; 4: top-left; 5: top-right; 6: bottom-left; 7: bottom-right
				NextMove();

				int blockW = ((Game)engine.objects["game"]).currentFloor.currentRoom.gameBackground.blockW;
				int blockH = ((Game)engine.objects["game"]).currentFloor.currentRoom.gameBackground.blockH;
				if (this.x > (this.engine.width - blockW - this.radius * 2)) {
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
				} else {
					List<Collision> collisions = this.CheckCollisions ();
					if (collisions.Count > 0)
						Console.WriteLine ("Bullet collides with n." + collisions.Count);
					foreach (Collision collision in collisions) {
						if (collision.other.name == owner.name || collision.other.name.StartsWith ("bullet") || collision.other.name.StartsWith ("orb"))
							continue;
						Console.WriteLine ("Bullet hits enemy: " + collision.other.name);
						if (BounceOrDie (collision))
							NextMove ();
						if (collision.other.name.StartsWith ("enemy")) {
							Game game = (Game)this.engine.objects ["game"];

							Enemy enemy = collision.other as Enemy;
							game.Hits (this, enemy, collision);

							break;
						}
					}
				}
				lastPoint = new Vector2 (this.x, this.y);
			}
		}
	}
}

