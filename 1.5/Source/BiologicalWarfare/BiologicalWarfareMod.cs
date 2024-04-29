using HarmonyLib;
using RimWorld;
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
            float severitySliderValue = listingStandard.Slider(Settings.GasSeverityMultiplier, 0.01f, 1.5f);
            listingStandard.Label("USH_GasMultplierSettingDesc".Translate(severitySliderValue.ToStringPercent()));
            Settings.GasSeverityMultiplier = severitySliderValue;

            //MaxGasInfectionCount
            listingStandard.Label("\n");
            listingStandard.Label("USH_GasInfectionCountSetting".Translate());
            int countSliderValue = MaxCountFormatted(listingStandard.Slider(Settings.MaxGasInfectionCount, 0f, 6f));
            listingStandard.Label("USH_GasInfectionCountSettingDesc".Translate(countSliderValue.ToString()));
            Settings.MaxGasInfectionCount = countSliderValue;

            //ShamblersSpreadNecroa
            if (ModsConfig.AnomalyActive)
            {
                listingStandard.Label("\n");
                listingStandard.CheckboxLabeled("USH_ShamblerInfectionSetting".Translate(), ref Settings.ShamblersSpreadNecroa);
                listingStandard.Label("USH_ShamblerInfectionSettingDesc".Translate());
            }

            //Reset button
            listingStandard.Label("\n");
            bool shouldReset = listingStandard.ButtonText("USH_ResetSettings".Translate());
            if (shouldReset) Settings.ResetAll();

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
        public bool ShamblersSpreadNecroa = true;

        public void ResetAll()
        {
            GasSeverityMultiplier = 1f;
            MaxGasInfectionCount = 2;
            ShamblersSpreadNecroa = true;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref GasSeverityMultiplier, "USH_GasSeverityMultiplier", 1f);
            Scribe_Values.Look(ref MaxGasInfectionCount, "USH_MaxGasInfectionCount", 2);
            Scribe_Values.Look(ref ShamblersSpreadNecroa, "USH_ShamblersSpreadNecroa", true);
            base.ExposeData();
        }
    }
}