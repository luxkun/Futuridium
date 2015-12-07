using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Aiv.Engine;
using OpenTK.Input;

namespace Futuridium
{
    public class Game : GameObject
    {
        private static readonly float GameOverDelay = 1f;
        private static readonly float WindowChangeDelay = 0.5f;
        // T (triangle) -> int etc.
        private static readonly Dictionary<string, int> ThrustmasterConfig = new Dictionary<string, int>
        {
            {"T", 3},
            {"C", 2},
            {"S", 1},
            {"X", 0},
            {"RB", -1},
            {"LB", -1},
            {"RT", -1},
            {"LT", -1},
            {"SL", 8},
            {"ST", 9},
            {"Lx", 0},
            {"Ly", 1},
            {"Rx", 2},
            {"Ry", 3}
        };

        private static readonly Dictionary<string, int> Ds4Config = new Dictionary<string, int>
        {
            // tutti buggati tranne Lxy e Rxy
            {"T", 3},
            {"C", 2},
            {"S", 0},
            {"X", 1},
            {"RB", -1},
            {"LB", -1},
            {"RT", -1},
            {"LT", -1},
            {"SL", 8},
            {"ST", 9},
            {"Lx", 0},
            {"Ly", 1},
            {"Rx", 3},
            {"Ry", 4}
        };

        private static readonly string[] joystickButtons = {"T", "C", "S", "X", "SL", "ST"};
        private int floorIndex = -1;
        private float gameOverTimer;
        //public List<Floor> floors;

        private string lastWindow;
        private float lastWindowChange;

        public Floor CurrentFloor { get; private set; }

        public bool GameOver { get; private set; }

        public TKJoystick Joystick { get; private set; }

        public Dictionary<string, int> JoyStickConfig { get; private set; }

        public string MainWindow { get; private set; }

        public Player Player { get; private set; }

        public RandomSeed Random { get; private set; }

        public Dictionary<string, List<string>> SpritesAnimations { get; set; }

        public bool UsingOpenTK { get; set; }

        public Game()
        {
            Random = new RandomSeed(Utils.RandomString(5));
            SpritesAnimations = new Dictionary<string, List<string>>();

            JoyStickConfig = Ds4Config;
        }

        public void StartLoading()
        {
            if (MainWindow != "loading")
            {
                lastWindow = MainWindow;
                MainWindow = "loading";
            }
        }

        public void StopLoading()
        {
            MainWindow = lastWindow;
        }

        public void InitializeNewFloor()
        {
            StartLoading();
            if (floorIndex >= 0)
            {
                OnDestroyHelper(CurrentFloor.CurrentRoom);
            }
            floorIndex++;
            CurrentFloor = new Floor(floorIndex);
            engine.SpawnObject(CurrentFloor.name, CurrentFloor);
            CurrentFloor.RandomizeFloor((int) (6*Math.Max(1, (floorIndex + 1)/5.0)),
                (int) (8*Math.Max(1, (floorIndex + 1)/4.0)));
            CurrentFloor.OpenRoom(CurrentFloor.FirstRoom);
            StopLoading();
        }

        public override void Start()
        {
            GameBackground.Initialize(engine);

            engine.SpawnObject(new CharactersInfo());

            var logoObj = new SpriteObject {currentSprite = (SpriteAsset) engine.GetAsset("logo")};
            logoObj.x = engine.width/2 - logoObj.width/2;
            logoObj.y = engine.height/2 - logoObj.height/2;
            engine.SpawnObject("logo", logoObj);
            MainWindow = "logo";
        }

        private void StartGame()
        {
            OnDestroyHelper((SpriteObject) engine.objects["logo"]);
            MainWindow = "game";

            Player = new Player
            {
                x = engine.width/2,
                y = engine.height/2,
                currentSprite = (SpriteAsset) engine.GetAsset("player")
            };
            engine.SpawnObject("player", Player);

            var hud = new Hud();
            engine.SpawnObject("hud", hud);

            InitializeNewFloor();
        }

        public void CharacterDied(Character character)
        {
            var enemyObj = character as Enemy;
            if (enemyObj != null)
            {
                CurrentFloor.CurrentRoom.RemoveEnemy(enemyObj);

                Debug.WriteLine("Enemies to go in current room: " + CurrentFloor.CurrentRoom.Enemies.Count);
                foreach (var en in CurrentFloor.CurrentRoom.Enemies)
                {
                    Debug.Write("{0} - ", en.name);
                }

                if (CurrentFloor.CurrentRoom.Enemies.Count == 0)
                {
                    CurrentFloor.CurrentRoom.GameBackground.OpenDoors();
                }
            }
        }

        private void ManageFloor()
        {
            if (CurrentFloor.RoomsList.Any(room => room.Enemies.Count > 0))
            {
                return;
            }
            var escapeFloorName = $"escape_floor_{CurrentFloor.FloorIndex}";
            var escapeFloorObj = new SpriteObject
            {
                order = 5,
                x = engine.width/2,
                y = engine.height/2,
                currentSprite = (SpriteAsset) engine.GetAsset("escape_floor")
            };
            escapeFloorObj.AddHitBox(escapeFloorName, 0, 0, 32, 32);
            engine.SpawnObject(escapeFloorName, escapeFloorObj);
        }

        private void ManageJoystick()
        {
            if (Joystick != null && Joystick.IsConnected()) return;
            Joystick = null;
            for (var i = 0; i < 8; i++)
            {
                if (engine.joysticks[i] != null)
                {
                    Joystick = (TKJoystick) engine.joysticks[i];
                    break;
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
            MainWindow = "map";
            var map = new Map();
            engine.SpawnObject("map", map);
        }

        private void CloseMap()
        {
            MainWindow = "game";
            OnDestroyHelper(engine.objects["map"]);
        }

        private void Pause()
        {
            MainWindow = "pause";
            var pause = new Pause();
            engine.SpawnObject("pause", pause);
        }

        private void UnPause()
        {
            MainWindow = "game";
            OnDestroyHelper(engine.objects["pause"]);
        }

        public static void OnDestroyHelper(GameObject objBeingDestroyed)
        {
            var toDestroy = objBeingDestroyed.engine.objects.Values.Where(obj => obj.name.StartsWith(objBeingDestroyed.name)).ToList();
            foreach (var obj in toDestroy)
                obj.Destroy();
        }

        private void ManageControls()
        {
            if (lastWindowChange > 0)
                lastWindowChange -= deltaTime;
            if (!(lastWindowChange <= 0)) return;
            var startingWindow = MainWindow;

            switch (MainWindow)
            {
                case "game":
                    if (engine.IsKeyDown((int) Key.M) ||
                        (Joystick != null && Joystick.GetButton(JoyStickConfig["SL"])))
                        OpenMap();
                    else if (engine.IsKeyDown((int) Key.Escape) ||
                             (Joystick != null && Joystick.GetButton(JoyStickConfig["ST"])))
                        Pause();
                    break;
                case "map":
                    if (engine.IsKeyDown((int) Key.M) || engine.IsKeyDown((int) Key.Escape) ||
                        (Joystick != null && Joystick.GetButton(JoyStickConfig["SL"])))
                        CloseMap();
                    break;
                case "pause":
                    if (engine.IsKeyDown((int) Key.P) || engine.IsKeyDown((int) Key.Escape) ||
                        (Joystick != null && Joystick.GetButton(JoyStickConfig["ST"])))
                        UnPause();
                    break;
                case "logo":
                    if (AnyKeyDown() || (Joystick != null && AnyJoystickButtonPressed()))
                        StartGame();
                    break;
                case "gameover":
                    if (gameOverTimer > 0)
                        gameOverTimer -= deltaTime;
                    if (gameOverTimer <= 0 && (AnyKeyDown() || (Joystick != null && AnyJoystickButtonPressed())))
                        engine.isGameRunning = false;
                    break;
            }
            if (startingWindow != MainWindow)
                lastWindowChange = WindowChangeDelay;
        }

        public bool AnyJoystickButtonPressed()
        {
            return joystickButtons.Any(button => Joystick.GetButton(JoyStickConfig[button]));
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

        private void StartGameOver()
        {
            MainWindow = "gameover";
            var background = new RectangleObject
            {
                color = Color.Black,
                fill = true,
                width = engine.width,
                height = engine.height,
                order = 10
            };
            engine.SpawnObject("gameover_background", background);
            var gameOver = new TextObject("Phosphate", 80, "red") {text = "GAMEOVER"};
            var gameOverSize = TextRenderer.MeasureText(gameOver.text, gameOver.font);
            gameOver.x = engine.width/2 - gameOverSize.Width/2;
            gameOver.y = engine.height/2 - gameOverSize.Height/2;
            gameOver.order = 11;
            engine.SpawnObject("gameover_text", gameOver);
            gameOverTimer = GameOverDelay;
        }

        public override void Update()
        {
            ManageJoystick();
            ManageControls();
            if (MainWindow == "game")
            {
                if (Player.Level != null && !Player.IsAlive)
                    GameOver = true;
                // check for gameOver
                if (GameOver)
                {
                    StartGameOver();
                }

                ManageFloor();
            }
        }
    }
}