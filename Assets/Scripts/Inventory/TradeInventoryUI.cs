using UnityEngine;

public class TradeInventoryUI : ExchangeInventoryUI {

    public override void Initialize() {

        inventory = FindFirstObjectByType<TradeInventory>(FindObjectsInactive.Include); // find the inventory in the scene (must be done before base.Initialize() to ensure inventory is set)
        exchangeInventory = (TradeInventory) inventory;
        base.Initialize();

    }
}
