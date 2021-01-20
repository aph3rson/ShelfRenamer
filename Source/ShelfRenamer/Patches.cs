using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ShelfRenamer
{
    // Add a "Rename" Gizmo to storage buildings.
    [HarmonyPatch(typeof(Building_Storage))]
    [HarmonyPatch("GetGizmos")]
    public static class Patch_Building_Storage_GetGizmos
    {
        public static void Postfix(Building_Storage __instance, ref IEnumerable<Gizmo> __result)
        {
            // ShelfRenamer.Instance.Log("Gizmoing " + __instance.def.thingClass.Name);

            // RimFridge already has its own renamer.
            if (__instance.def.thingClass.Name == "Building_Refrigerator")
            {
                return;
            }
            // If it has a user-accessible storage tab, then allow renaming.
            if (__instance.StorageTabVisible)
            {
                var tempList = __result.ToList();
                // Add our own rename button, since none appears to exist already.
                tempList.Add(new Command_Action
                {
                    icon = ContentFinder<Texture2D>.Get("UI/Icons/ShelfRenamer", true),
                    defaultDesc = "Rename".Translate(),
                    defaultLabel = "Rename".Translate(),
                    activateSound = SoundDef.Named("Click"),
                    action = delegate { Find.WindowStack.Add(new Dialog_Rename(__instance)); },               
                });
                __result = tempList;
            }
        }
    }

    // Label checks if we've got a renamed shelf. If not, runs the original code.
    // Building_Storage inherits Label from Thing, so that's what we need to patch.
    [HarmonyPatch(typeof(Thing))]
	[HarmonyPatch("Label", MethodType.Getter)]
    public static class Patch_Building_Storage_Label
    {
        public static bool Prefix(Thing __instance, ref string __result)
        {
            if (ShelfRenamer.Instance.IsRenamed(__instance))
            {
                __result = ShelfRenamer.Instance.NameOf(__instance);
                return false; // Don't run original method.
            }

            // Shelf isn't renamed, run original Label method.
            return true;
        }
    }

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
