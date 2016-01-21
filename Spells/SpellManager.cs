using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Aiv.Engine;
using Futuridium.Characters;

namespace Futuridium.Spells
{
    public class SpellManager : GameObject
    {
        public delegate void SpellCdChangedEventHandler(object sender);

        private const float SwapDelay = 0.5f;
        private Dictionary<Type, List<Spell>> activeSpells;
        private int chosenSpellIndex = -1;
        private int spellCounter;

        public SpellManager(Character owner)
        {
            Owner = owner;
            Name = $"{owner.Name}_spellManager";
        }

        public Dictionary<Type, float> SpellsCd { get; private set; }

        public Spell LastCastedSpell { get; private set; }

        public Type ChosenSpell { get; private set; }

        private int ChosenSpellIndex
        {
            get { return chosenSpellIndex; }
            set
            {
                if (value != -1)
                {
                    chosenSpellIndex = value%Owner.Level.SpellList.Count;
                    ChosenSpell = Owner.Level.SpellList[chosenSpellIndex];
                }
                else
                {
                    chosenSpellIndex = value;
                }
            }
        }

        private Character Owner { get; }

        public string ChosenSpellName => (string) ChosenSpell.GetField("spellName").GetValue(null);

        // if returns true then the hitted enemy can be hitten
        public Func<GameObject, bool> Mask { get; set; }

        public event SpellCdChangedEventHandler OnSpellCdChanged;

        public override void Start()
        {
            base.Start();
            activeSpells = new Dictionary<Type, List<Spell>>();
            SpellsCd = new Dictionary<Type, float>();

            UpdateSpells();
        }

        public override void Update()
        {
            base.Update();
            if (Game.Game.Instance.MainWindow != "game") return;
            var cdChanged = false;
            foreach (var key in SpellsCd.Keys.ToList())
            {
                if (SpellsCd[key] > 0f)
                {
                    SpellsCd[key] -= DeltaTime;
                    cdChanged = true;
                }
            }
            if (cdChanged)
                OnSpellCdChanged?.Invoke(this);
        }

        public bool SpellOnCd(Type spellType = null)
        {
            if (spellType == null)
                spellType = ChosenSpell;
            var result = SpellsCd[spellType] > 0;
            // if spell is alive and still casting
            if (!result && LastCastedSpell != null && LastCastedSpell.IsCasting)
                return true;
            return result;
        }

        public void SwapSpell()
        {
            if (Owner.Level.SpellList.Count == 0 || Timer.Get("lastSwapTimer") > 0)
                return;
            Timer.Set("lastSwapTimer", SwapDelay);
            ChosenSpellIndex++;
            OnSpellCdChanged?.Invoke(this);
            Debug.WriteLine($"Chosen spell '{ChosenSpell}'.");
        }

        public void ChangeSpell(Type spellType)
        {
            var newIndex = Owner.Level.SpellList.IndexOf(spellType);
            ChosenSpellIndex = newIndex;
        }

        public Spell ActivateSpell(Type spellType = null, Func<bool> castCheck = null, bool simulate = false)
        {
            if (!simulate && SpellOnCd(spellType))
                return null;
            //    throw new Exception("Spell '$(spellType)' is on cold down.");
            if (spellType == null)
                spellType = ChosenSpell;
            // if is an activated spell and it's already casted disactivate it
            if (activeSpells[spellType].Count > 0 && activeSpells[spellType][0].ActivatedSpell && !simulate)
            {
                SpellsCd[spellType] = activeSpells[spellType][0].StartingCd;
                DisactivateSpell(activeSpells[spellType][0]);
                return null;
            }
            var spell = (Spell) Activator.CreateInstance(spellType, this, Owner);
            if (Owner.Level.Energy < spell.EnergyUsage && !simulate)
                return null;
            activeSpells[spellType].Add(spell);
            LastCastedSpell = spell;
            if (spell.ContinuousSpell)
                spell.CastCheck = castCheck;
            spell.OnDestroy += sender => DisactivateSpell(spell, false);
            spell.Name = (spell.RoomConstricted
                ? Game.Game.Instance.CurrentFloor.CurrentRoom.Name + "_"
                : "") + $"{Name}_spell_{spell.SpellName}_{spellCounter++}";
            SpellsCd[spellType] = spell.StartingCd;
            return spell;
        }

        public void DisactivateSpell(Spell spell, bool destroy = true)
        {
            if (!activeSpells[spell.GetType()].Contains(spell))
                return;
            activeSpells[spell.GetType()].Remove(spell);
            if (destroy)
                spell.Destroy();
        }

        public void UpdateSpells()
        {
            foreach (var type in Owner.Level.SpellList)
            {
                if (!activeSpells.ContainsKey(type))
                    activeSpells[type] = new List<Spell>();
                if (!SpellsCd.ContainsKey(type))
                    SpellsCd[type] = 0f;
            }
            ChosenSpellIndex = -1;
            SwapSpell();
        }
    }
}