using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace BiologicalWarfare;

public class FloatMenuOptionProvider_InsertMemoryCell : FloatMenuOptionProvider
{
    protected override bool Drafted => true;
    protected override bool Undrafted => true;
    protected override bool Multiselect => false;
    protected override bool RequiresManipulation => true;

    private static readonly TargetingParameters targetingParameters;

    static FloatMenuOptionProvider_InsertMemoryCell()
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
        if (!clickedThing.TryGetComp(out CompDiseaseSample memoryCell))
            yield break;

        yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("USH_GE_InsertMemoryCell".Translate(clickedThing.Label), delegate
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

        Job job = JobMaker.MakeJob(USHDefOf.USH_InsertSample, item, targetBuilding, targetBuilding.InteractionCell);
        job.count = 1;
        p.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
    }

}