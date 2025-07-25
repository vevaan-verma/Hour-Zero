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
    private TradeData currTradeData; // current trade data being processed
    private Coroutine fadeCoroutine;

    [Header("UI References")]
    [SerializeField] private CanvasGroup menuPanel;
    [SerializeField] private TradeInventoryUI tradeInventoryUI;
    [SerializeField] private Button tradeButton;
    [SerializeField] private Button closeMenuButton;
    private BackpackUI tradeBackpackUI; // reference to the backpack UI used for trading
    private bool isMenuOpen;

    [Header("Settings")]
    [SerializeField] private float menuFadeDuration;

    private void OnEnable() {

        tradeInventory = FindFirstObjectByType<TradeInventory>();
        tradeInventory.onContentsUpdated += CheckTradeRequirements;

    }

    private void Start() {

        backpack = FindFirstObjectByType<Backpack>(FindObjectsInactive.Include);
        tradeBackpackUI = FindObjectsByType<BackpackUI>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(ui => ui.GetBackpackType() == BackpackType.Trade); // find the trade backpack UI
        alertManager = FindFirstObjectByType<AlertManager>();
        uiManager = FindFirstObjectByType<UIManager>();

        tradeButton.onClick.AddListener(ProcessTrade); // add listener to trade button; call the ConductTrade method to process the trade
        closeMenuButton.onClick.AddListener(() => uiManager.CloseTradeMenu()); // add listener to close menu button; call the UIManager method to close the trade menu rather than this class directly to ensure the extra logic is executed (e.g., re-opening the hotbar UI)

        menuPanel.gameObject.SetActive(false); // make sure the menu is hidden by default

    }

    private void OnDisable() => tradeInventory.onContentsUpdated -= CheckTradeRequirements; // unsubscribe from the onContentsUpdated event to prevent memory leaks

    public void OpenMenu(TradeData tradeData) {

        this.currTradeData = tradeData; // set the current trade data for this trade menu

        isMenuOpen = true; // set the menu state to open
        menuPanel.gameObject.SetActive(true); // make sure the menu is active

        tradeButton.interactable = false; // disable the trade button initially until the trade requirements are checked

        tradeInventory.Initialize(tradeData); // initialize the trade inventory with the required stacks
        tradeInventoryUI.FillOutputSlots(); // fill the output slots in the trade inventory UI based on the trade data
        tradeBackpackUI.OpenInventory(); // open the backpack UI for trading (do this after starting the coroutine to ensure the menu is active)
        tradeInventoryUI.OpenInventory(); // open the trade inventory UI

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine); // stop any ongoing fade coroutine
        fadeCoroutine = StartCoroutine(Fade(menuPanel, 1f, menuFadeDuration)); // fade in the menu

    }

    public void CloseMenu(bool tradeRequirementsMet) {

        // if the trade requirements are not met, return all the items in the trade inventory back to the backpack
        if (!tradeRequirementsMet)
            ReturnAllItems();

        isMenuOpen = false; // set the menu state to closed
        tradeBackpackUI.CloseInventory(); // close the backpack UI
        tradeInventoryUI.CloseInventory(); // close the trade inventory UI

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine); // stop any ongoing fade coroutine
        fadeCoroutine = StartCoroutine(Fade(menuPanel, 0f, menuFadeDuration)); // fade out the menu

    }

    // checks if the trade requirements are met, i.e., if all the required input item stacks are present in the trade inventory
    private void CheckTradeRequirements() {

        ItemStack[] inputStacks = currTradeData.GetInputItemStacks(); // get the input stacks for the trade

        foreach (ItemStack stack in inputStacks) {

            if (!tradeInventory.ContainsItemStack(stack)) { // check if the trade inventory contains the required input item stack

                tradeButton.interactable = false; // if any required stack is missing, disable the trade button to prevent trading
                return;

            }
        }

        // at this point, all required stacks are present for trading

        tradeButton.interactable = true; // enable the trade button to allow the player to conduct the trade

    }

    private void ProcessTrade() {

        // lock the slots in the trade inventory to prevent further modifications while the trade is being processed
        foreach (Slot slot in tradeInventoryUI.GetInventorySlots())
            slot.SetLocked(true);

        // unlock the slots in the trade inventory after the trade is processed
        foreach (Slot slot in tradeInventoryUI.GetInventorySlots())
            slot.SetLocked(false);

        uiManager.CloseTradeMenu(true); // close the menu when the trade inventory is full (with a flag that the trade requirements were met); don't call CloseMenu() directly to ensure the UIManager logic is executed (e.g., re-opening the hotbar UI)

        ItemStack[] outputStacks = currTradeData.GetOutputItemStacks(); // get the output stacks for the trade

        // TODO: replace with item dropping logic if the player does not have enough space in their backpack to conduct the trade
        if (!backpack.CanAddFullItemStacks(outputStacks)) {

            // TODO: send player a notification that they don't have enough space in their backpack to conduct the trade
            ReturnAllItems(); // return all items in the trade inventory back to the backpack because the trade cannot be conducted
            alertManager.SendAlert(new Alert("Not enough space in backpack to conduct trade.", AlertType.Failure));
            return; // if the backpack cannot hold the output stacks, do not proceed with the trade

        }

        foreach (ItemStack outputStack in outputStacks)
            backpack.AddItemStack(outputStack); // no need to store the remainder since we are guaranteed to have enough space in the backpack

        tradeInventory.Clear();

        // TODO: notify the player that the trade was successful? phone notification?
        alertManager.SendAlert(new Alert("Trade successful!", AlertType.Success));

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
