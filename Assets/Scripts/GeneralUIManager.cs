using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GeneralUIManager : MonoBehaviour {

    [Header("References")]
    private TimeManager timeManager;

    [Header("Backpack")]
    [SerializeField, Tooltip("This is the backpack UI used for the opening the backpack itself, not exchange menus")] private BackpackUI primaryBackpackUI;
    [SerializeField] private float backpackFadeDuration;
    [SerializeField] private KeyCode backpackKey;

    [Header("System Repair")]
    [SerializeField] private SystemRepairMenu systemRepairMenu; // reference to the system repair menu (used for opening the menu when interacting with a system interactable)

    [Header("Crosshair")]
    [SerializeField] private Image crosshair;
    [SerializeField] private Sprite interactCrosshair;
    private Sprite defaultCrosshair;

    [Header("Time")]
    [SerializeField] private TMP_Text timeText;

    private void Start() {

        // hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        systemRepairMenu = FindFirstObjectByType<SystemRepairMenu>();
        timeManager = FindFirstObjectByType<TimeManager>();

        primaryBackpackUI.Initialize(); // initialize the primary backpack UI
        defaultCrosshair = crosshair.sprite;

        UpdateTimeText(timeManager.GetHour(), timeManager.GetMinute(), timeManager.IsAM());

    }

    private void Update() {

        if (Input.GetKeyDown(backpackKey))
            if (primaryBackpackUI.IsBackpackOpen()) // close backpack if it is open
                ClosePrimaryBackpack();
            else if (!IsMenuOpen()) // only open backpack if no other menu is open
                OpenPrimaryBackpack();

        // close backpack if escape is pressed and backpack is open
        if (Input.GetKeyDown(KeyCode.Escape) && primaryBackpackUI.IsBackpackOpen())
            ClosePrimaryBackpack();

        UpdateTimeText(timeManager.GetHour(), timeManager.GetMinute(), timeManager.IsAM());

    }

    public void OpenPrimaryBackpack() {

        if (IsMenuOpen() || primaryBackpackUI.IsBackpackOpen()) return; // do nothing if another menu is open or if the primary backpack is already open
        primaryBackpackUI.OpenBackpack(); // open the primary backpack UI

    }

    public void ClosePrimaryBackpack() {

        if (!primaryBackpackUI.IsBackpackOpen()) return; // do nothing if the primary backpack is not open
        primaryBackpackUI.CloseBackpack(); // close the primary backpack UI

    }

    public void OpenSystemRepairMenu() {

        if (IsMenuOpen() || systemRepairMenu.IsMenuOpen()) return; // do nothing if another menu is open or if the system repair menu is already open
        systemRepairMenu.ShowMenu(); // open the system repair menu

    }

    public void CloseSystemRepairMenu() {

        if (!systemRepairMenu.IsMenuOpen()) return; // do nothing if the system repair menu is not open
        systemRepairMenu.HideMenu(); // close the system repair menu

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

    public bool IsPrimaryBackpackOpen() => primaryBackpackUI.IsBackpackOpen();

    public bool IsMenuOpen() => primaryBackpackUI.IsBackpackOpen() || systemRepairMenu.IsMenuOpen();

}

public enum CrosshairType {

    Default,
    Interact

}
