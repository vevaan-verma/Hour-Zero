using UnityEngine;

public class TradeInventory : Inventory {

    [Header("References")]
    private TradeMenu npcTradeMenu;
    private TradeData tradeData;

    public void Initialize(TradeData tradeData) {

        this.tradeData = tradeData; // set the trade data for this trade inventory

        ItemStack[] inputStacks = tradeData.GetInputItems(); // get the input stacks for the trade

        this.initialSlotCount = inputStacks.Length; // set the initial slot count to the number of input item types required for the trade

        this.itemTypeFilterType = FilterType.Whitelist; // set the item type filter type to whitelist since we only want to allow the item types that are in the trade stacks

        this.filteredItemTypes = new ItemType[inputStacks.Length]; // set the filtered item types to the item types of the trade stacks

        for (int i = 0; i < inputStacks.Length; i++)
            this.filteredItemTypes[i] = inputStacks[i].GetItem().GetItemType(); // set the filtered item types to the item types of the trade stacks

        this.itemFilterType = FilterType.Whitelist; // set the filter type to whitelist since we only want to allow the items that are in the trade stacks

        // set the item whitelist to the trade stack items
        this.filteredItems = new Item[inputStacks.Length];

        for (int i = 0; i < inputStacks.Length; i++)
            this.filteredItems[i] = inputStacks[i].GetItem();

        base.Initialize(); // initialize at the end to ensure the properties are set before calling the base method (especially the slot count)

    }

    private void OnEnable() => onContentsUpdated += OnItemStackAdded;

    private void Start() => npcTradeMenu = FindFirstObjectByType<TradeMenu>();

    private void OnDisable() => onContentsUpdated -= OnItemStackAdded;

    private void OnItemStackAdded() {

        ItemStack[] inputStacks = tradeData.GetInputItems(); // get the input stacks for the trade

        foreach (ItemStack stack in inputStacks)
            if (!ContainsItemStack(stack))
                return;

        // at this point, all required stacks are present for trading

        npcTradeMenu.OnTradeRequirementsMet(tradeData); // notify the trade menu that the trade inventory is full, which means the necessary items for trading were put in

    }

    public override int GetEffectiveStackLimit(Item item) {

        ItemStack[] inputStacks = tradeData.GetInputItems(); // get the input stacks for the trade

        // return the amount of items required to trade if the item is in the trade stacks
        for (int i = 0; i < inputStacks.Length; i++)
            if (inputStacks[i].GetItem().Equals(item))
                return inputStacks[i].GetCount();

        return 0;

    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(TradeInventory))]
// using UnityEditor prefix to avoid needing to hide the import in the final build
public class TradeInventoryEditor : UnityEditor.Editor {

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
