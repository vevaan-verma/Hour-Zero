using UnityEngine;

public class TradeInventoryUI : ExchangeInventoryUI {

    [Header("References")]
    [SerializeField] private Transform inputSlotsContainer;
    [SerializeField] private Transform outputSlotsContainer;
    private Slot[] inputSlots;
    private Slot[] outputSlots;

    public override void Initialize() {

        inventory = FindFirstObjectByType<TradeInventory>(FindObjectsInactive.Include); // find the inventory in the scene (must be done before base.Initialize() to ensure inventory is set)
        base.Initialize();

    }

    public void FillOutputSlots() {

        TradeInventory tradeInventory = (TradeInventory) inventory;
        ItemStack[] inputStacks = tradeInventory.GetTradeData().GetInputItemStacks(); // get the input stacks for the trade
        ItemStack[] outputStacks = tradeInventory.GetTradeData().GetOutputItemStacks(); // get the output stacks for the trade

        // fill the output slots with the output stacks
        for (int i = 0; i < outputStacks.Length; i++)
            tradeInventory.SetItemStack(outputStacks[i], i + inputStacks.Length); // set the item stack for the output slot to the corresponding output stack from the trade data

        tradeInventory.SetFilters(); // now, set the filters for the trade inventory because output slots have been filled with the output stacks, and the filters need to block the output stack items from being dragged into the input slots

    }

    public override void RefreshInventory() {

        // initialSlotCount is the total amount of slots in the trade inventory, or the amount of input item stacks plus the amount of output item stacks (each item gets a slot)

        TradeInventory tradeInventory = (TradeInventory) inventory;
        TradeData tradeData = tradeInventory.GetTradeData(); // get the trade data from the inventory

        inventorySlots = new Slot[tradeInventory.GetInitialSlotCount()];
        inputSlots = new Slot[tradeData.GetInputItemStacks().Length]; // create an array for input slots based on the number of input item stacks
        outputSlots = new Slot[tradeData.GetOutputItemStacks().Length]; // create an array for output slots based on the number of output item stacks

        // delete all existing input slots
        foreach (Transform child in inputSlotsContainer)
            Destroy(child.gameObject); // destroy the child game object

        // delete all existing output slots
        foreach (Transform child in outputSlotsContainer)
            Destroy(child.gameObject); // destroy the child game object

        ItemStack[] inputStacks = tradeData.GetInputItemStacks(); // get the input stacks for the trade
        inputSlotCount = inputStacks.Length; // set the input slot count based on the number of input item stacks as this is the amount of input slots to be displayed in the UI

        // instantiate the slots for input items in the input slots container
        for (int i = 0; i < inputSlotCount; i++) {

            Slot slot = Instantiate(slotPrefab, inputSlotsContainer.transform);
            slot.transform.name = $"InputSlot{i + 1}";
            ItemStack itemStack = inventory.GetItemStack(i); // get the item stack from the inventory at the corresponding index
            slot.Initialize(inventory, this, i, new ItemStack(itemStack.GetItem(), itemStack.GetCount()), showItemInfoWidgetOnHover); // initialize the slot
            inventorySlots[i] = slot; // store the slot in the array for later reference
            inputSlots[i] = slot; // store the slot in the input slots array for later reference

        }

        ItemStack[] outputStacks = tradeData.GetOutputItemStacks(); // get the output stacks for the trade
        outputSlotCount = outputStacks.Length; // set the output slot count based on the number of output item stacks as this is the amount of output slots to be displayed in the UI

        // instantiate the slots for output items in the output slots container
        for (int i = 0; i < outputSlotCount; i++) {

            Slot slot = Instantiate(slotPrefab, outputSlotsContainer.transform);
            slot.transform.name = $"OutputSlot{i + 1}";
            ItemStack itemStack = inventory.GetItemStack(i + inputStacks.Length); // get the item stack from the inventory at the corresponding index
            slot.Initialize(inventory, this, i + inputStacks.Length, new ItemStack(itemStack.GetItem(), itemStack.GetCount()), showItemInfoWidgetOnHover); // initialize the slot
            slot.SetLocked(true); // lock the output slots to prevent interaction
            inventorySlots[i + inputSlotCount] = slot; // store the slot in the array for later reference; add the input slot count to the index to ensure the output slots are stored after the input slots
            outputSlots[i] = slot; // store the slot in the output slots array for later reference

        }

        UpdatePlaceholderCycle(); // update the placeholder cycle to ensure it reflects the current state of the inventory

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
}
