using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class ExchangeInventoryUI : InventoryUI {

    // TODO: make sure the filtered items have no duplicates and validate it so two slots aren't allocated for the same item

    [Header("References")]
    private Dictionary<int, Item> filledSlots;

    [Header("Settings")]
    [SerializeField, Min(0.1f), Tooltip("The amount of time the placeholder shows up for before cycling")] private float placeholderCycleDelay;
    protected Coroutine placeholderCycleCoroutine;

    private void OnDisable() {

        if (inventorySlots != null)
            foreach (Slot slot in inventorySlots)
                slot.onItemStackSet -= OnSlotItemStackSet; // unsubscribe from the onItemStackSet event of each slot

    }

    public override void RefreshInventory() {

        // initialSlotCount is the amount of items that are needed to conduct the exchange (each item gets a slot)

        inventorySlots = new Slot[inventory.GetInitialSlotCount()];
        filledSlots = new Dictionary<int, Item>(inventory.GetInitialSlotCount());

        // delete all existing slots in the inventory contents
        foreach (Transform child in inventoryContents.transform) {

            // unsubscribe from the onItemStackSet event of the slot if it exists
            Slot slot = child.GetComponent<Slot>();

            if (slot)
                child.GetComponent<Slot>().onItemStackSet -= OnSlotItemStackSet;

            Destroy(child.gameObject);

        }

        // instantiate the slots based on the current capacity of the inventory
        for (int i = 0; i < inventorySlots.Length; i++) {

            Slot slot = Instantiate(slotPrefab, inventoryContents.transform);
            slot.transform.name = $"Slot{i + 1}";
            ItemStack itemStack = inventory.GetItemStack(i); // get the item stack from the inventory at the corresponding index
            slot.Initialize(inventory, this, i, new ItemStack(itemStack.GetItem(), itemStack.GetCount()), showItemInfoWidgetOnHover); // initialize the slot
            slot.onItemStackSet += OnSlotItemStackSet; // subscribe to the onItemStackSet event of the slot
            inventorySlots[i] = slot; // store the slot in the array for later reference

        }

        if (placeholderCycleCoroutine != null) {

            StopCoroutine(placeholderCycleCoroutine); // stop the previous placeholder cycle coroutine if it exists
            placeholderCycleCoroutine = null; // reset the coroutine reference

        }

        placeholderCycleCoroutine = StartCoroutine(HandlePlaceholderCycle()); // start the placeholder cycling coroutine

    }

    private void OnSlotItemStackSet(int index, Item item) => filledSlots.Add(index, item); // add the index and item to the filledSlots dictionary when an item stack is set in a slot

    private IEnumerator HandlePlaceholderCycle() {

        List<Item> placeholderItems = inventory.GetFilteredItems().ToList();

        // loop through the filled slot items and remove them from the placeholder items array so only the items that haven't been set in the slots are used as placeholders (done at the start of the coroutine only due to the assumption that the inventory is refreshed every time a slot is updated, which would restart this coroutine)
        for (int i = 0; i < inventorySlots.Length; i++)
            if (inventorySlots[i].IsItemStackSet())
                placeholderItems.Remove(inventorySlots[i].GetItem()); // remove the item from the placeholder items list

        // if there are no placeholder items left, exit the coroutine as there is nothing to cycle through (all exchange items have been set in the slots)
        if (placeholderItems.Count == 0)
            yield break;

        while (true) {

            // offset shifts the starting index for cycling through placeholder items, creating a rotating effect in the inventory slots (offset decreasing each cycle makes the placeholder items appear to shift right)
            for (int offset = placeholderItems.Count - 1; offset >= 0; offset--) {

                // cycle through the inventory slots and set the placeholder items
                for (int i = 0; i < inventorySlots.Length; i++) {

                    if (inventorySlots[i].IsItemStackSet()) continue; // skip the slot if it already has an item stack set since it doesn't need a placeholder

                    int itemIndex = (i + offset) % placeholderItems.Count; // calculate the index of the placeholder item to set in the slot, wrapping around if necessary
                    Item placeholderItem = placeholderItems[itemIndex]; // get the placeholder item from the filtered items based on the calculated index

                    inventorySlots[i].SetPlaceholderItem(placeholderItem);

                }

                yield return new WaitForSeconds(placeholderCycleDelay); // wait for the specified delay before cycling to the placeholder items again

            }
        }
    }
}
