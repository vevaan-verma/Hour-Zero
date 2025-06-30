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

    public override bool Interact() {

        if (!base.Interact()) return false; // if the base interaction fails, do not proceed

        uiManager.OpenSystemRepairMenu(repairStacks, repairPercent, systemType); // open the system repair menu
        return true;

    }
}
