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

        public List<Def> defsToFormat = new List<Def>();
        public List<ThingDef> thingDefsToColor = new List<ThingDef>();

        public ThingDef sampleDef;
        public ThingDef pathogenDef;

        public ThingDef gasDef;
        public DamageDef damageDef;
        public ThingDef shellDef;
        public ThingDef shellBulletDef;
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

            description = hediffDef.description;

            foreach (Def def in defsToFormat)
                FormatDef(def);

            foreach (ThingDef thingDef in thingDefsToColor)
                ColorThingDef(thingDef);

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

            gasDef.modExtensions.Add(toxicExtansion);

            CompProperties_Explosive explosive = shellDef.GetCompProperties<CompProperties_Explosive>();

            explosive.explosiveDamageType = damageDef;
            explosive.postExplosionSpawnThingDef = gasDef;
            shellDef.projectileWhenLoaded = shellBulletDef;

            shellBulletDef.projectile.damageDef = damageDef;
            shellBulletDef.projectile.postExplosionSpawnThingDef = gasDef;
        }

        private void FormatDef(Def def)
        {
            if (def == null)
                return;

            def.label = Formatted(def.label);

            def.description = Formatted(def.description);
        }

        private void ColorThingDef(ThingDef thingDef)
        {
            if (thingDef == null)
                return;

            thingDef.graphicData.color = colorInt.ToColor;
        }

        private string Formatted(string toFormat)
        {
            string label = hediffDef.label;
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
