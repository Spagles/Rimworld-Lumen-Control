using UnityEngine;
using Verse;

namespace Lumen_Control
{
    public static class LightRadiusUtility
    {
        public static void Apply(CompGlower glower, float requestedRadius)
        {
            LightRadiusRule rule = ModSettings.GetOrCreateRule(glower.parent.def);
            if (rule == null)
            {
                return;
            }

            ModSettings.NormalizeRule(rule);
            glower.GlowRadius = Mathf.Clamp(requestedRadius, rule.minRadius, rule.maxRadius);

            if (glower.parent.Spawned && glower.Glows)
            {
                glower.ForceRegister(glower.parent.Map);
            }
        }
    }
}
