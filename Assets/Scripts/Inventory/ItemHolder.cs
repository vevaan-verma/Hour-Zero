using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemHolder : MonoBehaviour {

    [Header("References")]
    protected Item item;

    [Header("UI References")]
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Sprite emptyIcon; // icon to show when there is no item
    protected Image itemIcon;

    [Header("Settings")]
    protected int count;

    public virtual void Initialize() => itemIcon = GetComponent<Image>();

    public void SetItem(Item item, int count) {

        this.item = item;

        itemIcon.sprite = item == null ? emptyIcon : item.GetIcon(); // set the image sprite to the item's icon

        this.count = count;
        countText.text = count.ToString(); // set the count text to the item's stack size or empty if no item

        countText.gameObject.SetActive(item != null && count > 1); // only show the count text if there is an item and its stack size is greater than 1

    }

    public void RemoveItem(Item item, int count) {

        if (this.item != item) return; // if the item in this slot is not the same as the one to remove, do nothing

        this.count -= count; // decrease the count

        if (this.count <= 0)
            SetItem(null, 0); // if the count is 0 or less, set the slot to empty
        else
            countText.text = this.count.ToString(); // otherwise, update the count text

    }

    public Item GetItem() => item;

    public int GetCount() => count;

}
