using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ShelfRenamer
{
    // Add a "Rename" Gizmo to storage buildings.
    [HarmonyPatch]
    public static class Patch_Building_Storage_GetGizmos
    {
        // List of already-seen thingClasses and reasons
        private static List<(Type, String)> alreadySeen = new List<(Type, String)>();
        
        // List of subclasses with their own GetGizmos
        // We'll short-circuit the base postfix if they're in this list.
        private static List<Type> customGetGizmos = new List<Type>();
        
        // Debug logging method
        private static void LogNotPatching(Building_Storage thing, string reason)
        {
            Type thingClass = thing.def.thingClass;
            (Type, String) index = (thingClass, reason);
            if (!alreadySeen.Contains(index))
            {
                alreadySeen.Add(index);
                ShelfRenamer.Instance.Log(
                    $"Not patching gizmos for {thing.def.thingClass.Name} because {reason}."
                );
            }
        }
        
        // Locate our base GetGizmos and any subclasses
        static IEnumerable<MethodBase> TargetMethods()
        {
            var ret = new List<MethodBase>();
            
            // Our base GetGizmos
            var baseGetGizmos = typeof(Building_Storage).GetMethod("GetGizmos");
            ret.Add(baseGetGizmos);
            
            // Find all subclasses of GetGizmo
            var subclasses =
                from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetTypes()
                where type.IsSubclassOf(typeof(Building_Storage))
                select type;

            // Find methods which are not the base method
            var subsGetGizmos =
                from subclass in subclasses
                let subGetGizmo = subclass.GetMethod("GetGizmos")
                where !subGetGizmo.Equals(baseGetGizmos)
                select subGetGizmo;
            ret.AddRange(subsGetGizmos);
            
            // Add classes with custom GetGizmos to our lookup list
            var subGetGizmosClasses =
                from method in subsGetGizmos
                select method.ReflectedType;
            customGetGizmos.AddRange(subGetGizmosClasses);
            ShelfRenamer.Instance.Log(
                $"Patching {customGetGizmos.Count} additional GetGizmos methods for: " +
                String.Join(", ", customGetGizmos));

            return ret;
        }
        public static void Postfix(Building_Storage __instance, ref IEnumerable<Gizmo> __result, MethodBase __originalMethod)
        {
            // ShelfRenamer.Instance.Log("Gizmoing " + __instance.def.thingClass.Name);

            // RimFridge already has its own renamer.
            // if (__instance.def.thingClass.Name == "Building_Refrigerator" ||
            //     __instance.def.thingClass.Name == "RimFridge_Building")
            // {
            //     return;
            // }
            
            // Short-circuit the base GetGizmos patch if it'll be called later-on
            if (customGetGizmos.Contains(__instance.GetType()) &&
                __originalMethod.DeclaringType == typeof(Building_Storage))
            {
                LogNotPatching(__instance, "GetGizmos will be overridden");
                return;
            }
            
            // Loop through existing Gizmos
            foreach (Gizmo giz in __result)
            {
                // If the gizmo is a command...
                if (giz is Command)
                {
                    Command command = (Command)giz;
                    // ...and is labeled "Rename":
                    if (command.defaultLabel == "Rename" || // covers untranslated strings
                        command.defaultLabel == "Rename".Translate())
                    {
                        // Log to the console that we're not patching this,
                        LogNotPatching(__instance, "has gizmo already");
                        // and skip adding a gizmo
                        return;
                    }
                    
                }
            }

            // If it has a user-accessible storage tab, then allow renaming.
            if (!__instance.StorageTabVisible)
            {
                LogNotPatching(__instance, "no storage tab");
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