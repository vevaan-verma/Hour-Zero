using UnityEngine;
using UnityEngine.EventSystems;

public class SystemSlot : MonoBehaviour, IDropHandler {

    public void OnDrop(PointerEventData eventData) {

        ItemHolder droppedItemHolder = eventData.pointerDrag.GetComponent<ItemHolder>();

    }
}
