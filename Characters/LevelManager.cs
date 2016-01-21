using System;
using System.Collections.Generic;

namespace Futuridium.Characters
{
    public class Level
    {
        private float attack;
        private float energy;
        private float hp;
        private float luck = 1f;
        private float maxEnergy;
        private float maxHp;

        private long neededXp = 100;
        private float size = 1f;
        private float speed;
        private float spellCd;
        private float spellEnergyModifier = 1f;
        private float spellKnockBack = 1f;

        public List<Type> SpellList { get; set; }
        private float spellRange;
        private float spellSize;
        private float spellSpeed;

        // only level0 should be initialized like this
        public Level()
        {
        }

        public Level(LevelManager levelManager)
        {
            LevelManager = levelManager;
        }

        public float Hp
        {
            get { return hp + LevelManager.GetStatBuff("hp"); }
            set
            {
                if (value > MaxHp)
                    value = MaxHp;
                hp = value;
                if (Activated)
                    LevelManager.character.HpChanged();
            }
        }

        public float Energy
        {
            get { return energy + LevelManager.GetStatBuff("energy"); }
            set
            {
                if (value > MaxEnergy)
                    value = MaxEnergy;
                energy = value;
                if (Activated)
                    LevelManager.character.EnergyChanged();
            }
        }

        public long NeededXp
        {
            get { return neededXp; }
            set
            {
                neededXp = value;
                if (Activated)
                    LevelManager.character.XpChanged(0);
            }
        }

        public int level { get; set; }

        public float SpellCd
        {
            // capped at 0.1f (10 bullets/s)
            get { return Math.Max(0.1f, spellCd + LevelManager.GetStatBuff("spellCd")); }
            set { spellCd = value; }
        }

        public float SpellSize
        {
            get { return spellSize + LevelManager.GetStatBuff("spellSize"); }
            set { spellSize = value; }
        }

        public float SpellRange
        {
            get { return spellRange + LevelManager.GetStatBuff("spellRange"); }
            set { spellRange = value; }
        }

        public float SpellSpeed
        {
            get { return spellSpeed + LevelManager.GetStatBuff("spellSpeed"); }
            set { spellSpeed = value; }
        }

        public float Speed
        {
            get { return speed + LevelManager.GetStatBuff("speed"); }
            set { speed = value; }
        }

        public float Luck
        {
            get { return luck + LevelManager.GetStatBuff("luck"); }
            set { luck = value; }
        }

        public long XpReward { get; set; }

        public LevelManager LevelManager { get; set; }

        public bool Activated { get; set; }

        public float MaxHp
        {
            get { return maxHp + LevelManager.GetStatBuff("maxHp"); }
            set { maxHp = value; }
        }

        public float Attack
        {
            get { return attack + LevelManager.GetStatBuff("attack"); }
            set { attack = value; }
        }

        public float MaxEnergy
        {
            get { return maxEnergy + LevelManager.GetStatBuff("maxEnergy"); }
            set { maxEnergy = value; }
        }

        public float Size
        {
            get { return size + LevelManager.GetStatBuff("size"); }
            set { size = value; }
        }

        public float SpellKnockBack
        {
            get { return spellKnockBack + LevelManager.GetStatBuff("spellKnockBack"); }
            set { spellKnockBack = value; }
        }

        public float SpellEnergyModifier
        {
            get { return spellEnergyModifier + LevelManager.GetStatBuff("spellEnergyModifier"); }
            set { spellEnergyModifier = value; }
        }

        public void Init(Level oldLevel)
        {
            if (SpellList == null)
                SpellList = new List<Type>();
            Activated = true;
            if (oldLevel != null)
            {
                Hp = oldLevel.Hp;
                Hp += MaxHp * 0.1f;
                Energy = oldLevel.Energy;
                Energy += MaxEnergy * 0.1f;
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
        private readonly Dictionary<string, List<Buff>> buffs;
        public readonly Character character;
        private readonly Dictionary<Buff.BuffType, List<Buff>> rawBuffs;
        public Level[] levelUpTable;

        public LevelManager(Character character, Level level0)
        {
            this.character = character;

            character.OnRoomChange += RoomChangeEvent;
            character.OnUpdate += UpdateEvent;

            buffs = new Dictionary<string, List<Buff>>();
            rawBuffs = new Dictionary<Buff.BuffType, List<Buff>>();
            foreach (Buff.BuffType e in Enum.GetValues(typeof (Buff.BuffType)))
            {
                rawBuffs[e] = new List<Buff>();
            }

            CreateLevelUpTable(level0);
        }

        private void UpdateEvent(object sender)
        {
            foreach (var buff in rawBuffs[Buff.BuffType.Time].ToArray())
            {
                buff.Expire -= character.DeltaTime;
                if (buff.Expire <= 0)
                    RemoveBuff(buff);
            }
        }

        private void RoomChangeEvent(object sender)
        {
            foreach (var buff in rawBuffs[Buff.BuffType.PerRoom].ToArray())
            {
                RemoveBuff(buff);
            }
        }

        private void RemoveBuff(Buff buff)
        {
            buffs[buff.Stat].Remove(buff);
            rawBuffs[buff.Type].Remove(buff);
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
                    MaxHp = level0.MaxHp*(1 + level*0.02f),
                    MaxEnergy = level0.MaxEnergy*(1 + level*0.02f),
                    Attack = level0.Attack*(1 + level*0.02f),
                    Speed = level0.Speed*(1 + level*0.01f),
                    SpellCd = level0.SpellCd*Math.Max(0.25f, 1 - level*0.01f),
                    SpellSpeed = level0.SpellSpeed*(1 + level*0.02f),
                    SpellRange = level0.SpellRange*(1 + level*0.02f),
                    SpellSize = level0.SpellSize*(1 + level*0.01f),
                    SpellKnockBack = level0.SpellKnockBack*(1 + level*0.01f),
                    SpellEnergyModifier = level0.SpellEnergyModifier*(1 + level*0.01f),
                    XpReward = (int) (level0.XpReward*level),
                    NeededXp = (int) (level0.NeededXp*level*level),
                    Luck = level0.Luck*(1f + level*0.01f),
                    SpellList = level0.SpellList,
                    Size = level0.Size,
                    Hp = 1,
                    Energy = 1
                };
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
                nextLevel.Init(character.Level);
                character.Level = nextLevel;
                CheckLevelUp();
                return true;
            }
            return false;
        }

        public void AddBuff(string key, float value, float expire, Buff.BuffType buffType)
        {
            var buff = new Buff
            {
                Stat = key,
                Value = value,
                Expire = expire,
                Type = buffType
            };
            AddBuff(buff);
        }

        public void AddBuff(Buff buff)
        {
            if (!buffs.ContainsKey(buff.Stat))
                buffs[buff.Stat] = new List<Buff>();
            buffs[buff.Stat].Add(buff);
            rawBuffs[buff.Type].Add(buff);
        }

        public float GetStatBuff(string key)
        {
            if (!buffs.ContainsKey(key))
                return 0f;
            var result = 0f;
            foreach (var tuple in buffs[key])
            {
                var buffValue = tuple.Value;
                result += buffValue;
            }
            return result;
        }
    }

    public class Buff
    {
        public enum BuffType
        {
            Time,
            PerRoom
        }

        public string Stat { get; set; }
        public BuffType Type { get; set; }
        public float Value { get; set; }
        public float Expire { get; set; }
    }
}