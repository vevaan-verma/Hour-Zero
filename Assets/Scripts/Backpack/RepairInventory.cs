using System.Collections.Generic;
using UnityEngine;

public class RepairInventory : Inventory {

    [Header("Data")]
    private List<ItemStack> contents;

    [Header("Settings")]
    [SerializeField] private int initialCapacity;
    [SerializeField] private int slotStackLimit;
    private int currCapacity;

    public override void Initialize() {

        contents = new List<ItemStack>(currCapacity);
        currCapacity = initialCapacity;

        for (int i = 0; i < currCapacity; i++)
            contents.Add(new ItemStack(null, 0));

    }

    // returns the amount of items that could not be added to the inventory; null item signifies that the slot should be set to empty
    public override int SetItemStack(int index, Item item, int count) {

        if (count < 0 || index < 0 || index >= contents.Count) return count; // return the count since we couldn't add anything

        // if the item is null, set the slot to an empty stack
        if (item == null || count == 0) {

            contents[index] = new ItemStack(null, 0);
            return 0;

        }

        int stackLimit = GetEffectiveStackLimit(item);
        int toSet = Mathf.Min(stackLimit, count); // how many can we set in the slot
        contents[index] = new ItemStack(item, toSet); // set the item stack in the slot
        return count - toSet; // return the count of items that could not be set

    }

    // returns the amount of items that could not be added to the backpack
    public override int AddItemStack(Item item, int count) {

        if (item == null || count <= 0) return count; // return the count since we couldn't add anything

        int stackLimit = GetEffectiveStackLimit(item);

        // first, try to stack into existing stacks
        for (int i = 0; i < contents.Count; i++) {

            ItemStack stack = contents[i];

            if (stack.GetItem() != null && stack.GetItem().Equals(item)) { // if the stack already contains the item

                int currentCount = stack.GetCount();
                int toAdd = Mathf.Min(stackLimit - currentCount, count); // how many can we add to this stack

                if (toAdd > 0) {

                    SetItemStack(i, item, currentCount + toAdd); // set the item stack in the slot with the new count
                    count -= toAdd; // reduce the count of items to add

                }

                if (count <= 0)
                    return 0; // return 0 since all items were added

            }
        }

        // then, try to add to empty slots
        for (int i = 0; i < contents.Count; i++) {

            ItemStack stack = contents[i];

            if (stack.GetItem() == null || stack.GetCount() == 0) { // check if the slot is empty

                int toAdd = Mathf.Min(stackLimit, count); // how many can we add to this slot
                SetItemStack(i, item, toAdd); // set the item stack in the slot
                count -= toAdd; // reduce the count of items to add based on what was added right now

                if (count <= 0)
                    return 0; // return 0 since all items were added

            }
        }

        // if we reach here, not all items could be added
        return count;

    }

    // returns the amount of items that were removed from the inventory
    public override int RemoveItemStack(Item item, int count) {

        if (item == null || count <= 0) return 0; // return 0 since we couldn't remove anything

        int removed = 0;

        // remove from the last slots first
        for (int i = contents.Count - 1; i >= 0; i--) {

            ItemStack stack = contents[i];

            if (stack.GetItem() != null && stack.GetItem().Equals(item) && stack.GetCount() > 0) {

                int toRemove = Mathf.Min(stack.GetCount(), count - removed);
                stack.RemoveItem(toRemove); // remove the items from the stack
                removed += toRemove;

                // if the stack is now empty, set it to null
                if (stack.GetCount() == 0)
                    contents[i] = new ItemStack(null, 0);

                // if we have removed enough items, return the count of removed items
                if (removed >= count)
                    return removed;

            }
        }

        return removed;

    }

    public bool ContainsItemStack(Item item, int count) {

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

    // helper to get the effective stack limit for an item in a slot
    public override int GetEffectiveStackLimit(Item item) {

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

    public int GetInitialCapacity() => initialCapacity;

    public override ItemStack GetItemStack(int index) => contents[index];

    public override void SwapItemStacks(int indexA, int indexB) => (contents[indexB], contents[indexA]) = (contents[indexA], contents[indexB]); // swap the item stacks at the given indices

}
