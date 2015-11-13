using System;
using Aiv.Engine;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

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

		public Engine.Joystick joystick;
		// T (triangle) -> int etc.
		public Dictionary<string, int> ds4Config = new Dictionary<string, int> { {"T", 5}, {"C", 4}, {"S", 2}, {"X", 3}, {"L1", 6}, {"R1", 7}, {"L2", 8}, {"R2", 9}, {"SL", 10}, {"ST", 11}};
		public Dictionary<string, int> thrustmasterConfig = new Dictionary<string, int> { {"T", 6}, {"C", 5}, {"S", 4}, {"X", 3}, {"L1", 7}, {"R1", 9}, {"L2", 8}, {"R2", 10}, {"SL", -1}, {"ST", -1}}; //TODO: SL
		public Dictionary<string, int> joyStickConfig;

		private int lastWindowChange = 0;
		private int windowChangeDelay = 500;
			
		public Game (Engine engine)
		{
			this.engine = engine;
			spritesAnimations = new Dictionary<string, List<string>> ();

			joyStickConfig = ds4Config;
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
				if (mainWindow == "game") {
					// select, open map
					if (engine.IsKeyDown ((int)OpenTK.Input.Key.M) || (joystick != null && joystick.buttons [joyStickConfig ["SL"]]))
						OpenMap ();
					else if (this.engine.IsKeyDown ((int)OpenTK.Input.Key.Escape) || (joystick != null && joystick.buttons [joyStickConfig ["ST"]]))
						Pause ();
				} else if (mainWindow == "map") {
					if (this.engine.IsKeyDown ((int)OpenTK.Input.Key.M) || this.engine.IsKeyDown ((int)OpenTK.Input.Key.Escape) || (joystick != null && joystick.buttons [joyStickConfig ["SL"]]))
						CloseMap ();
				} else if (mainWindow == "pause") {
					if (this.engine.IsKeyDown ((int)OpenTK.Input.Key.P) || this.engine.IsKeyDown ((int)OpenTK.Input.Key.Escape) || (joystick != null && joystick.buttons [joyStickConfig ["ST"]]))
						UnPause ();
				} else if (mainWindow == "logo") {
					if (AnyKeyDown() || (joystick != null && joystick.anyButton()))
						StartGame ();
				} else if (mainWindow == "gameover") {
					if (AnyKeyDown() || (joystick != null && joystick.anyButton()))
						this.engine.isGameRunning = false;
				}
				if (startingWindow != mainWindow)
					lastWindowChange = windowChangeDelay;
			}
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

