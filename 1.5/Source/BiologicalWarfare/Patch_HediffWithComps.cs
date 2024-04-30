using HarmonyLib;
using Verse;

namespace BiologicalWarfare
{
    [HarmonyPatch(typeof(HediffWithComps), nameof(HediffWithComps.Notify_PawnUsedVerb))]
    public static class Patch_HediffWithComps
    {
        private const float INFECTION_SEVERITY = 0.02f;

        [HarmonyPostfix]
        public static void AddNecroaInfection(HediffWithComps __instance, Verb verb, LocalTargetInfo target)
        {
            if (__instance as Hediff_Shambler == null)
                return;

            if (!__instance.pawn.IsShambler)
                return;

            if (!(target.Pawn is Pawn attackedPawn))
                return;

            if (!BiologicalWarfareMod.Settings.ShamblersSpreadNecroa)
                return;

            BiologicalUtils.AddInfectionSeverity(attackedPawn, USHDefOf.USH_Necroa, INFECTION_SEVERITY);

            (attackedPawn.health?.hediffSet?.GetFirstHediffOfDef(USHDefOf.USH_Necroa)
                .TryGetComp<HediffCompNecroa>()).PostMortemFaction = __instance.pawn.Faction;
        }
    }
}
