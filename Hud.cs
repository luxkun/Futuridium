using System;
using System.Drawing;
using System.Windows.Forms;
using Aiv.Engine;

namespace Futuridium
{
    public class Hud : GameObject
    {
        private const int hudWidth = 200;
        private const int fontSize = 16;
        // Hp bar
        protected RectangleObject hpBar;
        protected RectangleObject hpBarContainer;
        protected TextObject hpTextObj;
        // Energy bar
        protected RectangleObject energyBar;
        protected RectangleObject energyBarContainer;
        protected TextObject energyTextObj;
        // Xp bar
        protected TextObject levelTextObj;
        protected RectangleObject xpBar;
        protected RectangleObject xpBarContainer;

        public Hud()
        {
            order = 9;
            name = "hud";
        }

        public override void Start()
        {
            var border = 1;
            var padding = 10;
            var HP = new TextObject("Arial Black", fontSize, "darkred")
            {
                order = order,
                text = "HP",
                x = padding,
                y = padding
            };
            var HPSize = TextRenderer.MeasureText(HP.text, HP.font);
            hpBarContainer = new RectangleObject
            {
                x = HP.x + padding + HPSize.Width,
                y = HP.y,
                order = order,
                width = hudWidth,
                height = HPSize.Height,
                color = Color.Black
            };
            hpBar = new RectangleObject
            {
                order = order,
                x = hpBarContainer.x + border,
                y = hpBarContainer.y + border,
                color = Color.DarkRed,
                fill = true,
                height = hpBarContainer.height - border*2
            };
            hpTextObj = new TextObject("Arial Black", fontSize, "darkred")
            {
                order = order,
                x = hpBarContainer.x + hpBarContainer.width + padding,
                y = HP.y
            };

            var energyText = new TextObject("Arial Black", fontSize, "darkred")
            {
                text = "EN",
                x = padding,
                order = order,
                y = padding + HP.y + HPSize.Height
            };
            var energyTextSize = TextRenderer.MeasureText(energyText.text, energyText.font);
            energyBarContainer = new RectangleObject
            {
                x = energyText.x + padding + energyTextSize.Width,
                y = energyText.y,
                order = order,
                width = hudWidth,
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
                height = energyBarContainer.height - border*2
            };
            energyTextObj = new TextObject("Arial Black", fontSize, "darkred")
            {
                order = order,
                x = energyBarContainer.x + energyBarContainer.width + padding,
                y = energyText.y
            };

            var XP = new TextObject("Arial Black", fontSize, "darkgreen")
            {
                order = order,
                text = "XP",
                x = padding,
                y = padding + energyText.y + energyTextSize.Height
            };
            var XPSize = TextRenderer.MeasureText(XP.text, XP.font);
            xpBarContainer = new RectangleObject
            {
                x = XP.x + padding + XPSize.Width,
                y = XP.y,
                order = order,
                width = hudWidth,
                height = XPSize.Height,
                color = Color.Black
            };
            xpBar = new RectangleObject
            {
                order = order,
                x = xpBarContainer.x + border,
                y = xpBarContainer.y + border,
                color = Color.DarkOliveGreen,
                fill = true,
                height = xpBarContainer.height - border*2
            };
            levelTextObj = new TextObject("Arial Black", fontSize, "darkgreen")
            {
                order = order,
                text = "0% to 1",
                x = xpBarContainer.x + xpBarContainer.width + padding,
                y = XP.y
            };

            engine.SpawnObject(name + "_hpText", HP);
            engine.SpawnObject(name + "_hpBarContainer", hpBarContainer);
            engine.SpawnObject(name + "_hpBar", hpBar);
            engine.SpawnObject(name + "_hpTextObj", hpTextObj);
            engine.SpawnObject(name + "_energyText", energyText);
            engine.SpawnObject(name + "_energyBarContainer", energyBarContainer);
            engine.SpawnObject(name + "_energyBar", energyBar);
            engine.SpawnObject(name + "_energyTextObj", energyTextObj);
            engine.SpawnObject(name + "_xpText", XP);
            engine.SpawnObject(name + "_xpBarContainer", xpBarContainer);
            engine.SpawnObject(name + "_xpBar", xpBar);
            engine.SpawnObject(name + "_levelTextObj", levelTextObj);
        }

        public void UpdateXPBar()
        {
            var player = ((Game) engine.objects["game"]).Player;
            if (player.Level == null)
                return;
            var xp = (int)player.Xp;
            var level = player.Level;
            var levelManager = ((Game) engine.objects["game"]).Player.LevelManager;
            var xpPercentage = Math.Min(1, (double) xp/levelManager.levelUpTable[level.level + 1].NeededXp);
            levelTextObj.text = Math.Round(xpPercentage*100, 2) + "% to " + (level.level + 1);
            var border = 1; //1px border?
            int newWidth;
            if (level.level == 99)
                newWidth = xpBarContainer.width - border*2;
            else
                newWidth = (int) ((xpBarContainer.width - border*2)*xpPercentage);
            if (xpBar != null)
            {
                xpBar.width = newWidth;
            }
        }

        public void UpdateEnergyBar()
        {
            var player = ((Game)engine.objects["game"]).Player;
            if (player.Level == null)
                return;
            var border = 1; //1px border?
            var energy = (int) player.Level.Energy;
            var newWidth = (int)((energyBarContainer.width - border * 2) * (energy / (double)player.Level.maxEnergy));
            energyTextObj.text = string.Format("{0} / {1}", energy, player.Level.maxEnergy);
            if (energyBar != null)
            {
                energyBar.width = newWidth;
            }
        }

        public void UpdateHPBar()
        {
            var player = ((Game) engine.objects["game"]).Player;
            if (player.Level == null)
                return;
            var border = 1; //1px border?
            var hp = (int) player.Level.Hp;
            var newWidth = (int) ((hpBarContainer.width - border*2)*(hp/(double) player.Level.maxHp));
            hpTextObj.text = string.Format("{0} / {1}", hp, player.Level.maxHp);
            if (hpBar != null)
            {
                hpBar.width = newWidth;
            }
        }
    }
}