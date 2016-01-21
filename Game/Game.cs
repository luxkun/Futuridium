using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Aiv.Engine;
using Aiv.Fast2D;
using Futuridium.Characters;
using Futuridium.Items;
using Futuridium.UI;
using Futuridium.World;
using TextObject = Aiv.Engine.TextObject;

namespace Futuridium.Game
{
    internal class Game : GameObject
    {
        private static readonly Dictionary<string, int> Ds4Config = new Dictionary<string, int>
        {
            {"T", 3},
            {"C", 1},
            {"S", 2},
            {"X", 0},
            {"RB", 4},
            {"LB", 5},
            {"RT", -1},
            {"LT", -1},
            {"SL", 6},
            {"ST", 7},
            {"Lx", 0},
            {"Ly", 1},
            {"Rx", 3},
            {"Ry", 4}
        };

        private static readonly float GameOverDelay = 1f;

        private static Game instance;

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

        private static readonly float WindowChangeDelay = 0.5f;

        public static readonly string[] JoystickButtons = {"T", "C", "S", "X", "SL", "ST"};
        private int floorIndex = -1;

        //public TKJoystick Joystick { get; private set; }
        public Engine.Joystick Joystick = null;
        //public List<Floor> floors;

        private string lastWindow;

        private Game()
        {
            Random = new RandomSeed(Utils.RandomString(5));

            JoyStickConfig = Ds4Config;
        }

        public Floor CurrentFloor { get; private set; }

        public bool GameOver { get; private set; }

        public static Game Instance => instance ?? (instance = new Game());

        public Dictionary<string, int> JoyStickConfig { get; }

        public string MainWindow { get; private set; }

        public RandomSeed Random { get; private set; }

        public bool UsingOpenTK { get; set; }
        public Score Score { get; private set; }

        private void CloseMap()
        {
            MainWindow = "game";
            OnDestroyHelper(Engine.Objects["map"]);
        }

        private void ManageControls()
        {
            if (Timer.Get("lastWindowChange") > 0f) return;
            var startingWindow = MainWindow;

            switch (MainWindow)
            {
                case "game":
                    if (Engine.IsKeyDown(KeyCode.M) ||
                        (Joystick != null && Joystick.GetButton(JoyStickConfig["SL"])))
                        OpenMap();
                    else if (Engine.IsKeyDown(KeyCode.Esc) ||
                             (Joystick != null && Joystick.GetButton(JoyStickConfig["ST"])))
                        Pause();
                    break;

                case "map":
                    if (Engine.IsKeyDown(KeyCode.M) || Engine.IsKeyDown(KeyCode.Esc) ||
                        (Joystick != null && Joystick.GetButton(JoyStickConfig["SL"])))
                        CloseMap();
                    break;

                case "pause":
                    if (Engine.IsKeyDown(KeyCode.P) || Engine.IsKeyDown(KeyCode.Esc) ||
                        (Joystick != null && Joystick.GetButton(JoyStickConfig["ST"])))
                        UnPause();
                    break;

                case "logo":
                    if (AnyKeyDown() || (Joystick != null && AnyJoystickButtonPressed()))
                        StartGame();
                    break;

                case "gameover":
                    if (Timer.Get("gameOverTimer") <= 0 &&
                        (AnyKeyDown() || (Joystick != null && AnyJoystickButtonPressed())))
                        Engine.IsGameRunning = false;
                    break;
            }
            if (startingWindow != MainWindow)
                Timer.Set("lastWindowChange", WindowChangeDelay, ignoreTimeModifier: true);
        }

        private void ManageJoystick()
        {
            //if (Joystick != null && Joystick.IsConnected()) return;
            //Joystick = null;
            //for (var i = 0; i < 8; i++)
            //{
            //    if (Engine.joysticks[i] != null)
            //    {
            //        Joystick = (TKJoystick)Engine.joysticks[i];
            //        break;
            //    }
            //}
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

        // TODO: window state class
        private void OpenMap()
        {
            MainWindow = "map";
            var map = new Map();
            Engine.SpawnObject("map", map);
        }

        private void Pause()
        {
            MainWindow = "pause";
            var pause = new Pause();
            Engine.SpawnObject("pause", pause);
        }

        private void StartGame()
        {
            OnDestroyHelper((SpriteObject) Engine.Objects["logo"]);
            MainWindow = "game";

            Engine.SpawnObject(Player.Instance);

            Engine.SpawnObject(Hud.Instance);

            Score = new Score();
            Engine.SpawnObject("score", Score);

            InitializeNewFloor();
        }

        private void StartGameOver()
        {
            Engine.TimeModifier = 0f;

            AudioSource.Pause();
            MainWindow = "gameover";
            var background = new RectangleObject(Engine.Width, Engine.Height)
            {
                Color = Color.FromArgb(125, 0, 0, 0),
                Fill = true,
                Order = 10,
                IgnoreCamera = true
            };
            Engine.SpawnObject("gameover_background", background);
            var gameOver = new TextObject(1.33f, Color.White)//Color.Red)
            {
                Text = "GAMEOVER",
                Order = 11,
                IgnoreCamera = true
            };
            // replace with alignment.center or use TextObject.Measure
            var gameOverSize = gameOver.Measure();
            gameOver.X = Engine.Width/2f - gameOverSize.X/2;
            gameOver.Y = Engine.Height/2f - gameOverSize.Y/2;
            Engine.SpawnObject("gameover_text", gameOver);

            Timer.Set("gameOverTimer", GameOverDelay, ignoreTimeModifier: true);
        }

        private void UnPause()
        {
            MainWindow = "game";
            OnDestroyHelper(Engine.Objects["pause"]);
        }

        public bool AnyJoystickButtonPressed()
        {
            return false;
            //return JoystickButtons.Any(button => Joystick.GetButton(JoyStickConfig[button]));
        }

        public bool AnyKeyDown()
        {
            foreach (KeyCode key in Enum.GetValues(typeof (KeyCode)))
            {
                if (Engine.IsKeyDown(key))
                    return true;
            }
            return false;
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
                    Debug.Write("{0} - ", en.Name);
                }

                if (CurrentFloor.CurrentRoom.Enemies.Count == 0)
                {
                    CurrentFloor.CurrentRoom.GameBackground.OpenDoors();
                    // check if the floor has been cleared
                    ManageFloorExit();
                }
            }
        }

        public void InitializeNewFloor()
        {
            StartLoading();

            if (floorIndex >= 0)
            {
                OnDestroyHelper(CurrentFloor);
            }
            floorIndex++;
            CurrentFloor = new Floor(floorIndex);
            Engine.SpawnObject(CurrentFloor.Name, CurrentFloor);
            var minRooms = (int) (5*((floorIndex + 9)/10f));
            var maxRooms = (int) (8*((floorIndex + 9)/9f));
            CurrentFloor.RandomizeFloor(minRooms, maxRooms);
            CurrentFloor.OpenRoom(CurrentFloor.FirstRoom);

            StopLoading();
        }

        public void ManageFloorExit()
        {
            if (CurrentFloor.RoomsList.Any(room => room.Enemies.Count > 0))
            {
                return;
            }
            var escapeFloorName = $"escape_floor_{CurrentFloor.FloorIndex}";
            if (!Engine.Objects.ContainsKey(escapeFloorName))
            {
                var spriteAsset = (SpriteAsset) Engine.GetAsset("escape_floor");
                var escapeFloorObj = new SpriteObject(spriteAsset.Width, spriteAsset.Height)
                {
                    Name = escapeFloorName,
                    Order = 5,
                    X = Engine.Width/2,
                    Y = Engine.Height/2,
                    CurrentSprite = spriteAsset
                };
                escapeFloorObj.AddHitBox(escapeFloorName, 0, 0, 32, 32);
                Engine.SpawnObject(escapeFloorObj);
            }
        }

        public static void OnDestroyHelper(GameObject objBeingDestroyed, bool ignoreSelf = false)
        {
            var toDestroy =
                objBeingDestroyed.Engine.Objects.Values.Where(
                    obj => obj.Name.StartsWith(objBeingDestroyed.Name, StringComparison.Ordinal)).ToList();
            foreach (var obj in toDestroy)
                if (!ignoreSelf || obj != objBeingDestroyed)
                    obj.Destroy();
        }

        public override void Start()
        {
            base.Start();

            MainClass.LoadAssets(Engine);

            GameBackground.Initialize(Engine);

            Engine.SpawnObject(new CharactersInfo());

            var logoAsset = (SpriteAsset) Engine.GetAsset("logo");
            var logoObj = new SpriteObject(logoAsset.Width, logoAsset.Height)
            {
                CurrentSprite = logoAsset
            };
            logoObj.X = Engine.Width/2 - logoObj.Width/2;
            logoObj.Y = Engine.Height/2 - logoObj.Height/2;
            Engine.SpawnObject("logo", logoObj);
            MainWindow = "logo";

            BasicItems.Initialize(Engine);

            AudioSource.Volume = 0.66f;
            AudioSource.Stream(((AudioAsset)Engine.GetAsset("sound_soundtrack")).FileName, true);
        }

        public void StartLoading()
        {
            if (MainWindow != "loading")
            {
                Engine.TimeModifier = 0f;
                lastWindow = MainWindow;
                MainWindow = "loading";
            }
        }

        public void StopLoading()
        {
            Engine.TimeModifier = 1f;
            MainWindow = lastWindow;
        }

        public override void Update()
        {
            base.Update();
            ManageJoystick();
            ManageControls();
            if (MainWindow == "game")
            {
                if (Player.Instance.Level != null && !Player.Instance.IsAlive)
                    GameOver = true;
                // check for gameOver
                if (GameOver)
                {
                    StartGameOver();
                }
            }
        }

        ~Game()
        {
            //AudioSource.Stop();
        }
    }
}