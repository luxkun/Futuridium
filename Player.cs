using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Aiv.Engine;
using Futuridium.Spells;
using OpenTK;
using OpenTK.Input;

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

        private float disableRedTimer;
        private bool initHud;
        private float lastFloorChangeTimer;
        private float lastHitTimer;
        private Vector2 lastPosition;
        public string realName = "Rek";
        private RectangleObject redWindow;
        private int spawnedOrbs;

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
            Level0.SpellSize = 8;
            Level0.spellList = defaultSpells;

            OnStart += StartEvent;
            OnAfterUpdate += UpdateEvent;
        }

        public static Player Instance => instance ?? (instance = new Player());

        private void StartEvent(object sender)
        {
            x = engine.width/2;
            y = engine.height/2;
            currentSprite = (SpriteAsset) engine.GetAsset("player");

            redWindow = new RectangleObject
            {
                width = 0,
                height = 0,
                name = "redWindow",
                color = Color.Red,
                x = 0,
                y = 0,
                fill = true,
                order = 9
            };
            engine.SpawnObject("redWindow", redWindow);

            base.Start();
        }

        private void ManageControls()
        {
            // keyboard controls

            lastPosition = new Vector2(x, y);
            // Keys.Right for windows.form (Engine)
            // (int) Key.Right for OpenTK
            // should switch to Keys when game.usingOpenTK is false
            if (engine.IsKeyDown((int) Key.Right))
            {
                Vx += Level.Speed*deltaTime;
            }
            if (engine.IsKeyDown((int) Key.Left))
            {
                Vx -= Level.Speed*deltaTime;
            }
            if (engine.IsKeyDown((int) Key.Up))
            {
                Vy -= Level.Speed*deltaTime;
            }
            if (engine.IsKeyDown((int) Key.Down))
            {
                Vy += Level.Speed*deltaTime;
            }

            // joystick controls
            if (Game.Instance.Joystick != null)
            {
                var moveDirection = new Vector2(
                    Game.Instance.Joystick.GetAxis(Game.Instance.JoyStickConfig["Lx"])/127f,
                    Game.Instance.Joystick.GetAxis(Game.Instance.JoyStickConfig["Ly"])/127f
                    );
                if (moveDirection.Length > 0.2)
                {
                    Vx += Level.Speed*moveDirection.X*deltaTime;
                    Vy += Level.Speed*moveDirection.Y*deltaTime;
                }
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
                    Game.Instance.Joystick.GetAxis(Game.Instance.JoyStickConfig["Rx"])/127f,
                    Game.Instance.Joystick.GetAxis(Game.Instance.JoyStickConfig["Ry"])/127f
                    );
                //castJoyInfo = Tuple.Create(game.JoyStickConfig["Rx"], game.JoyStickConfig["Ry"], direction.X, direction.Y);
                if (direction.X > 0 || direction.Y > 0)
                {
                    Debug.Write("Shotting axis on joystick: ");
                    for (var axisIndex = 0; axisIndex < 6; axisIndex++)
                        Debug.Write($"{axisIndex}: {Game.Instance.Joystick.GetAxis(axisIndex)} ; ");
                    Debug.WriteLine("");
                }
                if (Game.Instance.Joystick.GetButton(Game.Instance.JoyStickConfig["RT"]))
                    spellManager.SwapSpell();
            }

            var joyStickConfig = Game.Instance.JoyStickConfig;
            var castKey = Key.Unknown;

            if (engine.IsKeyDown((int) Key.A) ||
                (Game.Instance.Joystick != null && Game.Instance.Joystick.GetButton(joyStickConfig["S"])))
            {
                direction = new Vector2(-1, 0);
                castKey = Key.A;
            }
            else if (engine.IsKeyDown((int) Key.W) ||
                     (Game.Instance.Joystick != null && Game.Instance.Joystick.GetButton(joyStickConfig["T"])))
            {
                direction = new Vector2(0, -1);
                castKey = Key.W;
            }
            else if (engine.IsKeyDown((int) Key.D) || engine.IsKeyDown((int)Key.Space) ||
                     (Game.Instance.Joystick != null && Game.Instance.Joystick.GetButton(joyStickConfig["C"])))
            {
                direction = new Vector2(1, 0);
                castKey = Key.D;
            }
            else if (engine.IsKeyDown((int) Key.S) ||
                     (Game.Instance.Joystick != null && Game.Instance.Joystick.GetButton(joyStickConfig["X"])))
            {
                direction = new Vector2(0, 1);
                castKey = Key.S;
            }
            else if (engine.IsKeyDown((int) Key.Q))
            {
                direction = new Vector2(-0.5f, -0.5f);
                castKey = Key.Q;
            }
            else if (engine.IsKeyDown((int) Key.E))
            {
                direction = new Vector2(0.5f, -0.5f);
                castKey = Key.E;
            }
            else if (engine.IsKeyDown((int) Key.Z))
            {
                direction = new Vector2(-0.5f, 0.5f);
                castKey = Key.Z;
            }
            else if (engine.IsKeyDown((int) Key.C))
            {
                direction = new Vector2(0.5f, 0.5f);
                castKey = Key.C;
            }
            else if (engine.IsKeyDown((int) Key.F))
            {
                spellManager.SwapSpell();
            }
            if (direction.Length >= 0.6)
            {
                Func<bool> castCheck;
                if (castKey != Key.Unknown)
                    castCheck = () => engine.IsKeyDown((int) castKey);
                else // for sure casted with joystick 
                    castCheck = () =>
                    {
                        var newDirection = new Vector2(
                            Game.Instance.Joystick.GetAxis(Game.Instance.JoyStickConfig["Rx"])/127f,
                            Game.Instance.Joystick.GetAxis(Game.Instance.JoyStickConfig["Ry"])/127f
                            );
                        // small change == still casting
                        return (direction - newDirection).Length < 0.1f;
                    };
                Shot(direction, castCheck);
            }
        }

        protected override void CreateHitBox()
        {
            var fwidth = Utils.FixBoxValue(width);
            var fheight = Utils.FixBoxValue(height);
            AddHitBox("mass", 0, (int)(fheight * 0.4f), fwidth, (int)(fheight * 0.6f));
        }

        private void ManageCollisions()
        {
            if (lastHitTimer > 0)
                lastHitTimer -= deltaTime;
            if (lastFloorChangeTimer > 0)
                lastFloorChangeTimer -= deltaTime;

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
                if (enemy != null && lastHitTimer <= 0)
                {
                    hittingEnemy = enemy;
                    lastHitTimer = MaxHitsPerTime;
                }
                if (collision.other.name.EndsWith("door") && lastFloorChangeTimer <= 0 &&
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
                        lastFloorChangeTimer = ChangeFloorDelay;
                    break;
                }
                if (collision.otherHitBox.StartsWith("wall"))
                {
                    x = (int) realLastPos.X;
                    y = (int) realLastPos.Y;
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
            disableRedTimer = 0.05f; // 50ms

            return base.GetDamage(enemy, damage);
        }

        private void UpdateEvent(object s)
        {
            if (Game.Instance.MainWindow != "game") return;
            if (!activated) return;
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
                if (disableRedTimer <= 0)
                {
                    redWindow.width = 0;
                    redWindow.height = 0;
                }
                else
                    disableRedTimer -= deltaTime;
            }
        }
    }
}