using System.Collections.Generic;
using Verse;

namespace BiologicalWarfare
{
    public class CombatDiseaseDef : Def
    {
        public HediffDef hediffDef;
        public DiseaseType diseaseType;
        public ColorInt colorInt;
        public bool autoComplete;

        public List<ThingDef> thingDefsToFormat = new List<ThingDef>();
        public List<ThingDef> thingDefsToColor = new List<ThingDef>();
        public List<ThingDef> thingDefsToFormatAndColor = new List<ThingDef>();

        public ThingDef sampleDef;
        public ThingDef pathogenDef;

        public override string ToString() => string.Format("{0} ({1})", defName, diseaseType.ToStringUncapitalized());

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

            description = hediffDef.description;

            foreach (ThingDef thingDef in thingDefsToFormat)
                FormatDef(thingDef);

            foreach (ThingDef thingDef in thingDefsToColor)
                ColorThingDef(thingDef);

            foreach (ThingDef thingDef in thingDefsToFormatAndColor)
            {
                FormatDef(thingDef);
                ColorThingDef(thingDef);
            }
        }

        private void FormatDef(ThingDef thingDef)
        {
            if (thingDef == null)
                return;

            ThingDef blueprintDef = thingDef.blueprintDef;

            if (blueprintDef != null)
                blueprintDef.label = Formatted(blueprintDef.label);

            ThingDef installBlueprintDef = thingDef.installBlueprintDef;

            if (installBlueprintDef != null)
                installBlueprintDef.label = Formatted(installBlueprintDef.label);

            ThingDef frameDef = thingDef.frameDef;

            if (frameDef != null)
                frameDef.label = Formatted(frameDef.label);

            foreach (RecipeDef recipeDef in thingDef.AllRecipes)
                recipeDef.label = Formatted(recipeDef.label);


            thingDef.label = Formatted(thingDef.label);

            thingDef.description = Formatted(thingDef.description);
        }

        private void ColorThingDef(ThingDef thingDef)
        {
            if (thingDef == null)
                return;

            thingDef.graphicData.color = colorInt.ToColor;
        }

        private string Formatted(string toFormat)
        {
            if (string.IsNullOrEmpty(toFormat))
                return string.Empty;

            string type = diseaseType.ToStringUncapitalized();

            return string.Format(toFormat, label, type).CapitalizeFirst();
        }
    }

    public enum DiseaseType
    {
        Bacteria,
        Virus,
        Parasite,
        Mechanites
    }
}
