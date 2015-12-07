namespace Futuridium
{
    public class Level
    {
        public int attack;
        public int maxHP;
        // hp
        private int _hp;
        public int hp
        {
            get { return _hp; }
            set
            {
                _hp = value;
                if (activated) { 
                    levelManager.character.HpChanged();
                }
            }
        }
        public int maxEnergy;
        // energy
        private int _energy;
        public int energy
        {
            get { return _energy; }
            set
            {
                _energy = value;
                if (activated)
                    levelManager.character.EnergyChanged();
            }
        }
        public int level;
        private long _neededXP = 100;
        public long neededXP
        {
            get { return _neededXP; }
            set
            {
                _neededXP = value;
                if (activated)
                    levelManager.character.XpChanged();
            }
        }
        public float shotDelay;
        public int shotRadius;
        public int shotRange;
        public float shotSpeed;
        public float speed;
        public long xpReward;
        public LevelManager levelManager;

        public bool activated;

        // only level0 should be initialized like this
        public Level()
        {
        }

        public Level(LevelManager levelManager)
        {
            this.levelManager = levelManager;
        }

        public void Init(Level oldLevel)
        {
            activated = true;
            if (oldLevel != null)
            {
                hp = oldLevel.hp;
                hp += (int)(maxHP * 0.25);
                if (hp > maxHP)
                    hp = maxHP;
                energy = oldLevel.energy;
                energy += (int)(maxEnergy * 0.25);
                if (energy > maxEnergy)
                    energy = maxEnergy;
            }
            else
            {
                hp = maxHP;
                energy = maxEnergy;
            }
        }

        public Level Clone()
        {
            var clone = (Level) MemberwiseClone();
            return clone;
        }
    }

    public class LevelManager
    {
        public readonly Character character;
        public Level[] levelUpTable;

        public LevelManager(Character character, Level level0)
        {
            this.character = character;
            CreateLevelUpTable(level0);
        }

        private void CreateLevelUpTable(Level level0)
        {
            level0.levelManager = this;
            levelUpTable = new Level[100];
            Level lvl;
            levelUpTable[0] = level0;
            for (var level = 1; level < 100; level++)
            {
                lvl = new Level(this)
                {
                    level = level,
                    maxHP = (int) (level0.maxHP*(1 + level/6f)),
                    maxEnergy = (int) (level0.maxEnergy*(1 + level/6f)),
                    xpReward = level0.xpReward*level*level,
                    attack = (int) (level0.attack*(1 + level/4f)),
                    speed = (float) (level0.speed*(1 + level/15f)),
                    shotDelay = (float) (level0.shotDelay*(1 - level/100f)),
                    shotSpeed = (float) (level0.shotSpeed*(1 + level/6f)),
                    shotRange = (int) (level0.shotRange*(1 + level/10f)),
                    neededXP = level0.neededXP*level*level*level
                };
                lvl.hp = lvl.maxHP;
                lvl.energy = lvl.maxEnergy;
                // x^3
                // x^2
                lvl.shotRadius = (int) (level0.shotRadius*(1 + level/10.0));
                levelUpTable[level] = lvl;
            }
        }

        public bool CheckLevelUp()
        {
            // base case
            if (character.Level == null)
            {
                character.Level = levelUpTable[0];
                character.Level.Init(null);
                CheckLevelUp();
                return true;
            }
            if (character.Level.level == levelUpTable.Length - 1)
                return false;
            var nextLevel = levelUpTable[character.Level.level + 1];
            if (character.Xp >= nextLevel.neededXP)
            {
                character.Level = nextLevel;
                nextLevel.Init(character.Level);
                CheckLevelUp();
                return true;
            }
            return false;
        }
    }
}