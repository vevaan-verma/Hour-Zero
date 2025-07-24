using UnityEngine;

public class RepairInventoryUI : ExchangeInventoryUI {

    public override void Initialize() {

        inventory = FindFirstObjectByType<RepairInventory>(FindObjectsInactive.Include); // find the inventory in the scene (must be done before base.Initialize() to ensure inventory is set)
        base.Initialize();

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
