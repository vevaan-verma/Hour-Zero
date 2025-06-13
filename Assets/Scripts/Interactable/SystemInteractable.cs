using UnityEngine;

public class SystemInteractable : Interactable {

    [Header("References")]
    private UIManager uiManager;

    [Header("Settings")]
    [SerializeField] private BunkerSystemType systemType;
    [SerializeField] private ItemStack[] repairStacks;
    [SerializeField, Min(1)] private int repairSlotCount;
    [SerializeField] private int repairPercent;

    private new void Start() {

        base.Start();
        uiManager = FindFirstObjectByType<UIManager>();

    }

    public override void Interact() => uiManager.OpenSystemRepairMenu(repairStacks, repairSlotCount, repairPercent, systemType); // open the system repair menu

}
