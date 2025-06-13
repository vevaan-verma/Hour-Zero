using UnityEngine;

public class SystemInteractable : Interactable {

    [Header("References")]
    private UIManager uiManager;

    [Header("Settings")]
    [SerializeField] private BunkerSystemType systemType;
    [SerializeField] private ItemStack repairStack;
    [SerializeField, Min(1)] private int repairSlotCount;
    [SerializeField] private int repairPercent;

    private new void Start() {

        base.Start();

        #region VALIDATION
        // make sure the repair stack count is completely divisible by the initial slot count
        if (repairStack.GetCount() % repairSlotCount != 0) {

            Debug.LogError("Repair stack count must be completely divisible by the initial slot count on " + name);
            return;

        }
        #endregion

        uiManager = FindFirstObjectByType<UIManager>();

    }

    public override void Interact() => uiManager.OpenSystemRepairMenu(repairStack, repairSlotCount, repairPercent, systemType); // open the system repair menu

}
