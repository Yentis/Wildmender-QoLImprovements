using HarmonyLib;
using System;

namespace WildmenderMod
{
    internal class CheatPatch
    {
        public static void Patch(Harmony harmony)
        {
            try
            {
                harmony.Patch(
                    original: typeof(LaunchParameters).GetMethod(nameof(LaunchParameters.TryGet), [typeof(string), typeof(bool).MakeByRefType()]),
                    postfix: new HarmonyMethod(typeof(CheatPatch).GetMethod(nameof(TryGet)))
                );
            }
            catch (Exception error)
            {
                Plugin.Logger.LogError($"{nameof(CheatPatch)} failed: {error}");
            }
        }

        public static void TryGet(ref bool __result, string key, out bool value)
        {
            var cheatsEnabled = Plugin.configCheats?.Value ?? Plugin.DEFAULT_CHEATS;

            if (!cheatsEnabled || key.ToLower() != "enableqa")
            {
                value = __result;
                return;
            }

            value = true;
            __result = true;
        }
    }
}
