using UnityEngine;
using UnityEngine.EventSystems;

public class BackpackSlot : MonoBehaviour, IDropHandler {

    public void OnDrop(PointerEventData eventData) {

        GameObject droppedItem = eventData.pointerDrag;
        BackpackSlotItem currSlotItem = GetComponentInChildren<BackpackSlotItem>();
        BackpackSlotItem newSlotItem = droppedItem.GetComponent<BackpackSlotItem>();

        if (currSlotItem == null) return; // if there is no item in this slot, do nothing
        if (newSlotItem == null) return; // if there is no BackpackSlotItem component on the dropped item, do nothing

        // if there is already an item in this slot, set its parent to the parent of the item being dropped into this slot to swap them
        if (currSlotItem != null) {

            currSlotItem.transform.SetParent(newSlotItem.GetInitialSlot().transform); // move the current item to the initial slot of the item being dropped
            currSlotItem.transform.position = newSlotItem.GetInitialSlot().transform.position; // move the current item to the position of the item being dropped

        }

        newSlotItem.transform.SetParent(transform); // set the parent of the dropped item to this slot
        newSlotItem.transform.position = transform.position; // move the dragged item to the position of this slot

    }
}
