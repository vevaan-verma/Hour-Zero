using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Item")]
public class Item : ScriptableObject {

    [Header("Properties")]
    [SerializeField] private string itemName;
    [SerializeField] private Sprite icon;
    [SerializeField, Tooltip("The maximum number of items that can be stacked in this item. If set to 0, it will either use the slot's stack limit, or if that is set to 0, an infinite limit"), Min(0)] private int stackLimit;

    public string GetName() => itemName;

    public Sprite GetIcon() => icon;

    public int GetStackSize() => stackLimit;

    public override bool Equals(object other) => other is Item item && itemName == item.itemName && icon == item.icon && stackLimit == item.stackLimit;

    public override int GetHashCode() => itemName.GetHashCode() ^ icon.GetHashCode() ^ stackLimit.GetHashCode(); // combine hash codes of item properties for uniqueness

}

[Serializable]
public class ItemStack {

    [Header("Data")]
    [SerializeField] private Item item;
    [SerializeField] private int count;

    public ItemStack(Item item, int count) {

        this.item = item;
        this.count = count;

    }

    public Item GetItem() => item;

    public int GetCount() => count;

}
