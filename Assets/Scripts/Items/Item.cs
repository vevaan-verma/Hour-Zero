using UnityEngine;

[CreateAssetMenu(menuName = "Item")]
public class Item : ScriptableObject {

    [Header("Properties")]
    [SerializeField] private string itemName;
    [SerializeField] private Sprite icon;
    [SerializeField] private int stackSize;

    public string GetName() => itemName;

    public Sprite GetIcon() => icon;

    public int GetStackSize() => stackSize;

    public override bool Equals(object other) => other is Item item && itemName == item.itemName && icon == item.icon && stackSize == item.stackSize;

    public override int GetHashCode() => itemName.GetHashCode() ^ icon.GetHashCode() ^ stackSize.GetHashCode(); // combine hash codes of item properties for uniqueness

}
