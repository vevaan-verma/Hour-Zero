using UnityEngine;

public class StartupManager : MonoBehaviour {

    private void Start() {

        // initialize the backpack first so that it can be used by other inventories (e.g. hotbar; active or inactive)
        foreach (Inventory inventory in FindObjectsByType<Inventory>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (inventory is Backpack)
                inventory.Initialize();

        // then, initialize all other inventories (active or inactive)
        foreach (Inventory inventory in FindObjectsByType<Inventory>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (inventory is not Backpack && inventory is not ExchangeInventory) // skip the backpack since it was already initialized and skip exchange inventories since they are initialized separately (e.g. trade, repair; initialized each time they are opened)
                inventory.Initialize();

        // finally, initialize all inventory UIs (active or inactive)
        foreach (InventoryUI inventoryUI in FindObjectsByType<InventoryUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            inventoryUI.Initialize();

    }
}
