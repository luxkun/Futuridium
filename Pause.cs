using Aiv.Engine;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Futuridium
{
    public class Pause : GameObject
    {
        public Pause()
        {
            name = "pause";
        }

        public override void Start()
        {
            base.Start();
            Debug.WriteLine("Paused");

            var background = new RectangleObject
            {
                color = Color.Black,
                fill = true,
                width = engine.width,
                height = engine.height,
                order = 10
            };
            engine.SpawnObject("pause_background", background);
            var pauseText = new TextObject("Phosphate", 80, "darkgreen") { text = "PAUSE" };
            var pauseTextSize = TextRenderer.MeasureText(pauseText.text, pauseText.font);
            pauseText.x = engine.width / 2 - pauseTextSize.Width / 2;
            pauseText.y = engine.height / 2 - pauseTextSize.Height / 2;
            pauseText.order = 11;
            engine.SpawnObject("pause_text", pauseText);
        }
    }
}