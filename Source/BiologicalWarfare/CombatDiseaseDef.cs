using RimWorld;
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

        public DamageDef damageDef;
        public ThingDef gasDef;
        public ThingDef shellDef;
        public ThingDef shellBulletDef;
        public ThingDef barrelDef;
        public ThingDef launcherDef;

        public override string ToString() => string.Format("{0} ({1})", defName, diseaseType.ToStringUncapitalized());

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string text in base.ConfigErrors())
                yield return text;

            foreach (string text in ConfigAutoCompleteErrors())
                yield return text;

            if (hediffDef == null)
                yield return "hediffDef is null";

            if (colorInt.a == 0)
                yield return "colorInt is fully transparent";

            yield break;
        }

        private IEnumerable<string> ConfigAutoCompleteErrors()
        {
            if (!autoComplete)
                yield break;

            if (gasDef == null)
                yield return "autoComplete is True and gasDef is null";

            if (damageDef == null)
                yield return "autoComplete is True and damageDef is null";

            if (shellDef == null)
                yield return "autoComplete is True and shellDef is null";

            if (shellBulletDef == null)
                yield return "autoComplete is True and shellBulletDef is null";

            if (barrelDef == null)
                yield return "autoComplete is True and barrelDef is null";

            if (launcherDef == null)
                yield return "autoComplete is True and launcherDef is null";

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

            AutoCompleteDefs();
        }

        private void AutoCompleteDefs()
        {
            if (!autoComplete)
                return;

            OPToxic.OPToxicDefs toxicExtansion = new OPToxic.OPToxicDefs
            {
                OPToxicHediff = hediffDef.defName,
                OPToxicSeverity = 0.15f,
                OPSevUpTickPeriod = 60
            };

            if (gasDef.modExtensions == null)
                gasDef.modExtensions = new List<DefModExtension>() { toxicExtansion };
            else
                gasDef.modExtensions.Add(toxicExtansion);

            CompProperties_Explosive explosiveShell = shellDef.GetCompProperties<CompProperties_Explosive>();

            explosiveShell.explosiveDamageType = damageDef;
            explosiveShell.postExplosionSpawnThingDef = gasDef;
            shellDef.projectileWhenLoaded = shellBulletDef;

            shellBulletDef.projectile.damageDef = damageDef;
            shellBulletDef.projectile.postExplosionSpawnThingDef = gasDef;

            CompProperties_Explosive explosiveBarrel = barrelDef.GetCompProperties<CompProperties_Explosive>();

            explosiveBarrel.explosiveDamageType = damageDef;
            explosiveBarrel.postExplosionSpawnThingDef = gasDef;
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
