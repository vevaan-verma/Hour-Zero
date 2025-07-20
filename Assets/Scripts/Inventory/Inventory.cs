using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Inventory : MonoBehaviour {

    [Header("Settings")]
    [SerializeField, Tooltip("The number of slots to show per row in the inventory UI (used to calculate the size of the inventory contents grid layout group)")] protected int slotsPerRow;
    [SerializeField, Min(1)] protected int initialSlotCount;
    [SerializeField, Tooltip("The maximum number of items that can be stacked in a slot. If set to 0, it will either use the item's stack limit, or if that is set to 0, an infinite limit")] private int slotStackLimit;
    [SerializeField, Tooltip("Type of filter to apply to the item types in the filteredItemTypes array (e.g., whitelist or blacklist)")] protected FilterType itemTypeFilterType;
    [SerializeField] protected ItemType[] filteredItemTypes;
    [SerializeField, Tooltip("Type of filter to apply to the filteredItems array (e.g., whitelist or blacklist)")] protected FilterType itemFilterType;
    [SerializeField] protected Item[] filteredItems;
    [SerializeField] private bool visibleByDefault;
    protected int currSlotCount;

    [Header("Data")]
    protected List<ItemStack> contents; // a list is used because the inventory size can change

    [Header("Actions")]
    public Action onContentsUpdated;

    public virtual void Initialize() {

        contents = new List<ItemStack>(currSlotCount);
        currSlotCount = initialSlotCount;

        // initialize the contents with empty ItemStacks
        for (int i = 0; i < currSlotCount; i++)
            contents.Add(new ItemStack(null, 0));

    }

    // returns the amount of items that could not be added to the inventory; null item signifies that the slot should be set to empty
    public int SetItemStack(ItemStack itemStack, int index) {

        Item item = itemStack.GetItem();
        int count = itemStack.GetCount();

        if (count < 0 || index < 0 || index >= contents.Count) return count; // return the count since we couldn't add anything

        // if the item is null, set the slot to an empty stack
        if (item == null || count == 0) {

            contents[index] = new ItemStack(null, 0);
            onContentsUpdated?.Invoke(); // invoke the item added event
            return 0;

        }

        if (filteredItemTypes.Length > 0) { // if there are filtered item types, check if the item type is allowed by the filter

            bool found = Array.FindIndex(filteredItemTypes, x => x.Equals(item.GetItemType())) >= 0; // use FindIndex to check if the item type is in the filtered item types list

            if ((itemTypeFilterType == FilterType.Whitelist && !found) || (itemTypeFilterType == FilterType.Blacklist && found)) // if the filter is a whitelist and the item type is not found, or if the filter is a blacklist and the item type is found, return the count because no items were added
                return count; // item type not allowed by filter

        }

        if (filteredItems.Length > 0) { // if there are filtered items, check if the item is allowed by the filter

            bool found = Array.FindIndex(filteredItems, x => x != null && x.Equals(item)) >= 0; // use FindIndex to check if the item is in the filtered items list

            if ((itemFilterType == FilterType.Whitelist && !found) || (itemFilterType == FilterType.Blacklist && found)) // if the filter is a whitelist and the item is not found, or if the filter is a blacklist and the item is found, return the count because no items were added
                return count; // item not allowed by filter

        }

        int stackLimit = GetEffectiveStackLimit(item);
        int toSet = Mathf.Min(stackLimit, count); // how many we can set in the slot
        contents[index] = new ItemStack(item, toSet); // set the item stack in the slot
        onContentsUpdated?.Invoke(); // invoke the item added event
        return count - toSet; // return the count of items that could not be set

    }

    // returns the amount of items that could not be added to the inventory
    public virtual int AddItemStack(ItemStack itemStack) {

        Item item = itemStack.GetItem();
        int count = itemStack.GetCount();

        if (item == null || count <= 0) return count; // return the count since we couldn't add anything

        if (filteredItemTypes.Length > 0) { // if there are filtered item types, check if the item type is allowed by the filter

            bool found = Array.FindIndex(filteredItemTypes, x => x.Equals(item.GetItemType())) >= 0; // use FindIndex to check if the item type is in the filtered item types list

            if ((itemTypeFilterType == FilterType.Whitelist && !found) || (itemTypeFilterType == FilterType.Blacklist && found)) // if the filter is a whitelist and the item type is not found, or if the filter is a blacklist and the item type is found, return the count because no items were added
                return count; // item type not allowed by filter

        }

        if (filteredItems.Length > 0) { // if there are filtered items, check if the item is allowed by the filter

            bool found = Array.FindIndex(filteredItems, x => x != null && x.Equals(item)) >= 0; // use FindIndex to check if the item is in the filtered items list

            if ((itemFilterType == FilterType.Whitelist && !found) || (itemFilterType == FilterType.Blacklist && found)) // if the filter is a whitelist and the item is not found, or if the filter is a blacklist and the item is found, return the count because no items were added
                return count; // item not allowed by filter

        }

        // first, try to stack into existing stacks
        for (int i = 0; i < contents.Count; i++) {

            ItemStack stack = contents[i];

            if (stack.GetItem() != null && stack.GetItem().Equals(item)) { // check if the stack already contains the item

                int currentCount = stack.GetCount();
                int remainder = SetItemStack(new ItemStack(item, currentCount + count), i); // set the item stack in the slot with the new count
                count = remainder; // update count to the remainder

                if (count <= 0) return 0; // return 0 since all items were added; no need to invoke the item added event here, as it will be invoked in the SetItemStack method if items were successfully added

            }
        }

        // then, try to add to empty slots (including whatever wasn't able to be stacked), starting from the first slot (top left to bottom right)
        for (int i = 0; i < contents.Count; i++) {

            ItemStack stack = contents[i];

            if (stack.GetItem() == null || stack.GetCount() == 0) { // check if the slot is empty

                count = SetItemStack(new ItemStack(item, count), i); // set the item stack in the slot and get the remainder of items that couldn't be added

                if (count <= 0) return 0; // return 0 since all items were added; no need to invoke the item added event here, as it will be invoked in the SetItemStack method if items were successfully added

            }
        }

        // if we reach here, not all items could be added

        // no need to invoke the item added event here, as it was already invoked in the SetItemStack method if items were successfully added

        return count; // return the count of items that could not be added

    }

    // returns the amount of items that could not be removed from the inventory; allows specifying a slot index to prioritize removing from, or null to remove from any slot
    public virtual int RemoveItemStack(ItemStack itemStack, int? slotIndex = null) {

        Item item = itemStack.GetItem();
        int count = itemStack.GetCount();

        if (item == null || count <= 0) return 0; // return 0 since we couldn't remove anything

        // if the slot index is specified, remove from there first
        if (slotIndex.HasValue && slotIndex >= 0 && slotIndex < contents.Count) {

            ItemStack stack = contents[(int) slotIndex];

            if (stack.GetItem() != null && stack.GetItem().Equals(item)) { // check if the stack contains the item

                int toRemove = Mathf.Min(stack.GetCount(), count); // how many we can remove
                int newCount = stack.GetCount() - toRemove; // calculate the new count after removing

                SetItemStack(newCount > 0 ? new ItemStack(item, newCount) : new ItemStack(null, 0), slotIndex.Value); // set the item stack in the slot with the new count or empty if the count is 0

                // SetItemStack returns the amount that could not be set, but for removal we already know the new count, so we just update count to reflect the removed items
                count -= toRemove;

                if (count <= 0)
                    return 0; // return 0 since all items were removed

            }
        }

        // if the slot index is not specified or there is still count left to remove, we need to search through the inventory contents

        // remove from the last slots first
        for (int i = contents.Count - 1; i >= 0; i--) {

            ItemStack stack = contents[i];

            if (stack.GetItem() != null && stack.GetItem().Equals(item)) { // check if the stack contains the item

                int toRemove = Mathf.Min(stack.GetCount(), count); // how many we can remove
                int newCount = stack.GetCount() - toRemove; // calculate the new count after removing

                SetItemStack(newCount > 0 ? new ItemStack(item, newCount) : new ItemStack(null, 0), i); // set the item stack in the slot with the new count or empty if the count is 0

                // SetItemStack returns the amount that could not be set, but for removal we already know the new count, so we just update count to reflect the removed items
                count -= toRemove;

                if (count <= 0)
                    return 0; // return 0 since all items were removed

            }
        }

        // if we reach here, not all items could be removed
        return count; // return the count of items that could not be removed

    }

    public virtual bool ContainsItemStack(ItemStack itemStack) {

        Item item = itemStack.GetItem();
        int count = itemStack.GetCount();

        int totalCount = 0;

        foreach (ItemStack stack in contents) {

            if (stack.GetItem() != null && stack.GetItem().Equals(item)) {

                totalCount += stack.GetCount();

                if (totalCount >= count)
                    return true; // if we have enough items, return true

            }
        }

        return false; // if we reach here, we don't have enough items

    }

    public void Clear() {

        contents = new List<ItemStack>(currSlotCount);

        // initialize the contents with empty ItemStacks
        for (int i = 0; i < currSlotCount; i++)
            contents.Add(new ItemStack(null, 0));

    }

    // helper to get the effective stack limit for an item in a slot
    public virtual int GetEffectiveStackLimit(Item item) {

        // how the effective stack limit is determined:
        // 1. if both the item and the slot stack limit are greater than 0, use the smaller of the two
        // 2. if the item limit is greater than 0 and the slot limit is 0, use the item limit
        // 3. if the slot limit is greater than 0 and the item limit is 0, use the slot limit
        // 4. if both the item and slot limits are 0, use an infinite stack limit (int.MaxValue)
        int itemLimit = item.GetStackSize();
        int slotLimit = slotStackLimit;

        if (itemLimit > 0 && slotLimit > 0)
            return Mathf.Min(itemLimit, slotLimit); // if both limits are greater than 0, use the smaller of the two
        if (itemLimit > 0 && slotLimit == 0)
            return itemLimit; // if item limit is greater than 0 and slot limit is 0, use item limit
        if (slotLimit > 0 && itemLimit == 0)
            return slotLimit; // if slot limit is greater than 0 and item limit is 0, use slot limit

        return int.MaxValue; // if both limits are 0, use an infinite stack limit

    }

    // targetQuickTransferInventory is the inventory to which the items will be quick transferred
    public virtual void QuickTransferItem(Inventory quickTransferInventory, ItemStack itemStack, int slotIndex) {

        if (quickTransferInventory == null) return; // if there is no quick transfer inventory, do nothing

        int remainder = quickTransferInventory.AddItemStack(itemStack); // try to add the item stack to the quick transfer inventory
        RemoveItemStack(new ItemStack(itemStack.GetItem(), itemStack.GetCount() - remainder), slotIndex); // remove the items that were successfully added to the quick transfer inventory from this inventory

    }

    public List<ItemStack> GetContents() => contents;

    public virtual ItemStack GetItemStack(int index) => contents[index];

    public int GetSlotsPerRow() => slotsPerRow;

    public int GetInitialSlotCount() => initialSlotCount;

    public int GetCurrentSlotCount() => currSlotCount;

    public bool IsVisibleByDefault() => visibleByDefault;

}

public enum FilterType {

    Whitelist, // only items in the filteredItems list are allowed
    Blacklist // items in the filteredItems list are not allowed

}
