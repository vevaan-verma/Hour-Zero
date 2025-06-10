using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IDropHandler {

    [Header("References")]
    private ItemHolder itemHolder;
    private BackpackUI backpackUI;
    private Backpack backpack;
    private bool initialized;

    [Header("Settings")]
    [SerializeField][Tooltip("The maximum number of items that can be held in this slot. If set to 0, it will either use the item's stack limit, or if that is set to 0, an infinite limit")][Min(0)] private int slotStackLimit;
    private string guid; // unique identifier for the slot, can be used to identify the slot

    public void Initialize(BackpackUI backpackUI) {

        this.backpackUI = backpackUI;
        itemHolder = GetComponentInChildren<ItemHolder>();
        backpack = FindFirstObjectByType<Backpack>();

        itemHolder.Initialize(); // initialize the item holder
        SetItem(null, 0); // initialize the slot with no item and count 0
        guid = Guid.NewGuid().ToString(); // generate a unique identifier for the slot

        initialized = true;

    }

    private void Start() { // called after Initialize would be called for instantiated objects

        // initialize the slot if not already initialized
        if (!initialized)
            Initialize(null);

    }

    public void OnDrop(PointerEventData eventData) {

        ItemHolder droppedItemHolder = eventData.pointerDrag.GetComponent<ItemHolder>();

        int fromIndex = droppedItemHolder.GetInitialSlot().GetSlotIndex();
        int toIndex = GetSlotIndex();

        Item itemToDrop = droppedItemHolder.GetItem();
        int countToDrop = droppedItemHolder.GetCount();

        if (GetItem() != null && GetItem() == itemToDrop) { // check if the item in this slot is the same as the one being dropped, which would allow stacking

            int stackLimit = GetStackLimit();
            int currentCount = GetCount();
            int canAdd = Mathf.Min(stackLimit - currentCount, countToDrop); // this is how many items we can add to this stack

            if (canAdd > 0) { // if we can add some items to this stack

                backpack.GetItemStack(toIndex).AddItem(canAdd); // add what we can to the target slot in Backpack
                backpack.GetItemStack(fromIndex).RemoveItem(canAdd); // remove from the source slot in Backpack

                // refresh the backpack UI if it exists
                if (backpackUI != null)
                    backpackUI.RefreshBackpack();

            } else {

                // the target slot is full, so we need to swap the items instead

                if (fromIndex != toIndex) { // if the source and target slots are different

                    backpack.SwapItemStacks(fromIndex, toIndex); // swap the item stacks in Backpack

                    // refresh the backpack UI if it exists
                    if (backpackUI != null)
                        backpackUI.RefreshBackpack();

                }
            }

            // TODO: think about if theres a remainder
            return; // exit early since we handled the stacking case

        }

        // if the target and source slots aren't the same, we can swap the items because, at this point, we know the items are different, so they can't stack
        if (fromIndex != toIndex) {

            print("swap");
            backpack.SwapItemStacks(fromIndex, toIndex);

            // refresh the backpack UI if it exists
            if (backpackUI != null)
                backpackUI.RefreshBackpack();

        }
    }

    public void SetItem(Item item, int count) {

        itemHolder.SetItem(item, count); // set the item and count in the new slot item holder
        itemHolder.transform.SetParent(transform); // set the parent of the new item to this slot
        itemHolder.transform.SetAsFirstSibling(); // set to the first sibling so the count text appears on top
        itemHolder.transform.position = transform.position; // move the new item to the position of this slot

    }

    public void AddItem(Item item, int count) => SetItem(item, itemHolder.GetCount() + count); // add the item and count to the current slot item holder

    public Item GetItem() => itemHolder.GetItem();

    public int GetCount() => itemHolder.GetCount();

    public int GetStackLimit() => slotStackLimit;

}
