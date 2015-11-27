using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Aiv.Engine;
using OpenTK.Input;

namespace StupidAivGame
{
    public class Game : GameObject
    {
        private static readonly int gameOverDelay = 1000;
        private static readonly int windowChangeDelay = 500;
        // T (triangle) -> int etc.
        public static Dictionary<string, int> thrustmasterConfig = new Dictionary<string, int>
        {
            {"T", 3},
            {"C", 2},
            {"S", 1},
            {"X", 0},
            {"SL", 8},
            {"ST", 9},
            {"Lx", 0},
            {"Ly", 1},
            {"Rx", 2},
            {"Ry", 3}
        };

        public static Dictionary<string, int> ds4Config = new Dictionary<string, int>
        {
            // tutti buggati tranne Lxy e Rxy
            {"T", 3},
            {"C", 2},
            {"S", 0},
            {"X", 1},
            {"SL", 8},
            {"ST", 9},
            {"Lx", 0},
            {"Ly", 1},
            {"Rx", 3},
            {"Ry", 4}
        };

        public static string[] joystickButtons = {"T", "C", "S", "X", "SL", "ST"};
        public Floor currentFloor;
        private int floorIndex = -1;
        public bool gameOver;
        private int gameOverTimer;
        //public List<Floor> floors;

        public TKJoystick joystick;
        public Dictionary<string, int> joyStickConfig;
        private string lastWindow;
        private int lastWindowChange;
        public string mainWindow; // game, map, ...
        public Player player;
        public RandomSeed random;
        public Dictionary<string, List<string>> spritesAnimations;
        internal bool usingOpenTK;

        public Game()
        {
            random = new RandomSeed(Utils.RandomString(5));
            spritesAnimations = new Dictionary<string, List<string>>();

            joyStickConfig = ds4Config;
        }

        public void StartLoading()
        {
            if (mainWindow != "loading")
            {
                lastWindow = mainWindow;
                mainWindow = "loading";
            }
        }

        public void StopLoading()
        {
            mainWindow = lastWindow;
        }

        public void InitializeNewFloor()
        {
            StartLoading();
            if (floorIndex >= 0)
            {
                OnDestroyHelper(currentFloor.currentRoom);
            }
            floorIndex++;
            currentFloor = new Floor(floorIndex);
            engine.SpawnObject(currentFloor.name, currentFloor);
            currentFloor.RandomizeFloor((int) (6*Math.Max(1, (floorIndex + 1)/5.0)),
                (int) (8*Math.Max(1, (floorIndex + 1)/4.0)));
            currentFloor.OpenRoom(currentFloor.firstRoom);
            StopLoading();
        }

        public override void Start()
        {
            GameBackground.Initialize(engine);

            engine.SpawnObject(new CharactersInfo());

            var logoObj = new SpriteObject();
            logoObj.currentSprite = (SpriteAsset) engine.GetAsset("logo");
            logoObj.x = engine.width/2 - logoObj.width/2;
            logoObj.y = engine.height/2 - logoObj.height/2;
            engine.SpawnObject("logo", logoObj);
            mainWindow = "logo";
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

                    Debug.WriteLine("Enemies to go in current room: " + currentFloor.currentRoom.enemies.Count);
                    foreach (var en in currentFloor.currentRoom.enemies)
                    {
                        Debug.Write("{0} - ", en.name);
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
            if (joystick == null || !joystick.IsConnected())
            {
                joystick = null;
                for (var i = 0; i < 8; i++)
                {
                    if (engine.joysticks[i] != null)
                    {
                        joystick = (TKJoystick) engine.joysticks[i];
                        break;
                    }
                }
            }
            /*if (joystick != null && false) {
				JoystickState otkjoy = Joystick.GetState (0);
				JoystickCapabilities otkcap = Joystick.GetCapabilities (0);
				Debug.WriteLine ("x{0} y{1} z{2} w{3} z+w{4}", joystick.x, joystick.y, joystick.z, joystick.w, new Vector2(joystick.z / 127f, joystick.w / 127f).Length);
				Debug.WriteLine ("x{0} y{1} z{2} w{3} z+w{4}", otkjoy.GetAxis(JoystickAxis.Axis0), otkjoy.GetAxis(JoystickAxis.Axis1), otkjoy.GetAxis(JoystickAxis.Axis2), otkjoy.GetAxis(JoystickAxis.Axis5),
					new Vector2(otkjoy.GetAxis(JoystickAxis.Axis2) / 127f, otkjoy.GetAxis(JoystickAxis.Axis3) / 127f).Length);

				var joystickState = Joystick.GetState(0);
				foreach (var key in joyStickConfig.Keys)
				{
					if (joystickState.GetButton ((JoystickButton) joyStickConfig [key]) == ButtonState.Pressed)
						Debug.WriteLine ((JoystickButton) joyStickConfig [key]);
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
            foreach (var button in joystickButtons)
            {
                if (joystick.GetButton(joyStickConfig[button]))
                    return true;
            }
            return false;
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
    }
}