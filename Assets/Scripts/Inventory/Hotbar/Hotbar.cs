using System;
using UnityEngine;

public class Hotbar : Inventory {

    [Header("References")]
    private PlayerController playerController;
    private Backpack backpack;

    [Header("Settings")]
    private int selectedIndex;
    private int lastSelectedIndex; // used to avoid re-updating the held item when the selected item and index are the same as the last selected item and index; essentially, this means the same item was re-equipped (prevents playing the equip animation each time the backpack contents are updated)
    private Item lastSelectedItem; // used to avoid re-updating the held item when the selected item and index are the same as the last selected item and index; essentially, this means the same item was re-equipped (prevents playing the equip animation each time the backpack contents are updated)

    [Header("Data")]
    public Action onSlotSelected;
    // don't use the hotbar's contents array to get the item stacks, use the backpack's contents array instead

    public override void Initialize() {

        playerController = FindFirstObjectByType<PlayerController>();
        backpack = FindFirstObjectByType<Backpack>();

        initialSlotCount = Mathf.Min(backpack.GetSlotsPerRow(), backpack.GetInitialSlotCount()); // set the initial slot count to the number of slots per row in the backpack (since the top row of the backpack is the hotbar)
        currSlotCount = initialSlotCount;

        backpack.onContentsUpdated += UpdateHeldItem; // subscribe to the backpack's contents updated event to update the held item when the contents change; backpack is used since the hotbar is a part of the backpack (the top row)

        SelectSlot(0); // select the first slot by default

        // no need to call the base class Initialize method, as it is not needed for the hotbar (the contents array from the backpack is used instead of the one from this class)

    }

    private void OnDisable() => backpack.onContentsUpdated -= UpdateHeldItem; // unsubscribe from the backpack's contents updated event to avoid memory leaks

    public void SelectSlot(int index) {

        if (index < 0 || index >= currSlotCount) return; // do nothing if the index is out of bounds
        selectedIndex = index; // set the selected index to the given index

        UpdateHeldItem(); // update the held item
        onSlotSelected?.Invoke(); // invoke the slot selected event

    }

    public void CycleSlot(int cycleAmount) {

        selectedIndex = (selectedIndex + cycleAmount) % currSlotCount; // cycle through the slots, wrapping around if necessary
        if (selectedIndex < 0) selectedIndex += currSlotCount; // ensure the index is not negative

        SelectSlot(selectedIndex); // select the new slot

        // no need to invoke the slot selected event here, as it is already invoked in SelectSlot

    }

    public void UpdateHeldItem() {

        Item currentItem = backpack.GetItemStack(selectedIndex).GetItem();

        // only update if the selected index or item has changed
        if (selectedIndex == lastSelectedIndex && currentItem == lastSelectedItem)
            return;

        if (currentItem == null)
            playerController.SetHeldItem(null); // if the selected slot is empty, set the player's held item to null to remove any held item
        else
            playerController.SetHeldItem(currentItem.GetHeldItemPrefab()); // set the player's held item to the held item prefab of the item in the selected slot of the backpack; backpack is used since the hotbar is a part of the backpack (the top row)

        // track last selected index and item to prevent unnecessary re-equip animations
        lastSelectedIndex = selectedIndex;
        lastSelectedItem = currentItem;

    }

    public override int AddItemStack(ItemStack itemStack) {

        // no point implementing this method since there isn't really a situation where you would want to add an item stack to the hotbar directly; instead, items should be added through the backpack

        Debug.LogError("Cannot add ItemStack directly to the hotbar. Please add items through the backpack."); // output error because ItemStacks cannot be added to the hotbar directly, they must go through the backpack instead
        return -1;

    }

    public override int RemoveItemStack(ItemStack itemStack, int? slotIndex = null) {

        int remainder = 0;

        // go through each slot, starting with the selected slot, then go through the slots from first to last (left to right)
        for (int i = 0; i < currSlotCount; i++) {

            remainder = backpack.RemoveItemStack(itemStack, selectedIndex); // start with the selected slot

            // if the item stack was removed successfully, return 0
            if (remainder == 0)
                return 0;

            // if there is still a remainder, remove the item stack from the rest of the hotbar slots starting from the first slot, skipping the selected slot
            for (int j = 0; j < currSlotCount; j++) {

                if (j == selectedIndex) continue; // skip the selected slot

                remainder = backpack.RemoveItemStack(itemStack, j); // remove the item stack from the current slot

                // if the item stack was removed successfully, return 0
                if (remainder == 0)
                    return 0;

            }
        }

        return remainder;

    }

    public override bool ContainsItemStack(ItemStack itemStack) {

        for (int i = 0; i < currSlotCount; i++)
            if (backpack.GetItemStack(i).GetItem() == itemStack.GetItem()) // check if the item in the slot is the same as the item in the stack; use the backpack's GetItemStack method to get the item stack in the hotbar
                return true; // if it is, return true

        return false; // if no matching item was found, return false

    }

    public override int GetEffectiveStackLimit(Item item) => backpack.GetEffectiveStackLimit(item); // since the hotbar is a part of the backpack, we can use the backpack's GetEffectiveStackLimit method to get the stack limit

    public override ItemStack GetItemStack(int index) => backpack.GetItemStack(index); // return the item stack from the backpack at the given index; backpack is used since the hotbar is a part of the backpack (the top row)

    public int GetSelectedIndex() => selectedIndex;

}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(Hotbar))]
// using UnityEditor prefix to avoid needing to hide the import in the final build
public class HotbarEditor : UnityEditor.Editor {

    public override void OnInspectorGUI() {

        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject,
            "slotsPerRow",
            "initialSlotCount",
            "slotStackLimit",
            "itemTypeFilterType",
            "filteredItemTypes",
            "itemFilterType",
            "filteredItems"
        );

        serializedObject.ApplyModifiedProperties();

    }
}
#endif
