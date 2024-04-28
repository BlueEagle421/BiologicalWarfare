using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace BiologicalWarfare
{
    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
    public static class Patch_FloatMenuMakerMap
    {
        private static TargetingParameters targetingParameters;

        private const int recacheTicks = 2000;

        private static int cachedTick;

        private static Pawn cachedPawn;

        private static IntVec3 cachedPosition;

        private static List<FloatMenuOption> cachedOptions = new List<FloatMenuOption>();

        static Patch_FloatMenuMakerMap()
        {
            targetingParameters = new TargetingParameters
            {
                canTargetPawns = false,
                canTargetItems = false,
                canTargetBuildings = true,
                validator = new Predicate<TargetInfo>(TargetValidator)
            };
        }

        private static bool TargetValidator(TargetInfo target) => target.Thing is Building building && building.TryGetComp<CompDiseaseSampleContainer>() != null;


        [HarmonyPostfix]
        public static void AddInsertItemInDisplayOption(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            try
            {
                if (pawn != null)
                {
                    Recache(clickPos, pawn);
                    opts.AddRange(cachedOptions);
                }
            }
            catch (Exception e)
            {
                Log.Error(string.Format("Caught unhandled exception: {0}", e));
            }
        }

        private static void Recache(Vector3 clickPos, Pawn pawn)
        {
            IntVec3 position = clickPos.ToIntVec3();
            bool needToRecache = pawn != cachedPawn || position != cachedPosition || GenTicks.TicksGame > cachedTick + recacheTicks;
            if (needToRecache)
            {
                cachedOptions.Clear();
                cachedTick = GenTicks.TicksGame;
                cachedPawn = pawn;
                cachedPosition = position;
                CacheDisplayableItemOptions();
            }
        }

        private static void CacheDisplayableItemOptions()
        {
            IEnumerable<Thing> thingsAtPosition = cachedPosition.GetThingList(cachedPawn.Map);
            using (IEnumerator<Thing> enumerator = thingsAtPosition.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Thing thing = enumerator.Current;
                    if (thing.def.HasComp(typeof(CompDiseaseSample)))
                    {
                        void action() => CreateInsertJobTargeter(thing);

                        string label = "USH_InsertSample".Translate(thing.Named("ITEM"));
                        cachedOptions.Add(new FloatMenuOption(label, action, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0));
                    }
                }
            }
        }

        private static void CreateInsertJobTargeter(Thing item)
        {
            Find.Targeter.BeginTargeting(targetingParameters, delegate (LocalTargetInfo target)
            {
                CompDiseaseSampleContainer sampleContainer = (target.Thing as Building).GetComp<CompDiseaseSampleContainer>();

                if (sampleContainer == null && sampleContainer.Full)
                    return;

                if (sampleContainer.Full)
                {
                    Messages.Message("USH_SampleContainerFull".Translate(target.Thing.Named("BUILDING")), cachedPawn, MessageTypeDefOf.CautionInput);
                    return;
                }

                CompDiseaseSample compDiseaseSample = item.TryGetComp<CompDiseaseSample>();

                AcceptanceReport canInsert = sampleContainer.CanInsert(cachedPawn, compDiseaseSample);

                if (!canInsert)
                {
                    Messages.Message(canInsert.Reason, target.Thing, MessageTypeDefOf.RejectInput);
                    return;
                }

                GiveJobToCachedPawn(target, item);
            }, null, null, null);
        }

        private static void GiveJobToCachedPawn(LocalTargetInfo target, Thing item)
        {
            Building targetBuilding = target.Thing as Building;


            Job job = JobMaker.MakeJob(USHDefOf.USH_InsertSample, item, targetBuilding, targetBuilding.InteractionCell);
            job.count = 1;
            cachedPawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);

        }
    }
}
