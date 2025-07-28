using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class ExchangeInventoryUI : InventoryUI {

    [Header("References")]
    [SerializeField] private Transform inputSlotsContainer;
    [SerializeField] private Transform outputSlotsContainer;
    protected ExchangeInventory exchangeInventory; // more specific reference to the exchange inventory (could be TradeInventory, RepairInventory, etc.); allows for more specific methods to be called on the exchange inventory
    private Slot[] inputSlots; // as the name suggests, this stores all the input slots in the exchange inventory
    private Slot[] outputSlots; // as the name suggests, this stores all the output slots in the exchange inventory
    // we still do use inventorySlots; it stores all the slots in the exchange inventory, which includes both input and output slots

    public void FillOutputSlots() {

        ItemStack[] inputStacks = exchangeInventory.GetExchangeData().GetInputItemStacks(); // get the input stacks for the exchange
        ItemStack[] outputStacks = exchangeInventory.GetExchangeData().GetOutputItemStacks(); // get the output stacks for the exchange

        // fill the output slots with the output stacks
        for (int i = 0; i < outputStacks.Length; i++)
            exchangeInventory.SetItemStack(outputStacks[i], i + inputStacks.Length); // set the item stack for the output slot to the corresponding output stack from the exchange data

        exchangeInventory.SetFilters(); // now, set the filters for the exchange inventory because output slots have been filled with the output stacks, and the filters need to block the output stack items from being dragged into the input slots

    }

    public override void RefreshInventory() {

        // initialSlotCount is the total amount of slots in the exchange inventory, or the amount of input item stacks plus the amount of output item stacks (each item gets a slot)

        ExchangeData exchangeData = exchangeInventory.GetExchangeData(); // get the exchange data from the inventory

        inventorySlots = new Slot[exchangeData.GetInputItemStacks().Length + exchangeData.GetOutputItemStacks().Length]; // create an array for inventory slots based on the total number of input and output item stacks (which would be the total number of slots in the exchange inventory)
        inputSlots = new Slot[exchangeData.GetInputItemStacks().Length]; // create an array for input slots based on the number of input item stacks
        outputSlots = new Slot[exchangeData.GetOutputItemStacks().Length]; // create an array for output slots based on the number of output item stacks

        // delete all existing input slots
        foreach (Transform child in inputSlotsContainer)
            Destroy(child.gameObject); // destroy the child game object

        // delete all existing output slots
        foreach (Transform child in outputSlotsContainer)
            Destroy(child.gameObject); // destroy the child game object

        ItemStack[] inputStacks = exchangeData.GetInputItemStacks(); // get the input stacks for the exchange

        // instantiate the slots for input items in the input slots container
        for (int i = 0; i < inputStacks.Length; i++) {

            Slot slot = Instantiate(slotPrefab, inputSlotsContainer.transform);
            slot.transform.name = $"InputSlot{i + 1}";
            ItemStack itemStack = inventory.GetItemStack(i); // get the item stack from the inventory at the corresponding index
            slot.Initialize(inventory, this, i, new ItemStack(itemStack.GetItem(), itemStack.GetCount()), showItemInfoWidgetOnHover); // initialize the slot
            inventorySlots[i] = slot; // store the slot in the array for later reference
            inputSlots[i] = slot; // store the slot in the input slots array for later reference

        }

        ItemStack[] outputStacks = exchangeData.GetOutputItemStacks(); // get the output stacks for the exchange

        // instantiate the slots for output items in the output slots container
        for (int i = 0; i < outputStacks.Length; i++) {

            Slot slot = Instantiate(slotPrefab, outputSlotsContainer.transform);
            slot.transform.name = $"OutputSlot{i + 1}";
            ItemStack itemStack = inventory.GetItemStack(i + inputStacks.Length); // get the item stack from the inventory at the corresponding index
            slot.Initialize(inventory, this, i + inputStacks.Length, new ItemStack(itemStack.GetItem(), itemStack.GetCount()), showItemInfoWidgetOnHover); // initialize the slot
            slot.SetLocked(true); // lock the output slots to prevent interaction
            inventorySlots[i + inputStacks.Length] = slot; // store the slot in the array for later reference; add the input slot count to the index to ensure the output slots are stored after the input slots
            outputSlots[i] = slot; // store the slot in the output slots array for later reference

        }

        UpdatePlaceholders(); // update the placeholder cycle to ensure it reflects the current state of the inventory

        // refresh the layout if the rect transform is active in hierarchy
        if (rectTransform.gameObject.activeInHierarchy)
            RefreshLayout(rectTransform);

    }

    // OpenInventory and CloseInventory could be placed in the ExchangeInventoryUI class, but they are kept here in case the inventory UIs ever have different behaviors for opening and closing (e.g. animations, sounds, etc.)
    public override void OpenInventory() {

        if (isInventoryOpen) return; // do nothing if the inventory is already open

        RefreshInventory(); // refresh the inventory slots to ensure they are up to date

        isInventoryOpen = true;
        uiPanel.gameObject.SetActive(true); // make sure the inventory panel is active while opening

        Cursor.lockState = CursorLockMode.None; // unlock cursor
        Cursor.visible = true; // make cursor visible

    }

    public override void CloseInventory() {

        if (!isInventoryOpen) return; // do nothing if the inventory is already closed

        RefreshInventory(); // refresh the inventory slots to ensure they are up to date

        isInventoryOpen = false; // set the state to closed before waiting for animation because it feels better if the player can move and look around while the inventory is closing
        uiPanel.gameObject.SetActive(true); // make sure the inventory panel is active while closing

        Cursor.lockState = CursorLockMode.Locked; // lock cursor
        Cursor.visible = false; // hide cursor

    }

    private void UpdatePlaceholders() {

        List<ItemStack> placeholderItems = exchangeInventory.GetExchangeData().GetInputItemStacks().ToList();

        // loop through the filled slot items and remove them from the placeholder items array so only the items that haven't been set in the slots are used as placeholders
        for (int i = 0; i < inputSlots.Length; i++)
            if (inputSlots[i].IsItemStackSet())
                placeholderItems.RemoveAll(item => item.GetItem().Equals(inputSlots[i].GetItemStack().GetItem())); // find the item stack in the placeholder items list with the same item as the slot's item stack and remove it

        // if there are no placeholder items left, return as there is nothing to cycle through (all exchange items have been set in the slots)
        if (placeholderItems.Count == 0)
            return;

        // offset shifts the starting index for cycling through placeholder items, creating a rotating effect in the inventory slots (offset decreasing each cycle makes the placeholder items appear to shift right)
        for (int offset = placeholderItems.Count - 1; offset >= 0; offset--) {

            // cycle through the INPUT slots and set the placeholder items
            for (int i = 0; i < inputSlots.Length; i++) {

                if (inputSlots[i].IsItemStackSet()) continue; // skip the slot if it already has an item stack set since it doesn't need a placeholder

                int itemIndex = (i + offset) % placeholderItems.Count; // calculate the index of the placeholder item to set in the slot, wrapping around if necessary
                inputSlots[i].SetPlaceholder(placeholderItems[itemIndex]);

            }
        }
    }
}
