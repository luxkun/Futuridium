using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Aiv.Engine;
using Aiv.Fast2D;
using Futuridium.Spells;
using Futuridium.UI;
using OpenTK;
using Utils = Futuridium.Game.Utils;

namespace Futuridium.Characters
{
    public sealed class Player : Character
    {
        private const float MaxHitsPerTime = 1f; // 500ms immunity after gets hit
        private const float ChangeFloorDelay = 2f;

        // singleton since the game supports only one player

        // TODO: make character get new spell by level/item
        private readonly List<Type> defaultSpells = new List<Type>
        {typeof (Bullet), typeof (DriveX), typeof (Orb)};

        private bool initHud;
        private Vector2 lastPosition;
        public string realName = "Rek";
        private RectangleObject redWindow;

        private Player(int width, int height) : base("player", "Player", "player", width, height)
        {
            delayBeforeActivation = 0f;

            Name = "Player";
            Order = 7;

            Level0.MaxHp = 423;
            Level0.MaxEnergy = 100;
            Level0.Speed = 150;
            Level0.SpellCd = 1.1f;
            Level0.Attack = 25;
            Level0.NeededXp = 100;
            Level0.SpellSpeed = 200f;
            Level0.SpellRange = 400;
            Level0.SpellSize = 14;
            Level0.SpellList = defaultSpells;
            Level0.DropModifier = 0f;

            OnStart += StartEvent;
            OnAfterUpdate += UpdateEvent;
            OnLevelup += LevelUpEvent;
        }

        public static Player Instance { get; private set; }

        private void LevelUpEvent(object sender)
        {
            if (Level.level > 0)
                AudioSource.Play(((AudioAsset)Engine.GetAsset("sound_levelup")).Clip);
        }

        private void StartEvent(object sender)
        {
            CurrentSprite = (SpriteAsset) Engine.GetAsset("player_animated_2_0");
            //var fWidth = Width;
            //var fHeight = Height;

            SpellManager.Mask = (GameObject enemy) => enemy is Enemy;

            X = Engine.Width/2 - Width/2;
            Y = Engine.Height/2 - Height/2;
            Order = 9;

            redWindow = new RectangleObject(Engine.Width, Engine.Height)
            {
                Name = "redWindow",
                Color = Color.FromArgb(155, 255, 0, 0),
                X = 0,
                Y = 0,
                Fill = true,
                Order = Order + 1,
                IgnoreCamera = true
            };
            redWindow.Box.scale = Vector2.Zero;
            Engine.SpawnObject("redWindow", redWindow);

            // Load animations
            var idleName = GetMovingStateString(MovingState.Idle);
            var movingLeftName = GetMovingStateString(MovingState.MovingLeft);
            var movingRightName = GetMovingStateString(MovingState.MovingRight);
            var movingDownName = GetMovingStateString(MovingState.MovingDown);
            var movingUpName = GetMovingStateString(MovingState.MovingUp);
            var baseSpriteName = "player_animated";
            AddAnimation(idleName, Utils.GetAssetName(baseSpriteName, 1, 0), 5).Loop = false;
            AddAnimation(movingRightName, Utils.GetAssetName(baseSpriteName, 0, 2, 3), 5);
            AddAnimation(movingLeftName, Utils.GetAssetName(baseSpriteName, 0, 1, 3), 5);
            AddAnimation(movingUpName, Utils.GetAssetName(baseSpriteName, 0, 3, 3), 5);
            AddAnimation(movingDownName, Utils.GetAssetName(baseSpriteName, 0, 0, 3), 5);
            //CalculateAnimationHitBoxes();
        }

        private void ManageControls()
        {
            //Debug.WriteLine($"Camera X,Y: {Engine.Camera.X},{Engine.Camera.Y}");
            // keyboard controls
            lastPosition = new Vector2(X, Y);
            // Keys.Right for windows.form (Engine)
            // (int) KeyCode.Right for OpenTK
            // should switch to Keys when game.usingOpenTK is false
            RealSpeed = Level.Speed;
            var movingDirection = new Vector2();
            if (Engine.IsKeyDown(KeyCode.D))
                movingDirection.X = 1f;
            if (Engine.IsKeyDown(KeyCode.A))
                movingDirection.X = -1f;
            if (Engine.IsKeyDown(KeyCode.W))
                movingDirection.Y = -1f;
            if (Engine.IsKeyDown(KeyCode.S))
                movingDirection.Y = 1f;

            // joystick controls
            if (Game.Game.Instance.Joystick != null)
            {
                var joyMovingDirection = new Vector2(
                    Game.Game.Instance.Joystick.GetAxis(Game.Game.Instance.JoyStickConfig["Lx"])/127f,
                    Game.Game.Instance.Joystick.GetAxis(Game.Game.Instance.JoyStickConfig["Ly"])/127f
                    );
                if (joyMovingDirection.Length > 0.2)
                {
                    movingDirection = joyMovingDirection;
                }
                /*foreach (var button in Game.JoystickButtons)
                    if (Game.Instance.Joystick.GetButton(Game.Instance.JoyStickConfig[button]))
                        Debug.WriteLine($"Pressed button: {button}");

                for (int index = 0; index < 150; index++)
                {
                    if (Game.Instance.Joystick.GetButton(index))
                        Debug.WriteLine($"Pressed button: {index}");
                }*/
            }
            if (movingDirection.LengthFast > 0.2f)
            {
                X += movingDirection.X*DeltaTime*RealSpeed;
                Y += movingDirection.Y*DeltaTime*RealSpeed;
                CalculateMovingState(movingDirection);
            }
            else
            {
                movingState = MovingState.Idle;
            }
        }

        private void ManageShot()
        {
            var direction = new Vector2();
            // axis1, axis2, value1, value2
            //Tuple<int, int, float, float> castJoyInfo;
            if (Game.Game.Instance.Joystick != null)
            {
                direction = new Vector2(
                    Game.Game.Instance.Joystick.GetAxis(Game.Game.Instance.JoyStickConfig["Rx"])/127f,
                    Game.Game.Instance.Joystick.GetAxis(Game.Game.Instance.JoyStickConfig["Ry"])/127f
                    );
                //castJoyInfo = Tuple.Create(game.JoyStickConfig["Rx"], game.JoyStickConfig["Ry"], direction.X, direction.Y);
                if (direction.X > 0 || direction.Y > 0)
                {
                    Debug.Write("Shotting axis on joystick: ");
                    for (var axisIndex = 0; axisIndex < 6; axisIndex++)
                        Debug.Write($"{axisIndex}: {Game.Game.Instance.Joystick.GetAxis(axisIndex)} ; ");
                    Debug.WriteLine("");
                }
                if (Game.Game.Instance.Joystick.GetButton(Game.Game.Instance.JoyStickConfig["RB"]) ||
                    Game.Game.Instance.Joystick.GetButton(Game.Game.Instance.JoyStickConfig["LB"]))
                    SpellManager.SwapSpell();
            }

            var joyStickConfig = Game.Game.Instance.JoyStickConfig;
            var castKey = KeyCode.Unknown;

            if (Engine.IsKeyDown(KeyCode.Left) ||
                (Game.Game.Instance.Joystick != null && Game.Game.Instance.Joystick.GetButton(joyStickConfig["S"])))
            {
                direction = new Vector2(-1, 0);
                castKey = KeyCode.Left;
            }
            else if (Engine.IsKeyDown(KeyCode.Up) ||
                     (Game.Game.Instance.Joystick != null && Game.Game.Instance.Joystick.GetButton(joyStickConfig["T"])))
            {
                direction = new Vector2(0, -1);
                castKey = KeyCode.Up;
            }
            else if (Engine.IsKeyDown(KeyCode.Right) || Engine.IsKeyDown(KeyCode.Space) ||
                     (Game.Game.Instance.Joystick != null && Game.Game.Instance.Joystick.GetButton(joyStickConfig["C"])))
            {
                direction = new Vector2(1, 0);
                castKey = KeyCode.Right;
            }
            else if (Engine.IsKeyDown(KeyCode.Down) ||
                     (Game.Game.Instance.Joystick != null && Game.Game.Instance.Joystick.GetButton(joyStickConfig["X"])))
            {
                direction = new Vector2(0, 1);
                castKey = KeyCode.Down;
            }
            else if (Engine.IsKeyDown(KeyCode.Q))
            {
                direction = new Vector2(-0.5f, -0.5f);
                castKey = KeyCode.Q;
            }
            else if (Engine.IsKeyDown(KeyCode.E))
            {
                direction = new Vector2(0.5f, -0.5f);
                castKey = KeyCode.E;
            }
            else if (Engine.IsKeyDown(KeyCode.Z))
            {
                direction = new Vector2(-0.5f, 0.5f);
                castKey = KeyCode.Z;
            }
            else if (Engine.IsKeyDown(KeyCode.C))
            {
                direction = new Vector2(0.5f, 0.5f);
                castKey = KeyCode.C;
            }
            else if (Engine.IsKeyDown(KeyCode.F))
            {
                SpellManager.SwapSpell();
            }
            if (direction.Length >= 0.6)
            {
                Func<bool> castCheck = null;
                if (castKey != KeyCode.Unknown)
                    castCheck = () => Engine.IsKeyDown(castKey);
                else // for sure casted with joystick
                    castCheck = () =>
                    {
                        var newDirection = new Vector2(
                            Game.Game.Instance.Joystick.GetAxis(Game.Game.Instance.JoyStickConfig["Rx"])/127f,
                            Game.Game.Instance.Joystick.GetAxis(Game.Game.Instance.JoyStickConfig["Ry"])/127f
                            );
                        // small change == still casting
                        //return (direction - newDirection).Length < 0.1f;
                        // keeps casting until the axis are activated
                        if (SpellManager.LastCastedSpell.UpdateDirection)
                            SpellManager.LastCastedSpell.Direction = newDirection;
                        return newDirection.Length > 0.1f;
                    };
                Shot(direction, castCheck);
            }
        }

        private void ManageCollisions()
        {
            var collisions = CheckCollisions();
            if (collisions.Count > 0)
                Debug.WriteLine("Character '{0}' collides with n.{1}", Name, collisions.Count);
            var realLastPos = new Vector2(lastPosition.X, lastPosition.Y);
            Enemy hittingEnemy = null; //only one enemy can hit per time
            foreach (var collision in collisions)
            {
                Debug.WriteLine(
                    $"Character '{Name}' ({collision.HitBox}) touches '{collision.Other.Name}' ({collision.OtherHitBox})");
                var enemy = collision.Other as Enemy;
                if (enemy != null && Timer.Get("lastHitTimer") <= 0)
                {
                    hittingEnemy = enemy;
                    Timer.Set("lastHitTimer", MaxHitsPerTime);
                }
                if (collision.Other.Name.EndsWith("door", StringComparison.Ordinal) && Timer.Get("lastFloorChangeTimer") <= 0 &&
                    collision.Other.Enabled)
                {
                    Debug.WriteLine("About to change room to: " + collision.Other.Name);
                    var changedFloor = false;
                    if (collision.Other.Name.EndsWith("top_door", StringComparison.Ordinal))
                        changedFloor =
                            Game.Game.Instance.CurrentFloor.OpenRoom(Game.Game.Instance.CurrentFloor.CurrentRoom.Top);
                    else if (collision.Other.Name.EndsWith("left_door", StringComparison.Ordinal))
                        changedFloor =
                            Game.Game.Instance.CurrentFloor.OpenRoom(Game.Game.Instance.CurrentFloor.CurrentRoom.Left);
                    else if (collision.Other.Name.EndsWith("bottom_door", StringComparison.Ordinal))
                        changedFloor =
                            Game.Game.Instance.CurrentFloor.OpenRoom(Game.Game.Instance.CurrentFloor.CurrentRoom.Bottom);
                    else if (collision.Other.Name.EndsWith("right_door", StringComparison.Ordinal))
                        changedFloor =
                            Game.Game.Instance.CurrentFloor.OpenRoom(Game.Game.Instance.CurrentFloor.CurrentRoom.Right);
                    if (changedFloor)
                        Timer.Set("lastFloorChangeTimer", ChangeFloorDelay);
                    break;
                }
                if (collision.OtherHitBox.StartsWith("wall", StringComparison.Ordinal))
                {
                    X = (int) realLastPos.X;
                    Y = (int) realLastPos.Y;
                }
                else if (collision.Other.Name.StartsWith("escape_floor_", StringComparison.Ordinal))
                {
                    collision.Other.Destroy();
                    Game.Game.Instance.InitializeNewFloor();
                }
            }
            hittingEnemy?.DoDamage(this);
        }

        protected override float GetDamage(Character enemy, Damage damage)
        {
            redWindow.Box.scale = Vector2.One;
            Debug.WriteLine("Player got damaged.");
            Timer.Set("disableRedTimer", 0.05f); // 50ms

            return base.GetDamage(enemy, damage);
        }

        private void UpdateEvent(object s)
        {
            if (Game.Game.Instance.MainWindow != "game") return;
            if (movingState == MovingState.Inactive) return;
            if (!initHud)
            {
                initHud = true;
                Hud.Instance.UpdateHpBar();
                Hud.Instance.UpdateXpBar();
                Hud.Instance.UpdateEnergyBar();
                Hud.Instance.UpdateSpellBar();
                OnHpChanged += sender => { Hud.Instance.UpdateHpBar(); };
                OnEnergyChanged += sender => { Hud.Instance.UpdateEnergyBar(); };
                OnXpChanged += (object sender, long delta) => { Hud.Instance.UpdateXpBar(); };
                OnLevelup += sender => { Hud.Instance.UpdateXpBar(); };
                SpellManager.OnSpellCdChanged += sender => { Hud.Instance.UpdateSpellBar(); };
            }
            if (Scale.X != Level.Size)
                Scale = new Vector2(Level.Size);
            ManageControls();
            ManageShot();

            ManageCollisions();

            ManageCamera();

            if (redWindow.Box.scale.X != 0 && redWindow.Box.scale.Y != 0)
            {
                if (Timer.Get("disableRedTimer") <= 0)
                {
                    redWindow.Box.scale = Vector2.Zero;
                }
            }
        }

        private void ManageCamera()
        {
            // center on the player
            var cameraX = X - Width/2 - Engine.Width/2;
            var cameraY = Y - Height/2 - Engine.Height/2;
            // check if it's out bounds
            var room = Game.Game.Instance.CurrentFloor.CurrentRoom;
            if (cameraX < 0)
                cameraX = 0;
            if (cameraX > room.Width - Engine.Width)
                cameraX = room.Width - Engine.Width;

            if (cameraY < 0)
                cameraY = 0;
            if (cameraY > room.Height - Engine.Height)
                cameraY = room.Height - Engine.Height;
            Engine.Camera.X = cameraX;
            Engine.Camera.Y = cameraY;
            //Debug.WriteLine($"{Engine.Camera.X},{Engine.Camera.Y}");
        }

        public static void Init(Engine engine)
        {
            var sprite = (SpriteAsset) engine.GetAsset("player_animated_2_0");
            Instance = new Player(sprite.Width, sprite.Height);
        }
    }
}