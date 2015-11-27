using System;
using System.Drawing;
using System.Windows.Forms;
using Aiv.Engine;

namespace StupidAivGame
{
    public class Hud : GameObject
    {
        private const int hudWidth = 200;
        private const int fontSize = 16;
        protected RectangleObject hpBar;
        protected RectangleObject hpBarContainer;
        protected TextObject hpTextObj;
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
            var HP = new TextObject("Arial Black", fontSize, "darkred");
            HP.order = order;
            HP.text = "HP";
            HP.x = padding;
            HP.y = padding;
            var HPSize = TextRenderer.MeasureText(HP.text, HP.font);
            hpBarContainer = new RectangleObject();
            hpBarContainer.x = HP.x + padding + HPSize.Width;
            hpBarContainer.y = HP.y;
            hpBarContainer.order = order;
            hpBarContainer.width = hudWidth;
            hpBarContainer.height = HPSize.Height;
            hpBarContainer.color = Color.Black;
            hpBar = new RectangleObject();
            hpBar.order = order;
            hpBar.x = hpBarContainer.x + border;
            hpBar.y = hpBarContainer.y + border;
            hpBar.color = Color.DarkRed;
            hpBar.fill = true;
            hpBar.height = hpBarContainer.height - border*2;
            hpTextObj = new TextObject("Arial Black", fontSize, "darkred");
            hpTextObj.order = order;
            hpTextObj.x = hpBarContainer.x + hpBarContainer.width + padding;
            hpTextObj.y = HP.y;

            var XP = new TextObject("Arial Black", fontSize, "darkgreen");
            XP.order = order;
            XP.text = "XP";
            XP.x = padding;
            XP.y = padding + HP.y + HPSize.Height;
            var XPSize = TextRenderer.MeasureText(XP.text, XP.font);
            xpBarContainer = new RectangleObject();
            xpBarContainer.x = XP.x + padding + XPSize.Width;
            xpBarContainer.y = XP.y;
            xpBarContainer.order = order;
            xpBarContainer.width = hudWidth;
            xpBarContainer.height = XPSize.Height;
            xpBarContainer.color = Color.Black;
            xpBar = new RectangleObject();
            xpBar.order = order;
            xpBar.x = xpBarContainer.x + border;
            xpBar.y = xpBarContainer.y + border;
            xpBar.color = Color.DarkOliveGreen;
            xpBar.fill = true;
            xpBar.height = xpBarContainer.height - border*2;
            levelTextObj = new TextObject("Arial Black", fontSize, "darkgreen");
            levelTextObj.order = order;
            levelTextObj.text = "0% to 1";
            levelTextObj.x = xpBarContainer.x + xpBarContainer.width + padding;
            levelTextObj.y = XP.y;

            engine.SpawnObject(name + "_hpText", HP);
            engine.SpawnObject(name + "_hpBarContainer", hpBarContainer);
            engine.SpawnObject(name + "_hpBar", hpBar);
            engine.SpawnObject(name + "_hpTextObj", hpTextObj);
            engine.SpawnObject(name + "_xpText", XP);
            engine.SpawnObject(name + "_xpBarContainer", xpBarContainer);
            engine.SpawnObject(name + "_xpBar", xpBar);
            engine.SpawnObject(name + "_levelTextObj", levelTextObj);
        }

        public void UpdateXPBar()
        {
            var player = ((Game) engine.objects["game"]).player;
            if (player.level == null)
                return;
            var xp = player.xp;
            var level = player.level;
            var levelManager = ((Game) engine.objects["game"]).player.levelManager;
            var xpPercentage = Math.Min(1, (double) xp/levelManager.levelUpTable[level.level + 1].neededXP);
            levelTextObj.text = Math.Round(xpPercentage*100, 2) + "% to " + (level.level + 1);
            var border = 1; //1px border?
            int newWidth;
            if (level.level == 99)
                newWidth = xpBarContainer.width - border*2;
            else
                newWidth = (int) ((xpBarContainer.width - border*2)*xpPercentage);
            var update = false;
            if (xpBar == null || (Math.Abs(newWidth - xpBar.width) > 10))
            {
                // ignore small changes
                update = true;
            }
            if (update)
            {
                xpBar.width = newWidth;
            }
        }

        public void UpdateHPBar()
        {
            var player = ((Game) engine.objects["game"]).player;
            if (player.level == null)
                return;
            var border = 1; //1px border?
            var newWidth = (int) ((hpBarContainer.width - border*2)*(player.level.hp/(double) player.level.maxHP));
            hpTextObj.text = string.Format("{0} / {1}", player.level.hp, player.level.maxHP);
            var update = false;
            if (hpBar == null || (Math.Abs(newWidth - hpBar.width) > 10))
            {
                // ignore small changes
                update = true;
            }
            if (update)
            {
                hpBar.width = newWidth;
            }
        }
    }
}