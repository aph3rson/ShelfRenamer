using HarmonyLib;
using Verse;

namespace ShelfRenamer
{
    [HarmonyPatch(typeof(Building))]
    [HarmonyPatch("DeSpawn")]
    public static class Patch_Building_Storage_DeSpawn
    {
        public static void Prefix(Thing __instance)
        {
            ShelfRenamer.Instance.ClearName(__instance);
        }
    }
}