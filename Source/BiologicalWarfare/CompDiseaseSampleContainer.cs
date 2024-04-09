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

        public CompDiseaseSample ContainedSampleComp() => ContainedThing.TryGetComp<CompDiseaseSample>();

        public virtual AcceptanceReport CanInsert(Pawn pawn, CompDiseaseSample compDiseaseSample)
        {
            if (compDiseaseSample.Props.combatDiseaseDef.diseaseType != ContainerProps.acceptableDiseaseType)
            {
                NamedArgument type = ContainerProps.acceptableDiseaseType.ToStringUncapitalized().Named("TYPE");
                return "USH_SampleContainerTypeMismach".Translate(parent.Named("BUILDING"), type);
            }

            return true;
        }

        public virtual AcceptanceReport CanExtract(Pawn pawn)
        {
            if (Empty)
                return "USH_SampleContainerEmpty".Translate(parent.Named("BUILDING"));

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
            AcceptanceReport canExtract = CanExtract(pawn);
            if (!canExtract)
            {
                Messages.Message(canExtract.Reason, parent, MessageTypeDefOf.CautionInput);
                return;
            }

            Job job = JobMaker.MakeJob(USH_DefOf.USH_ExtractSample, parent);
            pawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
        }

        public override string CompInspectStringExtra() => "USH_ContainedSample".Translate(SampleLabelFormatted());

        private string SampleLabelFormatted()
        {
            if (Empty)
                return (string)"Nothing".Translate();

            return ContainedThing.TryGetComp<CompDiseaseSample>().Props.combatDiseaseDef.label;
        }
    }
}
