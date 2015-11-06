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
		// T (triangle) -> int etc.
		public Dictionary<string, int> ds4Config = new Dictionary<string, int> { {"T", 5}, {"C", 4}, {"S", 2}, {"X", 3}, {"L1", 6}, {"R1", 7}, {"L2", 8}, {"R2", 9}};
		public Dictionary<string, int> thrustmasterConfig = new Dictionary<string, int> { {"T", 6}, {"C", 5}, {"S", 4}, {"X", 3}, {"L1", 7}, {"R1", 9}, {"L2", 8}, {"R2", 10}};
		public Dictionary<string, int> joyStickConfig;
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

			joyStickConfig = thrustmasterConfig;

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
			int blockW = ((Background) engine.objects["background"]).blockW;
			int blockH = ((Background) engine.objects["background"]).blockH;
			if (this.y < blockH)
				this.y = blockH;
			if (this.x < blockW)
				this.x = blockW;

			if (this.x > this.engine.width - this.width - blockW)
				this.x = this.engine.width - this.width - blockW;
			if (this.y > this.engine.height - this.height - blockH)
				this.y = this.engine.height - this.height - blockH;
		}

		private void ManageShot ()
		{
			if (lastShot > 0)
				lastShot -= this.deltaTicks;
			
			if (lastShot <= 0) {
				// TODO: use vector instead of int/hardcoded direction
				// spawn a new bullet in a choosen direction
				// 0 left; 1 top; 2 right; 3 bottom; 4: top-left; 5: top-right; 6: bottom-left; 7: bottom-right
				int direction = -1;
				if (this.engine.IsKeyDown (Keys.A) || joystick.buttons[joyStickConfig["S"]])
					direction = 0;
				else if (this.engine.IsKeyDown (Keys.W) || joystick.buttons[joyStickConfig["T"]])
					direction = 1;
				else if (this.engine.IsKeyDown (Keys.D) || joystick.buttons[joyStickConfig["C"]])
					direction = 2;
				else if (this.engine.IsKeyDown (Keys.S) || joystick.buttons[joyStickConfig["X"]])
					direction = 3;
				else if (this.engine.IsKeyDown (Keys.Q) || joystick.buttons[joyStickConfig["L2"]])
					direction = 4;
				else if (this.engine.IsKeyDown (Keys.E) || joystick.buttons[joyStickConfig["R2"]])
					direction = 5;
				else if (this.engine.IsKeyDown (Keys.Z) || joystick.buttons[joyStickConfig["L1"]])
					direction = 6;
				else if (this.engine.IsKeyDown (Keys.C) || joystick.buttons[joyStickConfig["R1"]])
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
				//Console.WriteLine ("{0}.{1} {2}", joystick.x, joystick.y, 
				//	(joystick.buttons.Length > 0) ? joystick.anyButton().ToString () : "N");
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

