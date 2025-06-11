using UnityEngine;

public class Backpack : Inventory {

    [Header("References")]
    private AlertManager alertManager;

    public override void Initialize() {

        base.Initialize();
        alertManager = FindFirstObjectByType<AlertManager>();

    }

    // returns the amount of items that could not be added to the backpack
    public override int AddItemStack(ItemStack itemStack) {

        Item item = itemStack.GetItem();
        int remainder = base.AddItemStack(itemStack); // get the remainder from the base class method

        // if the remainder is greater than 0, it means the backpack is full and we couldn't add all items
        if (remainder > 0)
            alertManager.SendAlert(new Alert($"Backpack is full! Could not add {remainder}x {item.GetName()} to backpack", AlertType.Failure)); // send an alert to the player that the backpack is full and could not add all items

        return remainder;

    }
}

public enum BackpackType {

    Primary, // the main backpack that the player uses for inventory management
    Repair // for repair menus

}
