using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace WildmenderMod
{
    internal class FrogImprovementPatch
    {
        private static readonly MethodInfo SweepOasisForItemsMethod = typeof(FrogAnimal).GetMethod(nameof(SweepOasisForItems), BindingFlags.Instance | BindingFlags.NonPublic);

        public static void Patch(Harmony harmony)
        {
            try
            {
                harmony.Patch(
                    original: typeof(FrogAnimal).GetMethod(nameof(FrogAnimal.FixedUpdate)),
                    prefix: new HarmonyMethod(typeof(FrogImprovementPatch).GetMethod(nameof(FixedUpdate)))
                );

                harmony.Patch(
                    original: SweepOasisForItemsMethod,
                    prefix: new HarmonyMethod(typeof(FrogImprovementPatch).GetMethod(nameof(SweepOasisForItems)))
                );
            }
            catch (Exception error)
            {
                Plugin.Logger.LogError($"{nameof(FrogImprovementPatch)} failed: {error}");
            }
        }

        public static bool SweepOasisForItems(ref FrogAnimal __instance)
        {
            if (__instance.ContainerChunk == null)
            {
                return false;
            }

            Plugin.Logger.LogInfo("Sweeping oasis for items...");
            var itemsToPickup = new List<SceneItem>();

            foreach (var worldEntityBehaviour in __instance.ContainerChunk.Features)
            {
                var sceneItem = worldEntityBehaviour as SceneItem;
                if (sceneItem == null || !__instance.ItemList.CanAddItem(sceneItem.Item) || !__instance.IsValidForPickup(sceneItem.Item)) continue;

                itemsToPickup.Add(sceneItem);
            }

            foreach (var sceneItem in itemsToPickup)
            {
                __instance.ItemList.AddItem(sceneItem.Item, -1);
                sceneItem.DestroyEntity();
            }

            return false;
        }

        private static readonly FieldInfo BaseSweepTimerField = typeof(FrogAnimal).GetField("baseSweepTimer", BindingFlags.Instance | BindingFlags.NonPublic);
        public static void FixedUpdate(ref FrogAnimal __instance)
        {
            if (!MultiplayerManager.IsHost || !__instance.atBase || __instance.ContainerChunk == null || __instance.ContainerChunk.LevelOfDetail != WorldChunkBehaviour.ChunkLOD.Full) return;

            var baseSweepTimer = (float)BaseSweepTimerField.GetValue(__instance);
            var newBaseSweepTimer = Mathf.Min(20f, baseSweepTimer + Time.fixedDeltaTime);

            if (newBaseSweepTimer >= 20f)
            {
                SweepOasisForItemsMethod.Invoke(__instance, []);
                BaseSweepTimerField.SetValue(__instance, 0f);
            }
            else
            {
                BaseSweepTimerField.SetValue(__instance, newBaseSweepTimer);
            }
        }
    }
}
