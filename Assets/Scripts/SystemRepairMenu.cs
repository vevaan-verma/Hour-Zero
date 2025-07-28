using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class SystemRepairMenu : ExchangeMenu {

    [Header("References")]
    private BunkerManager bunkerManager;

    private new void OnEnable() {

        exchangeInventory = FindFirstObjectByType<RepairInventory>(); // find the repair inventory (must be done before base.Initialize() to ensure exchange inventory is set)
        base.OnEnable();

    }

    private new void Start() {

        bunkerManager = FindFirstObjectByType<BunkerManager>();

        exchangeBackpackUI = FindObjectsByType<BackpackUI>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(ui => ui.GetBackpackType() == BackpackType.Repair); // find the repair backpack UI (must be done before base.Start() to ensure the repair backpack UI is set)
        closeMenuButton.onClick.AddListener(() => uiManager.CloseSystemRepairMenu()); // add listener to close menu button; call the UIManager method to close the system repair menu rather than this class directly to ensure the extra logic is executed (e.g., re-opening the hotbar UI)
        base.Start();

    }

    protected override void ProcessExchange() {

        RepairData currRepairData = (RepairData) currExchangeData; // cast the current exchange data to RepairData to access repair-specific methods

        BunkerSystemType systemType = currRepairData.GetSystemType(); // get the system type from the repair data
        int repairPercent = currRepairData.GetRepairPercent(); // get the repair percent from the repair data

        uiManager.CloseSystemRepairMenu(true); // close the menu when the repair inventory is full (with a flag that the repair requirements were met); don't call CloseMenu() directly to ensure the UIManager logic is executed (e.g., re-opening the hotbar UI)
        bunkerManager.RepairSystem(systemType, repairPercent); // repair the system using the bunker manager
        exchangeInventory.Clear();

        string formattedSystemType = Regex.Replace(systemType.ToString(), "(\\B[A-Z])", " $1").ToLower(); // format the system type to be more readable by adding spaces in between the words (e.g., "AirFiltration" -> "Air Filtration") and convert to lowercase
        phoneManager.SendNotification(new NotificationData(null, "Bunka", $"The {formattedSystemType} system has been repaired by {repairPercent}%"));

    }
}
