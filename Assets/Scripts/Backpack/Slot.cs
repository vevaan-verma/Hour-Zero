using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IDropHandler {

    [Header("References")]
    private ItemHolder itemHolder;
    private Backpack backpack;
    private bool initialized;

    [Header("Settings")]
    [SerializeField][Tooltip("The maximum number of items that can be held in this slot. If set to 0, it will either use the item's stack limit, or if that is set to 0, an infinite limit")][Min(0)] private int slotStackLimit;

    public void Initialize() {

        itemHolder = GetComponentInChildren<ItemHolder>();
        backpack = FindFirstObjectByType<Backpack>();

        itemHolder.Initialize(); // initialize the item holder
        SetItem(null, 0); // initialize the slot with no item and count 0
        initialized = true;

    }

    private void Start() { // called after Initialize would be called for instantiated objects

        // initialize the slot if not already initialized
        if (!initialized)
            Initialize();

    }

    public void OnDrop(PointerEventData eventData) {

        ItemHolder droppedItemHolder = eventData.pointerDrag.GetComponent<ItemHolder>();
        int fromIndex = droppedItemHolder.GetInitialSlot().GetSlotIndex();
        int toIndex = GetSlotIndex();

        Item itemToDrop = droppedItemHolder.GetItem();
        int countToDrop = droppedItemHolder.GetCount();

        // use Backpack to add items, not Slot's own AddItem/SetItem, since it handles stacking and limits

        if (GetItem() != null && GetItem() == itemToDrop) { // check if the item in this slot is the same as the one being dropped, which, if they are, allows us to stack them

            int remainder = backpack.AddItem(itemToDrop, countToDrop); // try to add the item to this slot using Backpack's AddItem method and store the remainder of items that could not be added

            if (remainder > 0) // if there are items that could not be stacked, put them back in the slot that was dragged from
                droppedItemHolder.GetInitialSlot().SetItem(itemToDrop, remainder);
            else // if all the items were able to be stacked, set the contents of the slot that was dragged from to null
                droppedItemHolder.GetInitialSlot().SetItem(null, 0);

            return; // return early since we handled the stacking

        }

        // if the origin and destination slots are not the same, swap the items in the Backpack
        if (fromIndex != toIndex)
            backpack.SwapItemStacks(fromIndex, toIndex);

        // get the swapped items from the Backpack
        ItemStack stackA = backpack.GetItemStack(fromIndex);
        ItemStack stackB = backpack.GetItemStack(toIndex);

        // set the items in the slots based on the swapped stacks
        droppedItemHolder.GetInitialSlot().SetItem(stackA.GetItem(), stackA.GetCount());
        SetItem(stackB.GetItem(), stackB.GetCount());

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

    public int GetSlotIndex() => transform.GetSiblingIndex(); // Returns the index of the slot in its parent

}
