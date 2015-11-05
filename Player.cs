using System;
using Aiv.Engine;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

namespace StupidAivGame
{
	public class Player : Character
	{
		private const int maxHitsPerTime = 1000; // 500ms immunity after gets hit
		private int lastHit = 0;
		private int spawnedOrbs = 0;

		private Engine.Joystick joystick;
		//private List<int> pressedJoyButtons;
		public Player () : base ("player", "Player", "player")
		{
			level0.maxHP = 100;
			level0.speed = 10;
			level0.shotDelay = 1500;
			level0.attack = 50;
			level0.neededXP = 30;
			level0.shotSpeed = 10;
			level0.shotRange = 500;
			level0.shotRadius = 5;
			isCloseCombat = false;

			//pressedJoyButtons = new List<int> ();
		}

		public override void Start () 
		{
			this.AddHitBox ("player", 0, 0, this.width, this.height);

		}


		private void ManageControls ()
		{

			// keyboard controls

			if (this.engine.IsKeyDown (Keys.Right)) {
				this.x += level.speed;
			}
			if (this.engine.IsKeyDown (Keys.Left)) {
				this.x -= level.speed;
			}
			if (this.engine.IsKeyDown (Keys.Up)) {
				this.y -= level.speed;
			}
			if (this.engine.IsKeyDown (Keys.Down)) {
				this.y += level.speed;
			}

			// joystick controls
			if (joystick != null) {
				double axisX = joystick.x / 127.0;
				double axisY = joystick.y / 127.0;
				this.x += (int) (level.speed * axisX);
				this.y += (int) (level.speed * axisY);
			}

			// avoid the player to go out of the screen
			if (this.y < 0)
				this.y = 0;
			if (this.x < 0)
				this.x = 0;

			if (this.x > this.engine.width - this.width)
				this.x = this.engine.width - this.width;
			if (this.y > this.engine.height - this.height)
				this.y = this.engine.height - this.height;
		}

		private void Shot (int direction)
		{
			Console.WriteLine ("Shotting to direction: " + direction);
			// 0 left; 1 top; 2 right; 3 bottom; 4: top-left; 5: top-right; 6: bottom-left; 7: bottom-right
			Bullet bullet = new Bullet (this, direction);
			if (direction == 7) {
				bullet.x = this.x + this.width;
				bullet.y = this.y + this.height;
			} else if (direction == 6) {
				bullet.x = this.x;
				bullet.y = this.y + this.height;
			} else if (direction == 5) {
				bullet.x = this.x + this.width;
				bullet.y = this.y;
			} else if (direction == 4) {
				bullet.x = this.x;
				bullet.y = this.y;
			} else if (direction == 3) {
				bullet.x = this.x + (this.width / 2);
				bullet.y = this.y + this.height;
			} else if (direction == 2) {
				bullet.x = this.x + this.width;
				bullet.y = this.y + (this.height / 2);
			} else if (direction == 1) {
				bullet.x = this.x + (this.width / 2);
				bullet.y = this.y;// - this.height;
			} else if (direction == 0) {
				bullet.x = this.x;// - this.width;
				bullet.y = this.y + (this.height / 2);
			}

			bullet.radius = level.shotRadius;
			bullet.color = Color.White;
			this.engine.SpawnObject ("bullet_" + bulletCounter, bullet);
			bulletCounter++;
		}

		private void ManageShot ()
		{
			if (lastShot > 0)
				lastShot -= this.deltaTicks;

			if (lastShot <= 0) {
				// spawn a new bullet in a choosen direction
				// 0 left; 1 top; 2 right; 3 bottom; 4: top-left; 5: top-right; 6: bottom-left; 7: bottom-right
				int direction = -1;
				if (this.engine.IsKeyDown (Keys.A) || joystick.buttons[2])
					direction = 0;
				else if (this.engine.IsKeyDown (Keys.W) || joystick.buttons[5])
					direction = 1;
				else if (this.engine.IsKeyDown (Keys.D) || joystick.buttons[4])
					direction = 2;
				else if (this.engine.IsKeyDown (Keys.S) || joystick.buttons[3])
					direction = 3;
				else if (this.engine.IsKeyDown (Keys.Q) || joystick.buttons[8])
					direction = 4;
				else if (this.engine.IsKeyDown (Keys.E) || joystick.buttons[9])
					direction = 5;
				else if (this.engine.IsKeyDown (Keys.Z) || joystick.buttons[6])
					direction = 6;
				else if (this.engine.IsKeyDown (Keys.C) || joystick.buttons[7])
					direction = 7;
				if (direction >= 0) {
					Shot (direction);
					lastShot = level.shotDelay;
				}
			}
		}

		private void SpawnOrb () 
		{
			if (spawnedOrbs == 0) {
				spawnedOrbs++;
				Console.WriteLine ("Spawning orb.");
				Orb orb = new Orb (this);
				orb.radius = 8;
				orb.color = Color.Blue;
				this.engine.SpawnObject ("orb", orb);
			}
		}

		private void ManageStatistics () 
		{
			string newTextScore;
			string newTextStatistics;
			TextObject scoreTextObject = (TextObject)this.engine.objects ["xp"];
			TextObject statisticsTextObject = (TextObject)this.engine.objects ["statistics"];

			newTextScore = string.Format ("HP: {0}/{1}", level.hp, level.maxHP);
			newTextStatistics = string.Format ("XP: {0}/{1} - Level: {2}", xp, level.neededXP, level.level);
			// useless?
			if (scoreTextObject.text != newTextScore)
				scoreTextObject.text = newTextScore;
			if (statisticsTextObject.text != newTextStatistics)
				statisticsTextObject.text = newTextStatistics;
		}

		private void ManageCollisions () 
		{
			if (lastHit > 0)
				lastHit -= this.deltaTicks;

			if (lastHit <= 0) {
				List<Collision> collisions = this.CheckCollisions ();
				if (collisions.Count > 0)
					Console.WriteLine ("Character '{0}' collides with n.{1}", name, collisions.Count);
				foreach (Collision collision in collisions) {
					Console.WriteLine ("Character '{0}' hits '{1}'", name, collision.other.name);
					if (collision.other.name.StartsWith ("enemy")) {
						Game game = (Game)this.engine.objects ["game"];

						Enemy enemy = collision.other as Enemy;
						game.Hits (enemy, this, collision);

						Console.WriteLine ("{0}, {1}", level.hp, isAlive);
						if (!isAlive) {
							this.Destroy ();
						}

						lastHit = maxHitsPerTime;

						break;
					}
				}
			}
		}

		private void ManageJoystick ()
		{
			joystick = null;
			foreach (Engine.Joystick joy in engine.joysticks) {
				if (joy != null) {
					joystick = joy;
					break;
				}
			}
			if (joystick != null) {
				for (int i=0; i < joystick.buttons.Length; i++) {
					if (joystick.buttons [i]) {
						Console.WriteLine ("Pressed ({0})", i);
						//if (!pressedJoyButtons.Contains(i))
						//	pressedJoyButtons.Add (i);
					}// else if (pressedJoyButtons.Contains(i)) {
						//pressedJoyButtons.Remove(i);
					//}
				}
				Console.WriteLine ("{0}.{1} {2}", joystick.x, joystick.y, 
					(joystick.buttons.Length > 0) ? joystick.anyButton().ToString () : "N");
			}
		}

		public override void Update ()
		{
			base.Update ();
			ManageJoystick ();
			ManageControls ();
			ManageShot ();
			ManageCollisions ();
			ManageStatistics ();
			//if (this.engine.IsKeyDown (Keys.O))
			SpawnOrb ();
		}
	}
}

