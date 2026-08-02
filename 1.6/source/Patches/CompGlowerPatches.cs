using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Lumen_Control.Patches
{
    [HarmonyPatch(typeof(CompGlower), "PostExposeData")]
    public static class CompGlower_PostExposeData_Patch
    {
        public static void Postfix(CompGlower __instance)
        {
            bool hasOverride = Scribe.mode == LoadSaveMode.Saving && !Mathf.Approximately(__instance.GlowRadius, __instance.Props.glowRadius);
            float radius = __instance.GlowRadius;

            Scribe_Values.Look(ref hasOverride, "lumenControlHasGlowRadiusOverride", false);
            if (hasOverride)
            {
                Scribe_Values.Look(ref radius, "lumenControlGlowRadius", __instance.Props.glowRadius);
            }

            if (Scribe.mode == LoadSaveMode.LoadingVars && hasOverride)
            {
                LightRadiusUtility.Apply(__instance, radius);
            }
        }
    }

    [HarmonyPatch(typeof(CompGlower), "CompGetGizmosExtra")]
    public static class CompGlower_CompGetGizmosExtra_Patch
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, CompGlower __instance)
        {
            if (__result != null)
            {
                foreach (Gizmo gizmo in __result)
                {
                    yield return gizmo;
                }
            }

            if (!ModSettings.TryGetEnabledRule(__instance.parent.def, out _))
            {
                yield break;
            }

            yield return new Command_Action
            {
                defaultLabel = "LumenControl.AdjustGlowRadius".Translate(),
                defaultDesc = "LumenControl.AdjustGlowRadiusDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("LumenControl/UI/Commands/lightbulb"),
                groupable = false,
                action = delegate
                {
                    Find.WindowStack.Add(new Dialog_GlowRadius(__instance));
                }
            };
        }
    }
}
