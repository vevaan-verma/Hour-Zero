using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler {

    [Header("References")]
    [SerializeField] private ItemInfoWidget itemInfoWidgetPrefab;
    private ItemInfoWidget currItemInfoWidget;
    private ItemHolder itemHolder;
    private Inventory inventory;

    [Header("Settings")]
    [SerializeField] private bool showItemInfoWidgetOnHover;

    [Header("Data")]
    private Item item;
    private int index;

    public virtual void Initialize(Inventory inventory, int index, Item item, int count) {

        this.inventory = inventory;
        this.index = index;

        itemHolder = GetComponentInChildren<ItemHolder>();

        itemHolder.Initialize(); // initialize the item holder
        SetItem(item, count); // initialize the slot with no item and count 0

        transform.GetChild(0).name = $"ItemHolder{index + 1}"; // rename the item holder child to reflect its index

    }

    public void OnPointerEnter(PointerEventData eventData) {

        if (item == null || !showItemInfoWidgetOnHover) return; // if the slot is empty or the item info widget is disabled, do nothing

        currItemInfoWidget = Instantiate(itemInfoWidgetPrefab, Input.mousePosition, Quaternion.identity, transform.root); // instantiate the special item info widget at the mouse position and set its parent to the root transform; the root transform is used to ensure that the special item info widget is always in the same space as the root object and not in world space
        currItemInfoWidget.transform.SetAsLastSibling(); // set the widget to the front
        currItemInfoWidget.Initialize(item); // initialize the widget with the special item

    }

    public void OnPointerExit(PointerEventData eventData) {

        // destroy the special item info widget if it exists
        if (currItemInfoWidget != null)
            Destroy(currItemInfoWidget.gameObject);

    }

    public void OnDrop(PointerEventData eventData) { // this is called on the target slot when an item is dropped on it

        DraggableItemHolder droppedItemHolder = eventData.pointerDrag.GetComponent<DraggableItemHolder>();
        Slot sourceSlot = droppedItemHolder.GetInitialSlot();

        int sourceIndex = sourceSlot.GetIndex();
        int targetIndex = GetIndex();

        Inventory sourceInventory = sourceSlot.GetInventory();
        Inventory targetInventory = GetInventory();

        ItemStack sourceStack = sourceInventory.GetItemStack(sourceIndex);
        ItemStack targetStack = targetInventory.GetItemStack(targetIndex);

        if (sourceInventory == targetInventory) { // check if the source and target inventories are the same, so the stack limits are the same

            if (GetItem() != null && GetItem() == sourceStack.GetItem()) { // check if the item in this slot is the same as the one being dropped, which would allow stacking (same regardless of if the interaction is between different inventories or not)

                int remainder = sourceInventory.SetItemStack(new ItemStack(sourceStack.GetItem(), GetCount() + sourceStack.GetCount()), targetIndex); // set the item stack in the target inventory to the one being dropped and get the remainder of items that couldn't be added
                sourceInventory.SetItemStack(new ItemStack(sourceStack.GetItem(), remainder), sourceIndex); // set the source slot to empty or the remainder of the source stack that wasn't stacked

            } else { // items are different, so we can't stack them; swapping is needed here

                if (targetStack.GetItem() == null) { // if the target slot is empty, we can just set the item there

                    targetInventory.SetItemStack(new ItemStack(sourceStack.GetItem(), sourceStack.GetCount()), targetIndex); // set the item stack in the target inventory to the one being dropped
                    sourceInventory.SetItemStack(new ItemStack(null, 0), sourceIndex); // set the source slot to empty

                } else { // if the target slot is not empty, we need to swap the items

                    // since they are in the same inventory, we can swap them directly, without regarding stack limits (since they are the same for each slot within an inventory)
                    sourceInventory.SetItemStack(new ItemStack(targetStack.GetItem(), targetStack.GetCount()), sourceIndex); // set the source slot to the target slot item
                    targetInventory.SetItemStack(new ItemStack(sourceStack.GetItem(), sourceStack.GetCount()), targetIndex); // set the target slot to the source slot item

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

                    sourceInventory.SetItemStack(new ItemStack(sourceStack.GetItem(), remainder), sourceIndex); // since a remainder was returned, we need to set the source slot to the item in the source stack with the remainder count because we prioritize the remainder over the target slot item
                    remainder = sourceInventory.AddItemStack(new ItemStack(targetStack.GetItem(), targetStack.GetCount())); // add the target slot item to the source inventory if possible and get the remainder of items that couldn't be added (since we prioritize the remainder of the dropped item over the target slot item)

                    // TODO: drop the remainder on the ground if it is still greater than 0 since we couldn't add it to the source inventory (no space)

                } else {

                    sourceInventory.SetItemStack(new ItemStack(targetStack.GetItem(), targetStack.GetCount()), sourceIndex); // since no remainder was returned, we can set the source slot to the item in the target slot

                }
            }
        }
    }

    public void SetItem(Item item, int count) {

        this.item = item; // set the item in this slot to the one being dropped

        itemHolder.SetItem(item, count); // set the item and count in the new slot item holder
        itemHolder.transform.SetParent(transform); // set the parent of the new item to this slot
        itemHolder.transform.SetAsFirstSibling(); // set to the first sibling so the count text appears on top
        itemHolder.transform.position = transform.position; // move the new item to the position of this slot

    }

    public Inventory GetInventory() => inventory;

    public Item GetItem() => itemHolder.GetItem();

    public int GetCount() => itemHolder.GetCount();

    public int GetIndex() => index;

}
