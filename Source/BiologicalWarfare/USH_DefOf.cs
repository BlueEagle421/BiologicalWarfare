using RimWorld;
using Verse;

namespace BiologicalWarfare
{
    [DefOf]
    public static class USH_JobDefOf
    {
        static USH_JobDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(USH_JobDefOf));

        public static JobDef USH_InsertSample;

        public static JobDef USH_ExtractSample;
    }
}
