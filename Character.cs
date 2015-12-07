using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Aiv.Engine;
using Futuridium.Spells;
using OpenTK;

namespace Futuridium
{
    public class Character : SpriteObject
    {
        public delegate void EnergyChangedEventHandler(object sender);

        public delegate void HPChangedEventHandler(object sender);

        public delegate void XPChangedEventHandler(object sender);

        public enum ShotStatus
        {
            BULLET,
            DRIVEX
        }

        private readonly Color bulletColor = Color.GhostWhite;

        private readonly Dictionary<ShotStatus, Type> shotMap = new Dictionary<ShotStatus, Type>
        {
            {ShotStatus.DRIVEX, typeof (DriveX)}
        };

        public Dictionary<Type, Spell> activeSpells;
        private int bulletCounter;

        private readonly List<Force> forces;

        protected bool isCloseCombat = true;
        protected float lastShotTimer = 0;

        protected ShotStatus shotStatus = ShotStatus.BULLET;
        private int spellCounter;
        private long xp;

        public Character(string name, string formattedName, string characterName)
        {
            forces = new List<Force>();
            order = 6;

            Level0 = new Level();
            this.name = name;
            FormattedName = formattedName;
            CharacterName = characterName;
        }

        public long Xp
        {
            get { return xp; }
            set
            {
                xp = value;
                XpChanged();
                LevelCheck();
            }
        }

        public bool IsAlive => Level.hp > 0;

        public string CharacterName { get; set; }

        public string FormattedName { get; set; }

        public Hud Hud { get; set; } = null;

        public Level Level { get; set; }

        public Level Level0 { get; set; }

        public LevelManager LevelManager { get; set; }

        public bool UseAnimations { get; set; } = false;

        public event HPChangedEventHandler OnHpChanged;
        public event EnergyChangedEventHandler OnEnergyChanged;

        internal void EnergyChanged()
        {
            OnEnergyChanged?.Invoke(this);
        }

        public event XPChangedEventHandler OnXpChanged;

        internal void XpChanged()
        {
            OnXpChanged?.Invoke(this);
        }

        public void LevelCheck()
        {
            if (LevelManager == null)
                LevelManager = new LevelManager(this, Level0);
            if (LevelManager.CheckLevelUp() && this is Player && Level.level > 0)
            {
                //engine.PlaySound("levelup_sound");
            }
        }

        // not started before update??
        public override void Start()
        {
            activeSpells = new Dictionary<Type, Spell>();
            LevelCheck();
        }


        protected void Shot(Vector2 direction)
        {
            direction.Normalize();
            Debug.WriteLine("{0} is shotting to Direction: {1}", name, direction);
            if (shotStatus == ShotStatus.BULLET)
            {
                var xMod = direction.X + 0.5f;
                var yMod = direction.Y + 0.5f;
                var bullet = new Bullet(this, direction)
                {
                    x = x + (int) (width*xMod),
                    y = y + (int) (height*yMod),
                    radius = Level.shotRadius,
                    color = bulletColor
                };
                engine.SpawnObject(
                    string.Format("{2}_bullet_{0}_{1}", name, bulletCounter,
                        ((Game) engine.objects["game"]).CurrentFloor.CurrentRoom.name), bullet
                    );
                bulletCounter++;
            }
            else if (!activeSpells.ContainsKey(shotMap[shotStatus]))
            {
                ActivateSpell(shotMap[shotStatus]);
            }
        }

        protected Spell ActivateSpell(Type spellType)
        {
            if (activeSpells.ContainsKey(shotMap[shotStatus]))
                throw new Exception("Spell '$(spellType)' has already been casted.");
            var spell = (Spell) Activator.CreateInstance(spellType);
            activeSpells[spellType] = spell;
            spell.Owner = this;
            spell.name = name + "_spell" + spellCounter++;
            if (spell.EnergyUsage <= Level.energy) // cast cost
                Level.energy -= spell.EnergyUsage;
            engine.SpawnObject(spell);
            return spell;
        }

        public void DisactivateSpell(Type spellType)
        {
            if (!activeSpells.ContainsKey(shotMap[shotStatus]))
                throw new Exception("Spell '$(spellType)' hasn't already been casted.");
            activeSpells[spellType].Destroy();
            activeSpells.Remove(spellType);
            throw new Exception("Spell '$(spellType)' hasn't already been casted.");
        }

        internal void HpChanged()
        {
            OnHpChanged?.Invoke(this);
            Console.WriteLine($"{name} hp changed to {Level.hp}");
            if (Level.hp <= 0)
                Destroy();
        }

        // bullet hits enemy
        public virtual bool DoDamage(Bullet bullet, Character enemy, Collision collision)
        {
            var damage = new Damage(bullet.Owner, enemy)
            {
                Direction = bullet.Direction
            };
            damage.DamageFunc =
                (Character ch0, Character ch1) => (int) (ch0.Level.attack*((double) bullet.Speed/bullet.StartingSpeed));
            return DoDamage(enemy, damage);
        }

        public virtual bool DoDamage(Character enemy, Damage damage = null)
        {
            if (damage == null)
            {
                damage = new Damage(this, enemy);
                damage.DamageFunc = (Character ch0, Character ch1) => ch1.Level.attack;
            }

            enemy.GetDamage(this, damage);
            if (!enemy.IsAlive)
                Xp += enemy.Level.xpReward;
            return enemy.IsAlive;
        }

        protected virtual int GetDamage(Character enemy, Damage damage)
        {
            LevelCheck(); // could happen that the player kills the enemy before he fully spawn (before Start "starts")
            enemy.LevelCheck();
            var dmg = damage.Caculate(this, enemy);
            Level.hp -= dmg; //enemy.level.attack;

            var floatingText = new FloatingText(this, "-" + dmg, "orange");
            engine.SpawnObject(
                floatingText.name, floatingText
                );

            // bounce back only if the damage is from a ranged enemy
            if (!damage.IsCloseCombat)
                BounceBack(damage.InverseDirection);
            return Level.hp;
        }

        private void BounceBack(Vector2 inverseDirection)
        {
            const float destroyTimer = 0.2f;
            const float step = 50/destroyTimer;
            forces.Add(new Force
            {
                Owner = this,
                Direction = inverseDirection,
                BreakChain = true,
                Step = step,
                DestroyTimer = destroyTimer
            });
        }
    }
}