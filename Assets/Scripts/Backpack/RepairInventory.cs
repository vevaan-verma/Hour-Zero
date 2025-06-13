using UnityEditor;
using UnityEngine;

public class RepairInventory : Inventory {

    [Header("References")]
    private SystemRepairMenu systemRepairMenu;
    private ItemStack[] repairStacks;
    private int repairPercent;
    private BunkerSystemType systemType;

    public void Initialize(ItemStack[] repairStacks, int repairSlotCount, int repairPercent, BunkerSystemType systemType) {

        this.repairStacks = repairStacks; // set the repair stack to the stack that is required for repairing
        this.initialSlotCount = repairStacks.Length; // set the initial slot count to the number of items that are required for repairing
        this.repairPercent = repairPercent; // set the repair percent to the percent durability of the system that is to be repaired
        this.systemType = systemType; // set the system type to the type of system that is to be repaired

        // set the item whitelist to the repair stack items
        this.itemWhitelist = new Item[repairStacks.Length];

        for (int i = 0; i < repairStacks.Length; i++)
            this.itemWhitelist[i] = repairStacks[i].GetItem();

        base.Initialize(); // initialize at the end to ensure the properties are set before calling the base method (especially the slot count)

    }

    private void OnEnable() => onItemStackAdded += OnItemStackAdded;

    private void Start() => systemRepairMenu = FindFirstObjectByType<SystemRepairMenu>();

    private void OnDisable() => onItemStackAdded -= OnItemStackAdded;

    private void OnItemStackAdded() {

        foreach (ItemStack stack in repairStacks)
            if (!ContainsItemStack(stack))
                return;

        // if we reach here, all required stacks are present for repairing
        systemRepairMenu.OnRepairInventoryFull(repairStacks, repairPercent, systemType); // notify the system repair menu that the repair inventory is full, which means the necessary items for repairing were put in

    }

    // helper to get the effective stack limit for an item in a slot
    public override int GetEffectiveStackLimit(Item item) {

        // return the amount of items required to repair the system if the item is in the repair stack
        for (int i = 0; i < repairStacks.Length; i++)
            if (repairStacks[i].GetItem().Equals(item))
                return repairStacks[i].GetCount();

        return 0;

    }

    public ItemStack[] GetRepairStacks() => repairStacks;

}

#if UNITY_EDITOR
[CustomEditor(typeof(RepairInventory))]
public class RepairInventoryEditor : Editor {

    public override void OnInspectorGUI() {

        serializedObject.Update();

        // draw all properties except the excluded ones
        DrawPropertiesExcluding(serializedObject,
            "initialSlotCount",
            "slotStackLimit",
            "itemWhitelist"
        );

        serializedObject.ApplyModifiedProperties();

    }
}
#endif
