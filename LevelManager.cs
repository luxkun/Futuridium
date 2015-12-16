using System;
using System.Collections.Generic;
using Futuridium.Spells;

namespace Futuridium
{
    public class Level
    {
        // Hp
        private float hp;
        public float Hp
        {
            get { return hp; }
            set
            {
                hp = value;
                if (Activated)
                    LevelManager.character.HpChanged();
            }
        }

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

        public float SpellCd { get; set; }

        public int SpellSize { get; set; }

        public int SpellRange { get; set; }

        public float SpellSpeed { get; set; }

        public float Speed { get; set; }

        public long XpReward { get; set; }

        public LevelManager LevelManager { get; set; }

        public bool Activated { get; set; }

        public float MaxHp { get; set; }

        public float Attack { get; set; }

        public float MaxEnergy { get; set; }

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
                Hp += (int)(MaxHp * 0.25);
                if (Hp > MaxHp)
                    Hp = MaxHp;
                Energy = oldLevel.Energy;
                Energy += (int)(MaxEnergy * 0.25);
                if (Energy > MaxEnergy)
                    Energy = MaxEnergy;
            }
            else
            {
                Hp = MaxHp;
                Energy = MaxEnergy;
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
            levelUpTable[0] = level0;
            for (var level = 1; level < 100; level++)
            {
                var lvl = new Level(this)
                {
                    level = level,
                    MaxHp = (int) (level0.MaxHp*(1 + level/6f)),
                    MaxEnergy = (int) (level0.MaxEnergy*(1 + level/6f)),
                    XpReward = level0.XpReward*level*level,
                    Attack = (int) (level0.Attack*(1 + level/4f)),
                    Speed = (float) (level0.Speed*(1 + level/15f)),
                    SpellCd = (float) (level0.SpellCd*(1 - level/100f)),
                    SpellSpeed = (float) (level0.SpellSpeed*(1 + level/6f)),
                    SpellRange = (int) (level0.SpellRange*(1 + level/10f)),
                    NeededXp = level0.NeededXp*level*level*level,
                    spellList = level0.spellList
                };
                lvl.Hp = lvl.MaxHp;
                lvl.Energy = lvl.MaxEnergy;
                // x^3
                // x^2
                lvl.SpellSize = (int) (level0.SpellSize*(1 + level/10.0));
                levelUpTable[level] = lvl;
            }
            level0.NeededXp = 0;
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