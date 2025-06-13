using UnityEngine;

public class RepairInventoryUI : InventoryUI {

    public override void Initialize() {

        inventory = FindFirstObjectByType<RepairInventory>(FindObjectsInactive.Include); // find the inventory in the scene
        base.Initialize();

    }

    public override void RefreshInventory() {

        // initialSlotCount is the amount of repair items that are needed to repair the system (each item gets a slot)
        inventorySlots = new Slot[inventory.GetInitialCapacity()];

        // delete all existing slots in the inventory contents
        foreach (Transform child in inventoryContents)
            Destroy(child.gameObject);

        // instantiate the slots based on the current capacity of the inventory
        for (int i = 0; i < inventorySlots.Length; i++) {

            Slot slot = Instantiate(slotPrefab, inventoryContents);
            slot.transform.name = $"Slot{i + 1}";
            slot.Initialize(inventory, i, inventory.GetItemStack(i).GetItem(), inventory.GetItemStack(i).GetCount()); // initialize the slot
            inventorySlots[i] = slot; // store the slot in the array for later reference

        }
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
}
