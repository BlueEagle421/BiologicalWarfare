using OPToxic;
using RimWorld;
using Verse;

namespace BiologicalWarfare
{
    public class DamageWorkerPathogenGas : DamageWorker_OPToxic
    {
        private const int GOODWILL_CHANGE = -20;

        public override DamageResult Apply(DamageInfo dinfo, Thing victim)
        {
            DamageResult baseResult = base.Apply(dinfo, victim);

            if (dinfo.Instigator == null)
                return baseResult;

            if (dinfo.Instigator.Faction == null)
                return baseResult;

            if (victim.Faction == null)
                return baseResult;

            Log.Message($"dinfo.Instigator: {dinfo.Instigator}");
            Log.Message($"victim.Faction: {victim.Faction}");

            dinfo.Instigator?.Faction?.TryAffectGoodwillWith(
                victim?.Faction,
                GOODWILL_CHANGE,
                true,
                true,
                HistoryEventDefOf.UsedHarmfulItem,
                victim);

            return base.Apply(dinfo, victim);
        }
    }
}
