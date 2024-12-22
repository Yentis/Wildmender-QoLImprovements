using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace WildmenderMod
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string
            MODNAME = "QoLImprovements",
            AUTHOR = "Yentis",
            GUID = "com." + AUTHOR + "." + MODNAME,
            VERSION = "1.0.0";

        internal static new ManualLogSource Logger;

        public const bool DEFAULT_PLANT_TIREDNESS = false;
        public static ConfigEntry<bool> configPlantTiredness;

        private void Awake()
        {
            Logger = base.Logger;
            var harmony = new Harmony(GUID);

            configPlantTiredness = Config.Bind(
                section: "General",
                key: "PlantTiredness",
                defaultValue: DEFAULT_PLANT_TIREDNESS,
                description: "Do plants get tired?"
            );

            BedrollSleepPatch.Patch(harmony);
            FrogImprovementPatch.Patch(harmony);
            PlantTirednessPatch.Patch(harmony);
            DepositDestroyItemsPatch.Patch(harmony);

            Logger.LogInfo($"Plugin {GUID} is loaded!");
        }
    }
}
