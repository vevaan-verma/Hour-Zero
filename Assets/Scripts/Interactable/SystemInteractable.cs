using UnityEngine;

public class SystemInteractable : Interactable {

    [Header("References")]
    private UIManager uiManager;

    [Header("Settings")]
    [SerializeField] private RepairData repairData;

    private new void Start() {

        base.Start();
        uiManager = FindFirstObjectByType<UIManager>();

    }

    public override bool Interact() {

        if (!base.Interact()) return false; // if the base interaction fails, do not proceed

        uiManager.OpenSystemRepairMenu(repairData); // open the system repair menu
        return true;

    }
}
