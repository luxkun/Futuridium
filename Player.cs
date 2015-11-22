using System;
using System.Drawing;
using System.Threading;
using Aiv.Engine;
using OpenTK;
using OpenTK.Input;

namespace StupidAivGame
{
    public class Player : Character
    {
        private const int maxHitsPerTime = 1000; // 500ms immunity after gets hit
        private readonly int changeFloorDelay = 2000;
        private bool initHUD;
        private int lastFloorChange;
        private int lastHit;
        private Vector2 lastPosition;
        public string realName = "Rek";
        protected RectangleObject redWindow;
        private int spawnedOrbs;
        private Vector2 virtPos;
        //private List<int> pressedJoyButtons;
        public Player() : base("player", "Player", "player")
        {
            order = 7;

            level0.maxHP = 200;
            level0.speed = 25;
            level0.shotDelay = 1500;
            level0.attack = 25;
            level0.neededXP = 100;
            level0.shotSpeed = 20;
            level0.shotRange = 400;
            level0.shotRadius = 8;
            isCloseCombat = false;


            //pressedJoyButtons = new List<int> ();
        }

        public override void Start()
        {
            AddHitBox("player", 0, 0, width, height);

            redWindow = new RectangleObject();
            redWindow.width = 0;
            redWindow.height = 0;
            redWindow.name = "redWindow";
            redWindow.color = Color.Red;
            redWindow.x = 0;
            redWindow.y = 0;
            redWindow.fill = true;
            redWindow.order = 9;
            engine.SpawnObject("redWindow", redWindow);

            base.Start();
        }

        private void ManageControls()
        {
            // keyboard controls

            // why need casting?
            lastPosition = new Vector2(x, y);
            if (engine.IsKeyDown((int) Key.Right))
            {
                virtPos.X += level.speed*(deltaTicks/100f);
            }
            if (engine.IsKeyDown((int) Key.Left))
            {
                virtPos.X -= level.speed*(deltaTicks/100f);
            }
            if (engine.IsKeyDown((int) Key.Up))
            {
                virtPos.Y -= level.speed*(deltaTicks/100f);
            }
            if (engine.IsKeyDown((int) Key.Down))
            {
                virtPos.Y += level.speed*(deltaTicks/100f);
            }

            // joystick controls
			Game game = (Game)engine.objects["game"];
			if (game.joystick != null)
            {
				var moveDirection = new Vector2(
					game.joystick.GetAxis(game.joyStickConfig["Lx"]) / 127f, 
					game.joystick.GetAxis(game.joyStickConfig["Ly"]) / 127f
				);
                if (moveDirection.Length > 0.2)
                {
                    virtPos.X += level.speed*moveDirection.X*(deltaTicks/100f);
                    virtPos.Y += level.speed*moveDirection.Y*(deltaTicks/100f);
                }
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
            if (lastShot > 0)
                lastShot -= deltaTicks;

            if (lastShot <= 0)
            {
                // TODO: use vector instead of int/hardcoded direction
                // spawn a new bullet in a choosen direction
				// 0 left; 1 top; 2 right; 3 bottom; 4: top-left; 5: top-right; 6: bottom-left; 7: bottom-right
				Game game = (Game)engine.objects["game"];

                var direction = new Vector2();
				if (game.joystick != null)
					direction = new Vector2(
						game.joystick.GetAxis(game.joyStickConfig["Rx"]) / 127f, 
						game.joystick.GetAxis(game.joyStickConfig["Ry"]) / 127f
					);
				
				var joyStickConfig = game.joyStickConfig;

                if (engine.IsKeyDown((int) Key.A) ||
					(game.joystick != null && game.joystick.GetButton(joyStickConfig["S"])))
                    direction = new Vector2(-1, 0);
				else if (engine.IsKeyDown((int) Key.W) ||
					(game.joystick != null && game.joystick.GetButton(joyStickConfig["T"])))
                    direction = new Vector2(0, -1);
				else if (engine.IsKeyDown((int) Key.D) ||
					(game.joystick != null && game.joystick.GetButton(joyStickConfig["C"])))
                    direction = new Vector2(1, 0);
				else if (engine.IsKeyDown((int) Key.S) ||
					(game.joystick != null && game.joystick.GetButton(joyStickConfig["X"])))
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
                    lastShot = level.shotDelay;
                }
            }
        }

        private void SpawnOrb()
        {
            if (spawnedOrbs == 0)
            {
                spawnedOrbs++;
                Console.WriteLine("Spawning orb.");
                var orb = new Orb(this);
                orb.radius = 8;
                orb.color = Color.Blue;
                engine.SpawnObject("orb", orb);
            }
        }

        private void ManageCollisions()
        {
            if (lastHit > 0)
                lastHit -= deltaTicks;
            if (lastFloorChange > 0)
                lastFloorChange -= deltaTicks;

            if (lastHit <= 0)
            {
                var collisions = CheckCollisions();
                if (collisions.Count > 0)
                    Console.WriteLine("Character '{0}' collides with n.{1}", name, collisions.Count);
                foreach (var collision in collisions)
                {
                    Console.WriteLine("Character '{0}' touches '{1}'", name, collision.other.name);
                    var game = (Game) engine.objects["game"];
                    if (collision.other.name.StartsWith("enemy"))
                    {
                        var enemy = collision.other as Enemy;
                        game.Hits(enemy, this, collision, null);

                        Console.WriteLine("{0}, {1}", level.hp, isAlive);
                        if (!isAlive)
                        {
                            Destroy();
                        }

                        lastHit = maxHitsPerTime;

                        break;
                    }
                    if (collision.other.name.EndsWith("block"))
                    {
                        x = (int) lastPosition.X;
                        y = (int) lastPosition.Y;
                    }
                    else if (collision.other.name.EndsWith("door") && lastFloorChange <= 0 && collision.other.enabled)
                    {
                        x = (int) lastPosition.X;
                        y = (int) lastPosition.Y;
                        Console.WriteLine("About to change room to: " + collision.other.name);
                        var changedFloor = false;
                        if (collision.other.name.EndsWith("top_door"))
                            changedFloor = game.currentFloor.OpenRoom(game.currentFloor.currentRoom.top);
                        else if (collision.other.name.EndsWith("left_door"))
                            changedFloor = game.currentFloor.OpenRoom(game.currentFloor.currentRoom.left);
                        else if (collision.other.name.EndsWith("bottom_door"))
                            changedFloor = game.currentFloor.OpenRoom(game.currentFloor.currentRoom.bottom);
                        else if (collision.other.name.EndsWith("right_door"))
                            changedFloor = game.currentFloor.OpenRoom(game.currentFloor.currentRoom.right);
                        if (changedFloor)
                            lastFloorChange = changeFloorDelay;
                    }
                    else if (collision.other.name.StartsWith("escape_floor_"))
                    {
                        game.InitializeNewFloor();
                        collision.other.Destroy();
                    }
                }
            }
        }

        public override int GetDamage(Character enemy, Func<Character, Character, int> damageFunc)
        {
            redWindow.width = engine.width;
            redWindow.height = engine.height;
            Console.WriteLine("Player got damaged.");
            var thr = new Thread(
                () =>
                {
                    Thread.Sleep(50);

                    redWindow.width = 0;
                    redWindow.height = 0;
                }
                );
            thr.Start();

            return base.GetDamage(enemy, damageFunc);
        }

        public override void Update()
        {
            base.Update();
            if (((Game) engine.objects["game"]).mainWindow == "game")
            {
                if (!initHUD)
                {
                    initHUD = true;
                    hud = ((Hud) engine.objects["hud"]);
                    hud.UpdateHPBar();
                    hud.UpdateXPBar();
                }
                ManageControls();
                ManageShot();
                ManageCollisions();
                //if (this.engine.IsKeyDown (Keys.O))
                SpawnOrb();
            }
        }
    }
}