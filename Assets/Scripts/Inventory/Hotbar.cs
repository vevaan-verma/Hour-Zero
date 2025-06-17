using Pathfinding;
using System;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.Entities.EntitiesJournaling;
using static Unity.VisualScripting.Icons;

public class Hotbar : Inventory {

    [Header("References")]
    private PlayerController playerController;

    [Header("Settings")]
    private int selectedIndex;

    [Header("Data")]
    public Action onSlotSelected;

    public override void Initialize() {

        playerController = FindFirstObjectByType<PlayerController>();

        base.Initialize();
        SelectSlot(0);

    }

    public void SelectSlot(int index) {

        if (index < 0 || index >= currSlotCount) return; // do nothing if the index is out of bounds
        selectedIndex = index; // set the selected index to the given index

        if (contents[selectedIndex].GetItem() == null)
            playerController.SetHeldTool(null); // set the player's held tool to the one in the selected slot
        else if (contents[selectedIndex].GetItem().GetItemType() == ItemType.Tool)
            playerController.SetHeldTool(contents[selectedIndex].GetItem().GetHeldToolPrefab()); // set the player's held tool to the prefab of the item in the selected slot

        onSlotSelected?.Invoke(); // invoke the slot selected event

    }

    public void CycleSlot(int cycleAmount) {

        selectedIndex = (selectedIndex + cycleAmount) % currSlotCount; // cycle through the slots, wrapping around if necessary
        if (selectedIndex < 0) selectedIndex += currSlotCount; // ensure the index is not negative

        SelectSlot(selectedIndex); // select the new slot

        // no need to invoke the slot selected event here, as it is already invoked in SelectSlot

    }


    // TODO: add method to update the held tool in the player controller when the hotbar is changed

    public int GetSelectedIndex() => selectedIndex;

}
