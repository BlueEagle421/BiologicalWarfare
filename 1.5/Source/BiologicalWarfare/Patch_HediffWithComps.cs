using HarmonyLib;
using RimWorld;
using Verse;

namespace BiologicalWarfare
{
    [HarmonyPatch(typeof(HediffWithComps), nameof(HediffWithComps.Notify_PawnUsedVerb))]
    public static class Patch_HediffWithComps
    {
        private const float INFECTION_SEVERITY = 0.02f;

        [HarmonyPostfix]
        public static void AddNecroaInfection(HediffWithComps __instance, LocalTargetInfo target)
        {
            if (!BiologicalWarfareMod.Settings.ShamblersSpreadNecroa.Value)
                return;

            if (__instance as Hediff_Shambler == null)
                return;

            if (__instance.pawn == null)
                return;

            if (!__instance.pawn.IsShambler)
                return;

            if (target == null)
                return;

            if (target.Pawn == null)
                return;

            BiologicalUtils.AddInfectionSeverity(target.Pawn, USHDefOf.USH_Necroa, INFECTION_SEVERITY);

            SetNecroaFaction(target.Pawn, __instance.pawn.Faction);
        }

        private static void SetNecroaFaction(Pawn pawn, Faction faction)
        {
            if (faction == null)
                return;

            Hediff necroaHediff = pawn.health?.hediffSet?.GetFirstHediffOfDef(USHDefOf.USH_Necroa);

            if (necroaHediff == null)
                return;

            necroaHediff.TryGetComp<HediffCompNecroa>().PostMortemFaction = faction;
        }
    }
}
