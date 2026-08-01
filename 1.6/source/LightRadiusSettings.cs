using Verse;

namespace Lumen_Control
{
    public class LightRadiusRule : IExposable
    {
        public bool enabled = true;
        public float minRadius;
        public float maxRadius;

        public void ExposeData()
        {
            Scribe_Values.Look(ref enabled, "enabled", true);
            Scribe_Values.Look(ref minRadius, "minRadius");
            Scribe_Values.Look(ref maxRadius, "maxRadius");
        }
    }
}
