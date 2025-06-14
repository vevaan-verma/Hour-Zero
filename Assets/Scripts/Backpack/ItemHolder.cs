using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemHolder : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    [Header("References")]
    private Slot initialSlot; // the parent slot to set after dragging ends
    private Color initialColor;
    private Item item;

    [Header("UI References")]
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Sprite emptyIcon; // icon to show when there is no item
    private Image image;

    [Header("Settings")]
    [SerializeField] private float dragAlpha;
    private int count;

    public void Initialize() {

        image = GetComponent<Image>();
        initialColor = image.color;

    }

    public void SetItem(Item item, int count) {

        this.item = item;

        image.sprite = item == null ? emptyIcon : item.GetIcon(); // set the image sprite to the item's icon

        this.count = count;
        countText.text = count.ToString(); // set the count text to the item's stack size or empty if no item

        countText.gameObject.SetActive(item != null && count > 1); // only show the count text if there is an item and its stack size is greater than 1

    }

    public void RemoveItem(Item item, int count) {

        if (this.item != item) return; // if the item in this slot is not the same as the one to remove, do nothing

        this.count -= count; // decrease the count

        if (this.count <= 0)
            SetItem(null, 0); // if the count is 0 or less, set the slot to empty
        else
            countText.text = this.count.ToString(); // otherwise, update the count text

    }

    public void OnBeginDrag(PointerEventData eventData) {

        // if the item being dragged is null, destroy the dragged item to cancel the drag
        if (item == null) {

            Destroy(eventData.pointerDrag);
            return;

        }

        image.color = new Color(initialColor.r, initialColor.g, initialColor.b, dragAlpha); // set the image color to semi-transparent when dragging starts
        initialSlot = transform.parent.GetComponent<Slot>(); // store the original slot before changing it (so if not dropped, it can be reset)
        transform.SetParent(transform.root); // change the parent to the root canvas to allow free movement
        transform.SetAsLastSibling(); // bring the dragged item to the front
        image.raycastTarget = false; // disable raycast target to allow interaction with other UI elements while dragging

    }

    public void OnDrag(PointerEventData eventData) => transform.position = eventData.position; // update the position of the dragged item to follow the mouse pointer

    public void OnEndDrag(PointerEventData eventData) {

        // eventData.pointerCurrentRaycast returns the raycast result of what the pointer is currently over, whereas eventData.pointerDrag is the item being dragged
        // if the pointer is not over a valid slot, destroy the dragged item
        if (eventData.pointerCurrentRaycast.gameObject == null || !eventData.pointerCurrentRaycast.gameObject.GetComponent<Slot>())
            Destroy(eventData.pointerDrag);

        initialSlot.SetItem(item, count); // reset the item and count in the initial slot
        image.color = initialColor; // reset the image color when dragging ends
        image.raycastTarget = true; // re-enable raycast target

        foreach (InventoryUI ui in FindObjectsByType<InventoryUI>(FindObjectsSortMode.None))
            if (ui.IsInventoryOpen())
                ui.RefreshInventory();

    }

    public Slot GetInitialSlot() => initialSlot;

    public Item GetItem() => item;

    public int GetCount() => count;

}
