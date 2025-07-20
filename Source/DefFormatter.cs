using System.Collections.Generic;
using System.Linq;
using Verse;

namespace USH_BW
{
    public class DefFormatter
    {
        public List<Def> DefsToFormat { get; private set; } = new List<Def>();
        public List<object> FormatArgs { get; private set; } = new List<object>();

        public DefFormatter(List<Def> toFormat, List<object> args)
        {
            DefsToFormat = toFormat;
            FormatArgs = args;
        }

        public void AddDefsToFormat(List<Def> toFormat) => DefsToFormat.AddRange(toFormat);
        public void AddDefToFormat(Def toFormat) => DefsToFormat.Add(toFormat);

        public void Format()
        {
            foreach (Def def in DefsToFormat)
                FormatDef(def);
        }

        private void FormatDef(Def def)
        {
            if (def == null)
                return;

            def.label = Formatted(def.label);

            def.description = Formatted(def.description);

            FormatThingDef(def as ThingDef);
        }

        private void FormatThingDef(ThingDef thingDef)
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

        private string Formatted(string toFormat)
        {
            if (string.IsNullOrEmpty(toFormat))
                return toFormat;

            return string.Format(toFormat, FormatArgs.ToArray()).CapitalizeFirst();
        }
    }
}
