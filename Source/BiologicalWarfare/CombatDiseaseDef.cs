using System.Collections.Generic;
using Verse;

namespace BiologicalWarfare
{
    public class CombatDiseaseDef : Def
    {
        public HediffDef hediffDef;
        public DiseaseType diseaseType;
        public ColorInt colorInt;
        public bool formatLabels;
        public bool formatDescriptions;
        public bool replaceColors;
        public ThingDef sampleDef;
        public ThingDef pathogenDef;
        public ThingDef shellDef;
        public ThingDef barrelDef;
        public ThingDef launcherDef;

        public override string ToString() => string.Format("{0} ({1})", defName, hediffDef.ToString());

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string text in base.ConfigErrors())
                yield return text;

            if (hediffDef == null)
                yield return "hediffDef is null";

            if (colorInt.a == 0)
                yield return "colorInt is fully transparent";

            yield break;
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();

            FormatLabelAndDescription(sampleDef);
            FormatLabelAndDescription(pathogenDef);
            FormatLabelAndDescription(shellDef);
            FormatLabelAndDescription(barrelDef);
            FormatLabelAndDescription(launcherDef);

            Log.Message(ToString());
        }

        private void FormatLabelAndDescription(ThingDef thingDef)
        {
            if (thingDef == null)
                return;

            if (formatLabels)
                thingDef.label = Formatted(thingDef.label);

            if (formatDescriptions)
                thingDef.description = Formatted(thingDef.description);

            //if (replaceColors)
            //    thingDef.graphicData.color = colorInt.ToColor;
        }

        private string Formatted(string toFormat) => string.Format(toFormat, hediffDef.label, diseaseType.ToStringUncapitalized()).CapitalizeFirst();
    }

    public enum DiseaseType
    {
        Bacteria,
        Virus,
        Parasite,
        Mechanites
    }
}
