using System;
using Aiv.Engine;
using System.Drawing;
using System.Windows.Forms;

namespace StupidAivGame
{
	public class Hud : GameObject
	{
		const int hudWidth = 200;
		protected RectangleObject hpBarContainer;
		protected RectangleObject hpBar = null;
		protected RectangleObject xpBarContainer;
		protected RectangleObject xpBar = null;
		protected TextObject levelTextObj;
		protected TextObject hpTextObj;
		public Hud ()
		{
			this.order = 9;
			this.name = "hud";
		}

		public override void Start ()
		{
			int border = 1;
			int padding = 10;
			TextObject HP = new TextObject ("Arial Black", 18, "darkred");
			HP.order = this.order;
			HP.text = "HP";
			HP.x = padding;
			HP.y = padding;
			Size HPSize = TextRenderer.MeasureText (HP.text, HP.font);
			hpBarContainer = new RectangleObject ();
			hpBarContainer.x = HP.x + padding + HPSize.Width;
			hpBarContainer.y = HP.y;
			hpBarContainer.order = this.order;
			hpBarContainer.width = hudWidth;
			hpBarContainer.height = HPSize.Height;
			hpBarContainer.color = Color.Black;
			hpBar = new RectangleObject ();
			hpBar.order = this.order;
			hpBar.x = hpBarContainer.x + border;
			hpBar.y = hpBarContainer.y + border;
			hpBar.color = Color.DarkRed;
			hpBar.fill = true;
			hpBar.height = hpBarContainer.height - border * 2;
			hpTextObj = new TextObject ("Arial Black", 18, "darkred");
			hpTextObj.order = this.order;
			hpTextObj.x = hpBarContainer.x + hpBarContainer.width + padding;
			hpTextObj.y = HP.y;

			TextObject XP = new TextObject ("Arial Black", 18, "darkgreen");
			XP.order = this.order;
			XP.text = "XP";
			XP.x = padding;
			XP.y = padding + HP.y + HPSize.Height;
			Size XPSize = TextRenderer.MeasureText (XP.text, XP.font);
			xpBarContainer = new RectangleObject ();
			xpBarContainer.x = XP.x + padding + XPSize.Width;
			xpBarContainer.y = XP.y;
			xpBarContainer.order = this.order;
			xpBarContainer.width = hudWidth;
			xpBarContainer.height = XPSize.Height;
			xpBarContainer.color = Color.Black;
			xpBar = new RectangleObject ();
			xpBar.order = this.order;
			xpBar.x = xpBarContainer.x + border;
			xpBar.y = xpBarContainer.y + border;
			xpBar.color = Color.DarkOliveGreen;
			xpBar.fill = true;
			xpBar.height = xpBarContainer.height - border * 2;
			levelTextObj = new TextObject ("Arial Black", 18, "darkgreen");
			levelTextObj.order = this.order;
			levelTextObj.text = "0% to 1";
			levelTextObj.x = xpBarContainer.x + xpBarContainer.width + padding;
			levelTextObj.y = XP.y;

			engine.SpawnObject (this.name + "_hpText", HP);
			engine.SpawnObject (this.name + "_hpBarContainer", hpBarContainer);
			engine.SpawnObject (this.name + "_hpBar", hpBar);
			engine.SpawnObject (this.name + "_hpTextObj", hpTextObj);
			engine.SpawnObject (this.name + "_xpText", XP);
			engine.SpawnObject (this.name + "_xpBarContainer", xpBarContainer);
			engine.SpawnObject (this.name + "_xpBar", xpBar);
			engine.SpawnObject (this.name + "_levelTextObj", levelTextObj);
		}

		public void UpdateXPBar ()
		{
			Player player = ((Game)engine.objects ["game"]).player;
			if (player.level == null)
				return;
			long xp = player.xp;
			Level level = player.level;
			LevelManager levelManager = ((Game)engine.objects ["game"]).player.levelManager;
			double xpPercentage = Math.Min(1, (double)xp / levelManager.levelUpTable [level.level + 1].neededXP);
			levelTextObj.text = Math.Round(xpPercentage * 100, 2) + "% to " + (level.level + 1);
			int border = 1; //1px border?
			int newWidth;
			if (level.level == 99)
				newWidth = xpBarContainer.width - border*2;
			else
				newWidth = (int)((xpBarContainer.width - border*2) * xpPercentage);
			bool update = false;
			if (xpBar == null || (Math.Abs(newWidth - xpBar.width) > 10)) { // ignore small changes
				update = true;
			}
			if (update) {
				xpBar.width = newWidth;
			}
		}

		public void UpdateHPBar ()
		{
			Player player = ((Game)engine.objects ["game"]).player;
			if (player.level == null)
				return;
			int border = 1; //1px border?
			int newWidth = (int)((hpBarContainer.width - border*2) * ((double)player.level.hp / (double)player.level.maxHP));
			hpTextObj.text = string.Format ("{0} / {1}", player.level.hp, player.level.maxHP);
			bool update = false;
			if (hpBar == null || (Math.Abs(newWidth - hpBar.width) > 10)) { // ignore small changes
				update = true;
			}
			if (update) {
				hpBar.width = newWidth;
			}
		}
	}
}

