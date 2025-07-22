using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotItemHolder : MonoBehaviour {

    [Header("References")]
    protected InventoryUI inventoryUI;
    protected ItemStack itemStack;

    [Header("UI References")]
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Sprite emptyIcon; // icon to show when there is no item
    protected Image itemIcon;

    public virtual void Initialize(InventoryUI inventoryUI) {

        this.inventoryUI = inventoryUI;
        itemIcon = GetComponent<Image>();

    }

    public void SetItemStack(ItemStack itemStack) {

        this.itemStack = itemStack;

        itemIcon.sprite = itemStack.GetItem() == null ? emptyIcon : itemStack.GetItem().GetItemIcon(); // set the image sprite to the item's icon

        int count = itemStack.GetCount(); // get the count from the item stack
        countText.text = count.ToString(); // set the count text to the item's stack size or empty if no item

        countText.gameObject.SetActive(itemStack.GetItem() != null && count > 1); // only show the count text if there is an item and its stack size is greater than 1

    }

    public void RemoveItemStack(ItemStack itemStack) {

        if (this.itemStack.GetItem() != itemStack.GetItem()) return; // if the item in this slot is not the same as the one to remove, do nothing

        this.itemStack = new ItemStack(itemStack.GetItem(), this.itemStack.GetCount() - itemStack.GetCount()); // create a new item stack with the same item but a decreased count

        int count = this.itemStack.GetCount(); // get the new count

        if (count <= 0)
            SetItemStack(new ItemStack(null, 0)); // if the count is 0 or less, set the slot to empty
        else
            countText.text = count.ToString(); // otherwise, update the count text

    }

    public Item GetItem() => this.itemStack.GetItem();

    public int GetCount() => this.itemStack.GetCount();

}
