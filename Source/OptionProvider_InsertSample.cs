using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace USH_BW;

public class FloatMenuOptionProvider_InsertSample : FloatMenuOptionProvider
{
    protected override bool Drafted => true;
    protected override bool Undrafted => true;
    protected override bool Multiselect => false;
    protected override bool RequiresManipulation => true;

    private static readonly TargetingParameters targetingParameters;

    static FloatMenuOptionProvider_InsertSample()
    {
        targetingParameters = new TargetingParameters
        {
            canTargetPawns = false,
            canTargetItems = false,
            canTargetBuildings = true,
            validator = new Predicate<TargetInfo>(TargetValidator)
        };
    }


    private static bool TargetValidator(TargetInfo target)
    {
        if (target.Thing is not Building building)
            return false;

        if (building.TryGetComp<CompDiseaseSampleContainer>() == null)
            return false;

        return true;
    }

    public override IEnumerable<FloatMenuOption> GetOptionsFor(Thing clickedThing, FloatMenuContext context)
    {
        if (!clickedThing.TryGetComp(out CompDiseaseSample _))
            yield break;

        yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("USH_InsertSample".Translate(clickedThing.Label.Named("ITEM")), delegate
            {
                CreateInsertJobTargeter(context.FirstSelectedPawn, clickedThing);
            }), context.FirstSelectedPawn, new LocalTargetInfo(clickedThing));
    }

    private static void CreateInsertJobTargeter(Pawn p, Thing item)
    {
        Find.Targeter.BeginTargeting(targetingParameters, delegate (LocalTargetInfo target)
        {
            CompDiseaseSampleContainer container = (target.Thing as Building).GetComp<CompDiseaseSampleContainer>();

            if (container == null || container.Full)
                return;

            if (container.Full)
            {
                Messages.Message("USH_SampleContainerFull".Translate(target.Thing.Named("BUILDING")), container.parent, MessageTypeDefOf.RejectInput);
                return;
            }

            GiveJobToPawn(p, target, item);
        }, null, null, null);
    }

    private static void GiveJobToPawn(Pawn p, LocalTargetInfo target, Thing item)
    {
        Building targetBuilding = target.Thing as Building;

        CompDiseaseSampleContainer sampleContainer = (target.Thing as Building).GetComp<CompDiseaseSampleContainer>();

        if (sampleContainer == null && sampleContainer.Full)
            return;

        if (sampleContainer.Full)
        {
            Messages.Message("USH_SampleContainerFull".Translate(target.Thing.Named("BUILDING")), p, MessageTypeDefOf.RejectInput);
            return;
        }

        CompDiseaseSample compDiseaseSample = item.TryGetComp<CompDiseaseSample>();

        AcceptanceReport canInsert = sampleContainer.CanInsert(p, compDiseaseSample);

        if (!canInsert)
        {
            Messages.Message(canInsert.Reason, target.Thing, MessageTypeDefOf.RejectInput);
            return;
        }

        Job job = JobMaker.MakeJob(USH_DefOf.USH_InsertSample, item, targetBuilding, targetBuilding.InteractionCell);
        job.count = 1;
        p.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
    }

}