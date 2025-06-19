using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : InventoryUI {

    [Header("References")]
    private Hotbar hotbar;
    private Backpack backpack;
    private Coroutine hotbarFadeCoroutine;
    private Coroutine selectedToolTextFadeCoroutine;

    [Header("UI References")]
    [SerializeField] private TMP_Text selectedToolText;

    [Header("Settings")]
    [SerializeField] private float hotbarFadeDuration;
    [SerializeField] private float selectedTextFadeDuration;

    public override void Initialize() {

        inventory = FindFirstObjectByType<Hotbar>(FindObjectsInactive.Include); // find the hotbar in the scene
        hotbar = (Hotbar) inventory; // cast the inventory to a hotbar
        backpack = FindFirstObjectByType<Backpack>(FindObjectsInactive.Include); // find the backpack in the scene (needed because the hotbar is a special case of the backpack as it is the top row of the backpack)
        uiPanel.gameObject.SetActive(inventory.IsVisibleByDefault()); // set the UI panel to be active by default if the inventory is visible by default
        hotbar.onSlotSelected += RefreshInventory; // subscribe to the slot selected event to refresh the inventory UI when a slot is selected
        backpack.onContentsUpdated += RefreshInventory; // subscribe to the inventory's contents update event to refresh the UI when the contents change; use the backpack's event since the hotbar is a part of the backpack (the top row)

        RefreshInventory();

        // don't call base.Initialize() here as the hotbar is a special case that requires the backpack as the inventory, so the versatile base class Initialize() method would not work correctly

    }

    private void OnDisable() {

        hotbar.onSlotSelected -= RefreshInventory; // unsubscribe from the slot selected event to avoid memory leaks
        backpack.onContentsUpdated -= RefreshInventory; // unsubscribe from the inventory's contents update event to avoid memory leaks

    }

    public override void RefreshInventory() {

        if (!uiPanel.gameObject.activeSelf) return; // don't refresh the inventory if the UI is not active (special check since the hotbar UI is based on the backpack UI, so the hotbar UI could be forced to refresh when it is inactive due to the backpack being active/updated)

        RectTransform rectTransform = slotPrefab.GetComponent<RectTransform>();
        inventoryContents.cellSize = new Vector2(rectTransform.rect.width, rectTransform.rect.height); // set the cell size of the grid layout group to match the size of the slot prefab
        inventoryContents.constraint = GridLayoutGroup.Constraint.FixedRowCount; // set the constraint to fixed row count to ensure the hotbar is displayed in a single row
        inventoryContents.constraintCount = 1; // set the number of columns in the inventory contents grid layout group to 1 (since the hotbar is a single row)

        inventorySlots = new Slot[inventory.GetCurrentSlotCount()];

        // delete all existing slots in the inventory contents
        foreach (Transform child in inventoryContents.transform)
            Destroy(child.gameObject);

        // instantiate the slots based on the current capacity of the inventory
        for (int i = 0; i < inventorySlots.Length; i++) {

            Slot slot = Instantiate(slotPrefab, inventoryContents.transform);
            slot.transform.name = $"Slot{i + 1}";
            ItemStack itemStack = backpack.GetItemStack(i); // get the item stack from the backpack at the corresponding index (because the hotbar is the top row of the backpack)
            slot.Initialize(inventory, i, itemStack.GetItem(), itemStack.GetCount(), showItemInfoWidgetOnHover); // initialize the slot
            inventorySlots[i] = slot; // store the slot in the array for later reference

        }

        // update the slots in the hotbar UI
        for (int i = 0; i < inventorySlots.Length; i++) {

            HotbarSlot slot = (HotbarSlot) inventorySlots[i]; // cast the slot to a hotbar slot

            if (i == hotbar.GetSelectedIndex()) {

                slot.SelectSlot(); // select the slot if it is the currently selected one

                if (slot.GetItem() != null) {

                    selectedToolText.text = slot.GetItem().GetName(); // update the selected tool text to show the name of the item in the selected slot

                    if (selectedToolTextFadeCoroutine != null) StopCoroutine(selectedToolTextFadeCoroutine); // stop any existing selected tool text fade coroutine
                    selectedToolTextFadeCoroutine = StartCoroutine(Fade(selectedToolText.GetComponent<CanvasGroup>(), 1f, selectedTextFadeDuration)); // start the fade in coroutine for the selected tool text

                } else {

                    selectedToolText.text = ""; // clear the selected tool text if the slot is empty

                    if (selectedToolTextFadeCoroutine != null) StopCoroutine(selectedToolTextFadeCoroutine); // stop any existing selected tool text fade coroutine
                    selectedToolTextFadeCoroutine = StartCoroutine(Fade(selectedToolText.GetComponent<CanvasGroup>(), 0f, selectedTextFadeDuration)); // start the fade out coroutine for the selected tool text

                }
            } else {

                slot.DeselectSlot(); // deselect the slot if it is not the currently selected one

            }
        }
    }

    public override void OpenInventory() {

        if (hotbarFadeCoroutine != null) StopCoroutine(hotbarFadeCoroutine); // stop any existing inventory fade coroutine
        hotbarFadeCoroutine = StartCoroutine(Fade(uiPanel, 1f, hotbarFadeDuration)); // start the inventory fade in coroutine

        RefreshInventory(); // refresh the hotbar inventory UI to ensure it is up to date before opening; place this after the fade in coroutine to ensure the UI panel is active before refreshing (if it is not active, the inventory will not be refreshed)

    }

    public override void CloseInventory() {

        if (hotbarFadeCoroutine != null) StopCoroutine(hotbarFadeCoroutine); // stop any existing inventory fade coroutine
        hotbarFadeCoroutine = StartCoroutine(Fade(uiPanel, 0f, hotbarFadeDuration)); // start the inventory fade out coroutine

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

    }
}
