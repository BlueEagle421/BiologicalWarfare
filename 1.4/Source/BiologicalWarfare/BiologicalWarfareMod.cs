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
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label("USH_GasMultplierSetting".Translate());
            float sliderValue = listingStandard.Slider(Settings.GasSeverityMultiplier, 0.01f, 1f);
            listingStandard.Label("USH_GasMultplierSettingDesc".Translate(sliderValue.ToStringPercent()));
            Settings.GasSeverityMultiplier = sliderValue;
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory() => "Biological Warfare";
    }
    public class BiologicalWarfareSettings : ModSettings
    {
        public float GasSeverityMultiplier = 1f;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref GasSeverityMultiplier, "USH_GasSeverityMultiplier", 1f);
            base.ExposeData();
        }
    }
}