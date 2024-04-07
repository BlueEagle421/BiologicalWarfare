using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace BiologicalWarfare
{
    public class CompProperties_DiseaseSampleContainer : CompProperties_ThingContainer
    {
        public DiseaseType acceptableDiseaseType;
        public CompProperties_DiseaseSampleContainer() => compClass = typeof(CompDiseaseSampleContainer);
    }

    public class CompDiseaseSampleContainer : CompThingContainer
    {
        public CompProperties_DiseaseSampleContainer ContainerProps => (CompProperties_DiseaseSampleContainer)props;

        public bool CanAcceptSample(CompDiseaseSample compDiseaseSample)
        {
            if (compDiseaseSample.Props.combatDiseaseDef.diseaseType != ContainerProps.acceptableDiseaseType)
                return false;

            return true;
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption option in base.CompFloatMenuOptions(selPawn))
                yield return option;


            yield return new FloatMenuOption("USH_ExtractSample".Translate(), delegate ()
            {
                OrderExtractionJob(selPawn);
            }, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);

            yield break;
        }

        private void OrderExtractionJob(Pawn pawn)
        {
            if (Empty)
            {
                Messages.Message("USH_SampleContainerEmpty".Translate(parent.Named("BUILDING")), parent, MessageTypeDefOf.CautionInput);
                return;
            }

            Job job = JobMaker.MakeJob(USH_DefOf.USH_ExtractSample, parent);
            pawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
        }
    }
}
