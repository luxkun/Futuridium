using System;
using System.Drawing;
using System.Windows.Forms;
using Aiv.Engine;

namespace StupidAivGame
{
    public class Pause : GameObject
    {
        public Pause()
        {
            name = "pause";
        }

        public override void Start()
        {
            Console.WriteLine("Paused");

            var background = new RectangleObject();
            background.color = Color.Black;
            background.fill = true;
            background.width = engine.width;
            background.height = engine.height;
            background.order = 10;
            engine.SpawnObject("pause_background", background);
            var pauseText = new TextObject("Phosphate", 80, "darkgreen");
            pauseText.text = "PAUSE";
            var pauseTextSize = TextRenderer.MeasureText(pauseText.text, pauseText.font);
            pauseText.x = engine.width/2 - pauseTextSize.Width/2;
            pauseText.y = engine.height/2 - pauseTextSize.Height/2;
            pauseText.order = 11;
            engine.SpawnObject("pause_text", pauseText);
        }

        public override void Update()
        {
        }
    }
}