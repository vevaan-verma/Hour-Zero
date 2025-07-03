using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class SystemRepairMenu : MonoBehaviour {

    [Header("References")]
    private RepairInventory repairInventory;
    private Backpack backpack;
    private BunkerManager bunkerManager;
    private AlertManager alertManager;

    [Header("UI References")]
    [SerializeField] private CanvasGroup menuPanel;
    [SerializeField] private RepairInventoryUI repairInventoryUI; // reference to the repair inventory UI
    [SerializeField] private Button closeRepairMenuButton;
    private BackpackUI repairBackpackUI; // reference to the backpack UI used for repairing systems
    private bool isMenuOpen;
    private Coroutine fadeCoroutine;

    [Header("Settings")]
    [SerializeField] private float menuFadeDuration;

    private void Start() {

        repairInventory = FindFirstObjectByType<RepairInventory>();
        backpack = FindFirstObjectByType<Backpack>(FindObjectsInactive.Include);
        repairBackpackUI = FindObjectsByType<BackpackUI>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(ui => ui.GetBackpackType() == BackpackType.Repair); // find the repair backpack UI
        bunkerManager = FindFirstObjectByType<BunkerManager>();
        alertManager = FindFirstObjectByType<AlertManager>();

        closeRepairMenuButton.onClick.AddListener(() => CloseMenu()); // add listener to close menu button

        menuPanel.gameObject.SetActive(false); // make sure the menu is hidden by default

    }

    public void OpenMenu(ItemStack[] repairStacks, int repairPercent, BunkerSystemType systemType) {

        isMenuOpen = true; // set the menu state to open
        menuPanel.gameObject.SetActive(true); // make sure the menu is active

        repairInventory.Initialize(repairStacks, repairPercent, systemType); // initialize the repair inventory with the required stack, slot count, and repair percent
        repairBackpackUI.OpenInventory(); // open the backpack UI for repairing systems (do this after starting the coroutine to ensure the menu is active)
        repairInventoryUI.OpenInventory(); // open the repair inventory UI

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine); // stop any ongoing fade coroutine
        fadeCoroutine = StartCoroutine(Fade(menuPanel, 1f, menuFadeDuration)); // fade in the menu

    }

    public void CloseMenu(bool repairRequirementsMet = false) {

        // if the repair requirements are not met, return all the items in the repair inventory back to the backpack
        if (!repairRequirementsMet) {

            List<ItemStack> itemsToReturn = repairInventory.GetContents(); // get all the items in the repair inventory

            foreach (ItemStack itemStack in itemsToReturn)
                if (itemStack.GetItem() != null) // check if the item is not null
                    backpack.AddItemStack(itemStack); // add the item stack back to the repair backpack

        }

        isMenuOpen = false; // set the menu state to closed
        repairBackpackUI.CloseInventory(); // close the backpack UI
        repairInventoryUI.CloseInventory(); // close the repair inventory UI

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine); // stop any ongoing fade coroutine
        fadeCoroutine = StartCoroutine(Fade(menuPanel, 0f, menuFadeDuration)); // fade out the menu

    }

    // when the repair requirements are met, the player has put all the necessary items in the repair inventory to repair the system
    public void OnRepairRequirementsMet(int repairPercent, BunkerSystemType systemType) {

        CloseMenu(true); // close the menu when the repair inventory is full (with a flag that the repair requirements were met)
        bunkerManager.RepairSystem(systemType, repairPercent); // repair the system using the bunker manager
        repairInventory.Clear();

        string formattedSystemType = Regex.Replace(systemType.ToString(), "(\\B[A-Z])", " $1").ToLower(); // format the system type to be more readable by adding spaces in between the words (e.g., "AirFiltration" -> "Air Filtration") and convert to lowercase
        alertManager.SendAlert(new Alert($"System {formattedSystemType} durability repaired ({repairPercent}%)", AlertType.Success));

    }

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

        fadeCoroutine = null; // reset the coroutine reference

    }

    public bool IsMenuOpen() => isMenuOpen;

}
