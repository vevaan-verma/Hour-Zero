using UnityEngine;

public class HotbarUI : InventoryUI {

    [Header("References")]
    private Hotbar hotbar;

    public override void Initialize() {

        inventory = FindFirstObjectByType<Hotbar>(FindObjectsInactive.Include); // find the hotbar in the scene
        hotbar = (Hotbar) inventory; // cast the inventory to a hotbar
        hotbar.onSlotSelected += RefreshInventory; // subscribe to the slot selected event to refresh the inventory UI when a slot is selected

        base.Initialize();

    }

    public override void RefreshInventory() {

        base.RefreshInventory();

        // update the slots in the hotbar UI
        for (int i = 0; i < inventorySlots.Length; i++) {

            HotbarSlot slot = (HotbarSlot) inventorySlots[i]; // cast the slot to a hotbar slot

            if (i == hotbar.GetSelectedIndex())
                slot.SelectSlot(); // select the slot if it is the currently selected one
            else
                slot.DeselectSlot(); // deselect the slot if it is not the currently selected one

        }
    }

    public override void CloseInventory() { } // nothing needed here for the hotbar UI as it is always visible

    public override void OpenInventory() { } // nothing needed here for the hotbar UI as it is always visible

}
