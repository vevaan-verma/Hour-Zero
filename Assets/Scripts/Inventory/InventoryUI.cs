using UnityEngine;
using UnityEngine.UI;

public abstract class InventoryUI : MonoBehaviour {

    [Header("References")]
    [SerializeField] protected Slot slotPrefab;
    protected Inventory inventory;
    protected Slot[] inventorySlots;

    [Header("UI References")]
    [SerializeField] protected CanvasGroup uiPanel; // the panel that contains the inventory UI (used to allow the script to remain active while the UI is hidden)
    [SerializeField] protected GridLayoutGroup inventoryContents;

    [Header("Settings")]
    [SerializeField, Tooltip("Whether to show the item info widget when hovering over an item in the inventory")] protected bool showItemInfoWidgetOnHover;
    protected bool isInventoryOpen;

    public virtual void Initialize() {

        uiPanel.gameObject.SetActive(inventory.IsVisibleByDefault());

        RefreshInventory();
        inventory.onContentsUpdated += RefreshInventory; // subscribe to the inventory's contents update event to refresh the UI when the contents change

    }

    private void OnDisable() {

        if (inventory)
            inventory.onContentsUpdated -= RefreshInventory;

    }

    public virtual void RefreshInventory() {

        RectTransform rectTransform = slotPrefab.GetComponent<RectTransform>();
        inventoryContents.cellSize = new Vector2(rectTransform.rect.width, rectTransform.rect.height); // set the cell size of the grid layout group to match the size of the slot prefab
        inventoryContents.constraint = GridLayoutGroup.Constraint.FixedColumnCount; // set the constraint to fixed column count
        inventoryContents.constraintCount = inventory.GetSlotsPerRow(); // set the number of columns in the inventory contents grid layout group

        inventorySlots = new Slot[inventory.GetCurrentSlotCount()];

        // delete all existing slots in the inventory contents
        foreach (Transform child in inventoryContents.transform)
            Destroy(child.gameObject);

        // instantiate the slots based on the current capacity of the inventory
        for (int i = 0; i < inventorySlots.Length; i++) {

            Slot slot = Instantiate(slotPrefab, inventoryContents.transform);
            slot.transform.name = $"Slot{i + 1}";
            slot.Initialize(inventory, i, inventory.GetItemStack(i).GetItem(), inventory.GetItemStack(i).GetCount(), showItemInfoWidgetOnHover); // initialize the slot
            inventorySlots[i] = slot; // store the slot in the array for later reference

        }
    }

    public abstract void OpenInventory();

    public abstract void CloseInventory();

    public bool IsInventoryOpen() => isInventoryOpen;

}
