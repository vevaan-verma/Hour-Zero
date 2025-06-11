using UnityEngine;

public class StartupManager : MonoBehaviour {

    private void Start() {

        // initialize all inventories (active or inactive)
        foreach (var inventory in FindObjectsByType<Inventory>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            inventory.Initialize();

        // initialize all inventory UIs (active or inactive)
        foreach (var inventoryUI in FindObjectsByType<InventoryUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            inventoryUI.Initialize();

    }
}
