using UnityEngine;

public class Backpack : Inventory {

    [Header("References")]
    private AlertManager alertManager;

    [Header("Settings")]
    [SerializeField] private Color hotbarSlotColor;

    public override void Initialize() {

        base.Initialize();
        alertManager = FindFirstObjectByType<AlertManager>();

    }

    public Color GetHotbarSlotColor() => hotbarSlotColor;

}

public enum BackpackType {

    Primary, // the main backpack that the player uses for inventory management
    Repair, // for repair menus
    Trade // for trade menus

}
