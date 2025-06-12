using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Inventory : MonoBehaviour {

    [Header("Settings")]
    [SerializeField][Min(1)] protected int initialSlotCount;
    [SerializeField] protected int slotStackLimit;
    [SerializeField, Tooltip("Items that can be added to the inventory, if empty, all items are allowed")] protected Item[] itemWhitelist;
    private int currSlotCount;

    [Header("Data")]
    protected List<ItemStack> contents;
    protected Action onItemStackAdded;

    public virtual void Initialize() {

        contents = new List<ItemStack>(currSlotCount);
        currSlotCount = initialSlotCount;

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
            return 0;

        }

        if (itemWhitelist.Length > 0 && Array.FindIndex(itemWhitelist, x => x != null && x.Equals(item)) < 0) return count; // if the item is not in the whitelist, return the count because no items were added (use the FindIndex method to make sure the Equals method is used for comparison); do this after checking for null and count to ensure null items are part of the whitelist by default so the slots can actually be cleared

        int stackLimit = GetEffectiveStackLimit(item);
        int toSet = Mathf.Min(stackLimit, count); // how many we can set in the slot
        contents[index] = new ItemStack(item, toSet); // set the item stack in the slot
        onItemStackAdded?.Invoke(); // invoke the item added event
        return count - toSet; // return the count of items that could not be set

    }

    // returns the amount of items that could not be added to the backpack
    public virtual int AddItemStack(ItemStack itemStack) {

        Item item = itemStack.GetItem();
        int count = itemStack.GetCount();

        if (item == null || count <= 0) return count; // return the count since we couldn't add anything

        if (itemWhitelist.Length > 0 && Array.IndexOf(itemWhitelist, item) < 0) return count; // if the item is not in the whitelist, return the count because no items were added

        // first, try to stack into existing stacks
        for (int i = 0; i < contents.Count; i++) {

            ItemStack stack = contents[i];

            if (stack.GetItem() != null && stack.GetItem().Equals(item)) { // check if the stack already contains the item

                int currentCount = stack.GetCount();
                int remainder = SetItemStack(new ItemStack(item, currentCount + count), i); // set the item stack in the slot with the new count
                count = remainder; // update count to the remainder

                if (count <= 0) {

                    onItemStackAdded?.Invoke(); // invoke the item added event since we successfully added items (placed here because we only want to invoke it once)
                    return 0; // return 0 since all items were added

                }
            }
        }

        // then, try to add to empty slots (including whatever wasn't able to be stacked)
        for (int i = 0; i < contents.Count; i++) {

            ItemStack stack = contents[i];

            if (stack.GetItem() == null || stack.GetCount() == 0) { // check if the slot is empty

                count = SetItemStack(new ItemStack(item, count), i); // set the item stack in the slot and get the remainder of items that couldn't be added

                if (count <= 0) {

                    onItemStackAdded?.Invoke(); // invoke the item added event since we successfully added items (placed here because we only want to invoke it once)
                    return 0; // return 0 since all items were added

                }
            }
        }

        // if we reach here, not all items could be added

        // check if at least one item was added to the inventory
        if (count != itemStack.GetCount())
            onItemStackAdded?.Invoke(); // invoke the item added event since the count is different from the original count, meaning at least one item was added

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

    public int GetInitialCapacity() => initialSlotCount;

    public bool IsFull() {

        // TODO: should this use the slot stack limit or effective stack limit?
        // if any slot is not filled to the SLOT stack limit, return false
        foreach (ItemStack stack in contents)
            if (stack.GetItem() == null || stack.GetCount() < slotStackLimit)
                return false; // if the stack is empty or not filled to the SLOT stack limit, return false

        return true; // if we reach here, all slots are filled to the stack limit

    }
}
