using RimWorld;
using System.Linq;
using Verse;

namespace BiologicalWarfare
{
    public class CompAssignableToPawnGatherer : CompAssignableToPawn
    {
        public override string CompInspectStringExtra()
        {
            if (AssignedPawnsForReading.Count == 0)
                return "USH_User".Translate() + ": " + "Nobody".Translate();

            if (AssignedPawnsForReading.Count == 1)
                return "USH_User".Translate() + ": " + AssignedPawnsForReading[0].LabelShortCap;

            return "USH_Users".Translate() + ": " + string.Join(", ", AssignedPawnsForReading.Select(x => x.LabelShortCap));
        }

        protected override string GetAssignmentGizmoLabel() => "USH_SetUsers".Translate();

    }
}