using System;
using UnityEngine;

public class Hotbar : Inventory {

    [Header("References")]
    private Backpack backpack;

    [Header("Settings")]
    private int selectedIndex;
    private int lastSelectedIndex; // used to avoid re-updating the held item when the selected item and index are the same as the last selected item and index; essentially, this means the same item was re-equipped (prevents playing the equip animation each time the backpack contents are updated)
    private Item lastSelectedItem; // used to avoid re-updating the held item when the selected item and index are the same as the last selected item and index; essentially, this means the same item was re-equipped (prevents playing the equip animation each time the backpack contents are updated)

    [Header("Actions")]
    public Action onSlotSelected;

    // don't use the hotbar's contents array to get the item stacks, use the backpack's contents array instead

    public override void Initialize() {

        playerController = FindFirstObjectByType<PlayerController>(); // set the playerController here even though it is already set in the base class Initialize method, because the base method isn't called in this class
        backpack = FindFirstObjectByType<Backpack>();

        slotCount = Mathf.Min(backpack.GetSlotsPerRow(), backpack.GetSlotCount()); // set the slot count to the number of slots per row in the backpack (since the top row of the backpack is the hotbar)

        backpack.onContentsUpdated += UpdateHeldItem; // subscribe to the backpack's contents updated event to update the held item when the contents change; backpack is used since the hotbar is a part of the backpack (the top row)

        SelectSlot(0); // select the first slot by default

        // no need to call the base class Initialize method, as it is not needed for the hotbar (the contents array from the backpack is used instead of the one from this class)

    }

    private void OnDisable() => backpack.onContentsUpdated -= UpdateHeldItem; // unsubscribe from the backpack's contents updated event to avoid memory leaks

    public void SelectSlot(int index) {

        if (index < 0 || index >= slotCount) return; // do nothing if the index is out of bounds
        selectedIndex = index; // set the selected index to the given index

        UpdateHeldItem(); // update the held item
        onSlotSelected?.Invoke(); // invoke the slot selected event

    }

    public void CycleSlot(int cycleAmount) {

        selectedIndex = (selectedIndex + cycleAmount) % slotCount; // cycle through the slots, wrapping around if necessary
        if (selectedIndex < 0) selectedIndex += slotCount; // ensure the index is not negative

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

    public override int AddItemStack(ItemStack itemStack, bool dropRemainder) {

        Debug.LogError("Cannot add ItemStack directly to the hotbar. Please add items through the backpack."); // output error because ItemStacks cannot be added to the hotbar directly, they must go through the backpack instead
        return -1;

    }

    public override int RemoveItemStack(ItemStack itemStack, int? slotIndex = null) {

        Debug.LogError("Cannot remove ItemStack directly from the hotbar. Please remove items through the backpack."); // output error because ItemStacks cannot be removed from the hotbar directly, they must go through the backpack instead
        return -1;

    }

    public override bool ContainsItemStack(ItemStack itemStack) {

        for (int i = 0; i < slotCount; i++)
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
