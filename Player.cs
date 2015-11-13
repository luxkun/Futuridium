using System;
using Aiv.Engine;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Threading;
using OpenTK;
using OpenTK.Input;

namespace StupidAivGame
{
	public class Player : Character
	{
		private const int maxHitsPerTime = 1000; // 500ms immunity after gets hit
		private int lastHit = 0;
		private int spawnedOrbs = 0;
		private int lastFloorChange = 0;
		private int changeFloorDelay = 2000;

		private bool initHUD = false;

		protected RectangleObject redWindow;

		private Vector2 lastPosition;

		//private List<int> pressedJoyButtons;
		public Player () : base ("player", "Player", "player")
		{
			this.order = 7;

			level0.maxHP = 200;
			level0.speed = 6;
			level0.shotDelay = 1500;
			level0.attack = 50;
			level0.neededXP = 100;
			level0.shotSpeed = 5;
			level0.shotRange = 400;
			level0.shotRadius = 8;
			isCloseCombat = false;

			//pressedJoyButtons = new List<int> ();

		}

		public override void Start () 
		{
			this.AddHitBox ("player", 0, 0, this.width, this.height);

			redWindow = new RectangleObject ();
			redWindow.width = 0;
			redWindow.height = 0;
			redWindow.name = "redWindow";
			redWindow.color = Color.Red;
			redWindow.x = 0;
			redWindow.y = 0;
			redWindow.fill = true;
			this.engine.SpawnObject ("redWindow", redWindow);

			base.Start ();
		}


		private void ManageControls ()
		{

			// keyboard controls

			// why need casting?
			lastPosition = new Vector2 (this.x, this.y);
			if (this.engine.IsKeyDown ((int) OpenTK.Input.Key.Right)) {
				this.x += level.speed;
			}
			if (this.engine.IsKeyDown ((int) OpenTK.Input.Key.Left)) {
				this.x -= level.speed;
			}
			if (this.engine.IsKeyDown ((int) OpenTK.Input.Key.Up)) {
				this.y -= level.speed;
			}
			if (this.engine.IsKeyDown ((int) OpenTK.Input.Key.Down)) {
				this.y += level.speed;
			}

			// joystick controls
			if (((Game) engine.objects["game"]).joystick != -1) {
				JoystickState gamePadState = Joystick.GetState (((Game) engine.objects["game"]).joystick);
				Vector2 moveDirection = new Vector2 (gamePadState.GetAxis(JoystickAxis.Axis0), gamePadState.GetAxis(JoystickAxis.Axis1));
				if (moveDirection.LengthFast > 0.1) {
					this.x += (int)(level.speed * moveDirection.X);
					this.y += (int)(level.speed * moveDirection.Y);
				}
			}

			// avoid the player to go out of the screen
			//int blockW = ((Game)engine.objects["game"]).currentFloor.currentRoom.gameBackground.blockW;//((Background) engine.objects["background"]).blockW;
			//int blockH = ((Game)engine.objects["game"]).currentFloor.currentRoom.gameBackground.blockH;//((Background) engine.objects["background"]).blockH;

			/*if (this.y < blockH)
				this.y = blockH;
			if (this.x < blockW)
				this.x = blockW;

			if (this.x > (((this.engine.width - 1) / blockW) * blockW))
				this.x = this.engine.width - this.width - blockW;
			if (this.y > (((this.engine.height - 1) / blockH) * blockH))
				this.y = this.engine.height - this.height - blockH;*/
		}

		private void ManageShot ()
		{
			if (lastShot > 0)
				lastShot -= this.deltaTicks;
			
			if (lastShot <= 0) {
				// TODO: use vector instead of int/hardcoded direction
				// spawn a new bullet in a choosen direction
				// 0 left; 1 top; 2 right; 3 bottom; 4: top-left; 5: top-right; 6: bottom-left; 7: bottom-right
				Vector2 direction = new Vector2 ();
				int joystick = ((Game)engine.objects ["game"]).joystick;
				var joyStickConfig = ((Game)engine.objects ["game"]).joyStickConfig;
				JoystickState joystickState = Joystick.GetState (joystick);
				if (joystick != -1) {
					direction = new Vector2 (joystickState.GetAxis(JoystickAxis.Axis2), joystickState.GetAxis(JoystickAxis.Axis3));
				}
				if (this.engine.IsKeyDown ((int)OpenTK.Input.Key.A) || (joystick != -1 && joystickState.GetButton(joyStickConfig["S"]) == OpenTK.Input.ButtonState.Pressed))
					direction = new Vector2 (-1, 0);
				else if (this.engine.IsKeyDown ((int)OpenTK.Input.Key.W) || (joystick != -1 && joystickState.GetButton(joyStickConfig["T"]) == OpenTK.Input.ButtonState.Pressed))
					direction = new Vector2 (0, -1);
				else if (this.engine.IsKeyDown ((int)OpenTK.Input.Key.D) || (joystick != -1 && joystickState.GetButton(joyStickConfig["C"]) == OpenTK.Input.ButtonState.Pressed))
					direction = new Vector2 (1, 0);
				else if (this.engine.IsKeyDown ((int)OpenTK.Input.Key.S) || (joystick != -1 && joystickState.GetButton(joyStickConfig["X"]) == OpenTK.Input.ButtonState.Pressed))
					direction = new Vector2 (0, 1);
				else if (this.engine.IsKeyDown ((int)OpenTK.Input.Key.Q))
					direction = new Vector2 (-0.5f, -0.5f);
				else if (this.engine.IsKeyDown ((int)OpenTK.Input.Key.E))
					direction = new Vector2 (0.5f, -0.5f);
				else if (this.engine.IsKeyDown ((int)OpenTK.Input.Key.Z))
					direction = new Vector2 (-0.5f, 0.5f);
				else if (this.engine.IsKeyDown ((int)OpenTK.Input.Key.C))
					direction = new Vector2 (0.5f, 0.5f);
				if (direction.LengthFast >= 0.5) {
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

		private void ManageCollisions () 
		{
			if (lastHit > 0)
				lastHit -= this.deltaTicks;
			if (lastFloorChange > 0)
				lastFloorChange -= this.deltaTicks;

			if (lastHit <= 0) {
				List<Collision> collisions = this.CheckCollisions ();
				if (collisions.Count > 0)
					Console.WriteLine ("Character '{0}' collides with n.{1}", name, collisions.Count);
				foreach (Collision collision in collisions) {
					Console.WriteLine ("Character '{0}' touches '{1}'", name, collision.other.name);
					Game game = (Game)this.engine.objects ["game"];
					if (collision.other.name.StartsWith ("enemy")) {
						Enemy enemy = collision.other as Enemy;
						game.Hits (enemy, this, collision, null);

						Console.WriteLine ("{0}, {1}", level.hp, isAlive);
						if (!isAlive) {
							this.Destroy ();
						}

						lastHit = maxHitsPerTime;

						break;
					} else if (collision.other.name.EndsWith ("block")) {
						this.x = (int)lastPosition.X;
						this.y = (int)lastPosition.Y;
					} else if (collision.other.name.EndsWith ("door") && lastFloorChange <= 0 && collision.other.enabled) {
						this.x = (int)lastPosition.X;
						this.y = (int)lastPosition.Y;
						Console.WriteLine ("About to change room to: " + collision.other.name);
						bool changedFloor = false;
						if (collision.other.name.EndsWith ("top_door"))
							changedFloor = game.currentFloor.OpenRoom (game.currentFloor.currentRoom.top);
						else if (collision.other.name.EndsWith ("left_door"))
							changedFloor = game.currentFloor.OpenRoom (game.currentFloor.currentRoom.left);
						else if (collision.other.name.EndsWith ("bottom_door"))
							changedFloor = game.currentFloor.OpenRoom (game.currentFloor.currentRoom.bottom);
						else if (collision.other.name.EndsWith ("right_door"))
							changedFloor = game.currentFloor.OpenRoom (game.currentFloor.currentRoom.right);
						if (changedFloor)
							lastFloorChange = changeFloorDelay;
					} else if (collision.other.name.StartsWith("escape_floor_")) {
						//game.initializeNewFloor();
						game.StartBossFight();
						collision.other.Destroy ();
					}
				}
			}
		}

		public override int GetDamage (Character enemy, Func<Character, Character, int> damageFunc)
		{
			redWindow.width = engine.width;
			redWindow.height = engine.height;
			Console.WriteLine ("Player got damaged.");
			Thread thr = new Thread(
				() =>
				{
					Thread.Sleep(50);

					redWindow.width = 0;
					redWindow.height = 0;
				}
			);
			thr.Start();

			return base.GetDamage(enemy, damageFunc);
		}

		public override void Update ()
		{
			base.Update ();
			if (((Game)engine.objects ["game"]).mainWindow == "game") {
				if (!initHUD) {
					initHUD = true;
					hud = ((Hud)engine.objects ["hud"]);
					hud.UpdateHPBar ();
					hud.UpdateXPBar ();
				}
				ManageControls ();
				ManageShot ();
				ManageCollisions ();
				//if (this.engine.IsKeyDown (Keys.O))
				SpawnOrb ();
			}
		}
	}
}

