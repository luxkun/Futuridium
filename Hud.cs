using Aiv.Engine;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Futuridium
{
    public sealed class Hud : GameObject
    {
        private const int FontSize = 12;
        private const int HudBarWidth = 200;

        private static Hud instance;

        private readonly int border = 1;

        // Energy bar
        private RectangleObject energyBar;

        private RectangleObject energyBarContainer;
        private TextObject energyTextObj;

        // LEFTTOP HUD
        // Hp bar
        private RectangleObject hpBar;

        private RectangleObject hpBarContainer;
        private TextObject hpTextObj;

        // Xp bar
        private TextObject levelTextObj;

        private readonly int padding = 10;
        private RectangleObject spellCdBar;
        private RectangleObject spellCdBarContainer;

        // RIGHTTOP HUD
        // Xp bar
        private TextObject spellTextObj;

        private RectangleObject xpBar;
        private RectangleObject xpBarContainer;

        private Hud()
        {
            order = 9;
            name = "hud";
        }

        public override void Start()
        {
            base.Start();
            var hp = new TextObject("Arial Black", FontSize, "darkred")
            {
                order = order,
                text = "HP",
                x = padding,
                y = padding
            };
            var hpSize = TextRenderer.MeasureText(hp.text, hp.font);
            hpBarContainer = new RectangleObject
            {
                x = hp.x + padding + hpSize.Width,
                y = hp.y,
                order = order,
                width = HudBarWidth,
                height = hpSize.Height,
                color = Color.Black
            };
            hpBar = new RectangleObject
            {
                order = order,
                x = hpBarContainer.x + border,
                y = hpBarContainer.y + border,
                color = Color.DarkRed,
                fill = true,
                height = hpBarContainer.height - border * 2
            };
            hpTextObj = new TextObject("Arial Black", FontSize, "darkred")
            {
                order = order,
                x = hpBarContainer.x + hpBarContainer.width + padding,
                y = hp.y
            };

            var energyText = new TextObject("Arial Black", FontSize, "darkred")
            {
                text = "EN",
                x = padding,
                order = order,
                y = padding + hp.y + hpSize.Height
            };
            var energyTextSize = TextRenderer.MeasureText(energyText.text, energyText.font);
            energyBarContainer = new RectangleObject
            {
                x = energyText.x + padding + energyTextSize.Width,
                y = energyText.y,
                order = order,
                width = HudBarWidth,
                height = energyTextSize.Height,
                color = Color.Black
            };
            energyBar = new RectangleObject
            {
                order = order,
                x = energyBarContainer.x + border,
                y = energyBarContainer.y + border,
                color = Color.DarkBlue,
                fill = true,
                height = energyBarContainer.height - border * 2
            };
            energyTextObj = new TextObject("Arial Black", FontSize, "darkred")
            {
                order = order,
                x = energyBarContainer.x + energyBarContainer.width + padding,
                y = energyText.y
            };

            var xp = new TextObject("Arial Black", FontSize, "darkgreen")
            {
                order = order,
                text = "XP",
                x = padding,
                y = padding + energyText.y + energyTextSize.Height
            };
            var xpSize = TextRenderer.MeasureText(xp.text, xp.font);
            xpBarContainer = new RectangleObject
            {
                x = xp.x + padding + xpSize.Width,
                y = xp.y,
                order = order,
                width = HudBarWidth,
                height = xpSize.Height,
                color = Color.Black
            };
            xpBar = new RectangleObject
            {
                order = order,
                x = xpBarContainer.x + border,
                y = xpBarContainer.y + border,
                color = Color.DarkOliveGreen,
                fill = true,
                height = xpBarContainer.height - border * 2
            };
            levelTextObj = new TextObject("Arial Black", FontSize, "darkgreen")
            {
                order = order,
                text = "0% to 1",
                x = xpBarContainer.x + xpBarContainer.width + padding,
                y = xp.y
            };

            spellCdBarContainer = new RectangleObject
            {
                x = engine.width - HudBarWidth - padding,
                y = padding,
                order = order,
                width = HudBarWidth,
                height = xpSize.Height,
                color = Color.Black
            };
            spellCdBar = new RectangleObject
            {
                order = order,
                x = spellCdBarContainer.x + border,
                y = spellCdBarContainer.y + border,
                color = Color.Gray,
                fill = true,
                height = xpBarContainer.height - border * 2
            };
            spellTextObj = new TextObject("Arial Black", FontSize, "black")
            {
                order = order,
                text = "",
                x = 0,
                y = spellCdBarContainer.y
            };

            engine.SpawnObject(name + "_hpText", hp);
            engine.SpawnObject(name + "_hpBarContainer", hpBarContainer);
            engine.SpawnObject(name + "_hpBar", hpBar);
            engine.SpawnObject(name + "_hpTextObj", hpTextObj);
            engine.SpawnObject(name + "_energyText", energyText);
            engine.SpawnObject(name + "_energyBarContainer", energyBarContainer);
            engine.SpawnObject(name + "_energyBar", energyBar);
            engine.SpawnObject(name + "_energyTextObj", energyTextObj);
            engine.SpawnObject(name + "_xpText", xp);
            engine.SpawnObject(name + "_xpBarContainer", xpBarContainer);
            engine.SpawnObject(name + "_xpBar", xpBar);
            engine.SpawnObject(name + "_levelTextObj", levelTextObj);
            engine.SpawnObject(name + "_spellText", spellTextObj);
            engine.SpawnObject(name + "_spellCdBarContainer", spellCdBarContainer);
            engine.SpawnObject(name + "_spellCdBar", spellCdBar);
        }

        public void UpdateEnergyBar()
        {
            var player = Player.Instance;
            var energy = (int)player.Level.Energy;
            var newWidth = (int)((energyBarContainer.width - border * 2) * (energy / (double)player.Level.MaxEnergy));
            energyTextObj.text = $"{energy} / {player.Level.MaxEnergy}";
            if (energyBar != null)
            {
                energyBar.width = newWidth;
            }
        }

        public void UpdateHpBar()
        {
            var player = Player.Instance;
            var hp = (int)player.Level.Hp;
            var newWidth = (int)((hpBarContainer.width - border * 2) * (hp / (double)player.Level.MaxHp));
            hpTextObj.text = $"{hp} / {player.Level.MaxHp}";
            if (hpBar != null)
            {
                hpBar.width = newWidth;
            }
        }

        public void UpdateSpellBar()
        {
            var player = Player.Instance;
            if (player.Level == null)
                return;

            spellTextObj.text = player.spellManager.ChosenSpellName;
            var spellTextSize = TextRenderer.MeasureText(spellTextObj.text, spellTextObj.font);
            spellTextObj.x = spellCdBarContainer.x - spellTextSize.Width - padding;
            var lastCastedSpell = player.spellManager.LastCastedSpell;
            if (lastCastedSpell != null)
            {
                var newWidth = (int)(
                    (spellCdBarContainer.width - border * 2) *
                    (player.spellManager.spellsCd[lastCastedSpell.GetType()] / lastCastedSpell.StartingCd)
                    );
                spellCdBar.width = newWidth;
            }
        }

        public void UpdateXpBar()
        {
            var player = Player.Instance;
            var xp = (int)player.Xp;
            var level = player.Level;
            var levelManager = Player.Instance.LevelManager;
            var xpPercentage = Math.Min(1, (double)xp / levelManager.levelUpTable[level.level + 1].NeededXp);
            levelTextObj.text = Math.Round(xpPercentage * 100, 2) + "% to " + (level.level + 1);
            int newWidth;
            if (level.level == 99)
                newWidth = xpBarContainer.width - border * 2;
            else
                newWidth = (int)((xpBarContainer.width - border * 2) * xpPercentage);
            xpBar.width = newWidth;
        }

        public static Hud Instance => instance ?? (instance = new Hud());
    }
}