using HarmonyLib;
using InControl;
using SRML;
using SRML.Console;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Console = SRML.Console.Console;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace DroneInfiniteItemFix
{
    public class Main : ModEntryPoint
    {
        internal static Assembly modAssembly = Assembly.GetExecutingAssembly();
        internal static string modName = $"{modAssembly.GetName().Name}";
        internal static string modDir = $"{System.Environment.CurrentDirectory}\\SRML\\Mods\\{modName}";
        public static Main instance;

        public Main() => instance = this;

        public override void PreLoad()
        {
            HarmonyInstance.PatchAll();
        }
        public static void Log(string message) => instance.ConsoleInstance.Log($"[{modName}]: " + message);
        public static void LogError(string message) => instance.ConsoleInstance.LogError($"[{modName}]: " + message);
        public static void LogWarning(string message) => instance.ConsoleInstance.LogWarning($"[{modName}]: " + message);
        public static void LogSuccess(string message) => instance.ConsoleInstance.LogSuccess($"[{modName}]: " + message);
    }

    [HarmonyPatch(typeof(SpawnResource.GatherGroup))]
    static class Patch_SpawnResource_GatherGroup_Decrement
    {
        public static Dictionary<(SpawnResource,Identifiable.Id), int> removed = new Dictionary<(SpawnResource, Identifiable.Id), int>();
        [HarmonyPatch("Decrement")]
        [HarmonyPrefix]
        static void Decrement(SpawnResource.GatherGroup __instance, int decrement)
        {
            if (!removed.TryGetValue((__instance.resource,__instance.id), out var r))
                r = 0;
            removed[(__instance.resource, __instance.id)] = r + decrement;
        }
        [HarmonyPatch(MethodType.Constructor,typeof(SpawnResource),typeof(Identifiable.Id),typeof(int))]
        [HarmonyPrefix]
        static void ctor(SpawnResource resource, Identifiable.Id id, ref int count)
        {
            if (removed.TryGetValue((resource, id), out var r))
                count -= r;
        }
    }

    [HarmonyPatch(typeof(DroneFastForwarder), "FastForward_Pre")]
    static class Patch_FastForward_Pre
    {
        static void Postfix()
        {
            Patch_SpawnResource_GatherGroup_Decrement.removed.Clear();
        }
    }
}