using UnityEditor;
using UnityEngine;

public class RepairInventory : Inventory {

    [Header("References")]
    private SystemRepairMenu systemRepairMenu;
    private ItemStack repairStack;
    private int repairPercent;
    private BunkerSystemType systemType;

    public void Initialize(ItemStack repairStack, int repairSlotCount, int repairPercent, BunkerSystemType systemType) {

        this.repairStack = repairStack; // set the repair stack to the stack that is required for repairing
        this.initialSlotCount = repairSlotCount; // set the initial slot count to the number of slots that are required for repairing
        this.repairPercent = repairPercent; // set the repair percent to the percent durability of the system that is to be repaired
        this.systemType = systemType; // set the system type to the type of system that is to be repaired
        this.slotStackLimit = repairStack.GetCount(); // set the slot stack limit to the count of the stack that is required for repairing
        this.itemWhitelist = new Item[] { repairStack.GetItem() }; // set the item whitelist to only allow the item that is required for repairing
        base.Initialize(); // initialize at the end to ensure the properties are set before calling the base method (especially the slot count)

    }

    private void OnEnable() => onItemStackAdded += OnItemStackAdded;

    private void Start() => systemRepairMenu = FindFirstObjectByType<SystemRepairMenu>();

    private void OnDisable() => onItemStackAdded -= OnItemStackAdded;

    private void OnItemStackAdded() {

        if (IsFull())
            systemRepairMenu.OnRepairInventoryFull(repairStack, repairPercent, systemType); // notify the system repair menu that the repair inventory is full, which means the necessary items for repairing were put in

    }
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
