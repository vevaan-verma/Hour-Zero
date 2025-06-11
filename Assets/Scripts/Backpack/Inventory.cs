using UnityEngine;

public abstract class Inventory : MonoBehaviour {

    public abstract void Initialize();

    public abstract ItemStack GetItemStack(int index);

    public abstract int SetItemStack(int index, Item item, int count);

    // TODO: convert to itemstack
    public abstract int AddItemStack(Item item, int count);

    public abstract int RemoveItemStack(Item item, int count);

    public abstract void SwapItemStacks(int indexA, int indexB);

    public abstract int GetEffectiveStackLimit(Item item);

}
