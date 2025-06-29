using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Item")]
public class Item : ScriptableObject {

    [Header("Properties")]
    [SerializeField] private string itemName;
    [SerializeField] private string itemDescription;
    [SerializeField] private Sprite itemIcon;
    [SerializeField, Tooltip("The maximum number of items that can be stacked in this item. If set to 0, it will either use the slot's stack limit, or if that is set to 0, an infinite limit"), Min(0)] private int stackLimit;
    [SerializeField] private ItemType itemType;
    [SerializeField] private HeldItem heldItemPrefab;
    [Space]
    [SerializeField] private float attackDistance;
    [SerializeField] private float attackForce;
    [SerializeField] private float attackCooldown;

    public string GetName() => itemName;

    public string GetDescription() => itemDescription;

    public Sprite GetItemIcon() => itemIcon;

    public int GetStackSize() => stackLimit;

    public ItemType GetItemType() => itemType;

    public HeldItem GetHeldItemPrefab() => heldItemPrefab;

    public float GetAttackDistance() => attackDistance;

    public float GetAttackForce() => attackForce;

    public float GetAttackCooldown() => attackCooldown;

    public override bool Equals(object other) => other is Item item && itemName == item.itemName && itemDescription == item.itemDescription && itemIcon == item.itemIcon && itemType == item.itemType;

    public override int GetHashCode() => itemName.GetHashCode() ^ itemDescription.GetHashCode() ^ itemIcon.GetHashCode() ^ itemType.GetHashCode(); // combine hash codes of item properties for uniqueness

}

[Serializable]
public class ItemStack {

    // don't check if this object is null, check if the item is null instead because empty slots store empty item stacks, but null item objects

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

public enum ItemType {

    Consumable,
    Tool

}
