using System.Diagnostics;
using System.Drawing;
using Aiv.Engine;

namespace Futuridium.UI
{
    public class Pause : GameObject
    {
        public Pause()
        {
            Name = "pause";

            OnDestroy += DestroyEvent;
        }

        private void DestroyEvent(object sender)
        {
            Engine.TimeModifier = 1f;

            AudioSource.Resume();

            Hud.Instance.DestroyStats();
        }

        public override void Start()
        {
            base.Start();
            Debug.WriteLine("Paused");

            Engine.TimeModifier = 0f;

            var background = new RectangleObject(Engine.Width, Engine.Height)
            {
                Color = Color.FromArgb(125, 0, 0, 0),
                Fill = true,
                Order = 10,
                IgnoreCamera = true
            };
            Engine.SpawnObject("pause_background", background);
            var pauseText = new TextObject(1.33f, Color.White) {Text = "PAUSE", IgnoreCamera = true}; //DarkGreen
            var pauseTextSize = pauseText.Measure();
            pauseText.X = Engine.Width/2f - pauseTextSize.X/ 2;
            pauseText.Y = Engine.Height/2f - pauseTextSize.Y/ 2;
            pauseText.Order = 11;
            Engine.SpawnObject("pause_text", pauseText);

            AudioSource.Pause();

            Hud.Instance.SpawnStats();
        }
    }
}