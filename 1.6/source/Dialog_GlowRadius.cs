using UnityEngine;
using Verse;

namespace Lumen_Control
{
    public class Dialog_GlowRadius : Window
    {
        private readonly CompGlower glower;
        private float proposedRadius;

        public override Vector2 InitialSize => new Vector2(320f, 200f);

        public Dialog_GlowRadius(CompGlower glower)
        {
            this.glower = glower;
            proposedRadius = glower.GlowRadius;
            forcePause = true;
            closeOnClickedOutside = true;
            doCloseX = true;
            absorbInputAroundWindow = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (glower.parent.Destroyed || !ModSettings.TryGetEnabledRule(glower.parent.def, out LightRadiusRule rule))
            {
                Close();
                return;
            }

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 30f), glower.parent.LabelCap);
            Text.Font = GameFont.Small;

            float curY = 36f;
            Widgets.Label(new Rect(0f, curY, inRect.width, 24f), "LumenControl.BaseRadius".Translate(glower.Props.glowRadius.ToString("F1")));
            curY += 24f;
            Widgets.Label(new Rect(0f, curY, inRect.width, 24f), "LumenControl.CurrentRadius".Translate(proposedRadius.ToString("F1")));
            curY += 30f;

            proposedRadius = Widgets.HorizontalSlider(new Rect(0f, curY, inRect.width, 24f), proposedRadius, rule.minRadius, rule.maxRadius, roundTo: 0.1f);
            curY += 40f;

            float buttonWidth = (inRect.width - 10f) / 2f;
            if (Widgets.ButtonText(new Rect(0f, inRect.height - 35f, buttonWidth, 35f), "CancelButton".Translate()))
            {
                Close();
            }
            if (Widgets.ButtonText(new Rect(buttonWidth + 10f, inRect.height - 35f, buttonWidth, 35f), "OK".Translate()))
            {
                LightRadiusUtility.Apply(glower, proposedRadius);
                Close();
            }
        }
    }
}
