using System;
using System.Collections.Generic;
using Futuridium.Spells;

namespace Futuridium
{
    public class Level
    {
        public float attack;
        public float maxHp;
        // Hp
        private float hp;
        public float Hp
        {
            get { return hp; }
            set
            {
                hp = value;
                if (Activated) { 
                    LevelManager.character.HpChanged();
                }
            }
        }
        public float maxEnergy;
        // Energy
        private float energy;
        public float Energy
        {
            get { return energy; }
            set
            {
                energy = value;
                if (Activated)
                    LevelManager.character.EnergyChanged();
            }
        }

        private long neededXp = 100;
        public long NeededXp
        {
            get { return neededXp; }
            set
            {
                neededXp = value;
                if (Activated)
                    LevelManager.character.XpChanged();
            }
        }

        public int level { get; set; }

        public float ShotDelay { get; set; }

        public int ShotRadius { get; set; }

        public int ShotRange { get; set; }

        public float ShotSpeed { get; set; }

        public float Speed { get; set; }

        public long XpReward { get; set; }

        public LevelManager LevelManager { get; set; }

        public bool Activated { get; set; }


        public List<Type> spellList;

        // only level0 should be initialized like this
        public Level()
        {
        }

        public Level(LevelManager levelManager)
        {
            this.LevelManager = levelManager;
        }

        public void Init(Level oldLevel)
        {
            if (spellList == null)
                spellList = new List<Type>();
            Activated = true;
            if (oldLevel != null)
            {
                Hp = oldLevel.Hp;
                Hp += (int)(maxHp * 0.25);
                if (Hp > maxHp)
                    Hp = maxHp;
                Energy = oldLevel.Energy;
                Energy += (int)(maxEnergy * 0.25);
                if (Energy > maxEnergy)
                    Energy = maxEnergy;
            }
            else
            {
                Hp = maxHp;
                Energy = maxEnergy;
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
            level0.LevelManager = this;
            levelUpTable = new Level[100];
            Level lvl;
            levelUpTable[0] = level0;
            for (var level = 1; level < 100; level++)
            {
                lvl = new Level(this)
                {
                    level = level,
                    maxHp = (int) (level0.maxHp*(1 + level/6f)),
                    maxEnergy = (int) (level0.maxEnergy*(1 + level/6f)),
                    XpReward = level0.XpReward*level*level,
                    attack = (int) (level0.attack*(1 + level/4f)),
                    Speed = (float) (level0.Speed*(1 + level/15f)),
                    ShotDelay = (float) (level0.ShotDelay*(1 - level/100f)),
                    ShotSpeed = (float) (level0.ShotSpeed*(1 + level/6f)),
                    ShotRange = (int) (level0.ShotRange*(1 + level/10f)),
                    NeededXp = level0.NeededXp*level*level*level,
                    spellList = level0.spellList
                };
                lvl.Hp = lvl.maxHp;
                lvl.Energy = lvl.maxEnergy;
                // x^3
                // x^2
                lvl.ShotRadius = (int) (level0.ShotRadius*(1 + level/10.0));
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
            if (character.Xp >= nextLevel.NeededXp)
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