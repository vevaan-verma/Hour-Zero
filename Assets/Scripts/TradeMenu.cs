using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TradeMenu : MonoBehaviour {

    [Header("References")]
    private TradeInventory tradeInventory;
    private Backpack backpack;
    private AlertManager alertManager;
    private UIManager uiManager;
    private Coroutine processTradeCoroutine;
    private Coroutine fadeCoroutine;

    [Header("UI References")]
    [SerializeField] private CanvasGroup menuPanel;
    [SerializeField] private TradeInventoryUI tradeInventoryUI;
    [SerializeField] private Button closeMenuButton;
    private BackpackUI tradeBackpackUI; // reference to the backpack UI used for trading
    private bool isMenuOpen;

    [Header("Settings")]
    [SerializeField] private float menuFadeDuration;
    [SerializeField, Min(0.1f)] private float tradeProcessDuration;

    private void Start() {

        tradeInventory = FindFirstObjectByType<TradeInventory>();
        backpack = FindFirstObjectByType<Backpack>(FindObjectsInactive.Include);
        tradeBackpackUI = FindObjectsByType<BackpackUI>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(ui => ui.GetBackpackType() == BackpackType.Trade); // find the trade backpack UI
        alertManager = FindFirstObjectByType<AlertManager>();
        uiManager = FindFirstObjectByType<UIManager>();

        closeMenuButton.onClick.AddListener(() => uiManager.CloseTradeMenu()); // add listener to close menu button; call the UIManager method to close the trade menu rather than this class directly to ensure the extra logic is executed (e.g., re-opening the hotbar UI)

        menuPanel.gameObject.SetActive(false); // make sure the menu is hidden by default

    }

    public void OpenMenu(TradeData tradeData) {

        isMenuOpen = true; // set the menu state to open
        menuPanel.gameObject.SetActive(true); // make sure the menu is active

        tradeInventory.Initialize(tradeData); // initialize the trade inventory with the required stacks
        tradeBackpackUI.OpenInventory(); // open the backpack UI for trading (do this after starting the coroutine to ensure the menu is active)
        tradeInventoryUI.OpenInventory(); // open the trade inventory UI

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine); // stop any ongoing fade coroutine
        fadeCoroutine = StartCoroutine(Fade(menuPanel, 1f, menuFadeDuration)); // fade in the menu

    }

    public void CloseMenu(bool tradeRequirementsMet) {

        // if the trade requirements are not met, return all the items in the trade inventory back to the backpack
        if (!tradeRequirementsMet)
            ReturnAllItems();

        if (processTradeCoroutine != null) {

            StopCoroutine(processTradeCoroutine); // stop any ongoing trade processing coroutine
            processTradeCoroutine = null; // reset the coroutine reference

        }

        isMenuOpen = false; // set the menu state to closed
        tradeBackpackUI.CloseInventory(); // close the backpack UI
        tradeInventoryUI.CloseInventory(); // close the trade inventory UI

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine); // stop any ongoing fade coroutine
        fadeCoroutine = StartCoroutine(Fade(menuPanel, 0f, menuFadeDuration)); // fade out the menu

    }

    // when the trade requirements are met, the player has put all the necessary items in the trade inventory to conduct the trade
    public void OnTradeRequirementsMet(TradeData tradeData) {

        // if a trade is already being processed, do not start another one
        if (processTradeCoroutine != null)
            return;

        processTradeCoroutine = StartCoroutine(ProcessTrade(tradeData)); // start the trade processing coroutine

    }

    private IEnumerator ProcessTrade(TradeData tradeData) {

        tradeInventoryUI.SetSlotsLocked(true); // lock the slots in the trade inventory to prevent further modifications while the trade is being processed

        yield return new WaitForSeconds(tradeProcessDuration); // simulate the trade processing time

        tradeInventoryUI.SetSlotsLocked(false); // unlock the slots in the trade inventory after the trade is processed

        uiManager.CloseTradeMenu(true); // close the menu when the trade inventory is full (with a flag that the trade requirements were met); don't call CloseMenu() directly to ensure the UIManager logic is executed (e.g., re-opening the hotbar UI)

        ItemStack[] outputStacks = tradeData.GetOutputItems(); // get the output stacks for the trade

        // TODO: replace with item dropping logic if the player does not have enough space in their backpack to conduct the trade
        if (!backpack.CanAddFullItemStacks(outputStacks)) {

            // TODO: send player a notification that they don't have enough space in their backpack to conduct the trade
            ReturnAllItems(); // return all items in the trade inventory back to the backpack because the trade cannot be conducted
            alertManager.SendAlert(new Alert("Not enough space in backpack to conduct trade.", AlertType.Failure));
            processTradeCoroutine = null; // reset the coroutine reference
            yield break; // if the backpack cannot hold the output stacks, do not proceed with the trade

        }

        foreach (ItemStack outputStack in outputStacks)
            backpack.AddItemStack(outputStack); // no need to store the remainder since we are guaranteed to have enough space in the backpack

        tradeInventory.Clear();

        // TODO: notify the player that the trade was successful? phone notification?
        alertManager.SendAlert(new Alert("Trade successful!", AlertType.Success));

        processTradeCoroutine = null; // reset the coroutine reference

    }

    private void ReturnAllItems() {

        List<ItemStack> itemsToReturn = tradeInventory.GetContents(); // get all the items in the trade inventory

        foreach (ItemStack itemStack in itemsToReturn)
            if (itemStack.GetItem() != null) // check if the item is not null
                backpack.AddItemStack(itemStack); // add the item stack back to the trade backpack

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
