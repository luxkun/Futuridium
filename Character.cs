using Aiv.Engine;
using Futuridium.Spells;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

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

        public Dictionary<MovingState, Point> HitBoxOffSet { get; set; }
        public Dictionary<MovingState, Point> HitBoxSize { get; set; }

        public Dictionary<MovingState, SpriteAsset[]> animationsInfo;
        private MovingState lastAnimationState;
        private int lastAnimationIndex;
        private float animationFrequency;
        private float animationFPS;
        protected bool animating;

        private readonly List<Force> forces;

        private int forceCount;

        private Level level;
        public float RealSpeed { get; set; }
        public SpellManager SpellManager;

        private float vx;
        private float vy;
        private long xp;
        private bool startedActivationTimer;

        public Character(string name, string formattedName, string characterName)
        {
            forces = new List<Force>();
            animationsInfo = new Dictionary<MovingState, SpriteAsset[]>();

            HitBoxOffSet = new Dictionary<MovingState, Point>();
            HitBoxSize = new Dictionary<MovingState, Point>();
            foreach (MovingState state in Enum.GetValues(typeof(MovingState)))
            {
                HitBoxOffSet[state] = new Point();
                HitBoxSize[state] = new Point();
            }

            order = 6;
            
            Level0 = new Level();
            this.name = name;
            FormattedName = formattedName;
            CharacterName = characterName;

            DropManager = new DropManager(this);

            OnDestroy += DestroyEvent;
        }

        public float Vx
        {
            get { return vx; }
            set
            {
                vx = value;
                if (Math.Abs(vx) > 1)
                {
                    x += (int)vx;
                    vx -= (int)vx;
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
                    y += (int)vy;
                    vy -= (int)vy;
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

        public float AnimationFrequency
        {
            get { return animationFrequency; }
            set { animationFrequency = value; }
        }

        public bool IsAlive => Level.Hp > 0;

        public DropManager DropManager { get; private set; }

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

        public bool UseAnimations => animationsInfo.Count > 0;

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
            base.Start();
            LevelCheck();
            movingState = MovingState.Inactive;

            SpellManager = new SpellManager(this);
            engine.SpawnObject(SpellManager);
        }

        public override void Update()
        {
            base.Update();
            if (Game.Instance.MainWindow != "game") return;
            if (movingState == MovingState.Inactive)
            {
                if (!startedActivationTimer)
                {
                    Timer.Set("timeBeforeActivation", delayBeforeActivation);
                    startedActivationTimer = true;
                }
                else
                {
                    if (Timer.Get("timeBeforeActivation") <= 0)
                    {
                        CreateHitBox();
                        movingState = MovingState.Idle;
                    }
                }
            }
            else if (UseAnimations)
            {
                if (lastAnimationState != movingState)
                {
                    lastAnimationState = movingState;
                    lastAnimationIndex = 0;
                    animating = true;
                }
                if (Timer.Get("animationTimer") <= 0 && animating)
                {
                    currentSprite = animationsInfo[movingState][lastAnimationIndex];
                    if (animationsInfo[movingState].Length > 1)
                    {
                        lastAnimationIndex = (lastAnimationIndex + 1) % animationsInfo[movingState].Length;
                        // speedup if the speed has increased since start
                        Timer.Set("animationTimer", AnimationFrequency * (Level0.Speed / RealSpeed));
                        animating = true;
                    }
                    else
                    {
                        animating = false;
                    }
                }
            }
        }

        protected virtual void CreateHitBox()
        {
            AddHitBox("mass", 0, 0, 0, 0);
            UpdateHitBox();
        }

        private void UpdateHitBox()
        {
            float hitboxWidth;
            float hitboxHeight;
            if (HitBoxSize[movingState].X != 0 && HitBoxSize[movingState].Y != 0)
            {
                hitboxWidth = HitBoxSize[movingState].X;
                hitboxHeight = HitBoxSize[movingState].Y;
            }
            else
            {
                hitboxWidth = width;
                hitboxHeight = height;
            }

            var mod = 0.33f;
            if (height < 50)
                mod = 0f;
            hitBoxes["mass"].x = HitBoxOffSet[movingState].X;
            hitBoxes["mass"].y = HitBoxOffSet[movingState].Y + (int)(hitboxHeight * mod);
            hitBoxes["mass"].width = (int)hitboxWidth;
            hitBoxes["mass"].height = (int)(hitboxHeight * (1 - mod));
        }

        public Vector2 GetHitCenter()
        {
            return new Vector2(
                x + hitBoxes["mass"].x + hitBoxes["mass"].width / 2,
                y + hitBoxes["mass"].y + hitBoxes["mass"].height / 2
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
            float xMod = (direction.X + 1f) / 2f;
            float yMod = (direction.Y + 1f) / 2f;
            if (Math.Abs(direction.X) / Math.Abs(direction.Y) > (float)width / height)
            { 
                xMod = direction.X > 0 ? 1f : 0f;
                verticalDirection = true;
            }
            else
            { 
                yMod = direction.Y > 0 ? 1f : 0f;
                verticalDirection = false;
            }
            spell.xOffset = (int)(width * xMod);
            spell.yOffset = (int)(height * yMod);
            if (verticalDirection) { 
                spell.yOffset -= Level.SpellSize / 2;
                if (direction.X < 0)
                    spell.xOffset -= Level.SpellSize;
            }
            else
            { 
                spell.xOffset -= Level.SpellSize / 2;
                if (direction.Y < 0)
                    spell.yOffset -= Level.SpellSize;
            }
            if (recalculateDirection != null)
            {
                direction = recalculateDirection(spell);
            }
            direction.Normalize();

            Debug.WriteLine("{0} is shotting to Direction: {1}", name, direction);
            spell.Direction = direction;

            if (!simulate)
                engine.SpawnObject(spell);

            return spell;
        }

        private void DestroyEvent(object sender)
        {
            var roomName = Game.Instance.CurrentFloor.CurrentRoom.name;
            if (SpawnParticleOnDestroy)
            {
                var particleRadius = (int)((width + height) / 2f * 0.1f);
                if (particleRadius < 1)
                    particleRadius = 1;
                var particleSystem = new ParticleSystem(
                    $"{roomName}_{name}_psys",
                    "homogeneous",
                    particleRadius * 5,
                    particleRadius,
                    Color.DarkRed,
                    100 * particleRadius,
                    50 * particleRadius,
                    particleRadius * 2
                    )
                {
                    order = order,
                    x = x + width / 2,
                    y = y + height / 2,
                    fade = 25 * particleRadius
                };
                Debug.WriteLine(particleSystem.name);
                engine.SpawnObject(particleSystem.name, particleSystem);
            }
            if (DropManager.DropTable != null)
            {
                foreach (Item item in DropManager.Drop())
                {
                    Item newItem = (Item)item.Clone();
                    newItem.name = $"{name}_{newItem.ItemName}";
                }
            }
        }

        public bool SpawnParticleOnDestroy { get; set; }

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
                damage = new Damage(this, enemy) { DamageFunc = (Character ch0, Character ch1) => ch1.Level.Attack };
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

            var floatingText = new FloatingText(this, "-" + (int)dmg, "orange", 8 + (int)(dmg / 15));
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
                Step = BounceSpeed / BounceTime,
                DestroyTimer = BounceTime * damage.Spell.KnockBack
            };
            forces.Add(force);
            engine.SpawnObject($"{name}_bouncebackforce_{forceCount++}", force);
        }

        protected void CalculateMovingState(Vector2 direction)
        {
            direction.Normalize();
            //var cos = Math.Acos(direction.X);
            //var sen = Math.Asin(direction.Y);
            // top or bottom
            float x = direction.X;
            float y = direction.Y;
            if (Math.Abs(y) > Math.Abs(x))
                movingState = y >= 0 ? MovingState.MovingDown : MovingState.MovingUp;
            else
                movingState = x >= 0 ? MovingState.MovingRight : MovingState.MovingLeft;
        }

        private void CalculateRealSpriteHitBoxes(MovingState movingState)
        {
            var sprite = currentSprite;
            if (sprite == null || movingState != MovingState.Idle)
                sprite = animationsInfo[movingState][0];

            var bitmap = sprite.sprite;
            bool offSetDone = false;
            // CALCULATE Y
            for (int posY = 0; posY < bitmap.Height; posY++)
            {
                bool emptyRow = true;
                for (int posX = 0; posX < bitmap.Width; posX++)
                {
                    if (bitmap.GetPixel(posX, posY).A != 0)
                    {
                        emptyRow = false;
                        break;
                    }
                }
                if (emptyRow && !offSetDone)
                {
                    HitBoxOffSet[movingState] = new Point(HitBoxOffSet[movingState].X, posY);
                }
                else if (!emptyRow)
                {
                    offSetDone = true;
                    HitBoxSize[movingState] = new Point(HitBoxSize[movingState].X, posY - HitBoxOffSet[movingState].Y);
                }
            }
            // CALCULATE X
            offSetDone = false;
            for (int posX = 0; posX < bitmap.Width; posX++)
            {
                bool emptyCol = true;
                for (int posY = 0; posY < bitmap.Height; posY++)
                {
                    if (bitmap.GetPixel(posX, posY).A != 0)
                    {
                        emptyCol = false;
                        break;
                    }
                }
                if (emptyCol && !offSetDone)
                {
                    HitBoxOffSet[movingState] = new Point(posX, HitBoxOffSet[movingState].Y);
                }
                else if (!emptyCol)
                {
                    offSetDone = true;
                    HitBoxSize[movingState] = new Point(posX - HitBoxOffSet[movingState].X, HitBoxSize[movingState].Y);
                }
            }
            //Debug.WriteLine($"Calculated real hitbox: {CharacterName}, ({HitBoxOffSet.X},{HitBoxOffSet.Y}) to ({HitBoxSize.X},{HitBoxSize.Y})");
        }

        public void CalculateRealSpriteHitBoxes()
        {
            foreach (MovingState state in Enum.GetValues(typeof(MovingState)))
            {
                if (state != MovingState.Inactive && state == MovingState.Idle || (animationsInfo.ContainsKey(state) && animationsInfo[state].Length > 0))
                    CalculateRealSpriteHitBoxes(state);
            }
        }

        public enum MovingState
        {
            Inactive = 0,
            Idle = 1,
            MovingLeft = 2,
            MovingRight = 3,
            MovingDown = 4,
            MovingUp = 5
        }

        private MovingState _movingState;

        public MovingState movingState
        {
            get { return _movingState; }
            set
            {
                _movingState = value;
                if (value != MovingState.Inactive)
                    UpdateHitBox();
            }
        }
    }
}