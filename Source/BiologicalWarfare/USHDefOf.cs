using RimWorld;
using Verse;

namespace BiologicalWarfare
{
    [DefOf]
    public static class USHDefOf
    {
        static USHDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(USHDefOf));

        public static JobDef USH_InsertSample;

        public static JobDef USH_ExtractSample;

        public static HediffDef USH_VirusExtraction;

        public static HediffDef USH_ParasiticPerforation;

        public static ThingDef USH_DiseaseSampler;

        public static ThingDef USH_VaccineResearchStation;

        public static JobDef USH_ResearchVaccine;

        public static StatDef USH_ContaminationChanceFactor;
    }
}
