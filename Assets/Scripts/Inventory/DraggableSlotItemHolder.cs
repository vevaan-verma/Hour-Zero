using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableSlotItemHolder : SlotItemHolder, IBeginDragHandler, IDragHandler, IEndDragHandler {

    [Header("References")]
    private Slot initialSlot; // the parent slot to set after dragging ends
    private Color initialColor;

    [Header("Settings")]
    [SerializeField] private float dragAlpha;

    public override void Initialize() {

        base.Initialize();
        initialColor = itemIcon.color;

    }

    public void OnBeginDrag(PointerEventData eventData) {

        // if the item being dragged is null, destroy the dragged item to cancel the drag
        if (itemStack.GetItem() == null) {

            Destroy(eventData.pointerDrag);
            return;

        }

        itemIcon.color = new Color(initialColor.r, initialColor.g, initialColor.b, dragAlpha); // set the image color to semi-transparent when dragging starts
        initialSlot = transform.parent.GetComponent<Slot>(); // store the original slot before changing it (so if not dropped, it can be reset)
        transform.SetParent(transform.root); // change the parent to the root canvas to allow free movement
        transform.SetAsLastSibling(); // bring the dragged item to the front
        itemIcon.raycastTarget = false; // disable raycast target to allow interaction with other UI elements while dragging
        initialSlot.DestroyCurrentItemInfoWidget(); // destroy the item info widget if it exists in the initial slot (prevents the widget from being shown while dragging)

    }

    public void OnDrag(PointerEventData eventData) => transform.position = eventData.position; // update the position of the dragged item to follow the mouse pointer

    public void OnEndDrag(PointerEventData eventData) {

        // eventData.pointerCurrentRaycast returns the raycast result of what the pointer is currently over, whereas eventData.pointerDrag is the item being dragged
        // if the pointer is not over a valid slot, destroy the dragged item
        if (eventData.pointerCurrentRaycast.gameObject == null || !eventData.pointerCurrentRaycast.gameObject.GetComponent<Slot>())
            Destroy(eventData.pointerDrag);

        initialSlot.SetItemStack(itemStack); // reset the item and count in the initial slot
        itemIcon.color = initialColor; // reset the image color when dragging ends
        itemIcon.raycastTarget = true; // re-enable raycast target

        foreach (InventoryUI ui in FindObjectsByType<InventoryUI>(FindObjectsSortMode.None))
            if (ui.IsInventoryOpen())
                ui.RefreshInventory();

    }

    public Slot GetInitialSlot() => initialSlot;

}
