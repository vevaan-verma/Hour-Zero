using UnityEngine;

public class RepairInventoryUI : ExchangeInventoryUI {

    public override void Initialize() {

        inventory = FindFirstObjectByType<RepairInventory>(FindObjectsInactive.Include); // find the inventory in the scene (must be done before base.Initialize() to ensure inventory is set)
        exchangeInventory = (RepairInventory) inventory;
        base.Initialize();

    }
}
