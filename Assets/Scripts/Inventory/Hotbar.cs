using System;
using UnityEngine;

public class Hotbar : Inventory {

    [Header("References")]
    private PlayerController playerController;

    [Header("Settings")]
    private int selectedIndex;

    [Header("Data")]
    public Action onSlotSelected;

    public override void Initialize() {

        playerController = FindFirstObjectByType<PlayerController>();
        onContentsUpdated += UpdateHeldTool; // subscribe to the contents updated event to update the held tool when the contents change

        base.Initialize();
        SelectSlot(0);

    }

    public void SelectSlot(int index) {

        if (index < 0 || index >= currSlotCount) return; // do nothing if the index is out of bounds
        selectedIndex = index; // set the selected index to the given index

        UpdateHeldTool(); // update the held tool
        onSlotSelected?.Invoke(); // invoke the slot selected event

    }

    public void CycleSlot(int cycleAmount) {

        selectedIndex = (selectedIndex + cycleAmount) % currSlotCount; // cycle through the slots, wrapping around if necessary
        if (selectedIndex < 0) selectedIndex += currSlotCount; // ensure the index is not negative

        SelectSlot(selectedIndex); // select the new slot

        // no need to invoke the slot selected event here, as it is already invoked in SelectSlot

    }

    public void UpdateHeldTool() {

        if (contents[selectedIndex].GetItem() == null)
            playerController.SetHeldTool(null); // if the selected slot is empty, set the player's held tool to null to remove any held tool
        else if (contents[selectedIndex].GetItem().GetItemType() == ItemType.Tool)
            playerController.SetHeldTool(contents[selectedIndex].GetItem().GetHeldToolPrefab()); // set the player's held tool to the held tool prefab of the item in the selected slot

    }

    public int GetSelectedIndex() => selectedIndex;

}
