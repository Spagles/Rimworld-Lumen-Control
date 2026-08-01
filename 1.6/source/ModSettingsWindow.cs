using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Lumen_Control
{
    public static class ModSettingsWindow
    {
        private const float RowHeight = 30f;
        private const float HeaderHeight = 44f;
        private const float SearchBarHeight = 24f;
        private const float LightColumnFraction = 0.42f;
        private const float EnableColumnWidth = 100f;
        private const float NumericColumnWidth = 70f;
        private const float ResetColumnWidth = 70f;

        private static readonly Dictionary<string, string> MinBuffers = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> MaxBuffers = new Dictionary<string, string>();
        private static readonly QuickSearchWidget SearchWidget = new QuickSearchWidget();

        private static Vector2 scrollPosition;

        public static void Draw(Rect parent)
        {
            ModSettings.RemoveInvalidRuleEntries();

            Rect searchRect = new Rect(parent.x, parent.y, parent.width, SearchBarHeight);
            SearchWidget.OnGUI(searchRect);

            List<ThingDef> glowerDefs = DefDatabase<ThingDef>.AllDefsListForReading
                .Where(ModSettings.IsValidGlowerDef)
                .Where(def => SearchWidget.filter.Matches(def.label) || SearchWidget.filter.Matches(def.defName))
                .OrderBy(def => def.LabelCap.ToString())
                .ThenBy(def => def.defName)
                .ToList();

            Rect headerRect = new Rect(parent.x, parent.y + SearchBarHeight, parent.width, HeaderHeight);
            DrawHeader(headerRect);

            Rect outRect = new Rect(parent.x, parent.y + SearchBarHeight + HeaderHeight, parent.width, parent.height - SearchBarHeight - HeaderHeight);
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, glowerDefs.Count * RowHeight);

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            float curY = 0f;
            foreach (ThingDef def in glowerDefs)
            {
                DrawRow(new Rect(0f, curY, viewRect.width, RowHeight), def);
                curY += RowHeight;
            }
            Widgets.EndScrollView();
        }

        private static void DrawHeader(Rect rect)
        {
            float lightWidth = rect.width * LightColumnFraction;
            float x = rect.x;

            Widgets.Label(new Rect(x, rect.y, lightWidth, rect.height), "LumenControl.ColumnLight".Translate());
            x += lightWidth;
            Widgets.Label(new Rect(x, rect.y, EnableColumnWidth, rect.height), "LumenControl.ColumnEnable".Translate());
            x += EnableColumnWidth;
            Widgets.Label(new Rect(x, rect.y, NumericColumnWidth, rect.height), "LumenControl.ColumnMinimum".Translate());
            x += NumericColumnWidth;
            Widgets.Label(new Rect(x, rect.y, NumericColumnWidth, rect.height), "LumenControl.ColumnMaximum".Translate());

            Widgets.DrawLineHorizontal(rect.x, rect.yMax, rect.width);
        }

        private static void DrawRow(Rect rect, ThingDef def)
        {
            LightRadiusRule rule = ModSettings.GetOrCreateRule(def);
            float xmlRadius = def.GetCompProperties<CompProperties_Glower>().glowRadius;

            float lightWidth = rect.width * LightColumnFraction;
            float x = rect.x;

            Rect lightRect = new Rect(x, rect.y, lightWidth, rect.height);
            TaggedString lightLabel = "LumenControl.LightRow".Translate(def.LabelCap, def.defName, xmlRadius.ToString("F1"));
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(lightRect, lightLabel.Truncate(lightWidth - 4f));
            Text.Anchor = TextAnchor.UpperLeft;
            TooltipHandler.TipRegion(lightRect, lightLabel);
            x += lightWidth;

            bool enabled = rule.enabled;
            Widgets.Checkbox(new Vector2(x + EnableColumnWidth / 2f - 12f, rect.y + 3f), ref enabled);
            rule.enabled = enabled;
            x += EnableColumnWidth;

            DrawNumericField(new Rect(x + 4f, rect.y + 3f, NumericColumnWidth - 8f, rect.height - 6f), def, rule, isMin: true);
            x += NumericColumnWidth;

            DrawNumericField(new Rect(x + 4f, rect.y + 3f, NumericColumnWidth - 8f, rect.height - 6f), def, rule, isMin: false);
            x += NumericColumnWidth;

            if (Widgets.ButtonText(new Rect(x + 4f, rect.y + 2f, ResetColumnWidth - 8f, rect.height - 4f), "LumenControl.Reset".Translate()))
            {
                ModSettings.ResetRule(def, rule);
                MinBuffers.Remove(def.defName);
                MaxBuffers.Remove(def.defName);
            }
        }

        private static void DrawNumericField(Rect rect, ThingDef def, LightRadiusRule rule, bool isMin)
        {
            Dictionary<string, string> buffers = isMin ? MinBuffers : MaxBuffers;
            if (!buffers.TryGetValue(def.defName, out string buffer))
            {
                buffer = (isMin ? rule.minRadius : rule.maxRadius).ToString("F1");
            }

            float value = isMin ? rule.minRadius : rule.maxRadius;
            Widgets.TextFieldNumeric(rect, ref value, ref buffer, ModSettings.MinGlowRadius, ModSettings.MaxGlowRadius);
            buffers[def.defName] = buffer;

            if (isMin)
            {
                rule.minRadius = value;
            }
            else
            {
                rule.maxRadius = value;
            }

            float minBefore = rule.minRadius;
            float maxBefore = rule.maxRadius;
            ModSettings.NormalizeRule(rule);

            if (!Mathf.Approximately(minBefore, rule.minRadius))
            {
                MinBuffers[def.defName] = rule.minRadius.ToString("F1");
            }
            if (!Mathf.Approximately(maxBefore, rule.maxRadius))
            {
                MaxBuffers[def.defName] = rule.maxRadius.ToString("F1");
            }
        }
    }
}
