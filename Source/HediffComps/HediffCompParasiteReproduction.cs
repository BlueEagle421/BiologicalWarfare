using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace USH_BW
{
    public class HediffCompProperties_ParasiteReproduction : HediffCompProperties
    {
        public CombatDiseaseDef combatDiseaseDef;
        public int reproductionDurationTicks;
        public int basePathogenCount;
        public bool multiplyByBodySize;
        public HediffCompProperties_ParasiteReproduction() => compClass = typeof(HediffCompParasiteReproduction);
    }

    public class HediffCompParasiteReproduction : HediffComp
    {
        private int _reproductionTicks;

        public HediffCompProperties_ParasiteReproduction Props => (HediffCompProperties_ParasiteReproduction)props;

        public override void CompExposeData()
        {
            base.CompExposeData();

            Scribe_Values.Look(ref _reproductionTicks, "USH_ReproductionTicks");
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            ReproductionTick();
        }

        private void ReproductionTick()
        {
            if (Pawn.Dead)
                return;

            _reproductionTicks++;

            if (_reproductionTicks >= Props.reproductionDurationTicks)
                Reproduce();
        }

        private void Reproduce()
        {
            BiologicalUtils.SpawnThingAt(Pawn.Map, Pawn.CellsAdjacent8WayAndInside().ToList(), Props.combatDiseaseDef.pathogenDef, SpawnCount());
            Pawn.health.AddHediff(USHDefOf.USH_ParasiticPerforation, RandomOrganicPart());
            _reproductionTicks = 0;
        }

        private int SpawnCount() => (int)(Props.basePathogenCount * (Props.multiplyByBodySize ? Pawn.BodySize : 1));

        private bool IsBodyPartOrganic(BodyPartRecord bodyPartRecord)
        {
            BodyPartDef partDef = bodyPartRecord.def;
            return partDef.bleedRate != 0 && partDef.permanentInjuryChanceFactor != 0 && partDef.GetHitChanceFactorFor(DamageDefOf.Cut) != 0;
        }
        private BodyPartRecord RandomOrganicPart()
        {
            List<BodyPartRecord> organicParts = Pawn.health.hediffSet.GetNotMissingParts().Where(x => IsBodyPartOrganic(x)).ToList();

            return organicParts[UnityEngine.Random.Range(0, organicParts.Count)];
        }
    }
}