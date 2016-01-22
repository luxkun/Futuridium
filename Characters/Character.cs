using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Aiv.Engine;
using Futuridium.Game;
using Futuridium.Items;
using Futuridium.Spells;
using Futuridium.UI;
using OpenTK;

namespace Futuridium.Characters
{
    public class Character : SpriteObject
    {
        public delegate void EnergyChangedEventHandler(object sender);

        public delegate void HPChangedEventHandler(object sender);

        public delegate void LevelupEventHandler(object sender);

        public delegate void RoomChangedHandler(object sender);

        public delegate void XPChangedEventHandler(object sender, long delta);
        public delegate void DamageTakenEventHandler(object sender, float delta);

        public enum MovingState
        {
            Inactive = 0,
            Idle = 1,
            MovingLeft = 2,
            MovingRight = 3,
            MovingDown = 4,
            MovingUp = 5
        }

        private readonly List<Force> forces;

        private MovingState _movingState;

        protected float delayBeforeActivation = 1.5f;

        private int forceCount;

        private Level level;
        public SpellManager SpellManager;
        private bool startedActivationTimer;

        private long xp;

        public Character(string name, string formattedName, string characterName, int width, int height)
            : base(width, height, true)
        {
            forces = new List<Force>();

            HitBoxOffSet = new Dictionary<MovingState, Vector2>();
            HitBoxSize = new Dictionary<MovingState, Vector2>();
            foreach (MovingState state in Enum.GetValues(typeof (MovingState)))
            {
                HitBoxOffSet[state] = new Vector2();
                HitBoxSize[state] = new Vector2();
            }

            Order = 8;

            Level0 = new Level();
            Name = name;
            FormattedName = formattedName;
            CharacterName = characterName;

            DropManager = new DropManager(this);

            OnDestroy += DestroyEvent;
        }

        public Dictionary<MovingState, Vector2> HitBoxOffSet { get; set; }
        public Dictionary<MovingState, Vector2> HitBoxSize { get; set; }
        public float RealSpeed { get; set; }

        public long Xp
        {
            get { return xp; }
            set
            {
                var delta = value - xp;
                xp = value;
                XpChanged(delta);
                LevelCheck();
            }
        }

        public bool IsAlive => Level.Hp > 0;

        public DropManager DropManager { get; }

        public string CharacterName { get; set; }

        public string FormattedName { get; set; }

        public Hud Hud { get; set; } = null;

        public Vector2 MovingDirection { get; set; }

        public Level Level
        {
            get { return level; }
            set
            {
                level = value;

                SpellManager?.UpdateSpells();
            }
        }

        public Level Level0 { get; set; }

        public LevelManager LevelManager { get; set; }

        public float BounceTime { get; set; } = 0.1f;

        public float BounceSpeed { get; set; } = 20;

        public bool SpawnParticleOnDestroy { get; set; }

        public MovingState movingState
        {
            get { return _movingState; }
            set
            {
                _movingState = value;
                //if (value != MovingState.Inactive)
                //    UpdateHitBox();
            }
        }

        public event HPChangedEventHandler OnHpChanged;

        public event EnergyChangedEventHandler OnEnergyChanged;

        public event LevelupEventHandler OnLevelup;

        public void EnergyChanged()
        {
            OnEnergyChanged?.Invoke(this);
        }

        public event RoomChangedHandler OnRoomChange;

        public void RoomChanged()
        {
            OnRoomChange?.Invoke(this);
        }

        public event XPChangedEventHandler OnXpChanged;

        public void XpChanged(long delta)
        {
            OnXpChanged?.Invoke(this, delta);
        }

        public event DamageTakenEventHandler OnDamageTaken;

        private void TookDamage(float delta)
        {
            OnDamageTaken?.Invoke(this, delta);
        }

        public void LevelCheck()
        {
            if (LevelManager == null)
                LevelManager = new LevelManager(this, Level0);
            if (LevelManager.CheckLevelUp())
            {
                OnLevelup?.Invoke(this);
            }
        }

        public override void Start()
        {
            base.Start();

            LevelCheck();
            movingState = MovingState.Inactive;
            if (Animations != null)
                CurrentAnimation = GetMovingStateString(MovingState.Idle);

            SpellManager = new SpellManager(this);
            Engine.SpawnObject(SpellManager);
        }

        public override void Update()
        {
            base.Update();
            if (Game.Game.Instance.MainWindow != "game") return;
            if (movingState == MovingState.Inactive)
            {
                if (!startedActivationTimer)
                {
                    startedActivationTimer = true;
                }
                else
                {
                    // delayed by one frame
                    if (!Timer.Contains("timeBeforeActivation"))
                        Timer.Set("timeBeforeActivation", delayBeforeActivation);
                    if (Timer.Get("timeBeforeActivation") <= 0)
                    {
                        CreateHitBox();
                        movingState = MovingState.Idle;
                    }
                }
            }
            else if (Animations != null)
            {
                CurrentAnimation = GetMovingStateString();
            }
        }

        protected virtual void CreateHitBox()
        {
            //AddHitBox("mass", 0, 0, 0, 0);
            //UpdateHitBox();
        }

        private void UpdateHitBox()
        {
            var hitboxWidth = HitBoxSize[movingState].X;
            var hitboxHeight = HitBoxSize[movingState].Y;

            var mod = 0f;
            if (Height < 50)
                mod = 0f;
            HitBoxes["mass"].X = HitBoxOffSet[movingState].X;
            HitBoxes["mass"].Y = HitBoxOffSet[movingState].Y + hitboxHeight*mod;
            HitBoxes["mass"].Width = (int) hitboxWidth;
            HitBoxes["mass"].Height = (int) (hitboxHeight*(1 - mod));
        }

        public Vector2 GetHitCenter()
        {
            return new Vector2(
                X + HitBoxes["auto"].X + HitBoxes["auto"].Width/2,
                Y + HitBoxes["auto"].Y + HitBoxes["auto"].Height/2
                );
        }

        public Spell Shot(
            Vector2 direction, Func<bool> castCheck = null,
            Func<Spell, Vector2> recalculateDirection = null, bool simulate = false
            )
        {
            direction.Normalize();
            var spell = SpellManager.ActivateSpell(castCheck: castCheck, simulate: simulate);
            if (spell == null)
                return null;

            bool verticalDirection;
            var xMod = (direction.X + 1f)/2f;
            var yMod = (direction.Y + 1f)/2f;
            if (Math.Abs(direction.X)/Math.Abs(direction.Y) > Width/Height)
            {
                xMod = direction.X > 0 ? 1f : 0f;
                verticalDirection = true;
            }
            else
            {
                yMod = direction.Y > 0 ? 1f : 0f;
                verticalDirection = false;
            }
            spell.XOffset = (int) (Width*xMod);
            spell.YOffset = (int) (Height*yMod);
            if (verticalDirection)
            {
                spell.YOffset -= Level.SpellSize/2;
                if (direction.X < 0)
                    spell.XOffset -= Level.SpellSize;
            }
            else
            {
                spell.XOffset -= Level.SpellSize/2;
                if (direction.Y < 0)
                    spell.YOffset -= Level.SpellSize;
            }
            if (recalculateDirection != null)
            {
                direction = recalculateDirection(spell);
            }
            direction.Normalize();

            Debug.WriteLine("{0} is shotting to Direction: {1}", Name, direction);
            spell.Direction = direction;

            if (!simulate)
                Engine.SpawnObject(spell);

            return spell;
        }

        private void DestroyEvent(object sender)
        {
            var roomName = Game.Game.Instance.CurrentFloor.CurrentRoom.Name;
            if (SpawnParticleOnDestroy)
            {
                //var particleRadius = (Width + Height)/2f*0.1f;
                //if (particleRadius < 1)
                //    particleRadius = 1;
                //var particleSystem = new ParticleSystem(
                //    $"{roomName}_{Name}_psys",
                //    "homogeneous",
                //    (int) (particleRadius*5),
                //    particleRadius,
                //    Color.DarkRed,
                //    100*particleRadius,
                //    50*particleRadius,
                //    particleRadius*2
                //    )
                //{
                //    Order = Order,
                //    X = X + Width/2,
                //    Y = Y + Height/2,
                //    fade = 25*particleRadius
                //};
                //Debug.WriteLine(particleSystem.Name);
                //Engine.SpawnObject(particleSystem.Name, particleSystem);
            }
            DropManager.DropAndSpawn(LastHitCharacter);
        }

        internal void HpChanged()
        {
            OnHpChanged?.Invoke(this);
            Debug.WriteLine($"{Name} hp changed to {Level.Hp}");
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

            LastHitCharacter = enemy;

            var floatingText = new FloatingText(this, "-" + (int) dmg, Color.Orange, 0.6f + dmg/300f);
            Engine.SpawnObject(
                floatingText.Name, floatingText
                );

            // bounce back only if the damage is from a ranged enemy
            if (damage.Spell != null && damage.Spell.KnockBack > 0f)
                BounceBack(damage);

            TookDamage(dmg);
            return Level.Hp;
        }

        public Character LastHitCharacter { get; private set; }

        private void BounceBack(Damage damage)
        {
            var force = new Force
            {
                Owner = this,
                Direction = damage.Direction,
                Step = BounceSpeed/BounceTime,
                DestroyTimer = BounceTime*damage.Spell.KnockBack
            };
            forces.Add(force);
            Engine.SpawnObject($"{Name}_bouncebackforce_{forceCount++}", force);
        }

        protected void CalculateMovingState(Vector2 direction)
        {
            direction.Normalize();
            //var cos = Math.Acos(direction.X);
            //var sen = Math.Asin(direction.Y);
            // top or bottom
            var x = direction.X;
            var y = direction.Y;
            if (Math.Abs(y) > Math.Abs(x))
                movingState = y >= 0 ? MovingState.MovingDown : MovingState.MovingUp;
            else
                movingState = x >= 0 ? MovingState.MovingRight : MovingState.MovingLeft;
        }

        private void CalculateAnimationHitBoxes(MovingState movingState)
        {
            var sprite = CurrentSprite;
            if (sprite == null || movingState != MovingState.Idle)
                sprite = Animations[GetMovingStateString(movingState)].Sprites[0];

            var resultTuple = sprite.CalculateRealHitBox();
            HitBoxOffSet[movingState] = resultTuple.Item1;
            HitBoxSize[movingState] = resultTuple.Item2;
            Debug.WriteLine($"Calculated real hitbox: {CharacterName}, ({resultTuple.Item1.X},{resultTuple.Item1.Y}) to ({resultTuple.Item2.X},{resultTuple.Item2.Y})");
        }

        public void CalculateAnimationHitBoxes()
        {
            foreach (MovingState state in Enum.GetValues(typeof (MovingState)))
            {
                if (state != MovingState.Inactive && state == MovingState.Idle ||
                    Animations.ContainsKey(GetMovingStateString(state)))
                    CalculateAnimationHitBoxes(state);
            }
        }

        public string GetMovingStateString()
        {
            return GetMovingStateString(movingState);
        }

        public static string GetMovingStateString(MovingState state)
        {
            switch (state)
            {
                case MovingState.Idle:
                    return "idle";
                case MovingState.Inactive:
                    return "inactive";
                case MovingState.MovingLeft:
                    return "movingLeft";
                case MovingState.MovingRight:
                    return "movingRight";
                case MovingState.MovingDown:
                    return "movingDown";
                case MovingState.MovingUp:
                default:
                    return "movingUp";
            }
        }

        public void ApplyEffect(Action<Character> effect, Item.EffectType effectType)
        {
            if (effectType == Item.EffectType.Instant)
                effect(this);
            else if (effectType == Item.EffectType.Persistent)
            {
                effect(this);
                OnLevelup += (object sender) => effect(this);
            }
            else if (effectType == Item.EffectType.PerRoom)
            {
                effect(this);
                OnRoomChange += (object sender) =>
                {
                    if (Game.Game.Instance.CurrentFloor.CurrentRoom.Enemies.Count > 0)
                        effect(this);
                };
            }
        }
    }
}