using RimWorld;
using Verse;

namespace BiologicalWarfare
{
    [DefOf]
    public static class USH_DefOf
    {
        static USH_DefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(USH_DefOf));

        public static JobDef USH_InsertSample;

        public static JobDef USH_ExtractSample;

        public static HediffDef USH_VirusExtraction;

        public static HediffDef USH_ParasiticPerforation;

        public static BodyPartGroupDef FullHead;
    }
}
