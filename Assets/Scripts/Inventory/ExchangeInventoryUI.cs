using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class ExchangeInventoryUI : InventoryUI {

    [Header("Settings")]
    protected int inputSlotCount;
    protected int outputSlotCount;

    // updates the placeholder items in the inventory slots to cycle through the available items that can be exchanged
    protected void UpdatePlaceholderCycle() {

        List<Item> placeholderItems = inventory.GetFilteredItems().ToList();

        // loop through the filled slot items and remove them from the placeholder items array so only the items that haven't been set in the slots are used as placeholders
        for (int i = 0; i < inventorySlots.Length; i++)
            if (inventorySlots[i].IsItemStackSet())
                placeholderItems.Remove(inventorySlots[i].GetItemStack().GetItem()); // remove the item from the placeholder items list

        // if there are no placeholder items left, return as there is nothing to cycle through (all exchange items have been set in the slots)
        if (placeholderItems.Count == 0)
            return;

        // offset shifts the starting index for cycling through placeholder items, creating a rotating effect in the inventory slots (offset decreasing each cycle makes the placeholder items appear to shift right)
        for (int offset = placeholderItems.Count - 1; offset >= 0; offset--) {

            // cycle through the INPUT slots and set the placeholder items (subtract outputSlotCount from the total length to only set placeholders for input slots)
            for (int i = 0; i < inventorySlots.Length - outputSlotCount; i++) {

                if (inventorySlots[i].IsItemStackSet()) continue; // skip the slot if it already has an item stack set since it doesn't need a placeholder

                int itemIndex = (i + offset) % placeholderItems.Count; // calculate the index of the placeholder item to set in the slot, wrapping around if necessary
                Item placeholderItem = placeholderItems[itemIndex]; // get the placeholder item from the filtered items based on the calculated index

                inventorySlots[i].SetPlaceholderItem(placeholderItem);

            }
        }
    }
}
