using UnityEngine;

public class SystemInteractable : Interactable {

    [Header("References")]
    private UIManager uiManager;

    [Header("Settings")]
    [SerializeField] private BunkerSystemType systemType;
    [SerializeField] private Item repairItem;
    [SerializeField] private int repairItemCount;
    [SerializeField] private int repairPercent;
    [SerializeField] private int repairSlotCount;

    private new void Start() {

        base.Start();
        uiManager = FindFirstObjectByType<UIManager>();

    }

    public override void Interact() => uiManager.OpenSystemRepairMenu(new ItemStack(repairItem, repairItemCount), repairSlotCount, repairPercent, systemType); // open the system repair menu

}
