using RimWorld;
using Verse;

namespace ShelfRenamer
{
    public class Dialog_Rename : Verse.Dialog_Rename
    {
        private readonly Building_Storage building;

        public Dialog_Rename(Building_Storage building)
        {
            this.building = building;
            curName = building.Label;
        }

        // By default empty strings are not allowed. We'll override
        // that and accept everything; an empty string will reset our
        // name to the default.
        protected override AcceptanceReport NameIsValid(string name)
        {
            return true;
        }

        protected override void SetName(string newName)
        {
            ShelfRenamer.Instance.SetName(building, newName);
        }
    }
}