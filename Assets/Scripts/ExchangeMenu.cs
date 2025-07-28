using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class ExchangeMenu : Menu {

    [Header("References")]
    protected ExchangeInventory exchangeInventory;
    protected PhoneManager phoneManager;
    protected UIManager uiManager;
    protected Backpack backpack;
    protected ExchangeData currExchangeData; // current exchange data being processed
    private Coroutine fadeCoroutine;

    [Header("UI References")]
    [SerializeField] private CanvasGroup menuPanel;
    [SerializeField] private ExchangeInventoryUI exchangeInventoryUI;
    [SerializeField] private GameObject exchangeDivider;
    [SerializeField] private Button exchangeButton;
    [SerializeField] protected Button closeMenuButton;
    protected BackpackUI exchangeBackpackUI; // reference to the backpack UI used for the exchange
    private bool isMenuOpen;

    [Header("Settings")]
    [SerializeField] private float menuFadeDuration;

    // exchangeInventory must be set in derived classes before calling base.OnEnable()
    protected void OnEnable() => exchangeInventory.onContentsUpdated += CheckExchangeRequirements;

    protected void Start() {

        backpack = FindFirstObjectByType<Backpack>();
        phoneManager = FindFirstObjectByType<PhoneManager>();
        uiManager = FindFirstObjectByType<UIManager>();

        exchangeButton.onClick.AddListener(ProcessExchange); // add listener to exchange button

        menuPanel.gameObject.SetActive(false); // make sure the menu is hidden by default

    }

    private void OnDisable() => exchangeInventory.onContentsUpdated -= CheckExchangeRequirements; // unsubscribe from the onContentsUpdated event to prevent memory leaks

    public void OpenMenu(ExchangeData exchangeData) {

        this.currExchangeData = exchangeData;

        isMenuOpen = true; // set the menu state to open
        menuPanel.gameObject.SetActive(true); // make sure the menu is active

        exchangeDivider.SetActive(exchangeData.GetOutputItemStacks().Length > 0); // show the exchange divider if there are output item stacks in the exchange data
        exchangeButton.interactable = false; // disable the exchange button initially until the exchange requirements are checked

        exchangeInventory.Initialize(exchangeData); // initialize the exchange inventory with the exchange data
        exchangeInventoryUI.FillOutputSlots(); // fill the output slots in the exchange inventory UI based on the exchange data
        exchangeBackpackUI.OpenInventory(); // open the backpack UI for exchanging (do this after starting the coroutine to ensure the menu is active)
        exchangeInventoryUI.OpenInventory(); // open the exchange inventory UI

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine); // stop any ongoing fade coroutine
        fadeCoroutine = StartCoroutine(Fade(menuPanel, 1f, menuFadeDuration)); // fade in the menu

    }

    public void CloseMenu(bool exchangeRequirementsMet) {

        // if the exchange requirements are not met, return all the items in the exchange inventory back to the backpack
        if (!exchangeRequirementsMet)
            ReturnAllItems();

        isMenuOpen = false; // set the menu state to closed
        exchangeBackpackUI.CloseInventory(); // close the backpack UI
        exchangeInventoryUI.CloseInventory(); // close the exchange inventory UI

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine); // stop any ongoing fade coroutine
        fadeCoroutine = StartCoroutine(Fade(menuPanel, 0f, menuFadeDuration)); // fade out the menu

    }

    // checks if the exchange requirements are met
    private void CheckExchangeRequirements() {

        ItemStack[] inputStacks = currExchangeData.GetInputItemStacks(); // get the input stacks for the exchange

        foreach (ItemStack stack in inputStacks) {

            if (!exchangeInventory.ContainsItemStack(stack)) { // check if the exchange inventory contains the required input item stack

                exchangeButton.interactable = false; // if any required stack is missing, disable the exchange button to prevent exchanging
                return;

            }
        }

        // at this point, all required stacks are present for exchanging

        exchangeButton.interactable = true; // enable the exchange button to allow the player to conduct the traexchangede

    }

    private void ReturnAllItems() {

        List<ItemStack> itemsToReturn = exchangeInventory.GetContents(); // get all the items in the exchange inventory

        foreach (ItemStack itemStack in itemsToReturn)
            if (itemStack.GetItem() != null) // check if the item is not null
                backpack.AddItemStack(itemStack, true); // add the item stack back to the exchange backpack and drop the remainder if the backpack is full

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

    protected abstract void ProcessExchange();

    public bool IsMenuOpen() => isMenuOpen;

}
