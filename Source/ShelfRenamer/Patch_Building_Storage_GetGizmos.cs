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
        // List of already-seen thingClasses, decisions, and reasons
        private static List<(Type, bool, String)> alreadySeen = new List<(Type, bool, String)>();
        
        // List of subclasses with their own GetGizmos
        // We'll short-circuit the base postfix if they're in this list.
        private static List<Type> customGetGizmos = new List<Type>();
        
        // Debug logging method
        private static void LogOnlyOnce(Type type, bool willPatch, string reason)
        {
            (Type, bool, String) index = (type, willPatch, reason);
            if (!alreadySeen.Contains(index))
            {
                alreadySeen.Add(index);
                ShelfRenamer.Instance.Log(
                    $"Patch gizmos: {willPatch} for {type} because {reason}."
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
            
            // Find all subclasses of Building_Storage
            var subclasses =
                from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetTypes()
                where type.IsSubclassOf(typeof(Building_Storage))
                select type;
            ShelfRenamer.Instance.Log(
                $"{subclasses.Count()} subclasses of Building_Storage: {String.Join(", ", subclasses)}");

            // Find methods which are not the base method
            var subsGetGizmos =
                from subclass in subclasses
                let subGetGizmo = subclass.GetMethod("GetGizmos")
                where !subGetGizmo.MethodHandle.Equals(baseGetGizmos.MethodHandle)
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
            // Short-circuit the base GetGizmos patch if it'll be called later-on
            if (customGetGizmos.Contains(__instance.GetType()) &&
                __originalMethod.DeclaringType == typeof(Building_Storage))
            {
                LogOnlyOnce(__instance.GetType(), false, "GetGizmos will be overridden");
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
                        LogOnlyOnce(__instance.GetType(), false, "has gizmo already");
                        // and skip adding a gizmo
                        return;
                    }
                    
                }
            }

            // If it has a user-accessible storage tab, then allow renaming.
            if (!__instance.StorageTabVisible)
            {
                LogOnlyOnce(__instance.GetType(), false, "no storage tab");
                return;
            }

            LogOnlyOnce(__instance.GetType(), true, $"OK for {__instance.GetType()}");
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