using System.Collections;
using System.Linq;
using UnityEngine;

public class SystemRepairMenu : MonoBehaviour {

    [Header("References")]
    [SerializeField] private RepairInventory repairInventory;

    [Header("UI References")]
    [SerializeField] private CanvasGroup menuPanel;
    [SerializeField] private RepairInventoryUI repairInventoryUI; // reference to the repair inventory UI
    [SerializeField] private Slot repairSlot;
    private bool isMenuOpen;
    private Coroutine fadeCoroutine;
    private BackpackUI repairBackpackUI; // reference to the backpack UI used for repairing systems

    [Header("Settings")]
    [SerializeField] private float menuFadeDuration;

    private void Start() {

        repairBackpackUI = FindObjectsByType<BackpackUI>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .FirstOrDefault(ui => ui.GetBackpackType() == BackpackType.Repair); // find the repair backpack UI
        menuPanel.gameObject.SetActive(false); // make sure the menu is hidden by default

    }

    public void ShowMenu() {

        isMenuOpen = true; // set the menu state to open
        menuPanel.gameObject.SetActive(true); // make sure the menu is active
        repairBackpackUI.OpenInventory(); // open the backpack UI for repairing systems (do this after starting the coroutine to ensure the menu is active)
        repairInventoryUI.OpenInventory(); // open the repair inventory UI

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine); // stop any ongoing fade coroutine
        fadeCoroutine = StartCoroutine(Fade(menuPanel, 1f, menuFadeDuration)); // fade in the menu

    }

    public void HideMenu() {

        isMenuOpen = false; // set the menu state to closed
        repairBackpackUI.CloseInventory(); // close the backpack UI
        repairInventoryUI.CloseInventory(); // close the repair inventory UI

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine); // stop any ongoing fade coroutine
        fadeCoroutine = StartCoroutine(Fade(menuPanel, 0f, menuFadeDuration)); // fade out the menu

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
