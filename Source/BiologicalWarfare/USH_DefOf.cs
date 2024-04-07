using RimWorld;
using Verse;

namespace BiologicalWarfare
{
    [DefOf]
    public static class USH_DefOf
    {
        static USH_DefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(USH_DefOf));

        public static DesignationDef USH_ExtractDesignation;

        public static JobDef USH_ExtractSampleFromDesignator;

        public static JobDef USH_InsertSample;

        public static JobDef USH_ExtractSample;
    }
}
