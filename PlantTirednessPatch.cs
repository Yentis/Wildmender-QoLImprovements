using HarmonyLib;
using System;

namespace WildmenderMod
{
    internal class PlantTirednessPatch
    {
        public static void Patch(Harmony harmony)
        {
            try
            {
                harmony.Patch(
                    original: typeof(BasicPlant).GetMethod(nameof(BasicPlant.GetHarvestGrowthRate)),
                    prefix: new HarmonyMethod(typeof(PlantTirednessPatch).GetMethod(nameof(GetHarvestGrowthRate)))
                );
            }
            catch (Exception error)
            {
                Plugin.Logger.LogError($"{nameof(PlantTirednessPatch)} failed: {error}");
            }
        }

        public static void GetHarvestGrowthRate(ref BasicPlant __instance)
        {
            if (Plugin.configPlantTiredness?.Value ?? Plugin.DEFAULT_PLANT_TIREDNESS) return;

            if (__instance.ExhaustionHarvestCount > 0)
            {
                __instance.ExhaustionHarvestCount = 0;
            }
        }
    }
}
