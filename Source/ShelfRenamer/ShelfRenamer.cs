using HugsLib;
using HugsLib.Utils;
using Verse;

namespace ShelfRenamer
{
    // HugsLib config
    public class ShelfRenamer : ModBase
    {
        private DataStore _dataStore;

        public ShelfRenamer()
        {
            Instance = this;
        }

        // Having a state variable means our patches can find us and our data store.
        internal static ShelfRenamer Instance { get; private set; }

        public override string ModIdentifier => "ShelfRenamer";

        public override void WorldLoaded()
        {
            base.WorldLoaded();
            _dataStore = UtilityWorldObjectManager.GetUtilityWorldObject<DataStore>();
        }

        // Set the name of a thing.
        public void SetName(Thing thing, string name)
        {
            if (thing == null || _dataStore == null)
            {
                // Theoretically we'll never hit this, but we're being cautious.
                return;
            }

            if (name.Length == 0)
            {
                ClearName(thing);
            }
            else
            {
                _dataStore.shelfNames[thing.ThingID] = name;
            }
        }

        public void Log(string str)
        {
            Logger.Message(str);
        }

        public void ClearName(Thing thing)
        {
            // The many question-marks in this code protect against this being called during WorldGen when our data-store doesn't exist yet.
            _dataStore?.shelfNames?.Remove(thing.ThingID);
        }

        public bool IsRenamed(Thing thing)
        {
            if (thing == null || _dataStore == null)
            {
                return false;
            }

            return _dataStore.shelfNames.ContainsKey(thing.ThingID);
        }

        public string NameOf(Thing thing)
        {
            return _dataStore.shelfNames[thing.ThingID];
        }
    }

    // Data store where we keep our shelf names.
}