using UnityEngine;

public abstract class InventoryUI : MonoBehaviour {

    [Header("Settings")]
    protected bool isInventoryOpen;

    public abstract void Initialize();

    public abstract void RefreshInventory();

    public abstract void OpenInventory();

    public abstract void CloseInventory();

    public abstract bool IsInventoryOpen();

}
