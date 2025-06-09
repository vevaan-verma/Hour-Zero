using UnityEngine;
using UnityEngine.EventSystems;

public class BackpackSlot : MonoBehaviour, IDropHandler {

    [Header("References")]
    private BackpackItemHolder itemHolder;

    public void Initialize() {

        itemHolder = GetComponentInChildren<BackpackItemHolder>();
        itemHolder.Initialize(); // initialize the item holder
        SetItem(null, 0); // initialize the slot with no item and count 0

    }

    public void OnDrop(PointerEventData eventData) {

        BackpackItemHolder droppedItemHolder = eventData.pointerDrag.GetComponent<BackpackItemHolder>();

        Item itemToSwap = droppedItemHolder.GetItem(); // get the item from the dropped item holder
        int countToSwap = droppedItemHolder.GetCount(); // get the count from the dropped item holder

        droppedItemHolder.GetInitialSlot().SetItem(GetItem(), GetCount()); // set the item and count of the slot that the dropped item came from to the current slot's item and count; if the current slot has no item, it will set it to null and 0
        SetItem(itemToSwap, countToSwap); // set the item and count of this slot to the dropped item's item and count

    }

    // returns the amount of items that could not be added to the slot
    public int SetItem(Item item, int count) {

        int remainder = 0;

        // clamp the count to the item's stack size if it is greater than the maximum stack size
        if (item != null && count > item.GetStackSize()) {

            remainder = count - item.GetStackSize();
            count = item.GetStackSize();

        }

        itemHolder.SetItem(item, count); // set the item and count in the new slot item holder
        itemHolder.transform.SetParent(transform); // set the parent of the new item to this slot
        itemHolder.transform.SetAsFirstSibling(); // set to the first sibling so the count text appears on top
        itemHolder.transform.position = transform.position; // move the new item to the position of this slot

        return remainder; // will return the remainder if the count is greater than the item's stack size, otherwise 0 will be returned

    }

    // returns the amount of items that could not be added to the slot
    public int AddItem(Item item, int count) {

        if (itemHolder.GetItem() != null && itemHolder.GetItem() != item) {

            Debug.LogWarning("Cannot add item: " + item.GetName() + " to slot: " + gameObject.name + " because it already contains a different item: " + itemHolder.GetItem().GetName());
            return 0;

        }

        return SetItem(item, itemHolder.GetCount() + count); // add the item and count to the current slot item holder

    }

    public void RemoveItem(Item item, int count) {

        if (itemHolder.GetItem() != item) {

            Debug.LogWarning("Cannot remove item: " + item.GetName() + " from slot: " + gameObject.name + " because it does not match the current item: " + itemHolder.GetItem()?.GetName());
            return; // if the item in this slot is not the same as the one to remove, do nothing

        }

        itemHolder.RemoveItem(item, count); // remove the item and count from the current slot item holder

    }

    public void ClearItems() => RemoveItem(itemHolder.GetItem(), itemHolder.GetCount()); // remove all of othe item from the current slot item holder

    public Item GetItem() => itemHolder.GetItem();

    public int GetCount() => itemHolder.GetCount();

    public bool HasItem() => itemHolder.GetItem() != null;

}
