using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace BiologicalWarfare
{

    public class ParasiteReproductionExtension : DefModExtension
    {
        public CombatDiseaseDef combatDiseaseDef;
        public int reproductionDurationTicks;
        public int basePathogenCount;
        public bool multiplyByBodySize;
    }

    public class HediffParasiteReproduction : HediffWithComps
    {
        private int _reproductionTicks;
        private ParasiteReproductionExtension _extension;

        public override void PostMake()
        {
            base.PostMake();
            _extension = def.GetModExtension<ParasiteReproductionExtension>();
        }

        public override void Tick()
        {
            base.Tick();

            if (pawn.Dead)
                return;

            _reproductionTicks++;

            if (_reproductionTicks >= _extension.reproductionDurationTicks)
                Reproduce();
        }

        private void Reproduce()
        {
            BiologicalUtils.SpawnThingAt(pawn.Map, pawn.CellsAdjacent8WayAndInside().ToList(), _extension.combatDiseaseDef.pathogenDef, SpawnCount());
            pawn.health.AddHediff(USHDefOf.USH_ParasiticPerforation, RandomOrganicPart());
            _reproductionTicks = 0;
        }

        private int SpawnCount() => (int)(_extension.basePathogenCount * (_extension.multiplyByBodySize ? pawn.BodySize : 1));

        private bool IsBodyPartOrganic(BodyPartRecord bodyPartRecord)
        {
            BodyPartDef partDef = bodyPartRecord.def;
            return partDef.bleedRate != 0 && partDef.permanentInjuryChanceFactor != 0 && partDef.GetHitChanceFactorFor(DamageDefOf.Cut) != 0;
        }
        private BodyPartRecord RandomOrganicPart()
        {
            List<BodyPartRecord> organicParts = pawn.health.hediffSet.GetNotMissingParts().Where(x => IsBodyPartOrganic(x)).ToList();

            return organicParts[UnityEngine.Random.Range(0, organicParts.Count)];
        }
    }
}