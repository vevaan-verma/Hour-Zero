using UnityEngine;

public class SystemInteractable : Interactable {

    [Header("References")]
    private UIManager uiManager;

    [Header("Settings")]
    [SerializeField] private BunkerSystemType systemType;
    [SerializeField] private ItemStack[] repairStacks;
    [SerializeField, Min(1)] private int repairSlotCount;
    [SerializeField] private int repairPercent;

    private new void Start() {

        base.Start();
        uiManager = FindFirstObjectByType<UIManager>();

    }

    public override void Interact() {

        // if the interactable requires a held item, check if the player is holding the required item and enough of it
        if (requireHeldItem) {

            ItemStack selectedItemStack = hotbar.GetSelectedItemStack(); // get the item stack in the currently selected hotbar slot

            // if the required item is not held or not enough of it is held, don't follow through with the interaction
            if (selectedItemStack.GetItem() == null || !selectedItemStack.GetItem().Equals(requiredHeldItem.GetItem()))
                return;

            // if the held item should be consumed, use the backpack inventory to remove as much of the item stack from the current selected hotbar slot as possible, then remove the remainder as normal
            if (consumeHeldItem)
                backpack.RemoveItemStack(new ItemStack(requiredHeldItem.GetItem(), requiredHeldItem.GetCount()), hotbar.GetSelectedIndex());

        }

        // if the interactable required items in the backpack, check if the player has those items and enough of them
        if (requireBackpackItems) {

            foreach (ItemStack requiredStack in requiredBackpackItems) {

                // if the backpack does not contain the required item stack, don't follow through with the interaction
                if (!backpack.ContainsItemStack(requiredStack))
                    return;

                // if the backpack items should be consumed, remove the required amount from the backpack
                if (consumeBackpackItems)
                    backpack.RemoveItemStack(new ItemStack(requiredStack.GetItem(), requiredStack.GetCount()));

            }
        }

        uiManager.OpenSystemRepairMenu(repairStacks, repairPercent, systemType); // open the system repair menu

    }
}
