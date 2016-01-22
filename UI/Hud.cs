using System;
using System.Drawing;
using Aiv.Engine;
using Futuridium.Characters;
using OpenTK;

namespace Futuridium.UI
{
    public sealed class Hud : GameObject
    {
        private const float FontScale = 0.4f;
        private const float FontScaleSecondary = 0.4f;
        private const int HudBarWidth = 160;
        private const float HudPadding = 10f;

        private static Hud instance;

        private readonly int border = 1;

        private readonly int padding = 3;

        private RectangleObject energyBar;

        private RectangleObject energyBarContainer;
        private TextObject energyTextObj;

        private RectangleObject hpBar;

        private RectangleObject hpBarContainer;
        private TextObject hpTextObj;

        private TextObject levelTextObj;
        private RectangleObject spellCdBar;
        private RectangleObject spellCdBarContainer;

        private TextObject spellTextObj;

        private RectangleObject xpBar;
        private RectangleObject xpBarContainer;

        private TextObject scoreTextObject;
        private TextObject scoreModTextObject;

        private Hud()
        {
            Order = 9;
            Name = "hud";
        }

        public static Hud Instance => instance ?? (instance = new Hud());

        public override void Start()
        {
            base.Start();

            // Top Left
            var yDiff = 2f;
            var hp = new TextObject(FontScale, Color.White)//Color.DarkRed)
            {
                Order = Order,
                Text = "HP",
                X = HudPadding,
                Y = HudPadding,
                IgnoreCamera = true
            };
            var hpSize = hp.Measure();
            hpBarContainer = new RectangleObject(HudBarWidth, (int) hpSize.Y)
            {
                X = hp.X + padding + hpSize.X,
                Y = hp.Y - yDiff,
                Order = Order,
                Color = Color.Black,
                IgnoreCamera = true
            };
            hpBar = new RectangleObject(HudBarWidth - border*2, (int) (hpBarContainer.Height - border*2))
            {
                Order = Order,
                X = hpBarContainer.X + border,
                Y = hpBarContainer.Y + border,
                Color = Color.DarkRed,
                Fill = true,
                IgnoreCamera = true
            };
            hpTextObj = new TextObject(FontScaleSecondary, Color.White)//Color.DarkRed)
            {
                Order = Order,
                X = hpBarContainer.X + hpBarContainer.Width + padding,
                Y = hp.Y,
                IgnoreCamera = true
            };

            var energyText = new TextObject(FontScale, Color.White)//Color.DarkRed)
            {
                Text = "EN",
                X = HudPadding,
                Order = Order,
                Y = padding + hp.Y + hpSize.Y,
                IgnoreCamera = true
            };
            var energyTextSize = energyText.Measure();
            energyBarContainer = new RectangleObject(HudBarWidth, (int) energyTextSize.Y)
            {
                X = energyText.X + padding + energyTextSize.X,
                Y = energyText.Y - yDiff,
                Order = Order,
                Color = Color.Black,
                IgnoreCamera = true
            };
            energyBar = new RectangleObject(HudBarWidth - border*2, (int) (energyBarContainer.Height - border*2))
            {
                Order = Order,
                X = energyBarContainer.X + border,
                Y = energyBarContainer.Y + border,
                Color = Color.DarkBlue,
                Fill = true,
                IgnoreCamera = true
            };
            energyTextObj = new TextObject(FontScaleSecondary, Color.White)//Color.DarkRed)
            {
                Order = Order,
                X = energyBarContainer.X + energyBarContainer.Width + padding,
                Y = energyText.Y,
                IgnoreCamera = true
            };

            var xp = new TextObject(FontScale, Color.White)//Color.DarkGreen)
            {
                Order = Order,
                Text = "XP",
                X = HudPadding,
                Y = padding + energyText.Y + energyTextSize.Y,
                IgnoreCamera = true
            };
            var xpSize = xp.Measure();
            xpBarContainer = new RectangleObject(HudBarWidth, (int) xpSize.Y)
            {
                X = xp.X + padding + xpSize.X,
                Y = xp.Y - yDiff,
                Order = Order,
                Color = Color.Black,
                IgnoreCamera = true
            };
            xpBar = new RectangleObject((int) (xpBarContainer.Width - border*2),
                (int) (xpBarContainer.Height - border*2))
            {
                Order = Order,
                X = xpBarContainer.X + border,
                Y = xpBarContainer.Y + border,
                Color = Color.DarkOliveGreen,
                Fill = true,
                IgnoreCamera = true
            };
            levelTextObj = new TextObject(FontScaleSecondary, Color.White)//Color.DarkGreen)
            {
                Order = Order,
                Text = "0% to 1",
                X = xpBarContainer.X + xpBarContainer.Width + padding,
                Y = xp.Y,
                IgnoreCamera = true
            };

            // Center
            scoreTextObject = new TextObject(FontScaleSecondary, Color.White)//Color.DarkGreen)
            {
                Order = Order,
                Text = " ", 
                Y = HudPadding,
                IgnoreCamera = true
            };
            scoreModTextObject = new TextObject(FontScaleSecondary * 0.8f, Color.White)//Color.DarkGreen)
            {
                Order = Order,
                Y = scoreTextObject.Y + padding + scoreTextObject.Measure().Y,
                IgnoreCamera = true
            };

            // Top Right
            spellCdBarContainer = new RectangleObject(HudBarWidth, (int) xpSize.Y)
            {
                X = Engine.Width - HudBarWidth - HudPadding,
                Y = HudPadding,
                Order = Order,
                Color = Color.Black,
                IgnoreCamera = true
            };
            spellCdBar = new RectangleObject((int) (spellCdBarContainer.Width - border*2),
                (int) (spellCdBarContainer.Height - border*2))
            {
                Order = Order,
                X = spellCdBarContainer.X + border,
                Y = spellCdBarContainer.Y + border,
                Color = Color.Gray,
                Fill = true,
                IgnoreCamera = true
            };
            spellTextObj = new TextObject(FontScaleSecondary, Color.White)//Color.Black
            {
                Order = Order,
                Text = "",
                X = 0,
                Y = spellCdBarContainer.Y,
                IgnoreCamera = true
            };

            // Bottom left
            var seedTextObj = new TextObject(FontScaleSecondary * 0.66f, Color.White, alpha: 0.66f) // Color.Black
            {
                Order = Order,
                Text = "Seed: " + Game.Game.Instance.Random.Seed,
                X = HudPadding,
                IgnoreCamera = true
            };
            seedTextObj.Y = Engine.Height - HudPadding - seedTextObj.Measure().Y;

            Engine.SpawnObject(Name + "_hpText", hp);
            Engine.SpawnObject(Name + "_hpBarContainer", hpBarContainer);
            Engine.SpawnObject(Name + "_hpBar", hpBar);
            Engine.SpawnObject(Name + "_hpTextObj", hpTextObj);
            Engine.SpawnObject(Name + "_energyText", energyText);
            Engine.SpawnObject(Name + "_energyBarContainer", energyBarContainer);
            Engine.SpawnObject(Name + "_energyBar", energyBar);
            Engine.SpawnObject(Name + "_energyTextObj", energyTextObj);
            Engine.SpawnObject(Name + "_xpText", xp);
            Engine.SpawnObject(Name + "_xpBarContainer", xpBarContainer);
            Engine.SpawnObject(Name + "_xpBar", xpBar);
            Engine.SpawnObject(Name + "_levelTextObj", levelTextObj);

            Engine.SpawnObject(Name + "_scoreTextObj", scoreTextObject);
            Engine.SpawnObject(Name + "_scoreModTextObj", scoreModTextObject);

            Engine.SpawnObject(Name + "_spellText", spellTextObj);
            Engine.SpawnObject(Name + "_spellCdBarContainer", spellCdBarContainer);
            Engine.SpawnObject(Name + "_spellCdBar", spellCdBar);

            Engine.SpawnObject(Name + "_seedTextObj", seedTextObj);
        }

        public void UpdateEnergyBar()
        {
            var player = Player.Instance;
            var energy = (int) player.Level.Energy;
            energyTextObj.Text = $"{energy} / {(int) player.Level.MaxEnergy}";
            if (energyBar != null)
            {
                //var newWidth = (int)((energyBarContainer.Width - border * 2) * (energy / (double)player.Level.MaxEnergy));
                //if (newWidth < 0)
                //    newWidth = 0;
                //energyBar.FillEnd = new Vector2(newWidth, energyBar.FillEnd.Y);
                energyBar.Box.scale = new Vector2(energy/player.Level.MaxEnergy, 1f);
            }
        }

        public void UpdateHpBar()
        {
            var player = Player.Instance;
            var hp = (int)player.Level.Hp;
            hpTextObj.Text = $"{hp} / {(int)player.Level.MaxHp}";
            if (hpBar != null)
            {
                //var newWidth = (int)((hpBarContainer.Width - border * 2) * (hp / (double)player.Level.MaxHp));
                //if (newWidth < 0)
                //    newWidth = 0;
                hpBar.Box.scale = new Vector2(hp / player.Level.MaxHp, 1f);
            }
        }

        public void UpdateScoreBar()
        {
            scoreTextObject.Text = $"Score {(int)Game.Game.Instance.Score.Value}";
            scoreTextObject.X = Engine.Width / 2f - scoreTextObject.Measure().X / 2;
            scoreModTextObject.Text = $"mod {Math.Round(Game.Game.Instance.Score.XpScoreModifier, 2)}";
            scoreModTextObject.X = Engine.Width / 2f - scoreModTextObject.Measure().X / 2;
        }

        public void UpdateSpellBar()
        {
            var player = Player.Instance;
            if (player.Level == null)
                return;

            spellTextObj.Text = player.SpellManager.ChosenSpellName;
            var spellTextSize = spellTextObj.Measure();
            spellTextObj.X = spellCdBarContainer.X - padding - spellTextSize.X;
            var lastCastedSpell = player.SpellManager.LastCastedSpell;
            if (lastCastedSpell != null)
            {
                //var newWidth = (int)(
                //    (spellCdBarContainer.Width - border * 2) *
                //    (player.SpellManager.spellsCd[lastCastedSpell.GetType()] / lastCastedSpell.StartingCd)
                //    );
                //if (newWidth < 0)
                //    newWidth = 0;
                //spellCdBar.FillEnd = new Vector2(newWidth, spellCdBar.FillEnd.Y);
                var cdPercentage = player.SpellManager.SpellsCd[lastCastedSpell.GetType()]/lastCastedSpell.StartingCd;
                if (cdPercentage < 0f)
                    cdPercentage = 0f;
                spellCdBar.Box.scale = new Vector2(
                    cdPercentage,
                    1f);
            }
        }

        public void UpdateXpBar()
        {
            var player = Player.Instance;
            var xp = (int) player.Xp;
            var level = player.Level;
            var levelManager = Player.Instance.LevelManager;
            var xpPercentage = Math.Min(1, (float) xp/levelManager.levelUpTable[level.level + 1].NeededXp);
            levelTextObj.Text = Math.Round(xpPercentage*100, 2) + "% to " + (level.level + 1);
            //float newWidth;
            //if (level.level == 99)
            //    newWidth = xpBarContainer.Width - border * 2;
            //else
            //    newWidth = (xpBarContainer.Width - border * 2f) * xpPercentage;
            //if (newWidth < 0)
            //    newWidth = 0;
            xpBar.Box.scale = new Vector2(xpPercentage, 1f);
        }

        public void SpawnStats()
        {
            var order = 11;
            var lvl = Player.Instance.Level;
            var attack = new TextObject(FontScale, Color.White)//Color.DarkGreen)
            {
                Order = order,
                Text = "Attack: " + lvl.Attack,
                X = HudPadding,
                IgnoreCamera = true
            };
            var fontHeight = attack.Measure().Y;
            attack.Y = padding + xpBarContainer.Y + fontHeight;
            var spellCd = new TextObject(FontScale, Color.White)//Color.DarkGreen)
            {
                Order = order,
                Text = "Spell CD: " + lvl.SpellCd,
                X = HudPadding,
                Y = padding + attack.Y + fontHeight,
                IgnoreCamera = true
            };
            var spellSpeed = new TextObject(FontScale, Color.White)//Color.DarkGreen)
            {
                Order = order,
                Text = "Spell speed: " + lvl.SpellSpeed,
                X = HudPadding,
                Y = padding + spellCd.Y + fontHeight,
                IgnoreCamera = true
            };
            var spellSize = new TextObject(FontScale, Color.White)//Color.DarkGreen)
            {
                Order = order,
                Text = "Spell size: " + lvl.SpellSize,
                X = HudPadding,
                Y = padding + spellSpeed.Y + fontHeight,
                IgnoreCamera = true
            };
            var spellRange = new TextObject(FontScale, Color.White)//Color.DarkGreen)
            {
                Order = order,
                Text = "Spell range: " + lvl.SpellRange,
                X = HudPadding,
                Y = padding + spellSize.Y + fontHeight,
                IgnoreCamera = true
            };
            var spellEnergyModifier = new TextObject(FontScale, Color.White)//Color.DarkGreen)
            {
                Order = order,
                Text = "Spell energy mod: " + lvl.SpellEnergyModifier,
                X = HudPadding,
                Y = padding + spellRange.Y + fontHeight,
                IgnoreCamera = true
            };
            var spellKnockBack = new TextObject(FontScale, Color.White)//Color.DarkGreen)
            {
                Order = order,
                Text = "Spell knockback: " + lvl.SpellKnockBack,
                X = HudPadding,
                Y = padding + spellEnergyModifier.Y + fontHeight,
                IgnoreCamera = true
            };
            var luck = new TextObject(FontScale, Color.White)//Color.DarkGreen)
            {
                Order = order,
                Text = "Luck: " + lvl.Luck,
                X = HudPadding,
                Y = padding + spellKnockBack.Y + fontHeight,
                IgnoreCamera = true
            };
            var speed = new TextObject(FontScale, Color.White)//Color.DarkGreen)
            {
                Order = order,
                Text = "Speed: " + lvl.Speed,
                X = HudPadding,
                Y = padding + luck.Y + fontHeight,
                IgnoreCamera = true
            };
            var size = new TextObject(FontScale, Color.White)//Color.DarkGreen)
            {
                Order = order,
                Text = "Size: " + lvl.Size,
                X = HudPadding,
                Y = padding + speed.Y + fontHeight,
                IgnoreCamera = true
            };

            Engine.SpawnObject("stats_attack", attack);
            Engine.SpawnObject("stats_spellCd", spellCd);
            Engine.SpawnObject("stats_spellSpeed", spellSpeed);
            Engine.SpawnObject("stats_spellSize", spellSize);
            Engine.SpawnObject("stats_spellRange", spellRange);
            Engine.SpawnObject("stats_spellEnergyModifier", spellEnergyModifier);
            Engine.SpawnObject("stats_spellKnockBack", spellKnockBack);
            Engine.SpawnObject("stats_luck", luck);
            Engine.SpawnObject("stats_speed", speed);
            Engine.SpawnObject("stats_size", size);
        }

        public void DestroyStats()
        {
            Engine.RemoveObject(Engine.Objects["stats_attack"]);
            Engine.RemoveObject(Engine.Objects["stats_spellCd"]);
            Engine.RemoveObject(Engine.Objects["stats_spellSpeed"]);
            Engine.RemoveObject(Engine.Objects["stats_spellSize"]);
            Engine.RemoveObject(Engine.Objects["stats_spellRange"]);
            Engine.RemoveObject(Engine.Objects["stats_spellEnergyModifier"]);
            Engine.RemoveObject(Engine.Objects["stats_spellKnockBack"]);
            Engine.RemoveObject(Engine.Objects["stats_luck"]);
            Engine.RemoveObject(Engine.Objects["stats_speed"]);
            Engine.RemoveObject(Engine.Objects["stats_size"]);
        }
    }
}