using System.Collections.Generic;
using HugsLib.Utils;
using Verse;

namespace ShelfRenamer
{
    public class DataStore : UtilityWorldObject
    {
        public Dictionary<string, string> shelfNames = new Dictionary<string, string>();

        // Expose our data store to the serialiser.
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref shelfNames, "shelfNames");
        }
    }
}