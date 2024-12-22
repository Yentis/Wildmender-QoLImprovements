using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace WildmenderMod
{
    internal class DepositDestroyItemsPatch
    {
        public static void Patch(Harmony harmony)
        {
            try
            {
                harmony.Patch(
                    original: typeof(UICharacterInventoryPanel).GetMethod("PaintSelectedItem", BindingFlags.Instance | BindingFlags.NonPublic),
                    postfix: new HarmonyMethod(typeof(DepositDestroyItemsPatch).GetMethod(nameof(PaintSelectedItem)))
                );
            }
            catch (Exception error)
            {
                Plugin.Logger.LogError($"{nameof(DepositDestroyItemsPatch)} failed: {error}");
            }
        }

        private static readonly Type SelectedItemType = typeof(UICharacterInventoryPanel).Assembly.GetType("UICharacterInventoryPanel+SelectedItem");
        private static readonly FieldInfo SelectedItemField = typeof(UICharacterInventoryPanel).GetField("selectedItem", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo ActionsField = typeof(UICharacterInventoryPanel).GetField("actions", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo StorageField = typeof(UICharacterInventoryPanel).GetField("storage", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo ItemField = SelectedItemType.GetField("_item", BindingFlags.Instance | BindingFlags.NonPublic);
        public static void PaintSelectedItem(ref UICharacterInventoryPanel __instance)
        {
            var selectedItem = SelectedItemField.GetValue(__instance);
            if (selectedItem == null) return;

            var storage = StorageField.GetValue(__instance);
            if (storage == null) return;

            var actions = (List<ButtonAction>)ActionsField.GetValue(__instance);
            actions.Add(new DepositItemsButtonAction());

            var item = (InventoryItem)ItemField.GetValue(selectedItem);
            if (storage.ToString().Contains("CompostBin") && item != null)
            {
                actions.Add(new DestroyItemButtonAction(() => item));
            }

            UICharacterMenu.Instance.ShowActionBar(actions);
        }

        private static void DoDeposit()
        {
            Plugin.Logger.LogInfo("Starting deposit");

            var player = OasisPlayer.Instance;
            var position = player.Position;

            var chunk = WorldController.Instance.GetContainingChunk(position);
            var baskets = new List<StorageEntity>();

            foreach (var feature in chunk.Features)
            {
                if (feature != null && feature.name != "StorageBasket") continue;
                baskets.Add((StorageEntity)feature);
            }

            var inventoryItems = new Dictionary<ItemKind, List<InventoryItem>>();

            foreach (var inventoryItem in player.ItemList.Items)
            {
                var kind = inventoryItem.GetKind();

                if (inventoryItems.ContainsKey(kind))
                {
                    inventoryItems[kind].Add(inventoryItem);
                }
                else
                {
                    inventoryItems[kind] = [inventoryItem];
                }
            }

            foreach (var basket in baskets)
            {
                foreach (var item in basket.ItemList.Items)
                {
                    var kind = item.GetKind();
                    if (!inventoryItems.ContainsKey(kind)) continue;

                    var matchingItems = inventoryItems[kind];
                    matchingItems.RemoveAll(matchingItem =>
                    {
                        var success = item.TryMergeStack(matchingItem);
                        if (success)
                        {
                            Plugin.Logger.LogInfo($"Deposited {kind.GetItemLabel(matchingItem)} into basket");
                        }

                        return matchingItem.Quantity <= 0;
                    });
                }
            }
        }

        private static void DoDestroy(InventoryItem item)
        {
            Plugin.Logger.LogInfo("Starting destroy");
            item.Quantity = 0;
        }

        private class DepositItemsButtonAction : ButtonAction
        {
            public DepositItemsButtonAction()
            {
                actionName = PlayerInput.UIAction4Name;
            }

            public override string Name => "Deposit Items";
            public override void Callback() => DoDeposit();
        }

        private class DestroyItemButtonAction : ButtonAction
        {
            private readonly Func<InventoryItem> getItem;

            public DestroyItemButtonAction(Func<InventoryItem> getItem)
            {
                this.getItem = getItem;
                actionName = PlayerInput.UIAction4Name;
            }

            public override string Name => "(Hold) Destroy Item";
            public override void HoldCallback() => DoDestroy(getItem());
        }
    }
}
