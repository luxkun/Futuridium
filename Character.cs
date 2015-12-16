using System;
using System.Collections.Generic;
using System.Diagnostics;
using Aiv.Engine;
using Futuridium.Spells;
using OpenTK;

namespace Futuridium
{
    public class Character : SpriteObject
    {
        // TODO: Energy regen? per room? per second?
        public delegate void EnergyChangedEventHandler(object sender);

        public delegate void HPChangedEventHandler(object sender);

        public delegate void XPChangedEventHandler(object sender);

        public delegate void LevelupEventHandler(object sender);

        protected float delayBeforeActivation = 0.5f;

        private readonly List<Force> forces;
        protected bool activated;

        private int forceCount;

        private Level level;
        public SpellManager spellManager;
        private float timeBeforeActivation;

        private float vx;
        private float vy;
        private long xp;
        private bool startedActivationTimer;

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
                vx = value;
                if (Math.Abs(vx) > 1)
                {
                    x += (int) vx;
                    vx -= (int) vx;
                }
            }
        }


        public float Vy
        {
            get { return vy; }
            set
            {
                vy = value;
                if (Math.Abs(vy) > 1)
                {
                    y += (int) vy;
                    vy -= (int) vy;
                }
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

        public Level Level
        {
            get { return level; }
            set
            {
                level = value;

                spellManager?.UpdateSpells();
            }
        }

        public Level Level0 { get; set; }

        public LevelManager LevelManager { get; set; }

        public bool UseAnimations { get; set; } = false;


        public float BounceTime { get; set; } = 0.1f;

        public float BounceSpeed { get; set; } = 20;

        public event HPChangedEventHandler OnHpChanged;
        public event EnergyChangedEventHandler OnEnergyChanged;
        public event LevelupEventHandler OnLevelup;

        public void EnergyChanged()
        {
            OnEnergyChanged?.Invoke(this);
        }

        public event XPChangedEventHandler OnXpChanged;

        public void XpChanged()
        {
            OnXpChanged?.Invoke(this);
        }

        public void LevelCheck()
        {
            if (LevelManager == null)
                LevelManager = new LevelManager(this, Level0);
            if (LevelManager.CheckLevelUp())
            {
                OnLevelup?.Invoke(this);
                //if (this is Player && Level.level > 0)
                //{
                //    //engine.PlaySound("levelup_sound");
                //}
            }
        }

        public override void Start()
        {
            LevelCheck();
            spellManager = new SpellManager(this);
            engine.SpawnObject(spellManager);
        }

        public override void Update()
        {
            if (!activated)
            {
                if (!startedActivationTimer) { 
                    timeBeforeActivation = delayBeforeActivation;
                    startedActivationTimer = true;
                }
                else
                {
                    if (timeBeforeActivation > 0)
                        timeBeforeActivation -= deltaTime;
                    if (timeBeforeActivation <= 0)
                    {
                        activated = true;
                        CreateHitBox();
                    }
                }
            }
        }

        protected virtual void CreateHitBox()
        {
            var mod = 0.2f;
            if (height < 50)
                mod = 0f;
            AddHitBox("mass", 0, (int)(height * mod), width, (int) (height * (1 - mod)));
        }

        protected void Shot(Vector2 direction, Func<bool> castCheck = null)
        {
            direction.Normalize();
            Debug.WriteLine("{0} is shotting to Direction: {1}", name, direction);
            var spell = spellManager.ActivateSpell(castCheck: castCheck);
            if (spell == null)
                return;

            float xMod = (direction.X + 1f) / 2f;
            float yMod = (direction.Y + 1f) / 2f;
            if (Math.Abs(direction.X) / Math.Abs(direction.Y) > (float)width/height)
                xMod = direction.X > 0 ? 1f : 0f;
            else
                yMod = direction.Y > 0 ? 1f : 0f;
            spell.xOffset = (int) (width*xMod);
            spell.yOffset = (int) (height*yMod);
            spell.Direction = direction;

            engine.SpawnObject(spell);
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
                DamageFunc = (Character ch0, Character ch1) => spell.CalculateDamage(ch1, 1f),
                Spell = spell
            };
            return DoDamage(enemy, damage);
        }

        public virtual bool DoDamage(Character enemy, Damage damage = null)
        {
            if (damage == null)
            {
                // simple (closecombat usually) damage
                damage = new Damage(this, enemy) {DamageFunc = (Character ch0, Character ch1) => ch1.Level.Attack};
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
            Level.Hp -= dmg;

            var floatingText = new FloatingText(this, "-" + (int) dmg, "orange", 8 + (int)(dmg / 15));
            engine.SpawnObject(
                floatingText.name, floatingText
                );

            // bounce back only if the damage is from a ranged enemy
            if (damage.Spell != null && damage.Spell.KnockBack > 0f)
                BounceBack(damage);
            return Level.Hp;
        }

        private void BounceBack(Damage damage)
        {
            var force = new Force
            {
                Owner = this,
                Direction = damage.Direction,
                Step = BounceSpeed/BounceTime,
                DestroyTimer = BounceTime * damage.Spell.KnockBack
            };
            forces.Add(force);
            engine.SpawnObject($"{name}_bouncebackforce_{forceCount++}", force);
        }
    }
}