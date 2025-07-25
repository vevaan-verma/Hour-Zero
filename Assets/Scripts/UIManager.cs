using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    [Header("Hotbar")]
    private HotbarUI hotbarUI;
    private NPCController currNPCController; // reference to the current NPC controller (used to tell the NPC to continue walking after the NPC menu is closed)

    [Header("Backpack")]
    [SerializeField] private float backpackFadeDuration;
    [SerializeField] private KeyCode backpackKey;
    private BackpackUI primaryBackpackUI; // this is the backpack UI used for the opening the backpack itself, not exchange menus

    [Header("System Repair")]
    private SystemRepairMenu systemRepairMenu; // reference to the system repair menu (used for opening the menu when interacting with a system interactable)

    [Header("NPC")]
    private NPCMenu npcMenu; // reference to the NPC menu (used for opening the menu when interacting with an NPC)

    [Header("Trade")]
    private TradeMenu tradeMenu; // reference to the trade menu (used for opening the menu when trading)

    [Header("Phone")]
    private PhoneManager phoneManager;

    [Header("Crosshair")]
    [SerializeField] private Image crosshair;
    [SerializeField] private Sprite defaultCrosshair;
    [SerializeField] private Sprite interactCrosshair;
    [SerializeField] private Sprite grabbableCrosshair;
    [SerializeField] private Sprite grabbingCrosshair;

    private void Start() {

        #region VALIDATION
        // make sure there is exactly one UI of each backpack type in the scene
        BackpackType[] backpackTypes = (BackpackType[]) System.Enum.GetValues(typeof(BackpackType));
        BackpackUI[] backpackUIs = FindObjectsByType<BackpackUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (BackpackType type in backpackTypes) {

            BackpackUI[] foundBackpacks = System.Array.FindAll(backpackUIs, ui => ui.GetBackpackType() == type);

            if (foundBackpacks.Length != 1) {

                Debug.LogError($"There should be exactly one BackpackUI of type {type} in the scene, found {foundBackpacks.Length}.");
                return;

            }
        }
        #endregion

        // hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        hotbarUI = FindFirstObjectByType<HotbarUI>();
        primaryBackpackUI = FindObjectsByType<BackpackUI>(FindObjectsSortMode.None).FirstOrDefault(ui => ui.GetBackpackType() == BackpackType.Primary); // find the primary backpack UI
        systemRepairMenu = FindFirstObjectByType<SystemRepairMenu>();
        npcMenu = FindFirstObjectByType<NPCMenu>();
        tradeMenu = FindFirstObjectByType<TradeMenu>();
        phoneManager = FindFirstObjectByType<PhoneManager>();

        crosshair.sprite = defaultCrosshair; // set the crosshair to the default crosshair at the start

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

    }

    public void OpenPrimaryBackpack() {

        if (IsMenuOpen()) return; // do nothing if a menu is open

        hotbarUI.CloseInventory(); // close the hotbar UI if it is open (this is done to ensure the hotbar is not visible when the primary backpack is open)
        crosshair.gameObject.SetActive(false); // hide the crosshair when the primary backpack is open
        primaryBackpackUI.OpenInventory(); // open the primary backpack UI

    }

    public void ClosePrimaryBackpack() {

        if (!primaryBackpackUI.IsInventoryOpen()) return; // do nothing if the primary backpack is not open

        hotbarUI.OpenInventory(); // re-open the hotbar UI
        crosshair.gameObject.SetActive(true); // show the crosshair when the primary backpack is closed
        primaryBackpackUI.CloseInventory(); // close the primary backpack UI

    }

    public void OpenSystemRepairMenu(ItemStack[] repairStacks, int repairPercent, BunkerSystemType systemType) {

        if (IsMenuOpen()) return; // do nothing if a menu is open

        hotbarUI.CloseInventory(); // close the hotbar UI if it is open (this is done to ensure the hotbar is not visible when the system repair menu is open)
        crosshair.gameObject.SetActive(false); // hide the crosshair when the system repair menu is open
        systemRepairMenu.OpenMenu(repairStacks, repairPercent, systemType); // open the system repair menu

    }

    public void CloseSystemRepairMenu(bool repairRequirementsMet = false) {

        if (!systemRepairMenu.IsMenuOpen()) return; // do nothing if the system repair menu is not open

        hotbarUI.OpenInventory(); // re-open the hotbar UI
        crosshair.gameObject.SetActive(true); // show the crosshair when the system repair menu is closed
        systemRepairMenu.CloseMenu(repairRequirementsMet); // close the system repair menu

    }

    public void OpenNPCMenu(NPCController npcController) {

        if (IsMenuOpen()) return; // do nothing if a menu is open

        this.currNPCController = npcController;

        Cursor.lockState = CursorLockMode.None; // unlock the cursor when the NPC menu is open
        Cursor.visible = true; // make the cursor visible when the NPC menu is open

        hotbarUI.CloseInventory(); // close the hotbar UI if it is open (this is done to ensure the hotbar is not visible when the NPC menu is open)
        crosshair.gameObject.SetActive(false); // hide the crosshair when the NPC menu is open
        npcMenu.OpenMenu(npcController.GetNPCData()); // open the NPC menu

    }

    public void CloseNPCMenu() {

        if (!npcMenu.IsMenuOpen()) return; // do nothing if the NPC menu is not open

        Cursor.lockState = CursorLockMode.Locked; // lock the cursor when the NPC menu is closed
        Cursor.visible = false; // hide the cursor when the NPC menu is closed

        hotbarUI.OpenInventory(); // re-open the hotbar UI
        crosshair.gameObject.SetActive(true); // show the crosshair when the NPC menu is closed
        npcMenu.CloseMenu(); // close the NPC menu

        currNPCController.OnEndInteraction(); // tell the NPC to continue walking after the NPC menu is closed

    }

    public void OpenTradeMenu(TradeData tradeData) {

        CloseNPCMenu();

        if (IsMenuOpen()) return; // do nothing if a menu is open

        hotbarUI.CloseInventory(); // close the hotbar UI if it is open (this is done to ensure the hotbar is not visible when the trade menu is open)
        crosshair.gameObject.SetActive(false); // hide the crosshair when the trade menu is open
        tradeMenu.OpenMenu(tradeData); // open the trade menu

    }

    public void CloseTradeMenu(bool tradeRequirementsMet = false) {

        if (!tradeMenu.IsMenuOpen()) return; // do nothing if the trade menu is not open

        hotbarUI.OpenInventory(); // re-open the hotbar UI
        crosshair.gameObject.SetActive(true); // show the crosshair when the trade menu is closed
        tradeMenu.CloseMenu(tradeRequirementsMet); // close the trade menu

    }

    public void OnPhoneStateCycle() {

        if (phoneManager.IsPhoneToFace()) {

            hotbarUI.CloseInventory(); // close the hotbar UI if the phone is to face
            crosshair.gameObject.SetActive(false); // hide the crosshair when the phone is to face

        } else {

            hotbarUI.OpenInventory(); // re-open the hotbar UI if the phone is not to face
            crosshair.gameObject.SetActive(true); // show the crosshair when the phone is not to face

        }
    }

    public void SetCrosshairType(CrosshairType type) {

        switch (type) {

            case CrosshairType.Default:
                crosshair.sprite = defaultCrosshair;
                break;

            case CrosshairType.Interact:
                crosshair.sprite = interactCrosshair;
                break;

            case CrosshairType.Grabbable:
                crosshair.sprite = grabbableCrosshair;
                break;

            case CrosshairType.Grabbing:
                crosshair.sprite = grabbingCrosshair;
                break;

            default:
                Debug.LogWarning($"Unknown crosshair type: {type}. Defaulting to default crosshair.");
                crosshair.sprite = defaultCrosshair;
                break;

        }
    }

    public bool IsMenuOpen() => primaryBackpackUI.IsInventoryOpen() || systemRepairMenu.IsMenuOpen() || npcMenu.IsMenuOpen() || tradeMenu.IsMenuOpen() || phoneManager.IsPhoneToFace();

}

public enum CrosshairType {

    Default,
    Interact,
    Grabbable,
    Grabbing

}
