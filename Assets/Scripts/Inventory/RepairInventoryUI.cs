using UnityEngine;

public class RepairInventoryUI : ExchangeInventoryUI {

    public override void Initialize() {

        inventory = FindFirstObjectByType<RepairInventory>(FindObjectsInactive.Include); // find the inventory in the scene (must be done before base.Initialize() to ensure inventory is set)
        base.Initialize();

    }

    public override void RefreshInventory() {

        // initialSlotCount is the total amount of slots in the repair inventory, or the amount of input item stacks (each item gets a slot)

        inventorySlots = new Slot[inventory.GetInitialSlotCount()];

        // delete all existing inventory slots
        foreach (Transform child in inventoryContents.transform)
            Destroy(child.gameObject); // destroy the child game object

        // instantiate the slots for input items in the inventory contents container
        for (int i = 0; i < inventorySlots.Length; i++) {

            Slot slot = Instantiate(slotPrefab, inventoryContents.transform);
            slot.transform.name = $"Slot{i + 1}";
            ItemStack itemStack = inventory.GetItemStack(i); // get the item stack from the inventory at the corresponding index
            slot.Initialize(inventory, this, i, new ItemStack(itemStack.GetItem(), itemStack.GetCount()), showItemInfoWidgetOnHover); // initialize the slot
            inventorySlots[i] = slot; // store the slot in the array for later reference

        }

        UpdatePlaceholderCycle(); // update the placeholder cycle to ensure it reflects the current state of the inventory

        // refresh the layout if the rect transform is active in hierarchy
        if (rectTransform.gameObject.activeInHierarchy)
            RefreshLayout(rectTransform);

    }

    // OpenInventory and CloseInventory could be placed in the ExchangeInventoryUI class, but they are kept here in case the inventory UIs ever have different behaviors for opening and closing (e.g. animations, sounds, etc.)
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
