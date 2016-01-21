using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aiv.Engine;
using Futuridium.Characters;
using Futuridium.UI;

namespace Futuridium.Game
{
    class Score : GameObject
    {
        public float Value { get; private set; }
        private const float BaseXpScoreModifier = 1f;
        private const float LostPerSecond = 0.33f;
        private const float ModifierIncreasePerXpIncrease = 0.33f;

        private float startLosingTimer = 10f;
        public float XpScoreModifier { get; set; } = 0.9f;

        public override void Start()
        {
            base.Start();
            Player.Instance.OnXpChanged += XpChangedEvent;
            Player.Instance.OnDamageTaken += DamageTaken;
        }

        private void DamageTaken(object sender, float delta)
        {
            if (delta <= 0) return;
            XpScoreModifier = BaseXpScoreModifier;
        }

        private void XpChangedEvent(object sender, long delta)
        {
            if (delta <= 0) return;
            Value += delta * XpScoreModifier;
            XpScoreModifier += ModifierIncreasePerXpIncrease;
        }

        public override void Update()
        {
            base.Update();
            if (startLosingTimer >= 0)
                startLosingTimer -= DeltaTime;
            if (startLosingTimer <= 0)
                Value -= LostPerSecond*DeltaTime;

            Hud.Instance.UpdateScoreBar();
        }
    }
}
