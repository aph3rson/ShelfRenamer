using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

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
            if (__instance.def.thingClass.Name == "Building_Refrigerator" ||
                __instance.def.thingClass.Name == "RimFridge_Building")
            {
                return;
            }

            // If it has a user-accessible storage tab, then allow renaming.
            if (!__instance.StorageTabVisible)
            {
                return;
            }

            var tempList = __result.ToList();
            // Add our own rename button, since none appears to exist already.
            tempList.Add(new Command_Action
            {
                icon = ContentFinder<Texture2D>.Get("UI/Icons/ShelfRenamer"),
                defaultDesc = "Rename".Translate(),
                defaultLabel = "Rename".Translate(),
                activateSound = SoundDef.Named("Click"),
                action = delegate { Find.WindowStack.Add(new Dialog_Rename(__instance)); }
            });
            __result = tempList;
        }
    }

    // Label checks if we've got a renamed shelf. If not, runs the original code.
    // Building_Storage inherits Label from Thing, so that's what we need to patch.
}