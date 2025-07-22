using UnityEngine;

public class TradeInventoryUI : InventoryUI {

    public override void Initialize() {

        inventory = FindFirstObjectByType<TradeInventory>(FindObjectsInactive.Include); // find the inventory in the scene
        base.Initialize();

    }

    public override void RefreshInventory() {

        // initialSlotCount is the amount of trade items that are needed to conduct the trade (each item gets a slot)
        inventorySlots = new Slot[inventory.GetInitialSlotCount()];

        // delete all existing slots in the inventory contents
        foreach (Transform child in inventoryContents.transform)
            Destroy(child.gameObject);

        // instantiate the slots based on the current capacity of the inventory
        for (int i = 0; i < inventorySlots.Length; i++) {

            Slot slot = Instantiate(slotPrefab, inventoryContents.transform);
            slot.transform.name = $"Slot{i + 1}";
            ItemStack itemStack = inventory.GetItemStack(i); // get the item stack from the inventory at the corresponding index
            slot.Initialize(inventory, this, i, new ItemStack(itemStack.GetItem(), itemStack.GetCount()), showItemInfoWidgetOnHover); // initialize the slot
            inventorySlots[i] = slot; // store the slot in the array for later reference

        }
    }

    public override void OpenInventory() {

        if (isInventoryOpen) return; // do nothing if the inventory is already open

        RefreshInventory(); // refresh the inventory slots to ensure they are up to date

        isInventoryOpen = true;
        uiPanel.gameObject.SetActive(true); // make sure the inventory panel is active while opening

        Cursor.lockState = CursorLockMode.None; // unlock cursor
        Cursor.visible = true; // make cursor visible

    }

    public override void CloseInventory() {

        if (!isInventoryOpen) return; // do nothing if the inventory is already closed

        RefreshInventory(); // refresh the inventory slots to ensure they are up to date

        isInventoryOpen = false; // set the state to closed before waiting for animation because it feels better if the player can move and look around while the inventory is closing
        uiPanel.gameObject.SetActive(true); // make sure the inventory panel is active while closing

        Cursor.lockState = CursorLockMode.Locked; // lock cursor
        Cursor.visible = false; // hide cursor

    }
}
