using UnityEngine;

public class RepairInventoryUI : InventoryUI {

    [Header("References")]
    [SerializeField] private Slot slotPrefab;
    private RepairInventory inventory;
    private Slot[] inventorySlots;

    [Header("UI References")]
    [SerializeField] private GameObject uiPanel; // the panel that contains the inventory UI (used to allow the script to remain active while the UI is hidden)
    [SerializeField] private Transform inventoryContents;

    public override void Initialize() {

        inventory = FindFirstObjectByType<RepairInventory>(FindObjectsInactive.Include); // find the inventory in the scene

        inventorySlots = new Slot[inventory.GetInitialCapacity()];

        uiPanel.SetActive(false); // make sure the inventory panel is hidden by default

    }

    public override void OpenInventory() {

        if (isInventoryOpen) return; // do nothing if the inventory is already open

        RefreshInventory(); // refresh the inventory slots to ensure they are up to date

        isInventoryOpen = true;
        uiPanel.SetActive(true); // make sure the inventory panel is active while opening

        Cursor.lockState = CursorLockMode.None; // unlock cursor
        Cursor.visible = true; // make cursor visible

    }

    public override void CloseInventory() {

        if (!isInventoryOpen) return; // do nothing if the inventory is already closed

        RefreshInventory(); // refresh the inventory slots to ensure they are up to date

        isInventoryOpen = false; // set the state to closed before waiting for animation because it feels better if the player can move and look around while the inventory is closing
        uiPanel.SetActive(true); // make sure the inventory panel is active while closing

        Cursor.lockState = CursorLockMode.Locked; // lock cursor
        Cursor.visible = false; // hide cursor

    }

    public override void RefreshInventory() {

        // delete all existing slots in the inventory contents
        foreach (Transform child in inventoryContents)
            Destroy(child.gameObject);

        // instantiate the slots based on the current capacity of the inventory
        for (int i = 0; i < inventorySlots.Length; i++) {

            Slot slot = Instantiate(slotPrefab, inventoryContents);
            slot.transform.name = $"Slot{i + 1}";
            slot.Initialize(inventory, this, i); // initialize the slot
            slot.SetItem(inventory.GetItemStack(i).GetItem(), inventory.GetItemStack(i).GetCount()); // set the item and count in the slot
            inventorySlots[i] = slot; // store the slot in the array for later reference

        }
    }

    public override bool IsInventoryOpen() => isInventoryOpen;

}
