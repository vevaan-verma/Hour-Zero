using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    [Header("References")]
    private TimeManager timeManager;

    [Header("Backpack")]
    [SerializeField] private float backpackFadeDuration;
    [SerializeField] private KeyCode backpackKey;
    private BackpackUI primaryBackpackUI; // this is the backpack UI used for the opening the backpack itself, not exchange menus

    [Header("System Repair")]
    [SerializeField] private SystemRepairMenu systemRepairMenu; // reference to the system repair menu (used for opening the menu when interacting with a system interactable)

    [Header("Crosshair")]
    [SerializeField] private Image crosshair;
    [SerializeField] private Sprite interactCrosshair;
    private Sprite defaultCrosshair;

    [Header("Time")]
    [SerializeField] private TMP_Text timeText;

    private void Start() {

        #region VALIDATION
        // make sure there is exactly one UI of each backpack type in the scene
        BackpackType[] backpackTypes = (BackpackType[]) System.Enum.GetValues(typeof(BackpackType));
        BackpackUI[] backpackUIs = FindObjectsByType<BackpackUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (BackpackType type in backpackTypes) {

            BackpackUI[] foundBackpacks = System.Array.FindAll(backpackUIs, ui => ui.GetBackpackType() == type);

            if (foundBackpacks.Length != 1) {

                Debug.LogError($"There should be exactly one BackpackUI of type {type} in the scene, found {foundBackpacks.Length}");
                return;

            }
        }
        #endregion

        // hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        primaryBackpackUI = FindObjectsByType<BackpackUI>(FindObjectsSortMode.None).FirstOrDefault(ui => ui.GetBackpackType() == BackpackType.Primary); // find the primary backpack UI
        systemRepairMenu = FindFirstObjectByType<SystemRepairMenu>();
        timeManager = FindFirstObjectByType<TimeManager>();

        defaultCrosshair = crosshair.sprite;

        UpdateTimeText(timeManager.GetHour(), timeManager.GetMinute(), timeManager.IsAM());

    }

    private void Update() {

        if (Input.GetKeyDown(backpackKey))
            if (primaryBackpackUI.IsInventoryOpen()) // close backpack if it is open
                ClosePrimaryBackpack();
            else if (!IsMenuOpen()) // only open backpack if no other menu is open
                OpenPrimaryBackpack();

        // close backpack if escape is pressed and backpack is open
        if (Input.GetKeyDown(KeyCode.Escape))
            if (primaryBackpackUI.IsInventoryOpen()) // close backpack if it is open
                ClosePrimaryBackpack();
            else if (systemRepairMenu.IsMenuOpen()) // close system repair menu if it is open
                CloseSystemRepairMenu();

        UpdateTimeText(timeManager.GetHour(), timeManager.GetMinute(), timeManager.IsAM());

    }

    // TODO: hide crosshair when backpack/menu is opened

    public void OpenPrimaryBackpack() {

        if (IsMenuOpen() || primaryBackpackUI.IsInventoryOpen()) return; // do nothing if another menu is open or if the primary backpack is already open
        primaryBackpackUI.OpenInventory(); // open the primary backpack UI

    }

    public void ClosePrimaryBackpack() {

        if (!primaryBackpackUI.IsInventoryOpen()) return; // do nothing if the primary backpack is not open
        primaryBackpackUI.CloseInventory(); // close the primary backpack UI

    }

    public void OpenSystemRepairMenu(ItemStack repairStack, int repairSlotCount, int repairPercent, BunkerSystemType systemType) {

        if (IsMenuOpen() || systemRepairMenu.IsMenuOpen()) return; // do nothing if another menu is open or if the system repair menu is already open
        systemRepairMenu.OpenMenu(repairStack, repairSlotCount, repairPercent, systemType); // open the system repair menu

    }

    public void CloseSystemRepairMenu() {

        if (!systemRepairMenu.IsMenuOpen()) return; // do nothing if the system repair menu is not open
        systemRepairMenu.CloseMenu(); // close the system repair menu

    }

    public void SetCrosshairType(CrosshairType type) {

        switch (type) {

            case CrosshairType.Default:
                crosshair.sprite = defaultCrosshair;
                break;

            case CrosshairType.Interact:
                crosshair.sprite = interactCrosshair;
                break;

        }
    }

    private void UpdateTimeText(int hour, int minute, bool isAM) => timeText.text = $"{hour:00}:{minute:00} " + (isAM ? "AM" : "PM");

    private IEnumerator Fade(CanvasGroup ui, float targetAlpha, float duration) {

        float currentTime = 0f;
        float startAlpha = ui.alpha;

        ui.gameObject.SetActive(true); // ensure UI is active before fading

        while (currentTime < duration) {

            currentTime += Time.deltaTime;
            ui.alpha = Mathf.Lerp(startAlpha, targetAlpha, currentTime / duration);
            yield return null;

        }

        ui.alpha = targetAlpha; // ensure final alpha is set

        // if the target alpha is 0, disable the UI
        if (targetAlpha == 0f)
            ui.gameObject.SetActive(false);

    }

    public bool IsPrimaryBackpackOpen() => primaryBackpackUI.IsInventoryOpen();

    public bool IsMenuOpen() => primaryBackpackUI.IsInventoryOpen() || systemRepairMenu.IsMenuOpen();

}

public enum CrosshairType {

    Default,
    Interact

}
