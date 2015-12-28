using Aiv.Engine;
using Futuridium.Spells;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace Futuridium
{
    public sealed class Player : Character
    {
        private const float MaxHitsPerTime = 1f; // 500ms immunity after gets hit
        private const float ChangeFloorDelay = 2f;

        // singleton since the game supports only one player
        private static Player instance;

        // TODO: make character get new spell by level/item
        private readonly List<Type> defaultSpells = new List<Type>
        {typeof (Bullet), typeof (DriveX), typeof (Orb)};

        private bool initHud;
        private Vector2 lastPosition;
        public string realName = "Rek";
        private RectangleObject redWindow;

        private Player() : base("player", "Player", "player")
        {
            delayBeforeActivation = 0f;

            name = "Player";
            order = 7;

            Level0.MaxHp = 423;
            Level0.MaxEnergy = 100;
            Level0.Speed = 150;
            Level0.SpellCd = 1.1f;
            Level0.Attack = 25;
            Level0.NeededXp = 100;
            Level0.SpellSpeed = 200f;
            Level0.SpellRange = 400;
            Level0.SpellSize = 14;
            Level0.spellList = defaultSpells;

            OnStart += StartEvent;
            OnAfterUpdate += UpdateEvent;
        }

        public static Player Instance => instance ?? (instance = new Player());

        private void StartEvent(object sender)
        {
            currentSprite = (SpriteAsset)engine.GetAsset("player_animated_2_0");

            //var fwidth = Utils.FixBoxValue(width);
            //var fheight = Utils.FixBoxValue(height);

            spellManager.Mask = (GameObject enemy) => enemy is Enemy;

            x = engine.width / 2 - width / 2;
            y = engine.height / 2 - height / 2;
            order = 9;

            redWindow = new RectangleObject
            {
                width = 0,
                height = 0,
                name = "redWindow",
                color = Color.Red,
                x = 0,
                y = 0,
                fill = true,
                order = order + 1
            };
            engine.SpawnObject("redWindow", redWindow);

            var baseSpriteName = $"player_animated";
            AnimationFrequency = 0.2f;
            animationsInfo[MovingState.Idle] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{baseSpriteName}_0_1")
            };
            animationsInfo[MovingState.MovingRight] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{baseSpriteName}_2_0"),
                (SpriteAsset) engine.GetAsset($"{baseSpriteName}_2_1"),
                (SpriteAsset) engine.GetAsset($"{baseSpriteName}_2_2")
            };
            animationsInfo[MovingState.MovingLeft] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{baseSpriteName}_1_0"),
                (SpriteAsset) engine.GetAsset($"{baseSpriteName}_1_1"),
                (SpriteAsset) engine.GetAsset($"{baseSpriteName}_1_2")
            };
            animationsInfo[MovingState.MovingUp] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{baseSpriteName}_3_0"),
                (SpriteAsset) engine.GetAsset($"{baseSpriteName}_3_1"),
                (SpriteAsset) engine.GetAsset($"{baseSpriteName}_3_2")
            };
            animationsInfo[MovingState.MovingDown] = new SpriteAsset[]
            {
                (SpriteAsset) engine.GetAsset($"{baseSpriteName}_0_0"),
                (SpriteAsset) engine.GetAsset($"{baseSpriteName}_0_1"),
                (SpriteAsset) engine.GetAsset($"{baseSpriteName}_0_2")
            };
            CalculateRealSpriteHitBoxes();
        }

        private void ManageControls()
        {
            // keyboard controls
            lastPosition = new Vector2(x, y);
            // Keys.Right for windows.form (Engine)
            // (int) Key.Right for OpenTK
            // should switch to Keys when game.usingOpenTK is false
            RealSpeed = Level.Speed;
            var changedState = false;
            var movingDirection = new Vector2();
            if (engine.IsKeyDown((int)Key.Right))
                movingDirection.X = 1f;
            if (engine.IsKeyDown((int)Key.Left))
                movingDirection.X = -1f;
            if (engine.IsKeyDown((int)Key.Up))
                movingDirection.Y = -1f;
            if (engine.IsKeyDown((int)Key.Down))
                movingDirection.Y = 1f;

            // joystick controls
            if (Game.Instance.Joystick != null)
            {
                var joyMovingDirection = new Vector2(
                    Game.Instance.Joystick.GetAxis(Game.Instance.JoyStickConfig["Lx"]) / 127f,
                    Game.Instance.Joystick.GetAxis(Game.Instance.JoyStickConfig["Ly"]) / 127f
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
                        Console.WriteLine($"Pressed button: {index}");
                }*/
            }
            if (movingDirection.LengthFast > 0.2f)
            {
                Vx += movingDirection.X * deltaTime * RealSpeed;
                Vy += movingDirection.Y * deltaTime * RealSpeed;
                CalculateMovingState(movingDirection);
            } else
            {
                movingState = MovingState.Idle;
            }
        }

        private void ManageShot()
        {
            var direction = new Vector2();
            // axis1, axis2, value1, value2
            //Tuple<int, int, float, float> castJoyInfo;
            if (Game.Instance.Joystick != null)
            {
                direction = new Vector2(
                    Game.Instance.Joystick.GetAxis(Game.Instance.JoyStickConfig["Rx"]) / 127f,
                    Game.Instance.Joystick.GetAxis(Game.Instance.JoyStickConfig["Ry"]) / 127f
                    );
                //castJoyInfo = Tuple.Create(game.JoyStickConfig["Rx"], game.JoyStickConfig["Ry"], direction.X, direction.Y);
                if (direction.X > 0 || direction.Y > 0)
                {
                    Debug.Write("Shotting axis on joystick: ");
                    for (var axisIndex = 0; axisIndex < 6; axisIndex++)
                        Debug.Write($"{axisIndex}: {Game.Instance.Joystick.GetAxis(axisIndex)} ; ");
                    Debug.WriteLine("");
                }
                if (Game.Instance.Joystick.GetButton(Game.Instance.JoyStickConfig["RB"]) ||
                    Game.Instance.Joystick.GetButton(Game.Instance.JoyStickConfig["LB"]))
                    spellManager.SwapSpell();
            }

            var joyStickConfig = Game.Instance.JoyStickConfig;
            var castKey = Key.Unknown;

            if (engine.IsKeyDown((int)Key.A) ||
                (Game.Instance.Joystick != null && Game.Instance.Joystick.GetButton(joyStickConfig["S"])))
            {
                direction = new Vector2(-1, 0);
                castKey = Key.A;
            }
            else if (engine.IsKeyDown((int)Key.W) ||
                     (Game.Instance.Joystick != null && Game.Instance.Joystick.GetButton(joyStickConfig["T"])))
            {
                direction = new Vector2(0, -1);
                castKey = Key.W;
            }
            else if (engine.IsKeyDown((int)Key.D) || engine.IsKeyDown((int)Key.Space) ||
                     (Game.Instance.Joystick != null && Game.Instance.Joystick.GetButton(joyStickConfig["C"])))
            {
                direction = new Vector2(1, 0);
                castKey = Key.D;
            }
            else if (engine.IsKeyDown((int)Key.S) ||
                     (Game.Instance.Joystick != null && Game.Instance.Joystick.GetButton(joyStickConfig["X"])))
            {
                direction = new Vector2(0, 1);
                castKey = Key.S;
            }
            else if (engine.IsKeyDown((int)Key.Q))
            {
                direction = new Vector2(-0.5f, -0.5f);
                castKey = Key.Q;
            }
            else if (engine.IsKeyDown((int)Key.E))
            {
                direction = new Vector2(0.5f, -0.5f);
                castKey = Key.E;
            }
            else if (engine.IsKeyDown((int)Key.Z))
            {
                direction = new Vector2(-0.5f, 0.5f);
                castKey = Key.Z;
            }
            else if (engine.IsKeyDown((int)Key.C))
            {
                direction = new Vector2(0.5f, 0.5f);
                castKey = Key.C;
            }
            else if (engine.IsKeyDown((int)Key.F))
            {
                spellManager.SwapSpell();
            }
            if (direction.Length >= 0.6)
            {
                Func<bool> castCheck;
                if (castKey != Key.Unknown)
                    castCheck = () => engine.IsKeyDown((int)castKey);
                else // for sure casted with joystick
                    castCheck = () =>
                    {
                        var newDirection = new Vector2(
                            Game.Instance.Joystick.GetAxis(Game.Instance.JoyStickConfig["Rx"]) / 127f,
                            Game.Instance.Joystick.GetAxis(Game.Instance.JoyStickConfig["Ry"]) / 127f
                            );
                        // small change == still casting
                        //return (direction - newDirection).Length < 0.1f;
                        // keeps casting until the axis are activated
                        if (spellManager.LastCastedSpell.UpdateDirection)
                            spellManager.LastCastedSpell.Direction = newDirection;
                        return newDirection.Length > 0.1f;
                    };
                Shot(direction, castCheck);
            }
        }

        private void ManageCollisions()
        {
            var collisions = CheckCollisions();
            if (collisions.Count > 0)
                Debug.WriteLine("Character '{0}' collides with n.{1}", name, collisions.Count);
            var realLastPos = new Vector2(lastPosition.X, lastPosition.Y);
            Enemy hittingEnemy = null; //only one enemy can hit per time
            foreach (var collision in collisions)
            {
                Debug.WriteLine(
                    $"Character '{name}' ({collision.hitBox}) touches '{collision.other.name}' ({collision.otherHitBox})");
                var enemy = collision.other as Enemy;
                if (enemy != null && Timer.Get("lastHitTimer") <= 0)
                {
                    hittingEnemy = enemy;
                    Timer.Set("lastHitTimer", MaxHitsPerTime);
                }
                if (collision.other.name.EndsWith("door") && Timer.Get("lastFloorChangeTimer") <= 0 &&
                    collision.other.enabled)
                {
                    Vx = 0;
                    Vy = 0;
                    Debug.WriteLine("About to change room to: " + collision.other.name);
                    var changedFloor = false;
                    if (collision.other.name.EndsWith("top_door"))
                        changedFloor =
                            Game.Instance.CurrentFloor.OpenRoom(Game.Instance.CurrentFloor.CurrentRoom.Top);
                    else if (collision.other.name.EndsWith("left_door"))
                        changedFloor =
                            Game.Instance.CurrentFloor.OpenRoom(Game.Instance.CurrentFloor.CurrentRoom.Left);
                    else if (collision.other.name.EndsWith("bottom_door"))
                        changedFloor =
                            Game.Instance.CurrentFloor.OpenRoom(Game.Instance.CurrentFloor.CurrentRoom.Bottom);
                    else if (collision.other.name.EndsWith("right_door"))
                        changedFloor =
                            Game.Instance.CurrentFloor.OpenRoom(Game.Instance.CurrentFloor.CurrentRoom.Right);
                    if (changedFloor)
                        Timer.Set("lastFloorChangeTimer", ChangeFloorDelay);
                    break;
                }
                if (collision.otherHitBox.StartsWith("wall"))
                {
                    x = (int)realLastPos.X;
                    y = (int)realLastPos.Y;
                }
                else if (collision.other.name.StartsWith("escape_floor_"))
                {
                    collision.other.Destroy();
                    Game.Instance.InitializeNewFloor();
                }
            }
            hittingEnemy?.DoDamage(this);
        }

        protected override float GetDamage(Character enemy, Damage damage)
        {
            redWindow.width = engine.width;
            redWindow.height = engine.height;
            Debug.WriteLine("Player got damaged.");
            Timer.Set("disableRedTimer", 0.05f); // 50ms

            return base.GetDamage(enemy, damage);
        }

        private void UpdateEvent(object s)
        {
            if (Game.Instance.MainWindow != "game") return;
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
                OnXpChanged += sender => { Hud.Instance.UpdateXpBar(); };
                OnLevelup += sender => { Hud.Instance.UpdateXpBar(); };
                spellManager.OnSpellCdChanged += sender => { Hud.Instance.UpdateSpellBar(); };
            }
            ManageControls();
            ManageShot();

            ManageCollisions();

            if (redWindow.width != 0)
            {
                if (Timer.Get("disableRedTimer") <= 0)
                {
                    redWindow.width = 0;
                    redWindow.height = 0;
                }
            }
        }
    }
}