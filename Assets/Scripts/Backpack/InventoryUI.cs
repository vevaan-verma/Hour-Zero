using UnityEngine;

public abstract class InventoryUI : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Slot slotPrefab;
    protected Inventory inventory;
    private Slot[] inventorySlots;

    [Header("UI References")]
    [SerializeField] protected GameObject uiPanel; // the panel that contains the inventory UI (used to allow the script to remain active while the UI is hidden)
    [SerializeField] private Transform inventoryContents;

    [Header("Settings")]
    protected bool isInventoryOpen;

    public virtual void Initialize() => uiPanel.SetActive(false); // make sure the inventory panel is hidden by default

    public virtual void RefreshInventory() {

        inventorySlots = new Slot[inventory.GetInitialCapacity()];

        // delete all existing slots in the inventory contents
        foreach (Transform child in inventoryContents)
            Destroy(child.gameObject);

        // instantiate the slots based on the current capacity of the inventory
        for (int i = 0; i < inventorySlots.Length; i++) {

            Slot slot = Instantiate(slotPrefab, inventoryContents);
            slot.transform.name = $"Slot{i + 1}";
            slot.Initialize(inventory, i); // initialize the slot
            slot.SetItem(inventory.GetItemStack(i).GetItem(), inventory.GetItemStack(i).GetCount()); // set the item and count in the slot
            inventorySlots[i] = slot; // store the slot in the array for later reference

        }
    }

    public abstract void OpenInventory();

    public abstract void CloseInventory();

    public bool IsInventoryOpen() => isInventoryOpen;

}
