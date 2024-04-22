using HarmonyLib;
using UnityEngine;
using Verse;

namespace BiologicalWarfare
{
    public class BiologicalWarfareMod : Mod
    {
        public static BiologicalWarfareSettings Settings { get; private set; }
        public BiologicalWarfareMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<BiologicalWarfareSettings>();

            Harmony.DEBUG = false;
            Harmony harmony = new Harmony("biologicalwarfare");

            harmony.PatchAll();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            //GasSeverityMultiplier
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label("USH_GasMultplierSetting".Translate());
            float severitySliderValue = listingStandard.Slider(Settings.GasSeverityMultiplier, 0.01f, 1f);
            listingStandard.Label("USH_GasMultplierSettingDesc".Translate(severitySliderValue.ToStringPercent()));
            Settings.GasSeverityMultiplier = severitySliderValue;

            //MaxGasInfectionCount

            listingStandard.Label("\n");
            listingStandard.Label("USH_GasInfectionCountSetting".Translate());
            int countSliderValue = MaxCountFormatted(listingStandard.Slider(Settings.MaxGasInfectionCount, 0f, 6f));
            listingStandard.Label("USH_GasInfectionCountSettingDesc".Translate(countSliderValue.ToString()));
            Settings.MaxGasInfectionCount = countSliderValue;

            //End
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        private int MaxCountFormatted(float sliderValue)
        {
            if (sliderValue < 1f)
                return -1;

            return (int)sliderValue;
        }

        public override string SettingsCategory() => "Biological Warfare";
    }
    public class BiologicalWarfareSettings : ModSettings
    {
        public float GasSeverityMultiplier = 1f;
        public int MaxGasInfectionCount = 2;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref GasSeverityMultiplier, "USH_GasSeverityMultiplier", 1f);
            Scribe_Values.Look(ref MaxGasInfectionCount, "USH_MaxGasInfectionCount", 2);
            base.ExposeData();
        }
    }
}