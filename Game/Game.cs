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
using OpenTK;
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
                var escapeFloorObj = new SpriteObject(spriteAsset.Width, spriteAsset.Height, true)
                {
                    Name = escapeFloorName,
                    Order = 5,
                    X = Engine.Width/2,
                    Y = Engine.Height/2,
                    CurrentSprite = spriteAsset
                };
                //escapeFloorObj.AddHitBox(escapeFloorName, 0, 0, 32, 32);
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

        private static Dictionary<char, Tuple<Vector2, Vector2>> charToSprite = new Dictionary<char, Tuple<Vector2, Vector2>>()
        {
                {'0', Tuple.Create(new Vector2(0f, 0f), new Vector2(44f, 31f))},
                {'1', Tuple.Create(new Vector2(45f, 0f), new Vector2(22f, 31f))},
                {'2', Tuple.Create(new Vector2(66f, 0f), new Vector2(44f, 31f))},
                {'3', Tuple.Create(new Vector2(109f, 0f), new Vector2(44f, 31f))},
                {'4', Tuple.Create(new Vector2(152f, 0f), new Vector2(44f, 31f))},
                {'5', Tuple.Create(new Vector2(195f, 0f), new Vector2(44f, 31f))},
                {'6', Tuple.Create(new Vector2(239f, 0f), new Vector2(44f, 31f))},
                {'7', Tuple.Create(new Vector2(281f, 0f), new Vector2(44f, 31f))},
                {'8', Tuple.Create(new Vector2(325f, 0f), new Vector2(44f, 31f))},
                {'9', Tuple.Create(new Vector2(369f, 0f), new Vector2(44f, 31f))},
                {'A', Tuple.Create(new Vector2(411f, 0f), new Vector2(51f, 31f))},
                {'B', Tuple.Create(new Vector2(462f, 0f), new Vector2(46f, 31f))},
                {'C', Tuple.Create(new Vector2(0f, 31f), new Vector2(44f, 31f))},
                {'D', Tuple.Create(new Vector2(44f, 31f), new Vector2(44f, 31f))},
                {'E', Tuple.Create(new Vector2(88f, 31f), new Vector2(44f, 31f))},
                {'F', Tuple.Create(new Vector2(132f, 31f), new Vector2(44f, 31f))},
                {'G', Tuple.Create(new Vector2(175f, 31f), new Vector2(44f, 31f))},
                {'H', Tuple.Create(new Vector2(219f, 31f), new Vector2(44f, 31f))},
                {'I', Tuple.Create(new Vector2(262f, 31f), new Vector2(15f, 31f))},
                {'J', Tuple.Create(new Vector2(275f, 31f), new Vector2(44f, 31f))},
                {'K', Tuple.Create(new Vector2(319f, 31f), new Vector2(44f, 31f))},
                {'L', Tuple.Create(new Vector2(362f, 31f), new Vector2(44f, 31f))},
                {'M', Tuple.Create(new Vector2(404f, 31f), new Vector2(44f, 31f))},
                {'N', Tuple.Create(new Vector2(450f, 31f), new Vector2(44f, 31f))},
                {'O', Tuple.Create(new Vector2(0f, 62f), new Vector2(44f, 31f))},
                {'P', Tuple.Create(new Vector2(44f, 62f), new Vector2(44f, 31f))},
                {'Q', Tuple.Create(new Vector2(88f, 62f), new Vector2(44f, 31f))},
                {'R', Tuple.Create(new Vector2(131f, 62f), new Vector2(44f, 31f))},
                {'S', Tuple.Create(new Vector2(175f, 62f), new Vector2(44f, 31f))},
                {'T', Tuple.Create(new Vector2(218f, 62f), new Vector2(44f, 31f))},
                {'U', Tuple.Create(new Vector2(262f, 62f), new Vector2(44f, 31f))},
                {'V', Tuple.Create(new Vector2(306f, 62f), new Vector2(44f, 31f))},
                {'W', Tuple.Create(new Vector2(350f, 62f), new Vector2(44f, 31f))},
                {'X', Tuple.Create(new Vector2(395f, 62f), new Vector2(44f, 31f))},
                {'Y', Tuple.Create(new Vector2(439f, 62f), new Vector2(44f, 31f))},
                {'Z', Tuple.Create(new Vector2(0f, 93f), new Vector2(44f, 31f))},
                {'%', Tuple.Create(new Vector2(44f, 93f), new Vector2(44f, 31f))},
                {'!', Tuple.Create(new Vector2(87f, 93f), new Vector2(13f, 31f))},
                {'?', Tuple.Create(new Vector2(100f, 93f), new Vector2(44f, 31f))},
                {'+', Tuple.Create(new Vector2(142f, 93f), new Vector2(36f, 31f))},
                {'-', Tuple.Create(new Vector2(179f, 93f), new Vector2(30f, 31f))},
                {'*', Tuple.Create(new Vector2(209f, 93f), new Vector2(30f, 31f))},
                {'/', Tuple.Create(new Vector2(238f, 93f), new Vector2(34f, 31f))},
                {':', Tuple.Create(new Vector2(296f, 93f), new Vector2(13f, 31f))},
                {'.', Tuple.Create(new Vector2(272f, 93f), new Vector2(13f, 31f))},
                {',', Tuple.Create(new Vector2(272f, 93f), new Vector2(13f, 31f))},
                {'\'', Tuple.Create(new Vector2(285f, 93f), new Vector2(13f, 31f))}
        };

        public override void Start()
        {
            base.Start();

            MainClass.LoadAssets(Engine);

            TextConfig.Default = new TextConfig(new Asset("font.png"), charToSprite, 
                paddingFunc: (float width) =>
                {
                    float result = width;
                    result *= -0.066f;
                    return result;
                });
            
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