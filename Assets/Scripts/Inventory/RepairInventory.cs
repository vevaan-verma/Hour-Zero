using UnityEngine;

public class RepairInventory : Inventory {

    [Header("References")]
    private SystemRepairMenu systemRepairMenu;
    private ItemStack[] repairStacks;
    private int repairPercent;
    private BunkerSystemType systemType;

    public void Initialize(ItemStack[] repairStacks, int repairPercent, BunkerSystemType systemType) {

        this.repairStacks = repairStacks; // set the repair stacks to the stacks required for repairing
        this.initialSlotCount = repairStacks.Length; // set the initial slot count to the number of items that are required for repairing
        this.repairPercent = repairPercent; // set the repair percent to the percent durability of the system that is to be repaired
        this.systemType = systemType; // set the system type to the type of system that is to be repaired

        this.itemTypeFilterType = FilterType.Whitelist; // set the item type filter type to whitelist since we only want to allow the item types that are in the repair stacks

        this.filteredItemTypes = new ItemType[repairStacks.Length]; // set the filtered item types to the item types of the repair stacks

        for (int i = 0; i < repairStacks.Length; i++)
            this.filteredItemTypes[i] = repairStacks[i].GetItem().GetItemType(); // set the filtered item types to the item types of the repair stacks

        this.itemFilterType = FilterType.Whitelist; // set the filter type to whitelist since we only want to allow the items that are in the repair stacks

        // set the item whitelist to the repair stack items
        this.filteredItems = new Item[repairStacks.Length];

        for (int i = 0; i < repairStacks.Length; i++)
            this.filteredItems[i] = repairStacks[i].GetItem();

        base.Initialize(); // initialize at the end to ensure the properties are set before calling the base method (especially the slot count)

    }

    private void OnEnable() => onContentsUpdated += OnItemStackAdded;

    private void Start() => systemRepairMenu = FindFirstObjectByType<SystemRepairMenu>();

    private void OnDisable() => onContentsUpdated -= OnItemStackAdded;

    private void OnItemStackAdded() {

        foreach (ItemStack stack in repairStacks)
            if (!ContainsItemStack(stack))
                return;

        // at this point, all required stacks are present for repairing

        systemRepairMenu.OnRepairRequirementsMet(repairPercent, systemType); // notify the system repair menu that the repair inventory is full, which means the necessary items for repairing were put in

    }

    public override int GetEffectiveStackLimit(Item item) {

        // return the amount of items required to repair the system if the item is in the repair stacks
        for (int i = 0; i < repairStacks.Length; i++)
            if (repairStacks[i].GetItem().Equals(item))
                return repairStacks[i].GetCount();

        return 0;

    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(RepairInventory))]
// using UnityEditor prefix to avoid needing to hide the import in the final build
public class RepairInventoryEditor : UnityEditor.Editor {

    public override void OnInspectorGUI() {

        serializedObject.Update();

        // draw all properties except the excluded ones
        DrawPropertiesExcluding(serializedObject,
            "slotsPerRow",
            "initialSlotCount",
            "slotStackLimit",
            "itemTypeFilterType",
            "filteredItemTypes",
            "itemFilterType",
            "filteredItems"
        );

        serializedObject.ApplyModifiedProperties();

    }
}
#endif
