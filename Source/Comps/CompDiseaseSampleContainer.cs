using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace USH_BW
{
    public class CompProperties_DiseaseSampleContainer : CompProperties_ThingContainer
    {
        public bool acceptAllDiseaseTypes;
        public DiseaseType acceptableDiseaseType;
        public SoundDef insertedSoundDef, extractedSoundDef;
        public CompProperties_DiseaseSampleContainer() => compClass = typeof(CompDiseaseSampleContainer);
    }

    public class CompDiseaseSampleContainer : CompThingContainer
    {
        public CompProperties_DiseaseSampleContainer PropsSampleContainer => (CompProperties_DiseaseSampleContainer)props;
        public CompDiseaseSample ContainedSampleComp => _containedSampleComp;
        public CombatDiseaseDef ContainedCombatDiseaseDef => _containedSampleComp?.PropsDiseaseSample.combatDiseaseDef;
        private CompDiseaseSample _containedSampleComp;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            _containedSampleComp = ContainedThing?.TryGetComp<CompDiseaseSample>();
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption option in base.CompFloatMenuOptions(selPawn))
                yield return option;

            FloatMenuOption extractOption = new FloatMenuOption("USH_ExtractSample".Translate(), delegate ()
            {
                OrderExtractionJob(selPawn);
            }, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);

            AcceptanceReport report = CanExtract(selPawn);

            if (!report.Accepted)
            {
                extractOption.Label += string.Format(" ({0})", report.Reason.UncapitalizeFirst());
                extractOption.Disabled = true;
            }

            yield return extractOption;
        }

        private void OrderExtractionJob(Pawn pawn)
        {
            AcceptanceReport canExtract = CanExtract(pawn);
            if (!canExtract)
            {
                Messages.Message(canExtract.Reason, parent, MessageTypeDefOf.RejectInput);
                return;
            }

            Job job = JobMaker.MakeJob(USH_DefOf.USH_ExtractSample, parent);
            pawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
        }

        public override string CompInspectStringExtra() => "USH_ContainedSample".Translate(SampleLabelFormatted());

        public virtual AcceptanceReport CanInsert(Pawn pawn, CompDiseaseSample compDiseaseSample)
        {
            bool isMatchingDiseaseType = compDiseaseSample.PropsDiseaseSample.combatDiseaseDef.diseaseType
                == PropsSampleContainer.acceptableDiseaseType;

            if (!isMatchingDiseaseType && !PropsSampleContainer.acceptAllDiseaseTypes)
            {
                NamedArgument type = PropsSampleContainer.acceptableDiseaseType.ToStringUncapitalized().Named("TYPE");
                return "USH_SampleContainerTypeMismatch".Translate(parent.Named("BUILDING"), type);
            }

            return true;
        }

        public virtual AcceptanceReport CanExtract(Pawn pawn)
        {
            if (Empty)
                return "USH_SampleContainerEmpty".Translate();

            return true;
        }

        public virtual void OnInserted(Pawn pawn)
        {
            _containedSampleComp = ContainedThing?.TryGetComp<CompDiseaseSample>();

            SoundDef insertedSoundDef = PropsSampleContainer.insertedSoundDef;
            insertedSoundDef?.PlayOneShot(SoundInfo.InMap(parent));
        }

        public virtual void OnExtracted(Pawn pawn)
        {
            SoundDef extractedSoundDef = PropsSampleContainer.extractedSoundDef;
            extractedSoundDef?.PlayOneShot(SoundInfo.InMap(parent));
        }

        private string SampleLabelFormatted()
        {
            if (Empty)
                return (string)"Nothing".Translate();

            return ContainedCombatDiseaseDef.label;
        }
    }
}
