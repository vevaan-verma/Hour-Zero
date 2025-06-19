using System;
using UnityEditor;
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

        // Only update if the selected index or item has changed
        if (selectedIndex == lastSelectedIndex && currentItem == lastSelectedItem)
            return;

        if (currentItem == null)
            playerController.SetHeldItem(null); // if the selected slot is empty, set the player's held item to null to remove any held item
        else
            playerController.SetHeldItem(currentItem.GetHeldItemPrefab()); // set the player's held item to the held item prefab of the item in the selected slot of the backpack; backpack is used since the hotbar is a part of the backpack (the top row)

        // Track last selected index and item to prevent unnecessary re-equip animations
        lastSelectedIndex = selectedIndex;
        lastSelectedItem = currentItem;

    }

    public override int AddItemStack(ItemStack itemStack) {

        Debug.LogError("Cannot add ItemStack directly to the hotbar. Please add items through the backpack."); // output error because ItemStacks cannot be added to the hotbar directly, they must go through the backpack instead
        return -1;

    }

    public override int RemoveItemStack(ItemStack itemStack) {

        Debug.LogError("Cannot remove ItemStack directly from the hotbar. Please remove items through the backpack."); // output error because ItemStacks cannot be removed from the hotbar directly, they must go through the backpack instead
        return -1;

    }

    public override bool ContainsItemStack(ItemStack itemStack) {

        for (int i = 0; i < currSlotCount; i++)
            if (backpack.GetItemStack(i).GetItem() == itemStack.GetItem()) // check if the item in the slot is the same as the item in the stack; use the backpack's GetItemStack method to get the item stack in the hotbar
                return true; // if it is, return true

        return false; // if no matching item was found, return false

    }

    public override int GetEffectiveStackLimit(Item item) => backpack.GetEffectiveStackLimit(item); // since the hotbar is a part of the backpack, we can use the backpack's GetEffectiveStackLimit method to get the stack limit

    public int GetSelectedIndex() => selectedIndex;

}

// <summary>
// custom editor for the Hotbar class to allow for a range slider for the slotStackLimit field
// since we need to constrain the slot count between 1 and 9
// this is because each slot needs to have a single digit key binding from 1 to 9
// </summary>
[CustomEditor(typeof(Hotbar))]
public class HotbarEditor : Editor {

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
