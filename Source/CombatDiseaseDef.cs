using System.Collections.Generic;
using Verse;

namespace USH_BW
{
    public class CombatDiseaseDef : Def
    {
        public List<HediffDef> samplableHediffDefs = [];
        public HediffDef giveHediffDef;
        public DiseaseType diseaseType;
        public ColorInt colorInt;
        public string overrideDiseaseTypeLabel;

        public List<ThingDef> thingDefsToFormat = [];
        public List<ThingDef> thingDefsToColor = [];
        public List<ThingDef> thingDefsToColorIcons = [];

        public List<ResearchProjectDef> researchProjectsDefsToFormat = [];
        public List<HediffDef> hediffDefsToFormat = [];

        public ThingDef sampleDef;
        public ThingDef pathogenDef;

        public ResearchProjectDef vaccineResProjectDef;

        private const float GAS_ALPHA = 0.75f;

        public override string ToString() => string.Format("{0} ({1})", defName, diseaseType.ToStringUncapitalized());

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string text in base.ConfigErrors())
                yield return text;

            if (giveHediffDef == null)
                yield return "giveHediffDef is null";

            if (colorInt.a == 0)
                yield return "colorInt is fully transparent";

            yield break;
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();

            foreach (ThingDef thingDef in thingDefsToColor)
                ColorThingDef(thingDef);

            foreach (ThingDef thingDef in thingDefsToColor)
                ColorThingDef(thingDef);

            foreach (ThingDef thingDef in thingDefsToColorIcons)
                ColorThingDefIcon(thingDef);
        }

        private void ColorThingDef(ThingDef thingDef)
        {
            if (thingDef == null)
                return;

            if (thingDef.building != null && thingDef.building.turretTopLoadedGraphic != null)
                thingDef.building.turretTopLoadedGraphic.color = colorInt.ToColor;

            thingDef.graphicData.color = NewThingColor(thingDef, colorInt.ToColor);
        }

        private UnityEngine.Color NewThingColor(ThingDef thingDef, UnityEngine.Color color)
        {
            if (thingDef.thingClass == typeof(PathogenGas))
                return color.ToTransparent(GAS_ALPHA);

            return color;
        }

        private void ColorThingDefIcon(ThingDef thingDef)
        {
            if (thingDef == null)
                return;

            thingDef.uiIconColor = colorInt.ToColor;
        }

        public bool CanBeSampled => samplableHediffDefs.Count != 0;
    }

    public enum DiseaseType
    {
        Bacteria,
        Virus,
        Parasite,
        Mechanites,
        Chemical,
        Custom
    }
}
