using UnityEngine;

public class StartupManager : MonoBehaviour {

    private void Start() {

        // initialize the backpack first so that it can be used by other inventories (e.g. hotbar)
        foreach (Inventory inventory in FindObjectsByType<Inventory>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (inventory is Backpack)
                inventory.Initialize();

        // initialize all inventories (active or inactive)
        foreach (Inventory inventory in FindObjectsByType<Inventory>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (inventory is not Backpack) // don't initialize the backpack again
                inventory.Initialize();

        // initialize all inventory UIs (active or inactive)
        foreach (InventoryUI inventoryUI in FindObjectsByType<InventoryUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            inventoryUI.Initialize();

    }
}
