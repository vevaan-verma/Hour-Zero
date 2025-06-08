using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BackpackSlotItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    [Header("References")]
    private BackpackSlot initialSlot; // the parent slot to set after dragging ends
    private Color initialColor;

    [Header("UI References")]
    private Image image;

    [Header("Settings")]
    [SerializeField] private float dragAlpha;

    private void Start() {

        image = GetComponent<Image>();
        initialColor = image.color;

    }

    public void OnBeginDrag(PointerEventData eventData) {

        image.color = new Color(initialColor.r, initialColor.g, initialColor.b, dragAlpha); // set the image color to semi-transparent when dragging starts
        initialSlot = transform.parent.GetComponent<BackpackSlot>(); // store the original slot before changing it (so if not dropped, it can be reset)
        transform.SetParent(transform.root); // change the parent to the root canvas to allow free movement
        transform.SetAsLastSibling(); // bring the dragged item to the front
        image.raycastTarget = false; // disable raycast target to allow interaction with other UI elements while dragging

    }

    public void OnDrag(PointerEventData eventData) => transform.position = eventData.position; // update the position of the dragged item to follow the mouse pointer

    public void OnEndDrag(PointerEventData eventData) {

        // if the item is not dropped in a valid slot (i.e., not a BackpackSlot), reset it to its initial slot
        if (transform.parent.GetComponent<BackpackSlot>() == null) {

            transform.SetParent(initialSlot.transform); // reset parent to the initial slot
            transform.position = initialSlot.transform.position; // reset position to the initial slot position

        }

        image.color = initialColor; // reset the image color when dragging ends
        image.raycastTarget = true; // re-enable raycast target

    }

    public BackpackSlot GetInitialSlot() => initialSlot;

}
