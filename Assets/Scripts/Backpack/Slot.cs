using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IDropHandler {

    [Header("References")]
    private ItemHolder itemHolder;
    private bool initialized;

    [Header("Settings")]
    [SerializeField][Tooltip("The maximum number of items that can be held in this slot. If set to 0, it will either use the item's stack limit, or if that is set to 0, an infinite limit")] private int slotStackLimit;

    private void Start() {

        // initialize the slot if not already initialized
        if (!initialized)
            Initialize();

    }

    public void Initialize() {

        itemHolder = GetComponentInChildren<ItemHolder>();
        itemHolder.Initialize(); // initialize the item holder
        SetItem(null, 0); // initialize the slot with no item and count 0
        initialized = true;

    }

    public void OnDrop(PointerEventData eventData) {

        ItemHolder droppedItemHolder = eventData.pointerDrag.GetComponent<ItemHolder>();

        Item itemToDrop = droppedItemHolder.GetItem(); // get the item to drop from the dropped item holder
        int countToDrop = droppedItemHolder.GetCount(); // get the count to drop from the dropped item holder

        int remainder;

        if (GetItem() != null && GetItem() == itemToDrop) { // if the current slot already has an item, check if it is the same as the dropped item, which allows stacking

            // if the item in this slot is the same as the dropped item, stack the items as much as possible and store the remainder
            remainder = AddItem(itemToDrop, countToDrop);

            if (remainder > 0)
                droppedItemHolder.GetInitialSlot().SetItem(itemToDrop, remainder); // if there is a remainder, return it to the original slot
            else
                droppedItemHolder.GetInitialSlot().SetItem(null, 0); // if there is no remainder, set the initial slot to empty

            return; // exit the method since the item was successfully stacked

        }

        // if the current slot has a different item, we need to swap items

        // set the item and count of the slot that the dropped item came from to the current slot's item and count
        droppedItemHolder.GetInitialSlot().SetItem(GetItem(), GetCount());

        // set the item and count of this slot to the dropped item's item and count, and get the remainder
        remainder = SetItem(itemToDrop, countToDrop);

        // if there is a remainder, return it to the original slot
        if (remainder > 0)
            droppedItemHolder.GetInitialSlot().AddItem(itemToDrop, remainder);

    }

    // returns the amount of items that could not be added to the slot
    public int SetItem(Item item, int count) {

        int remainder = 0;

        // how the effective stack limit is determined:
        // 1. if both the item and the slot stack limit are greater than 0, use the smaller of the two
        // 2. if the item limit is greater than 0 and the slot limit is 0, use the item limit
        // 3. if the slot limit is greater than 0 and the item limit is 0, use the slot limit
        // 4. if both the item and slot limits are 0, use an infinite stack limit (int.MaxValue)
        int effectiveStackLimit;

        if (item != null) {

            int itemLimit = item.GetStackSize();
            int slotLimit = slotStackLimit;

            if (itemLimit > 0 && slotLimit > 0)
                effectiveStackLimit = Mathf.Min(itemLimit, slotLimit); // use the smaller of the two limits since both are greater than 0
            else if (itemLimit > 0 && slotLimit == 0)
                effectiveStackLimit = itemLimit; // use the item limit since it is greater than 0 and the slot limit is 0
            else if (slotLimit > 0 && itemLimit == 0)
                effectiveStackLimit = slotLimit; // use the slot limit since it is greater than 0 and the item limit is 0
            else
                effectiveStackLimit = int.MaxValue; // infinite stack limit if both item and slot limits are 0

            if (count > effectiveStackLimit) {

                remainder = count - effectiveStackLimit;
                count = effectiveStackLimit;

            }
        }

        itemHolder.SetItem(item, count); // set the item and count in the new slot item holder
        itemHolder.transform.SetParent(transform); // set the parent of the new item to this slot
        itemHolder.transform.SetAsFirstSibling(); // set to the first sibling so the count text appears on top
        itemHolder.transform.position = transform.position; // move the new item to the position of this slot

        return remainder; // will return the remainder if the count is greater than the effective stack size, otherwise 0 will be returned

    }

    // returns the amount of items that could not be added to the slot
    public int AddItem(Item item, int count) {

        if (itemHolder.GetItem() != null && itemHolder.GetItem() != item) {

            Debug.LogWarning("Cannot add item: " + item.GetName() + " to slot: " + gameObject.name + " because it already contains a different item: " + itemHolder.GetItem().GetName());
            return 0;

        }

        return SetItem(item, itemHolder.GetCount() + count); // add the item and count to the current slot item holder

    }

    // returns the amount of items that were removed from the slot
    public int RemoveItem(Item item, int count) {

        if (itemHolder.GetItem() == null || itemHolder.GetItem() != item) {

            Debug.LogWarning("Cannot remove item: " + item.GetName() + " from slot: " + gameObject.name + " because it does not contain this item.");
            return 0;

        }

        int removedCount = Mathf.Min(count, itemHolder.GetCount()); // get the minimum of the count to remove and the current count in the slot (this prevents removing more than available)
        itemHolder.RemoveItem(item, removedCount); // remove the item and count from the current slot item holder

        if (itemHolder.GetCount() <= 0) // if the count is 0 or less, set the slot to empty
            itemHolder.SetItem(null, 0);

        return removedCount;

    }

    public Item GetItem() => itemHolder.GetItem();

    public int GetCount() => itemHolder.GetCount();

    public bool HasItem() => itemHolder.GetItem() != null;

}
