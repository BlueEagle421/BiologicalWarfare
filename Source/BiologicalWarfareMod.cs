using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace USH_BW
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
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            //GasSeverityMultiplier
            listingStandard.Label("USH_GasMultplierSetting".Translate());
            float severitySliderValue = listingStandard.Slider(Settings.GasSeverityMultiplier.Value, 0.01f, 1.5f);
            listingStandard.Label("USH_GasMultplierSettingDesc".Translate(severitySliderValue.ToStringPercent()));
            Settings.GasSeverityMultiplier.Value = severitySliderValue;

            //MaxGasInfectionCount
            listingStandard.Label("\n");
            listingStandard.Label("USH_GasInfectionCountSetting".Translate());
            int countSliderValue = MaxCountFormatted(listingStandard.Slider(Settings.MaxGasInfectionCount.Value, 0f, 12f));
            listingStandard.Label("USH_GasInfectionCountSettingDesc".Translate(countSliderValue.ToString()));
            Settings.MaxGasInfectionCount.Value = countSliderValue;

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
        public Setting<float> GasSeverityMultiplier = new Setting<float>(1f);
        public Setting<int> MaxGasInfectionCount = new Setting<int>(4);

        public void ResetAll()
        {
            GasSeverityMultiplier.ToDefault();
            MaxGasInfectionCount.ToDefault();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            GasSeverityMultiplier.ExposeData(nameof(GasSeverityMultiplier));
            MaxGasInfectionCount.ExposeData(nameof(MaxGasInfectionCount));
        }

        public class Setting<T>
        {
            public T Value;
            public T DefaultValue { get; private set; }
            public Setting(T defaultValue) { DefaultValue = defaultValue; Value = defaultValue; }
            public void ToDefault() => Value = DefaultValue;
            public void ExposeData(string key) => Scribe_Values.Look(ref Value, $"USH_{key}", DefaultValue);
        }
    }
}