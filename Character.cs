using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
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

        private readonly List<Force> forces;

        public Dictionary<Type, List<Spell>> activeSpells;
        private int forceCount;

        protected bool isCloseCombat = true;

        private int spellCounter;

        private float vx;
        private float vy;
        private long xp;
        private int chosenSpellIndex = -1;

        public Character(string name, string formattedName, string characterName)
        {
            forces = new List<Force>();
            order = 6;

            Level0 = new Level();
            this.name = name;
            FormattedName = formattedName;
            CharacterName = characterName;
        }

        public float Vx
        {
            get { return vx; }
            set
            {
                if (Math.Abs(value) > 1)
                {
                    x += (int) value;
                    value -= (int) value;
                }
                vx = value;
            }
        }


        public float Vy
        {
            get { return vy; }
            set
            {
                if (Math.Abs(value) > 1)
                {
                    y += (int) value;
                    value -= (int) value;
                }
                vy = value;
            }
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

        protected int ChosenSpellIndex
        {
            get { return chosenSpellIndex; }
            set
            {
                chosenSpellIndex = value % Level.spellList.Count;
                ChosenSpell = Level.spellList[chosenSpellIndex];
            }
        }

        public Type ChosenSpell { get; private set; }

        public event HPChangedEventHandler OnHpChanged;
        public event EnergyChangedEventHandler OnEnergyChanged;

        public void EnergyChanged()
        {
            OnEnergyChanged?.Invoke(this);
        }

        public event XPChangedEventHandler OnXpChanged;

        public void XpChanged()
        {
            OnXpChanged?.Invoke(this);
        }

        protected bool SpellOnCd(Type spellType)
        {
            foreach (var spell in activeSpells[spellType])
            {
                if (spell.OnCd)
                    return true;

            }
            return false;
        }

        public void LevelCheck()
        {
            if (LevelManager == null)
                LevelManager = new LevelManager(this, Level0);
            if (LevelManager.CheckLevelUp())
            {
                foreach (var pair in Level.spellList)
                {
                    if (!activeSpells.ContainsKey(pair))
                        activeSpells[pair] = new List<Spell>();
                }
                if (this is Player && Level.level > 0)
                {
                    //engine.PlaySound("levelup_sound");
                }
            }
        }

        public override void Start()
        {
            activeSpells = new Dictionary<Type, List<Spell>>();
            LevelCheck();
            SwapSpell();
        }

        protected void Shot(Vector2 direction)
        {
            if (SpellOnCd(ChosenSpell))
                return;
            direction.Normalize();
            Debug.WriteLine("{0} is shotting to Direction: {1}", name, direction);
            var spell = ActivateSpell(ChosenSpell);
            spell.Direction = direction;

            var bullet = spell as Bullet;
            if (bullet != null)
            {
                var xMod = direction.X + 0.5f;
                var yMod = direction.Y + 0.5f;
                bullet.X = x + (int) (width*xMod);
                bullet.Y = y + (int) (height*yMod);
                bullet.Radius = Level.shotRadius;
                bullet.Color = Color.WhiteSmoke;
                bullet.CdTimer = Level.shotDelay;
            }
            engine.SpawnObject(spell);
        }

        protected void SwapSpell()
        {
            if (Level.spellList.Count == 0)
                return;
            ChosenSpellIndex++;
        }

        private Spell ActivateSpell(Type spellType)
        {
            //if (SpellOnCd(spellType))
            //    throw new Exception("Spell '$(spellType)' is on cold down.");
            var spell = (Spell) Activator.CreateInstance(spellType);
            if (Level.energy < spell.EnergyUsage)
                return null;
            activeSpells[spellType].Add(spell);
            spell.Owner = this;
            spell.name = name + "_spell" + spellCounter++;
            return spell;
        }

        public void DisactivateSpell(Spell spell)
        {
            if (!activeSpells[spell.GetType()].Contains(spell))
                throw new Exception("Spell '$(spellType)' hasn't already been casted.");
            activeSpells[spell.GetType()].Remove(spell);
            spell.Destroy();
            throw new Exception("Spell '$(spellType)' hasn't already been casted.");
        }

        internal void HpChanged()
        {
            OnHpChanged?.Invoke(this);
            Console.WriteLine($"{name} hp changed to {Level.hp}");
            if (Level.hp <= 0)
                Destroy();
        }

        // spell hits enemy
        public virtual bool DoDamage(Spell spell, Character enemy, Collision collision)
        {
            var damage = new Damage(spell.Owner, enemy)
            {
                Direction = spell.Direction
            };
            damage.DamageFunc =
                (Character ch0, Character ch1) => (int) (ch0.Level.attack*((double) spell.Speed/spell.StartingSpeed));
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
                BounceBack(damage.Direction);
            return Level.hp;
        }

        private void BounceBack(Vector2 inverseDirection)
        {
            const float destroyTimer = 0.1f;
            const float step = 20/destroyTimer;
            var force = new Force
            {
                Owner = this,
                Direction = inverseDirection,
                Step = step,
                DestroyTimer = destroyTimer
            };
            forces.Add(force);
            engine.SpawnObject($"{name}_bouncebackforce_{forceCount++}", force);
        }
    }
}