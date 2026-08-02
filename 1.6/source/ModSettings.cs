using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Lumen_Control
{
    public class ModSettings : Verse.ModSettings
    {
        public const float MinGlowRadius = 0f;
        public const float MaxGlowRadius = 40f;
        public const float DefaultRangePadding = 5f;

        public static Dictionary<string, LightRadiusRule> LightRulesByDefName = new Dictionary<string, LightRadiusRule>();

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref LightRulesByDefName, "lightRulesByDefName", LookMode.Value, LookMode.Deep);
            if (LightRulesByDefName == null)
            {
                LightRulesByDefName = new Dictionary<string, LightRadiusRule>();
            }
        }

        public static bool IsValidGlowerDef(ThingDef def)
        {
            return def != null && def.GetCompProperties<CompProperties_Glower>() != null;
        }

        public static LightRadiusRule GetOrCreateRule(ThingDef def)
        {
            if (!IsValidGlowerDef(def))
            {
                return null;
            }

            if (!LightRulesByDefName.TryGetValue(def.defName, out LightRadiusRule rule))
            {
                rule = DefaultRule(def);
                LightRulesByDefName[def.defName] = rule;
            }
            return rule;
        }

        public static bool TryGetEnabledRule(ThingDef def, out LightRadiusRule rule)
        {
            rule = GetOrCreateRule(def);
            return rule != null && rule.enabled;
        }

        public static void NormalizeRule(LightRadiusRule rule)
        {
            rule.minRadius = Mathf.Clamp(rule.minRadius, MinGlowRadius, MaxGlowRadius);
            rule.maxRadius = Mathf.Clamp(rule.maxRadius, MinGlowRadius, MaxGlowRadius);
            if (rule.minRadius > rule.maxRadius)
            {
                float swap = rule.minRadius;
                rule.minRadius = rule.maxRadius;
                rule.maxRadius = swap;
            }
        }

        public static void ResetRule(ThingDef def, LightRadiusRule rule)
        {
            LightRadiusRule defaults = DefaultRule(def);
            rule.enabled = defaults.enabled;
            rule.minRadius = defaults.minRadius;
            rule.maxRadius = defaults.maxRadius;
        }

        public static void RemoveInvalidRuleEntries()
        {
            if (DefDatabase<ThingDef>.DefCount == 0)
            {
                return;
            }

            List<string> invalidKeys = new List<string>();
            foreach (string defName in LightRulesByDefName.Keys)
            {
                if (!IsValidGlowerDef(DefDatabase<ThingDef>.GetNamedSilentFail(defName)))
                {
                    invalidKeys.Add(defName);
                }
            }
            foreach (string defName in invalidKeys)
            {
                LightRulesByDefName.Remove(defName);
            }
        }

        private static LightRadiusRule DefaultRule(ThingDef def)
        {
            float xmlRadius = def.GetCompProperties<CompProperties_Glower>().glowRadius;
            LightRadiusRule rule = new LightRadiusRule
            {
                enabled = true,
                minRadius = Mathf.Max(MinGlowRadius, xmlRadius - DefaultRangePadding),
                maxRadius = Mathf.Min(MaxGlowRadius, xmlRadius + DefaultRangePadding)
            };
            NormalizeRule(rule);
            return rule;
        }
    }
}
