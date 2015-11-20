using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Aiv.Engine;
using OpenTK.Input;
using ButtonState = OpenTK.Input.ButtonState;
using OpenTK;

namespace StupidAivGame
{
    public class Game : GameObject
    {
        private readonly int gameOverDelay = 1000;
        private readonly int windowChangeDelay = 500;
        public Floor currentFloor;
        private int floorIndex = -1;
        public bool gameOver;
        private int gameOverTimer;
        public Dictionary<string, int> joyStickConfig;
        private int lastWindowChange;
        public string mainWindow; // game, map, ...
        public Player player;
        public RandomSeed random;
        //public List<Floor> floors;

		public TKJoystick joystick;
        public Dictionary<string, List<string>> spritesAnimations;
        // T (triangle) -> int etc.
        // TODO: do.
        //public Dictionary<string, int> ds4Config = new Dictionary<string, int> { {"T", 5}, {"C", 4}, {"S", 2}, {"X", 3}, {"L1", 6}, {"R1", 7}, {"L2", 8}, {"R2", 9}, {"SL", 10}, {"ST", 11}};
		public Dictionary<string, int> thrustmasterConfig = new Dictionary<string, int>
		{
			{"T", 3}, {"C", 2}, {"S", 1}, {"X", 0}, {"SL", 8}, {"ST", 9}
		};public Dictionary<string, int> ds4Config = new Dictionary<string, int>
		{
			{"T", 3}, {"C", 2}, {"S", 0}, {"X", 1}, {"SL", 8}, {"ST", 9}
		};

        public Game()
        {
            random = new RandomSeed("SEED0");
            spritesAnimations = new Dictionary<string, List<string>>();

			joyStickConfig = ds4Config;
        }

        public void InitializeNewFloor()
        {
            if (floorIndex > 0)
            {
                OnDestroyHelper(currentFloor.currentRoom);
            }
            floorIndex++;
            currentFloor = new Floor(floorIndex);
            engine.SpawnObject(currentFloor.name, currentFloor);
            currentFloor.RandomizeFloor((int) (6*Math.Max(1, (floorIndex + 1)/5.0)),
                (int) (8*Math.Max(1, (floorIndex + 1)/4.0)));
            currentFloor.OpenRoom(currentFloor.firstRoom);
        }

        public override void Start()
        {
            var logoObj = new SpriteObject();
            logoObj.currentSprite = (SpriteAsset) engine.GetAsset("logo");
            logoObj.x = engine.width/2 - logoObj.width/2;
            logoObj.y = engine.height/2 - logoObj.height/2;
            engine.SpawnObject("logo", logoObj);
            mainWindow = "logo";

            //test
            this.engine.PlaySound ("levelup_sound");
        }

        private void StartGame()
        {
            OnDestroyHelper((SpriteObject) engine.objects["logo"]);
            mainWindow = "game";

            player = new Player();
            player.x = 40;
            player.y = 40;
            player.currentSprite = (SpriteAsset) engine.GetAsset("player");
            engine.SpawnObject("player", player);

            var hud = new Hud();
            engine.SpawnObject("hud", hud);

            InitializeNewFloor();
        }

        // bullet hits enemy
        public bool Hits(Bullet bullet, Character enemy, Collision collision)
        {
            return Hits((Character) bullet.owner, enemy, collision,
                (Character ch0, Character ch1) =>
                {
                    return (int) (ch0.level.attack*((double) bullet.speed/bullet.startingSpeed));
                });
        }

        // character hits enemy
        public bool Hits(Character character, Character enemy, Collision collision,
            Func<Character, Character, int> damageFunc)
        {
            if (damageFunc == null)
                damageFunc = (Character ch0, Character ch1) => { return ch1.level.attack; };

            enemy.GetDamage(character, damageFunc);

            if (!enemy.isAlive)
            {
                collision.other.Destroy();

                if (player != null)
                {
                    player.xp = player.xp + enemy.level.xpReward;
                }

                var enemyObj = enemy as Enemy;
                if (enemyObj != null)
                {
                    currentFloor.currentRoom.RemoveEnemy(enemyObj);

                    Console.WriteLine("Enemies to go in current room: " + currentFloor.currentRoom.enemies.Count);
                    foreach (var en in currentFloor.currentRoom.enemies)
                    {
                        Console.Write("{0} - ", en.name);
                    }
                }
            }
            return enemy.isAlive;
        }

        private void ManageFloor()
        {
            foreach (var room in currentFloor.roomsList)
            {
                if (room.enemies.Count > 0)
                    return;
            }
            var escapeFloorName = string.Format("escape_floor_{0}", currentFloor.floorIndex);
            var escapeFloorObj = new SpriteObject();
            escapeFloorObj.order = 5;
            escapeFloorObj.x = engine.width/2;
            escapeFloorObj.y = engine.height/2;
            escapeFloorObj.currentSprite = (SpriteAsset) engine.GetAsset("escape_floor");
            escapeFloorObj.AddHitBox(escapeFloorName, 0, 0, 32, 32);
            engine.SpawnObject(escapeFloorName, escapeFloorObj);
        }

		private void ManageJoystick()
		{
			if (joystick == null || !joystick.IsConnected()) {
				joystick = null;
				for (var i = 0; i < 8; i++) {
					if (engine.joysticks [i] != null) {
						joystick = (TKJoystick)engine.joysticks [i];
						break;
					}
				}
			}
			/*if (joystick != null) {
				JoystickState otkjoy = Joystick.GetState (0);
				Console.WriteLine ("x{0} y{1} z{2} w{3} z+w{4}", joystick.x, joystick.y, joystick.z, joystick.w, new Vector2(joystick.z / 127f, joystick.w / 127f).Length);

				var joystickState = Joystick.GetState(0);
				foreach (var key in joyStickConfig.Keys)
				{
					if (joystickState.GetButton ((JoystickButton) joyStickConfig [key]) == ButtonState.Pressed)
						Console.WriteLine ((JoystickButton) joyStickConfig [key]);
				}
			}*/

		}

        private void OpenMap()
        {	
            mainWindow = "map";
            var map = new Map();
            engine.SpawnObject("map", map);
        }

        private void CloseMap()
        {
            mainWindow = "game";
            OnDestroyHelper(engine.objects["map"]);
        }

        private void Pause()
        {
            mainWindow = "pause";
            var pause = new Pause();
            engine.SpawnObject("pause", pause);
        }

        private void UnPause()
        {
            mainWindow = "game";
            OnDestroyHelper(engine.objects["pause"]);
        }

        public static void OnDestroyHelper(GameObject objBeingDestroyed)
        {
            var toDestroy = new List<GameObject>();
            foreach (var obj in objBeingDestroyed.engine.objects.Values)
            {
                if (obj.name.StartsWith(objBeingDestroyed.name))
                    toDestroy.Add(obj);
            }
            foreach (var obj in toDestroy)
                obj.Destroy();
        }

        private void ManageControls()
        {
            if (lastWindowChange > 0)
                lastWindowChange -= deltaTicks;
            if (lastWindowChange <= 0)
            {
                var startingWindow = mainWindow;

                if (mainWindow == "game")
                {
                    if (engine.IsKeyDown((int) Key.M) ||
						(joystick != null && joystick.GetButton(joyStickConfig["SL"])))
                        OpenMap();
                    else if (engine.IsKeyDown((int) Key.Escape) ||
						(joystick != null && joystick.GetButton(joyStickConfig["ST"])))
                        Pause();
                }
                else if (mainWindow == "map")
                {
                    if (engine.IsKeyDown((int) Key.M) || engine.IsKeyDown((int) Key.Escape) ||
						(joystick != null && joystick.GetButton(joyStickConfig["SL"])))
                        CloseMap();
                }
                else if (mainWindow == "pause")
                {
                    if (engine.IsKeyDown((int) Key.P) || engine.IsKeyDown((int) Key.Escape) ||
						(joystick != null && joystick.GetButton(joyStickConfig["ST"])))
                        UnPause();
                }
                else if (mainWindow == "logo")
                {
                    if (AnyKeyDown() || (joystick != null && AnyJoystickButtonPressed()))
                        StartGame();
                }
                else if (mainWindow == "gameover")
                {
                    if (gameOverTimer > 0)
                        gameOverTimer -= deltaTicks;
                    if (gameOverTimer <= 0 && (AnyKeyDown() || (joystick != null && AnyJoystickButtonPressed())))
                        engine.isGameRunning = false;
                }
                if (startingWindow != mainWindow)
                    lastWindowChange = windowChangeDelay;
            }
        }

        public bool AnyJoystickButtonPressed()
        {
			if (joystick == null)
				return false;
			return joystick.anyButton ();
        }

        // TODO: better way to do this through the engine
        public bool AnyKeyDown()
        {
			foreach (Key key in Enum.GetValues(typeof (Key)))
			{
				if (engine.IsKeyDown((int) key))
					return true;
			}
			return false;
		}

        public void GameOver()
        {
            mainWindow = "gameover";
            var background = new RectangleObject();
            background.color = Color.Black;
            background.fill = true;
            background.width = engine.width;
            background.height = engine.height;
            background.order = 10;
            engine.SpawnObject("gameover_background", background);
            var gameOver = new TextObject("Phosphate", 80, "red");
            gameOver.text = "GAMEOVER";
            var gameOverSize = TextRenderer.MeasureText(gameOver.text, gameOver.font);
            gameOver.x = engine.width/2 - gameOverSize.Width/2;
            gameOver.y = engine.height/2 - gameOverSize.Height/2;
            gameOver.order = 11;
            engine.SpawnObject("gameover_text", gameOver);
            gameOverTimer = gameOverDelay;
        }

        public override void Update()
        {
            ManageJoystick();
            ManageControls();
            if (mainWindow == "game")
            {
                if (player.level != null && !player.isAlive)
                    gameOver = true;
                // check for gameOver
                if (gameOver)
                {
                    GameOver();
                }

                ManageFloor();
            }
        }

        public static void NormalizeTicks(ref int outerDeltaTicks)
        {
            if (outerDeltaTicks > 500) // super lag/debug or bug? bug!
                outerDeltaTicks = 0;
        }
    }
}