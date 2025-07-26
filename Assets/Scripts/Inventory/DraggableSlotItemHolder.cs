using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableSlotItemHolder : SlotItemHolder, IBeginDragHandler, IDragHandler, IEndDragHandler {

    [Header("References")]
    private PlayerController playerController;
    private Slot initialSlot; // the original slot that the item was in before the drag operation started
    private Color initialColor;

    [Header("Settings")]
    [SerializeField] private float dragAlpha;

    public override void Initialize(InventoryUI inventoryUI) {

        base.Initialize(inventoryUI);

        playerController = FindFirstObjectByType<PlayerController>();
        initialColor = itemIcon.color;

    }

    public void OnBeginDrag(PointerEventData eventData) {

        initialSlot = transform.parent.GetComponent<Slot>(); // store the original slot before changing it (so if not dropped, it can be reset)

        if (initialSlot.IsLocked() || itemStack.GetItem() == null) return; // if the initial slot is locked or the item being dragged is null, do not process the begin drag event

        itemIcon.color = new Color(initialColor.r, initialColor.g, initialColor.b, dragAlpha); // set the image color to semi-transparent when dragging starts
        transform.SetParent(transform.root); // change the parent to the root canvas to allow free movement
        transform.SetAsLastSibling(); // bring the dragged item to the front
        itemIcon.raycastTarget = false; // disable raycast target to allow interaction with other UI elements while dragging
        initialSlot.DestroyCurrentItemInfoWidget(); // destroy the item info widget if it exists in the initial slot (prevents the widget from being shown while dragging)

    }

    public void OnDrag(PointerEventData eventData) {

        if (initialSlot.IsLocked()) return; // if the initial slot is locked, do not process the drag event

        transform.position = eventData.position; // update the position of the dragged item to follow the mouse pointer

    }

    public void OnEndDrag(PointerEventData eventData) {

        if (initialSlot.IsLocked()) return; // if the initial slot is locked, do not process the end drag event

        // eventData.pointerCurrentRaycast returns the raycast result of what the pointer is currently over, whereas eventData.pointerDrag is the item being dragged
        if (eventData.pointerCurrentRaycast.gameObject == null) { // check if the pointer is not over any UI element (meaning the item can be dropped in the world)

            initialSlot.GetInventory().RemoveItemStack(itemStack, initialSlot.GetIndex()); // remove the dragged item stack from the inventory
            playerController.DropItemStack(itemStack); // drop the removed item stack in the world
            Destroy(eventData.pointerDrag); // destroy the dragged item

        } else if (!eventData.pointerCurrentRaycast.gameObject.GetComponent<Slot>()) { // check if the pointer is not over a slot

            Destroy(eventData.pointerDrag); // destroy the dragged item

        }

        initialSlot.SetItemStack(itemStack); // reset the item and count in the initial slot
        itemIcon.color = initialColor; // reset the image color when dragging ends
        itemIcon.raycastTarget = true; // re-enable raycast target

        foreach (InventoryUI ui in FindObjectsByType<InventoryUI>(FindObjectsSortMode.None))
            if (ui.IsInventoryOpen())
                ui.RefreshInventory();

    }

    public Slot GetInitialSlot() => initialSlot;

}
