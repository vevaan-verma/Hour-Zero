using System.Collections.Generic;
using UnityEngine;

public abstract class Inventory : MonoBehaviour {

    [Header("Settings")]
    [SerializeField] private int initialCapacity;
    [SerializeField] private int slotStackLimit;
    private int currCapacity;

    [Header("Data")]
    protected List<ItemStack> contents;

    public virtual void Initialize() {

        contents = new List<ItemStack>(currCapacity);
        currCapacity = initialCapacity;

        for (int i = 0; i < currCapacity; i++)
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
            return 0;

        }

        int stackLimit = GetEffectiveStackLimit(item);
        int toSet = Mathf.Min(stackLimit, count); // how many we can set in the slot
        contents[index] = new ItemStack(item, toSet); // set the item stack in the slot
        return count - toSet; // return the count of items that could not be set

    }

    // returns the amount of items that could not be added to the backpack
    public virtual int AddItemStack(ItemStack itemStack) {

        Item item = itemStack.GetItem();
        int count = itemStack.GetCount();

        if (item == null || count <= 0) return count; // return the count since we couldn't add anything

        // first, try to stack into existing stacks
        for (int i = 0; i < contents.Count; i++) {

            ItemStack stack = contents[i];

            if (stack.GetItem() != null && stack.GetItem().Equals(item)) { // check if the stack already contains the item

                int currentCount = stack.GetCount();
                int remainder = SetItemStack(new ItemStack(item, currentCount + count), i); // set the item stack in the slot with the new count
                count = remainder; // update count to the remainder

                if (count <= 0)
                    return 0; // return 0 since all items were added

            }
        }

        // then, try to add to empty slots (including whatever wasn't able to be stacked)
        for (int i = 0; i < contents.Count; i++) {

            ItemStack stack = contents[i];

            if (stack.GetItem() == null || stack.GetCount() == 0) { // check if the slot is empty

                count = SetItemStack(new ItemStack(item, count), i); // set the item stack in the slot and get the remainder of items that couldn't be added

                if (count <= 0)
                    return 0; // return 0 since all items were added

            }
        }

        // if we reach here, not all items could be added
        return count; // return the count of items that could not be added

    }

    // returns the amount of items that could not be removed from the backpack
    public int RemoveItemStack(ItemStack itemStack) {

        Item item = itemStack.GetItem();
        int count = itemStack.GetCount();

        if (item == null || count <= 0) return 0; // return 0 since we couldn't remove anything

        // remove from the last slots first
        for (int i = contents.Count - 1; i >= 0; i--) {

            ItemStack stack = contents[i];

            if (stack.GetItem() != null && stack.GetItem().Equals(item)) { // check if the stack contains the item

                int toRemove = Mathf.Min(stack.GetCount(), count); // how many we can remove
                int newCount = stack.GetCount() - toRemove; // calculate the new count after removing

                ItemStack newStack = newCount > 0 ? new ItemStack(item, newCount) : new ItemStack(null, 0); // create a new ItemStack with the reduced count or null if empty
                contents[i] = newStack; // set the new stack in the slot

                count -= toRemove; // reduce the count of items to remove

                if (count <= 0)
                    return 0; // return 0 since all items were removed

            }
        }

        // if we reach here, not all items could be removed
        return count; // return the count of items that could not be removed

    }

    public bool ContainsItemStack(ItemStack itemStack) {

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

    // helper to get the effective stack limit for an item in a slot
    public int GetEffectiveStackLimit(Item item) {

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

    public ItemStack GetItemStack(int index) => contents[index];

    public int GetInitialCapacity() => initialCapacity;

}
