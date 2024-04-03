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

            FormatLabel(thingDef);
            FormatDescription(thingDef);
        }

        private void FormatLabel(ThingDef thingDef)
        {
            if (!formatLabels)
                return;

            thingDef.label = Formatted(thingDef.label);
            Log.Message("New label is: " + thingDef.label);
        }

        private void FormatDescription(ThingDef thingDef)
        {
            if (!formatDescriptions)
                return;

            thingDef.description = Formatted(thingDef.description);
        }
        private string Formatted(string toFormat) => string.Format(toFormat, hediffDef.label, diseaseType.ToString().ToLower()).CapitalizeFirst();
    }

    public enum DiseaseType
    {
        Bacteria,
        Virus,
        Parasite,
        Mechanites
    }
}
