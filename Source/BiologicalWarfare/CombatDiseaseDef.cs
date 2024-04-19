using System.Collections.Generic;
using Verse;

namespace BiologicalWarfare
{
    public class CombatDiseaseDef : Def
    {
        public List<HediffDef> samplableHediffDefs = new List<HediffDef>();
        public HediffDef giveHediffDef;
        public DiseaseType diseaseType;
        public ColorInt colorInt;

        public List<ThingDef> thingDefsToFormat = new List<ThingDef>();
        public List<ThingDef> thingDefsToColor = new List<ThingDef>();
        public List<ThingDef> thingDefsToFormatAndColor = new List<ThingDef>();

        public List<ResearchProjectDef> researchProjectsDefsToFormat = new List<ResearchProjectDef>();
        public List<HediffDef> hediffDefsToFormat = new List<HediffDef>();

        public ThingDef sampleDef;
        public ThingDef pathogenDef;
        public ThingDef filthDef;

        public ResearchProjectDef vaccineResProjectDef;

        private DefFormatter _defFormatter;

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

            _defFormatter = new DefFormatter(new List<Def>(), new List<object> { label, diseaseType.ToStringUncapitalized() });

            _defFormatter.AddDefsToFormat(thingDefsToFormat.ConvertAll(x => (Def)x));
            _defFormatter.AddDefsToFormat(thingDefsToFormatAndColor.ConvertAll(x => (Def)x));
            _defFormatter.AddDefsToFormat(researchProjectsDefsToFormat.ConvertAll(x => (Def)x));
            _defFormatter.AddDefsToFormat(hediffDefsToFormat.ConvertAll(x => (Def)x));

            _defFormatter.Format();

            foreach (ThingDef thingDef in thingDefsToColor)
                ColorThingDef(thingDef);

            foreach (ThingDef thingDef in thingDefsToFormatAndColor)
                ColorThingDef(thingDef);
        }

        private void ColorThingDef(ThingDef thingDef)
        {
            if (thingDef == null)
                return;

            thingDef.graphicData.color = colorInt.ToColor;
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
