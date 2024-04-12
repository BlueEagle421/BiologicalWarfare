using System.Collections.Generic;
using System.Linq;
using Verse;

namespace BiologicalWarfare
{
    public class CombatDiseaseDef : Def
    {
        public HediffDef hediffDef;
        public DiseaseType diseaseType;
        public ColorInt colorInt;
        public bool canBeSampled;

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

            if (canBeSampled && sampleDef == null)
                yield return "canBeSampled is true, but sampleDef is null";

            if (colorInt.a == 0)
                yield return "colorInt is fully transparent";

            yield break;
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();

            AddHediffHyperlink();

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

        private void AddHediffHyperlink()
        {
            if (!canBeSampled)
                return;

            List<DefHyperlink> hyperlinks = USHDefOf.USH_DiseaseSampler.descriptionHyperlinks ?? new List<DefHyperlink>();
            hyperlinks.Add(new DefHyperlink(hediffDef));
            USHDefOf.USH_DiseaseSampler.descriptionHyperlinks = hyperlinks;
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
            {
                frameDef.label = Formatted(frameDef.label);
                frameDef.description = Formatted(frameDef.description);
            }

            foreach (RecipeDef recipeDef in DefDatabase<RecipeDef>.AllDefs.Where(x => IsRecipeRelevantFor(x, thingDef)))
            {
                recipeDef.label = Formatted(recipeDef.label);

                if (!string.IsNullOrEmpty(recipeDef.description))
                    recipeDef.description = Formatted(recipeDef.description);
            }

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

        private bool IsRecipeRelevantFor(RecipeDef recipeDef, ThingDef toFormat)
        {
            if (recipeDef.defName == "Make_" + toFormat.defName)
                return true;

            if (recipeDef.defName == "Make_" + toFormat.defName + "Bulk")
                return true;

            if (recipeDef.defName == "Administer_" + toFormat.defName)
                return true;

            return false;
        }
    }

    public enum DiseaseType
    {
        Bacteria,
        Virus,
        Parasite,
        Mechanites,
        Custom
    }
}
