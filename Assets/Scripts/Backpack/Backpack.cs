using System;
using System.Collections.Generic;
using UnityEngine;

public class Backpack : MonoBehaviour {

    [Header("References")]
    private AlertManager alertManager;

    [Header("UI References")]
    [SerializeField] private Slot backpackSlot; // used to get the slot stack limit
    private int slotStackLimit;

    [Header("Settings")]
    [SerializeField] private int initialCapacity;
    private int currCapacity;

    [Header("Data")]
    private List<ItemStack> contents;

    private void Start() {

        alertManager = FindFirstObjectByType<AlertManager>();
        contents = new List<ItemStack>(currCapacity);
        slotStackLimit = backpackSlot.GetStackLimit();
        currCapacity = initialCapacity;

        for (int i = 0; i < currCapacity; i++)
            contents.Add(new ItemStack(null, 0));

    }

    // returns the amount of items that could not be added to the backpack
    public int AddItem(Item item, int count) {

        int stackLimit = GetEffectiveStackLimit(item);

        // first, try to stack into existing stacks
        for (int i = 0; i < contents.Count; i++) {

            ItemStack stack = contents[i];

            if (stack.GetItem() != null && stack.GetItem().Equals(item)) { // if the stack already contains the item

                int currentCount = stack.GetCount();
                int toAdd = Math.Min(stackLimit - currentCount, count); // how many can we add to this stack

                if (toAdd > 0) {

                    stack.AddItem(toAdd); // add the items to the stack
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

                int toAdd = Math.Min(stackLimit, count);
                contents[i] = new ItemStack(item, toAdd);
                count -= toAdd;

                if (count <= 0)
                    return 0; // return 0 since all items were added

            }
        }

        // if we reach here, not all items could be added
        alertManager.SendAlert(new Alert($"Backpack is full! Could not add {count}x {item.GetName()} to backpack", AlertType.Failure));
        return count;

    }

    // returns the amount of items that were removed from the backpack
    public int RemoveItem(Item item, int count) {

        int removed = 0;

        // remove from the last slots first
        for (int i = contents.Count - 1; i >= 0; i--) {

            ItemStack stack = contents[i];

            if (stack.GetItem() != null && stack.GetItem().Equals(item) && stack.GetCount() > 0) {

                int toRemove = Math.Min(stack.GetCount(), count - removed);
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

    public bool ContainsItems(Item item, int count) {

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
    private int GetEffectiveStackLimit(Item item) {

        // how the effective stack limit is determined:
        // 1. if both the item and the slot stack limit are greater than 0, use the smaller of the two
        // 2. if the item limit is greater than 0 and the slot limit is 0, use the item limit
        // 3. if the slot limit is greater than 0 and the item limit is 0, use the slot limit
        // 4. if both the item and slot limits are 0, use an infinite stack limit (int.MaxValue)
        int itemLimit = item.GetStackSize();
        int slotLimit = slotStackLimit;

        if (itemLimit > 0 && slotLimit > 0)
            return Math.Min(itemLimit, slotLimit); // if both limits are greater than 0, use the smaller of the two
        if (itemLimit > 0 && slotLimit == 0)
            return itemLimit; // if item limit is greater than 0 and slot limit is 0, use item limit
        if (slotLimit > 0 && itemLimit == 0)
            return slotLimit; // if slot limit is greater than 0 and item limit is 0, use slot limit

        return int.MaxValue; // if both limits are 0, use an infinite stack limit

    }

    public ItemStack GetItemStack(int index) => contents[index];

    public int GetInitialCapacity() => initialCapacity;

    public void SwapItemStacks(int indexA, int indexB) => (contents[indexB], contents[indexA]) = (contents[indexA], contents[indexB]); // swap the item stacks at the given indices

}
