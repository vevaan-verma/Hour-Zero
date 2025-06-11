using System.Text.RegularExpressions;
using UnityEngine;

public class SystemInteractable : Interactable {

    [Header("References")]
    private UIManager uiManager;
    private AlertManager alertManager;

    [Header("Settings")]
    [SerializeField] private BunkerSystemType systemType;
    [SerializeField] private Item repairItem;
    [SerializeField] private int repairItemCount;
    [SerializeField] private int repairAmount;

    private new void Start() {

        base.Start();

        uiManager = FindFirstObjectByType<UIManager>();
        alertManager = FindFirstObjectByType<AlertManager>();

    }

    public override void Interact() {

        //if (backpack.ContainsItemStack(repairItem, repairItemCount)) {

        uiManager.OpenSystemRepairMenu(); // open the system repair menu

        //backpack.RemoveItem(repairItem, repairItemCount); // remove the required amount of repair items from the backpack
        //bunkerManager.RepairSystem(systemType, repairAmount);

        string formattedSystemType = Regex.Replace(systemType.ToString(), "(\\B[A-Z])", " $1"); // format the system type to be more readable by adding spaces in between the words (e.g., "AirFiltration" -> "Air Filtration")
        alertManager.SendAlert(new Alert($"Repaired {repairAmount}% {formattedSystemType} system durability using {repairItem.GetName()} x{repairItemCount}", AlertType.Success));

        //} else {

        //    alertManager.SendAlert(new Alert($"Not enough {repairItem.GetName()} in backpack to repair! Required: {repairItemCount}", AlertType.Failure));

        //}
    }
}
