using HarmonyLib;
using Verse;

namespace ShelfRenamer
{
    [HarmonyPatch(typeof(Thing))]
    [HarmonyPatch("Label", MethodType.Getter)]
    public static class Patch_Building_Storage_Label
    {
        public static bool Prefix(Thing __instance, ref string __result)
        {
            if (!ShelfRenamer.Instance.IsRenamed(__instance))
            {
                return true;
            }

            __result = ShelfRenamer.Instance.NameOf(__instance);
            return false; // Don't run original method.

            // Shelf isn't renamed, run original Label method.
        }
    }
}