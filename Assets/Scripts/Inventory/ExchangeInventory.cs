using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class ExchangeInventory : Inventory {

    [Header("References")]
    protected ExchangeData exchangeData;

    public void Initialize(ExchangeData exchangeData) {

        this.exchangeData = exchangeData; // set the exchange data for this exchange inventory

        this.slotCount = exchangeData.GetInputItemStacks().Length + exchangeData.GetOutputItemStacks().Length; // set the slot count to the number of input item stacks plus the number of output item stacks

        this.itemTypeFilterType = FilterType.Whitelist;
        this.filteredItemTypes = new ItemType[0]; // initialize the filtered item types to an empty array

        this.itemFilterType = FilterType.Whitelist;
        this.filteredItems = new Item[0]; // initialize the filtered items to an empty array

        // don't actually set the filters here because they need to be set after the output slots are filled; the actual filters exclude the items in the output stack, so if they are set right now, the items wouldn't be able to be put in the exchangeData inventory in the correct slots

        base.Initialize(); // initialize at the end to ensure the properties are set before calling the base method (especially the slot count)

    }

    public void SetFilters() {

        this.slotCount = exchangeData.GetInputItemStacks().Length; // update the slot count to only include the input item stacks, which is the number of input slots in the inventory; doing this prevents items from being added to the output slots

        ItemStack[] inputStacks = exchangeData.GetInputItemStacks(); // get the input stacks for the exchange

        this.itemTypeFilterType = FilterType.Whitelist; // set the item type filter type to whitelist since we only want to allow the item types that are in the exchange stacks

        HashSet<ItemType> uniqueItemTypes = new HashSet<ItemType>(); // use a hash set to ensure unique item types

        // add all item types from the input stacks to the hash set
        for (int i = 0; i < inputStacks.Length; i++)
            uniqueItemTypes.Add(inputStacks[i].GetItem().GetItemType());

        // don't include the output stacks in the item type filter, since they are not allowed to be put in the exchange inventory, they are only allowed to be taken out after trading

        this.filteredItemTypes = uniqueItemTypes.ToArray(); // convert the hash set to an array and assign it to the filtered item types

        this.itemFilterType = FilterType.Whitelist; // set the filter type to whitelist since we only want to allow the items that are in the exchange stacks

        HashSet<Item> uniqueItems = new HashSet<Item>(); // use a hash set to ensure unique items

        // add all items from the input stacks to the hash set
        for (int i = 0; i < inputStacks.Length; i++)
            uniqueItems.Add(inputStacks[i].GetItem());

        // don't include the output stacks in the item filter, since they are not allowed to be put in the exchange inventory, they are only allowed to be taken out after trading

        this.filteredItems = uniqueItems.ToArray(); // convert the hash set to an array and assign it to the filtered items

    }

    public override int GetEffectiveStackLimit(Item item) {

        ItemStack[] inputStacks = exchangeData.GetInputItemStacks(); // get the input stacks for the exchange

        // return the amount of items required to exchange if the item is in the input stacks
        for (int i = 0; i < inputStacks.Length; i++)
            if (inputStacks[i].GetItem().Equals(item))
                return inputStacks[i].GetCount();

        return 0;

    }

    public ExchangeData GetExchangeData() => exchangeData;

}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(ExchangeInventory), true)]
// using UnityEditor prefix to avoid needing to hide the import in the final build
public class ExchangeInventoryEditor : UnityEditor.Editor {

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
