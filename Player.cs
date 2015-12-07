using System;
using System.Diagnostics;
using System.Drawing;
using Aiv.Engine;
using OpenTK;
using OpenTK.Input;

namespace Futuridium
{
    public class Player : Character
    {
        private const float maxHitsPerTime = 1f; // 500ms immunity after gets hit
        private const float changeFloorDelay = 2f;
        private float disableRedTimer;
        private bool initHUD;
        private float lastFloorChangeTimer;
        private float lastHitTimer;
        private Vector2 lastPosition;
        public string realName = "Rek";
        protected RectangleObject redWindow;
        private int spawnedOrbs;
        private Vector2 virtPos;

        //private List<int> pressedJoyButtons;
        public Player() : base("player", "Player", "player")
        {
            order = 7;

            Level0.maxHP = 30000;
            Level0.maxEnergy = 100;
            Level0.speed = 150;
            Level0.shotDelay = 1.5f;
            Level0.attack = 25;
            Level0.neededXP = 100;
            Level0.shotSpeed = 200f;
            Level0.shotRange = 400;
            Level0.shotRadius = 8;
            isCloseCombat = false;


            //pressedJoyButtons = new List<int> ();
        }

        public override void Start()
        {
            int fwidth = Utils.FixBoxValue(width);
            int fheight = Utils.FixBoxValue(height);
            AddHitBox("player", 0, fheight / 3, fwidth, (int) (fheight * 2f/3));

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
                virtPos.X += Level.speed*deltaTime;
            }
            if (engine.IsKeyDown((int) Key.Left))
            {
                virtPos.X -= Level.speed*deltaTime;
            }
            if (engine.IsKeyDown((int) Key.Up))
            {
                virtPos.Y -= Level.speed*deltaTime;
            }
            if (engine.IsKeyDown((int) Key.Down))
            {
                virtPos.Y += Level.speed*deltaTime;
            }
            // setup shot type
            if (engine.IsKeyDown((int)Key.F))
                shotStatus = shotStatus == ShotStatus.DRIVEX ? ShotStatus.BULLET : ShotStatus.DRIVEX;

            // joystick controls
            var game = (Game) engine.objects["game"];
            if (game.Joystick != null)
            {
                var moveDirection = new Vector2(
                    game.Joystick.GetAxis(game.JoyStickConfig["Lx"])/127f,
                    game.Joystick.GetAxis(game.JoyStickConfig["Ly"])/127f
                    );
                if (moveDirection.Length > 0.2)
                {
                    virtPos.X += Level.speed*moveDirection.X*deltaTime;
                    virtPos.Y += Level.speed*moveDirection.Y*deltaTime;
                }
                if (game.Joystick.GetButton(game.JoyStickConfig["RT"])) 
                    shotStatus = shotStatus == ShotStatus.DRIVEX ? ShotStatus.BULLET : ShotStatus.DRIVEX;
            }

            if (Math.Abs(virtPos.X) > 1)
            {
                x += (int) virtPos.X;
                virtPos.X -= (int) virtPos.X;
            }
            if (Math.Abs(virtPos.Y) > 1)
            {
                y += (int) virtPos.Y;
                virtPos.Y -= (int) virtPos.Y;
            }
        }

        private void ManageShot()
        {
            if (lastShotTimer > 0)
                lastShotTimer -= deltaTime;

            // if is shotting bullet check lastShot
            if (shotStatus != ShotStatus.BULLET || lastShotTimer <= 0)
            {
                var game = (Game) engine.objects["game"];

                var direction = new Vector2();
                if (game.Joystick != null)
                {
                    Debug.Write("Shotting axis on joystick: ");
                    for (var x = 0; x < 6; x++)
                        Debug.Write($"{x}: {game.Joystick.GetAxis(x)} ; ");
                    Debug.WriteLine("");
                    direction = new Vector2(
                        game.Joystick.GetAxis(game.JoyStickConfig["Rx"])/127f,
                        game.Joystick.GetAxis(game.JoyStickConfig["Ry"])/127f
                        );
                }

                var joyStickConfig = game.JoyStickConfig;

                if (engine.IsKeyDown((int) Key.A) ||
                    (game.Joystick != null && game.Joystick.GetButton(joyStickConfig["S"])))
                    direction = new Vector2(-1, 0);
                else if (engine.IsKeyDown((int) Key.W) ||
                         (game.Joystick != null && game.Joystick.GetButton(joyStickConfig["T"])))
                    direction = new Vector2(0, -1);
                else if (engine.IsKeyDown((int) Key.D) ||
                         (game.Joystick != null && game.Joystick.GetButton(joyStickConfig["C"])))
                    direction = new Vector2(1, 0);
                else if (engine.IsKeyDown((int) Key.S) ||
                         (game.Joystick != null && game.Joystick.GetButton(joyStickConfig["X"])))
                    direction = new Vector2(0, 1);
                else if (engine.IsKeyDown((int) Key.Q))
                    direction = new Vector2(-0.5f, -0.5f);
                else if (engine.IsKeyDown((int) Key.E))
                    direction = new Vector2(0.5f, -0.5f);
                else if (engine.IsKeyDown((int) Key.Z))
                    direction = new Vector2(-0.5f, 0.5f);
                else if (engine.IsKeyDown((int) Key.C))
                    direction = new Vector2(0.5f, 0.5f);
                if (direction.Length >= 0.6)
                {
                    Shot(direction);
                    lastShotTimer = Level.shotDelay;
                }
            }
        }

        private void SpawnOrb()
        {
            if (spawnedOrbs == 0)
            {
                spawnedOrbs++;
                Debug.WriteLine("Spawning orb.");
                var orb = new Orb(this);
                orb.radius = 8;
                orb.color = Color.Blue;
                engine.SpawnObject("orb", orb);
            }
        }

        private void ManageCollisions()
        {
            if (lastHitTimer > 0)
                lastHitTimer -= deltaTime;
            if (lastFloorChangeTimer > 0)
                lastFloorChangeTimer -= deltaTime;
        
            if (lastHitTimer <= 0)
            {
                var collisions = CheckCollisions();
                if (collisions.Count > 0)
                    Debug.WriteLine("Character '{0}' collides with n.{1}", name, collisions.Count);
                foreach (var collision in collisions)
                {
                    Debug.WriteLine("Character '{0}' touches '{1}'", name, collision.other.name);
                    var game = (Game) engine.objects["game"];
                    if (collision.other as Enemy != null)
                    {
                        ((Enemy) collision.other).DoDamage(this);

                        lastHitTimer = maxHitsPerTime;
                    }
                    else if (collision.otherHitBox.StartsWith("wall"))
                    {
                        x = (int) lastPosition.X;
                        y = (int) lastPosition.Y;
                    }
                    else if (collision.other.name.EndsWith("door") && lastFloorChangeTimer <= 0 && collision.other.enabled)
                    {
                        x = (int) lastPosition.X;
                        y = (int) lastPosition.Y;
                        Debug.WriteLine("About to change room to: " + collision.other.name);
                        var changedFloor = false;
                        if (collision.other.name.EndsWith("top_door"))
                            changedFloor = game.CurrentFloor.OpenRoom(game.CurrentFloor.CurrentRoom.top);
                        else if (collision.other.name.EndsWith("left_door"))
                            changedFloor = game.CurrentFloor.OpenRoom(game.CurrentFloor.CurrentRoom.left);
                        else if (collision.other.name.EndsWith("bottom_door"))
                            changedFloor = game.CurrentFloor.OpenRoom(game.CurrentFloor.CurrentRoom.bottom);
                        else if (collision.other.name.EndsWith("right_door"))
                            changedFloor = game.CurrentFloor.OpenRoom(game.CurrentFloor.CurrentRoom.right);
                        if (changedFloor)
                            lastFloorChangeTimer = changeFloorDelay;
                    }
                    else if (collision.other.name.StartsWith("escape_floor_"))
                    {
                        game.InitializeNewFloor();
                        collision.other.Destroy();
                    }
                }
            }
        }

        protected override int GetDamage(Character enemy, Damage damage)
        {
            redWindow.width = engine.width;
            redWindow.height = engine.height;
            Debug.WriteLine("Player got damaged.");
            disableRedTimer = 0.05f; // 50ms

            return base.GetDamage(enemy, damage);
        }

        public override void Update()
        {
            base.Update();
            if (((Game) engine.objects["game"]).MainWindow == "game")
            {
                if (!initHUD)
                {
                    initHUD = true;
                    Hud = (Hud) engine.objects["hud"];
                    Hud.UpdateHPBar();
                    Hud.UpdateXPBar();
                    Hud.UpdateEnergyBar();
                    OnHpChanged += (Object sender) => { Hud.UpdateHPBar(); };
                    OnEnergyChanged += (Object sender) => { Hud.UpdateEnergyBar(); };
                    OnXpChanged += (Object sender) => { Hud.UpdateXPBar(); };
                }
                ManageControls();
                ManageShot();
                ManageCollisions();
                if (engine.IsKeyDown ((int) Key.O))
                    SpawnOrb();

                if (redWindow.width != 0)
                {
                    if (disableRedTimer < 0)
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
}