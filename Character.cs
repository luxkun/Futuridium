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
        // TODO: Energy regen
        public delegate void EnergyChangedEventHandler(object sender);

        public delegate void HPChangedEventHandler(object sender);

        public delegate void XPChangedEventHandler(object sender);

        private readonly List<Force> forces;

        public Dictionary<Type, List<Spell>> activeSpells;
        private int chosenSpellIndex = -1;
        private int forceCount;

        protected bool isCloseCombat = true;

        private int spellCounter;

        public Dictionary<Type, float> spellsCd;

        private float vx;
        private float vy;
        private long xp;

        public Character(string name, string formattedName, string characterName)
        {
            forces = new List<Force>();
            order = 6;

            Level0 = new Level();
            this.name = name;
            FormattedName = formattedName;
            CharacterName = characterName;

            OnUpdate += SpellsCdUpdateEvent;
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

        public bool IsAlive => Level.Hp > 0;

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
                chosenSpellIndex = value%Level.spellList.Count;
                ChosenSpell = Level.spellList[chosenSpellIndex];
            }
        }

        public Type ChosenSpell { get; private set; }

        public Spell LastCastedSpell { get; set; }

        public float BounceTime { get; set; } = 0.1f;

        public float BounceSpeed { get; set; } = 20;

        private void SpellsCdUpdateEvent(object sender)
        {
            foreach (var key in spellsCd.Keys.ToList())
            {
                if (spellsCd[key] > 0f)
                {
                    spellsCd[key] -= deltaTime;
                }
            }
        }

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
            /*foreach (var spell in activeSpells[spellType])
            {
                if (spell.OnCd)
                    return true;

            }*/
            var result = spellsCd[spellType] > 0;
            // if spell is alive and still casting
            if (!result && LastCastedSpell != null && LastCastedSpell.IsCasting)
                return true;
            return result;
        }

        public void LevelCheck()
        {
            if (LevelManager == null)
                LevelManager = new LevelManager(this, Level0);
            if (LevelManager.CheckLevelUp())
            {
                foreach (var type in Level.spellList)
                {
                    if (!activeSpells.ContainsKey(type))
                        activeSpells[type] = new List<Spell>();
                    if (!spellsCd.ContainsKey(type))
                        spellsCd[type] = 0f;
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
            spellsCd = new Dictionary<Type, float>();
            LevelCheck();
            SwapSpell();
        }

        protected void Shot(Vector2 direction, Func<bool> castCheck = null)
        {
            if (SpellOnCd(ChosenSpell))
                return;
            direction.Normalize();
            Debug.WriteLine("{0} is shotting to Direction: {1}", name, direction);
            var spell = ActivateSpell(ChosenSpell, castCheck);
            if (spell == null)
                return;

            var bullet = spell as Bullet;
            if (bullet != null)
            {
                bullet.Radius = Level.ShotRadius;
                bullet.Color = Color.LightGray;
            }
            var xMod = direction.X + 0.5f;
            var yMod = direction.Y + 0.5f;
            spell.xOffset = (int) (width*xMod);
            spell.yOffset = (int) (height*yMod);
            spell.Direction = direction;

            engine.SpawnObject(spell);
        }

        // TODO: swapspell cooldown
        protected void SwapSpell()
        {
            if (Level.spellList.Count == 0)
                return;
            ChosenSpellIndex++;
            Debug.WriteLine($"Chosen spell '{ChosenSpell}'.");
        }

        private Spell ActivateSpell(Type spellType, Func<bool> castCheck = null)
        {
            //if (SpellOnCd(spellType))
            //    throw new Exception("Spell '$(spellType)' is on cold down.");
            var spell = (Spell) Activator.CreateInstance(spellType);
            if (Level.Energy < spell.EnergyUsage)
                return null;
            activeSpells[spellType].Add(spell);
            // every spell should have different delay? same is ok for now since they're Energy bound
            LastCastedSpell = spell;
            spellsCd[spellType] = Level.ShotDelay;
            spell.CastCheck = castCheck;
            spell.OnDestroy += sender => DisactivateSpell(spell, false);
            spell.Owner = this;
            spell.name = spell.RoomConstricted
                ? ((Game) engine.objects["game"]).CurrentFloor.CurrentRoom.name
                : "" + name + "_spell" + spellCounter++;
            spell.order = order + 1;
            return spell;
        }

        public void DisactivateSpell(Spell spell, bool destroy = true)
        {
            if (!activeSpells[spell.GetType()].Contains(spell))
                return;
            //throw new Exception("Spell '$(spellType)' hasn't already been casted.");
            activeSpells[spell.GetType()].Remove(spell);
            if (destroy)
                spell.Destroy();
        }

        internal void HpChanged()
        {
            OnHpChanged?.Invoke(this);
            Console.WriteLine($"{name} hp changed to {Level.Hp}");
            if (Level.Hp <= 0)
                Destroy();
        }

        // spell hits enemy
        public virtual bool DoDamage(Spell spell, Character enemy, Collision collision)
        {
            var damage = new Damage(spell.Owner, enemy)
            {
                Direction = spell.Direction,
                DamageFunc = (Character ch0, Character ch1) => spell.CalculateDamage(ch1, 1f)
            };
            return DoDamage(enemy, damage);
        }

        public virtual bool DoDamage(Character enemy, Damage damage = null)
        {
            if (damage == null)
            {
                damage = new Damage(this, enemy) {DamageFunc = (Character ch0, Character ch1) => ch1.Level.attack};
            }

            enemy.GetDamage(this, damage);
            if (!enemy.IsAlive)
                Xp += enemy.Level.XpReward;
            return enemy.IsAlive;
        }

        protected virtual float GetDamage(Character enemy, Damage damage)
        {
            LevelCheck(); // could happen that the player kills the enemy before he fully spawn (before Start "starts")
            enemy.LevelCheck();
            var dmg = damage.Caculate(this, enemy);
            Level.Hp -= dmg; //enemy.level.attack;

            var floatingText = new FloatingText(this, "-" + dmg, "orange");
            engine.SpawnObject(
                floatingText.name, floatingText
                );

            // bounce back only if the damage is from a ranged enemy
            if (!damage.IsCloseCombat)
                BounceBack(damage.Direction);
            return Level.Hp;
        }

        private void BounceBack(Vector2 inverseDirection)
        {
            var force = new Force
            {
                Owner = this,
                Direction = inverseDirection,
                Step = BounceSpeed/BounceTime,
                DestroyTimer = BounceTime
            };
            forces.Add(force);
            engine.SpawnObject($"{name}_bouncebackforce_{forceCount++}", force);
        }
    }
}