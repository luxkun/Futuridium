using System;
using Aiv.Engine;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using OpenTK.Input;

namespace StupidAivGame
{
	public class Game : GameObject
	{
		public bool gameOver = false;
		private int floorIndex = -1;

		public string mainWindow; // game, map, ...

		public Player player;

		public Floor currentFloor;
		//public List<Floor> floors;

		public Dictionary<string, List<string>> spritesAnimations;

		public int joystick;
		// T (triangle) -> int etc.
		// TODO: do.
		//public Dictionary<string, int> ds4Config = new Dictionary<string, int> { {"T", 5}, {"C", 4}, {"S", 2}, {"X", 3}, {"L1", 6}, {"R1", 7}, {"L2", 8}, {"R2", 9}, {"SL", 10}, {"ST", 11}};
		public Dictionary<string, JoystickButton> thrustmasterConfig = new Dictionary<string, JoystickButton> { 
			{"T", JoystickButton.Button3}, {"C", JoystickButton.Button2}, {"S", JoystickButton.Button1}, {"X", JoystickButton.Button0}, {"SL", JoystickButton.Button8}, {"ST", JoystickButton.Button9}
		};
		public Dictionary<string, JoystickButton> joyStickConfig;

		private int lastWindowChange = 0;
		private int windowChangeDelay = 500;
		private int gameOverTimer = 0;
		private int gameOverDelay = 1000;
			
		public Game (Engine engine)
		{
			this.engine = engine;
			spritesAnimations = new Dictionary<string, List<string>> ();

			joyStickConfig = thrustmasterConfig;
		}

		public void initializeNewFloor ()
		{
			if (floorIndex > 0) {
				Game.OnDestroyHelper (currentFloor.currentRoom);
			}
			floorIndex++;
			currentFloor = new Floor (floorIndex);
			engine.SpawnObject (currentFloor.name, currentFloor);
			currentFloor.RandomizeFloor((int) (6 * ((floorIndex + 1) / 2.0)), (int) (12 * ((floorIndex + 1) / 2.0)));;
			currentFloor.OpenRoom (currentFloor.firstRoom);
		}

		public override void Start ()
		{
			SpriteObject logoObj = new SpriteObject ();
			logoObj.currentSprite = (SpriteAsset) engine.GetAsset ("logo");
			logoObj.x = engine.width / 2 - logoObj.width / 2;
			logoObj.y = engine.height / 2 - logoObj.height / 2;
			engine.SpawnObject ("logo", logoObj);
			mainWindow = "logo";
		}

		private void StartGame ()
		{
			Game.OnDestroyHelper ((SpriteObject)engine.objects ["logo"]);
			mainWindow = "game"; 

			player = new Player ();
			player.x = 40;
			player.y = 40;
			player.currentSprite = (SpriteAsset) engine.GetAsset ("player");
			engine.SpawnObject ("player", player);

			Hud hud = new Hud ();
			engine.SpawnObject ("hud", hud);

			initializeNewFloor ();
		}

		// override Start vs constructor?

		// bullet hits enemy
		public bool Hits (Bullet bullet, Character enemy, Collision collision)
		{ 
			return this.Hits ((Character)bullet.owner, enemy, collision, (Character ch0, Character ch1) => {
				return (int)(ch0.level.attack * ((double)bullet.speed / bullet.startingSpeed));
			});
		}

		// character hits enemy
		public bool Hits (Character character, Character enemy, Collision collision, Func<Character, Character, int> damageFunc)
		{ 
			if (damageFunc == null)
				damageFunc = (Character ch0, Character ch1) => {
				return ch1.level.attack;
				};

			enemy.GetDamage (character, damageFunc);

			if (!enemy.isAlive) {
				collision.other.Destroy ();

				if (player != null) {
					player.xp = player.xp + enemy.level.xpReward;
				}

				Enemy enemyObj = enemy as Enemy;
				if (enemyObj != null) {
					currentFloor.currentRoom.removeEnemy (enemyObj);

					Console.WriteLine ("Enemies to go in current room: " + currentFloor.currentRoom.enemies.Count);
					foreach (Enemy en in currentFloor.currentRoom.enemies) {
						Console.Write("{0} - ", en.name);
					}

					/*
					if (currentFloor.currentRoom.enemies.Count == 0) {
						if ((currentFloor.currentRoomIndex + 1) < currentFloor.rooms.Count) { 
							currentFloor.OpenRoom (currentFloor.currentRoomIndex + 1);
						} else {
							initializeNewFloor ();
						}
					}*/
				}
			}
			return enemy.isAlive;
		}

		private void ManageFloor ()
		{
			foreach (Room room in currentFloor.roomsList) {
				if (room.enemies.Count > 0)
					return;
			}
			string escapeFloorName = string.Format ("escape_floor_{0}", currentFloor.floorIndex);
			SpriteObject escapeFloorObj = new SpriteObject ();
			escapeFloorObj.order = 5;
			escapeFloorObj.x = engine.width / 2;
			escapeFloorObj.y = engine.height / 2;
			escapeFloorObj.currentSprite = (SpriteAsset)engine.GetAsset ("escape_floor");
			escapeFloorObj.AddHitBox (escapeFloorName, 0, 0, 32, 32);
			this.engine.SpawnObject (escapeFloorName, escapeFloorObj);
		}

		private void ManageJoystick ()
		{
			joystick = -1;
			for (int i = 0; i < 8; i++) {
				if (OpenTK.Input.Joystick.GetCapabilities (i).IsConnected) {
					joystick = i;
					break;
				}
			}
			/*
			if (joystick != -1) {
				JoystickState joystickState = Joystick.GetState (joystick);
				Console.WriteLine ("Axis: {0} {1} {2} {3}", joystickState.GetAxis (JoystickAxis.Axis0), joystickState.GetAxis (JoystickAxis.Axis1), joystickState.GetAxis (JoystickAxis.Axis2), joystickState.GetAxis (JoystickAxis.Axis3));
				Console.Write ("Buttons: ");
				foreach (string key in joyStickConfig.Keys) {
					if (joystickState.GetButton(joyStickConfig[key]) == OpenTK.Input.ButtonState.Pressed)
						Console.Write ("{0}", key);
				}
				Console.WriteLine ();
				/*foreach (OpenTK.Input.JoystickButton button in Enum.GetValues(typeof(OpenTK.Input.JoystickButton))) {
					if ((OpenTK.Input.ButtonState)gamePadState.Buttons.GetType ().GetProperty (button.ToString()).GetValue(gamePadState) == OpenTK.Input.ButtonState.Pressed)
						Console.WriteLine ("Pressed joystick button: " + button);
				}*/
			//}
		}

		private void OpenMap ()
		{
			this.mainWindow = "map";
			Map map = new Map ();
			this.engine.SpawnObject ("map", map);
		}

		private void CloseMap ()
		{
			this.mainWindow = "game";
			Game.OnDestroyHelper (this.engine.objects ["map"]);
		}

		private void Pause ()
		{
			this.mainWindow = "pause";
			Pause pause = new Pause ();
			this.engine.SpawnObject ("pause", pause);
		}

		private void UnPause ()
		{
			this.mainWindow = "game";
			Game.OnDestroyHelper (this.engine.objects ["pause"]);
		}

		public static void OnDestroyHelper (GameObject objBeingDestroyed)
		{
			List<GameObject> toDestroy = new List<GameObject> ();
			foreach (GameObject obj in objBeingDestroyed.engine.objects.Values) {
				if (obj.name.StartsWith (objBeingDestroyed.name))
					toDestroy.Add (obj);
			}
			foreach (GameObject obj in toDestroy)
				obj.Destroy ();
		}

		private void ManageControls ()
		{
			if (lastWindowChange > 0)
				lastWindowChange -= this.deltaTicks;
			if (lastWindowChange <= 0) {
				string startingWindow = mainWindow;
				JoystickState joystickState;
				joystickState = Joystick.GetState (joystick);

				if (mainWindow == "game") {
					if (engine.IsKeyDown ((int)OpenTK.Input.Key.M) || (joystick != -1 && joystickState.GetButton(joyStickConfig["SL"]) == OpenTK.Input.ButtonState.Pressed))
						OpenMap ();
					else if (this.engine.IsKeyDown ((int)OpenTK.Input.Key.Escape) || (joystick != -1 && joystickState.GetButton(joyStickConfig["ST"]) == OpenTK.Input.ButtonState.Pressed))
						Pause ();
				} else if (mainWindow == "map") {
					if (this.engine.IsKeyDown ((int)OpenTK.Input.Key.M) || this.engine.IsKeyDown ((int)OpenTK.Input.Key.Escape) ||
						(joystick != -1 && joystickState.GetButton(joyStickConfig["SL"]) == OpenTK.Input.ButtonState.Pressed))
						CloseMap ();
				} else if (mainWindow == "pause") {
					if (this.engine.IsKeyDown ((int)OpenTK.Input.Key.P) || this.engine.IsKeyDown ((int)OpenTK.Input.Key.Escape) ||
						(joystick != -1 && joystickState.GetButton(joyStickConfig["ST"]) == OpenTK.Input.ButtonState.Pressed))
						UnPause ();
				} else if (mainWindow == "logo") {
					if (AnyKeyDown() || (joystick != -1 && AnyJoystickButtonPressed()))
						StartGame ();
				} else if (mainWindow == "gameover") {
					if (gameOverTimer > 0)
						gameOverTimer -= this.deltaTicks;
					if (gameOverTimer <= 0 && (AnyKeyDown() || (joystick != -1 && AnyJoystickButtonPressed())))
						this.engine.isGameRunning = false;
				}
				if (startingWindow != mainWindow)
					lastWindowChange = windowChangeDelay;
			}
		}

		public bool AnyJoystickButtonPressed ()
		{
			if (joystick == -1)
				return false;
			JoystickState joystickState = Joystick.GetState(joystick);
			foreach (string key in joyStickConfig.Keys) {
				if (joystickState.GetButton (joyStickConfig [key]) == OpenTK.Input.ButtonState.Pressed)
					return true;
			}
			return false;
		}

		// TODO: better way to do this through the engine
		public bool AnyKeyDown ()
		{
			foreach (OpenTK.Input.Key key in Enum.GetValues(typeof(OpenTK.Input.Key))) {
				if (engine.IsKeyDown ((int) key))
					return true;
			}
			return false;
		}

		public void GameOver ()
		{
			mainWindow = "gameover";
			RectangleObject background = new RectangleObject ();
			background.color = Color.Black;
			background.fill = true;
			background.width = engine.width;
			background.height = engine.height;
			background.order = 10;
			engine.SpawnObject ("gameover_background", background);
			TextObject gameOver = new TextObject ("Phosphate", 80, "red");
			gameOver.text = "GAMEOVER";
			Size gameOverSize = TextRenderer.MeasureText (gameOver.text, gameOver.font);
			gameOver.x = engine.width / 2 - gameOverSize.Width / 2;
			gameOver.y = engine.height / 2 - gameOverSize.Height / 2;
			gameOver.order = 11;
			engine.SpawnObject ("gameover_text", gameOver);
			gameOverTimer = gameOverDelay;
		}

		public override void Update ()
		{
			ManageJoystick ();
			ManageControls ();
			if (mainWindow == "game") {
				if (player.level != null && !player.isAlive)
					gameOver = true;
				// check for gameOver
				if (this.gameOver) {
					GameOver ();
				}

				ManageFloor ();
			}
		}
	}
}

