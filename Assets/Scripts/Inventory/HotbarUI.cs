using UnityEngine;

public class HotbarUI : InventoryUI {

    public override void Initialize() {

        inventory = FindFirstObjectByType<Hotbar>(FindObjectsInactive.Include); // find the hotbar in the scene
        base.Initialize();

    }

    public override void CloseInventory() { } // nothing needed here for the hotbar UI as it is always visible

    public override void OpenInventory() { } // nothing needed here for the hotbar UI as it is always visible

}
