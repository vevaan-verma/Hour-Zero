using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

    [Header("References")]
    [SerializeField] private ItemInfoWidget itemInfoWidgetPrefab;
    [SerializeField] private Image placeholder;
    protected Image image;
    private ItemInfoWidget currItemInfoWidget;
    private SlotItemHolder slotItemHolder;
    private Inventory inventory;
    private InventoryUI inventoryUI;

    [Header("Settings")]
    private bool showItemInfoWidgetOnHover;

    [Header("Data")]
    private ItemStack itemStack;
    private int index;

    [Header("Actions")]
    public Action<int, Item> onItemStackSet; // action to be invoked when the item stack is set in this slot

    public virtual void Initialize(Inventory inventory, InventoryUI inventoryUI, int index, ItemStack itemStack, bool showItemInfoWidgetOnHover, Color? slotColor = null) {

        this.inventory = inventory;
        this.inventoryUI = inventoryUI;
        this.index = index;
        this.showItemInfoWidgetOnHover = showItemInfoWidgetOnHover;
        // no need to set item here since it gets set in the SetItem method

        slotItemHolder = GetComponentInChildren<SlotItemHolder>();
        image = GetComponent<Image>();

        // set the color of the slot to the one provided if it is not null, otherwise use the default color of the slot
        if (slotColor != null)
            image.color = (Color) slotColor;

        slotItemHolder.Initialize(inventoryUI); // initialize the item holder
        ForceSetItemStack(itemStack); // initialize the slot with the provided item stack

        transform.GetChild(0).name = $"ItemHolder{index + 1}"; // rename the item holder child to reflect its index

    }

    private void OnDisable() => DestroyCurrentItemInfoWidget(); // destroy the item info widget if it exists (prevents the widget from being shown when the inventory is closed)

    public void OnPointerClick(PointerEventData eventData) {

        if (inventoryUI.AreSlotsLocked()) return; // if the slots are locked, do not process the pointer click event

        // check if shift + left click is pressed to activate quick transfer and make sure the item is not null to make sure there is something to transfer
        if (Input.GetKey(KeyCode.LeftShift) && itemStack.GetItem() != null)
            inventory.QuickTransferItem(inventoryUI.GetQuickTransferInventory(), itemStack, index);

    }

    public void OnPointerEnter(PointerEventData eventData) {

        // this event gets processed even when the slot is locked because the item widget is still shown when hovering over a locked slot

        if (eventData.dragging || itemStack.GetItem() == null || !showItemInfoWidgetOnHover) return; // if the slot is empty or the item info widget is disabled, do nothing

        DestroyCurrentItemInfoWidget(); // destroy the item info widget if it exists

        currItemInfoWidget = Instantiate(itemInfoWidgetPrefab, Input.mousePosition, Quaternion.identity, transform.root); // instantiate the special item info widget at the mouse position and set its parent to the root transform; the root transform is used to ensure that the special item info widget is always in the same space as the root object and not in world space
        currItemInfoWidget.transform.SetAsLastSibling(); // set the widget to the front
        currItemInfoWidget.Initialize(itemStack); // initialize the widget with the special item

    }

    public void OnPointerExit(PointerEventData eventData) => DestroyCurrentItemInfoWidget(); // destroy the item info widget if it exists

    public void OnDrop(PointerEventData eventData) { // this is called on the target slot when an item is dropped on it

        if (inventoryUI.AreSlotsLocked()) return; // if the slots are locked, do not process the drop event

        DraggableSlotItemHolder droppedItemHolder = eventData.pointerDrag.GetComponent<DraggableSlotItemHolder>();
        Slot sourceSlot = droppedItemHolder.GetInitialSlot();

        if (sourceSlot == null) return; // if the source slot is null, do nothing (prevents errors when an item is dragged from a locked slot)

        int sourceIndex = sourceSlot.GetIndex();
        int targetIndex = GetIndex();

        Inventory sourceInventory = sourceSlot.GetInventory();
        Inventory targetInventory = GetInventory();

        ItemStack sourceStack = sourceInventory.GetItemStack(sourceIndex);
        ItemStack targetStack = targetInventory.GetItemStack(targetIndex);

        if (sourceInventory == targetInventory) { // check if the source and target inventories are the same, so the stack limits are the same

            if (sourceIndex == targetIndex) return; // if the source and target slots are the same, we can just return

            if (GetItem() != null && GetItem() == sourceStack.GetItem()) { // check if the item in this slot is the same as the one being dropped, which would allow stacking (same regardless of if the interaction is between different inventories or not)

                int remainder = sourceInventory.SetItemStack(new ItemStack(sourceStack.GetItem(), GetCount() + sourceStack.GetCount()), targetIndex); // set the item stack in the target inventory to the one being dropped and get the remainder of items that couldn't be added

                sourceInventory.SetItemStack(new ItemStack(sourceStack.GetItem(), remainder), sourceIndex); // set the source slot to empty or the remainder of the source stack that wasn't stacked

            } else { // items are different, so we can't stack them; swapping is needed here

                if (targetStack.GetItem() == null) { // if the target slot is empty, we can just set the item there

                    sourceInventory.SetItemStack(new ItemStack(sourceStack.GetItem(), sourceStack.GetCount()), targetIndex); // set the item stack in the target inventory to the one being dropped
                    sourceInventory.SetItemStack(new ItemStack(null, 0), sourceIndex); // set the source slot to empty

                } else { // if the target slot is not empty, we need to swap the items

                    // since they are in the same inventory, we can swap them directly, without regarding stack limits (since they are the same for each slot within an inventory)
                    sourceInventory.SetItemStack(new ItemStack(targetStack.GetItem(), targetStack.GetCount()), sourceIndex); // set the source slot to the target slot item
                    sourceInventory.SetItemStack(new ItemStack(sourceStack.GetItem(), sourceStack.GetCount()), targetIndex); // set the target slot to the source slot item

                }
            }
        } else { // the source and target inventories are different, so we need to handle the swapping differently

            if (GetItem() != null && GetItem() == sourceStack.GetItem()) { // check if the item in this slot is the same as the one being dropped, which would allow stacking (same regardless of if the interaction is between different inventories or not)

                int remainder = targetInventory.SetItemStack(new ItemStack(sourceStack.GetItem(), GetCount() + sourceStack.GetCount()), targetIndex); // set the item stack in the target inventory to the one being dropped and get the remainder of items that couldn't be added
                sourceInventory.SetItemStack(new ItemStack(sourceStack.GetItem(), remainder), sourceIndex); // set the source slot to empty or the remainder of the source stack that wasn't stacked

            } else {

                // if the items are different, we need to swap them between the two inventories

                int remainder = targetInventory.SetItemStack(new ItemStack(sourceStack.GetItem(), sourceStack.GetCount()), targetIndex); // set the item stack in the target inventory to the one being dropped and get the remainder of items that couldn't be added

                if (remainder > 0) {

                    //// if the remainder is equal to the count of the item being dropped, we can return as that means the target slot had no space for the item being dropped; this is here to prevent a re-equip animation from playing when the item being dropped is entirely returned to its source slot
                    //if (remainder == sourceStack.GetCount()) return;

                    sourceInventory.SetItemStack(new ItemStack(sourceStack.GetItem(), remainder), sourceIndex); // since a remainder was returned, we need to set the source slot to the item in the source stack with the remainder count because we prioritize the remainder over the target slot item
                    remainder = sourceInventory.AddItemStack(new ItemStack(targetStack.GetItem(), targetStack.GetCount())); // add the target slot item to the source inventory if possible and get the remainder of items that couldn't be added (since we prioritize the remainder of the dropped item over the target slot item)

                    // TODO: drop the remainder on the ground if it is still greater than 0 since we couldn't add it to the source inventory (no space)

                } else {

                    sourceInventory.SetItemStack(new ItemStack(targetStack.GetItem(), targetStack.GetCount()), sourceIndex); // since no remainder was returned, we can set the source slot to the item in the target slot

                }
            }
        }
    }

    public void SetItemStack(ItemStack itemStack) {

        if (inventoryUI.AreSlotsLocked()) return; // if the slots are locked, do not allow setting the item stack

        this.itemStack = itemStack; // set the item stack in this slot to the one being dropped

        slotItemHolder.SetItemStack(itemStack); // set the item stack in the new slot item holder
        slotItemHolder.transform.SetParent(transform); // set the parent of the new item to this slot
        slotItemHolder.transform.SetAsFirstSibling(); // set to the first sibling so the count text appears on top
        slotItemHolder.transform.position = transform.position; // move the new item to the position of this slot

        onItemStackSet?.Invoke(index, itemStack.GetItem()); // invoke the action to notify that the item stack has been set

    }

    public void ForceSetItemStack(ItemStack itemStack) {

        // this method is used to set the item stack without checking if the slots are locked, for example when initializing the slot

        this.itemStack = itemStack; // set the item stack in this slot to the one being dropped

        slotItemHolder.SetItemStack(itemStack); // set the item stack in the new slot item holder
        slotItemHolder.transform.SetParent(transform); // set the parent of the new item to this slot
        slotItemHolder.transform.SetAsFirstSibling(); // set to the first sibling so the count text appears on top
        slotItemHolder.transform.position = transform.position; // move the new item to the position of this slot

        onItemStackSet?.Invoke(index, itemStack.GetItem()); // invoke the action to notify that the item stack has been set

    }

    public void DestroyCurrentItemInfoWidget() {

        if (currItemInfoWidget != null)
            Destroy(currItemInfoWidget.gameObject); // destroy the item info widget if it exists

    }

    public void SetPlaceholderItem(Item placeholderItem) => placeholder.sprite = placeholderItem.GetItemIcon(); // set the placeholder sprite to the sprite of the placeholder item

    public Inventory GetInventory() => inventory;

    public Item GetItem() => slotItemHolder.GetItem();

    public int GetCount() => slotItemHolder.GetCount();

    public int GetIndex() => index;

    public bool IsItemStackSet() => itemStack != null && itemStack.GetItem() != null; // check if the item stack is set and the item is not null

}
