using HarmonyLib;
using System;
using System.Reflection;

namespace WildmenderMod
{
    internal class BedrollSleepPatch
    {
        public static void Patch(Harmony harmony)
        {
            try
            {
                harmony.Patch(
                    original: typeof(BedrollCrafting).GetMethod(nameof(BedrollCrafting.IsRecipeDisabled)),
                    postfix: new HarmonyMethod(typeof(BedrollSleepPatch).GetMethod(nameof(IsRecipeDisabled)))
                );
            }
            catch (Exception error)
            {
                Plugin.Logger.LogError($"{nameof(BedrollSleepPatch)} failed: {error}");
            }
        }

        private static readonly FieldInfo StartRestingRecipeField = typeof(BedrollCrafting).GetField("startRestingRecipe", BindingFlags.Instance | BindingFlags.NonPublic);
        public static void IsRecipeDisabled(ref BedrollCrafting __instance, IRecipe recipe, ref bool __result)
        {
            var startRestingRecipe = StartRestingRecipeField.GetValue(__instance);

            if (recipe != startRestingRecipe) { return; }
            if (!OasisPlayer.AllowRest) { return; }

            __result = false;
        }
    }
}
