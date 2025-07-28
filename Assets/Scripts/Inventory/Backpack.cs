using UnityEngine;

public class Backpack : Inventory {

    [Header("Settings")]
    [SerializeField] private Color hotbarSlotColor;

    public Color GetHotbarSlotColor() => hotbarSlotColor;

}

public enum BackpackType {

    Primary, // the main backpack that the player uses for inventory management
    Repair, // for repair menus
    Trade // for trade menus

}
