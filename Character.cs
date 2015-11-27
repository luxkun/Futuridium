using System;
using System.Diagnostics;
using System.Drawing;
using Aiv.Engine;
using OpenTK;

namespace StupidAivGame
{
    public class Character : SpriteObject
    {
        protected Level _level;

        protected long _xp;
        protected int bulletCounter;
        public string characterName;

        public string formattedName;

        public Hud hud = null;
        protected bool isCloseCombat = true;
        protected int lastShot = 0;
        public Level level0;
        public LevelManager levelManager;

        private readonly Color shotColor = Color.GhostWhite;

        public bool useAnimations = false;

        public Character(string name, string formattedName, string characterName)
        {
            order = 6;

            level0 = new Level();
            this.name = name;
            this.formattedName = formattedName;
            this.characterName = characterName;
        }

        public long xp
        {
            get { return _xp; }
            set
            {
                _xp = value;
                if (hud != null)
                {
                    hud.UpdateXPBar();
                }
                LevelCheck();
            }
        }

        public Level level
        {
            get { return _level; }
            set
            {
                _level = value;
                if (hud != null)
                {
                    hud.UpdateXPBar();
                    hud.UpdateHPBar();
                }
            }
        }

        public bool isAlive
        {
            get { return level.hp > 0; }
        }

        public void LevelCheck()
        {
            if (levelManager == null)
                levelManager = new LevelManager(this, level0);
            if (levelManager.CheckLevelUp() && this as Player != null && level.level > 0)
            {
                engine.PlaySound("levelup_sound");
            }
        }

        // not started before update??
        public override void Start()
        {
            LevelCheck();
        }


        public void Shot(Vector2 direction)
        {
            direction.Normalize();
            Debug.WriteLine("{0} is shotting to direction: {1}", name, direction);
            var bullet = new Bullet(this, direction);
            var xMod = direction.X + 0.5f;
            var yMod = direction.Y + 0.5f;
            bullet.x = x + (int) (width*xMod);
            bullet.y = y + (int) (height*yMod);

            bullet.radius = level.shotRadius;
            bullet.color = shotColor;
            engine.SpawnObject(
                string.Format("{2}_bullet_{0}_{1}", name, bulletCounter,
                    ((Game) engine.objects["game"]).currentFloor.currentRoom.name), bullet
                );
            bulletCounter++;
        }

        public virtual int GetDamage(Character enemy, Func<Character, Character, int> damageFunc)
        {
            LevelCheck(); // could happen that the player kills the enemy before he fully spawn (before Start "starts")
            enemy.LevelCheck();
            var dmg = damageFunc(this, enemy);
            level.hp -= dmg; //enemy.level.attack;
            if (hud != null)
                hud.UpdateHPBar();
            var floatingText = new FloatingText(this, "-" + dmg, "orange");
            engine.SpawnObject(
                floatingText.name, floatingText
                );
            return level.hp;
        }
    }
}