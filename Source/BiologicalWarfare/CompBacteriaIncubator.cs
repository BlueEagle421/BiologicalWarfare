using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiologicalWarfare
{
    public class CompProperties_VirusReplicator : CompProperties_Activable
    {
        public int maxRoomCellSize;
        public CompProperties_VirusReplicator() => compClass = typeof(CompVirusReplicator);
    }

    public class CompBacteriaIncubator : CompActivable
    {
        public CompProperties_VirusReplicator ReplicatorProps => (CompProperties_VirusReplicator)props;

        protected override bool TryUse()
        {
            throw new NotImplementedException();
        }
    }
}
