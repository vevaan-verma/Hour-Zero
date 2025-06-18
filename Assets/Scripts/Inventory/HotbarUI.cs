using System.Collections;
using TMPro;
using UnityEngine;

public class HotbarUI : InventoryUI {

    [Header("References")]
    private Hotbar hotbar;
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
        hotbar.onSlotSelected += RefreshInventory; // subscribe to the slot selected event to refresh the inventory UI when a slot is selected

        base.Initialize();

    }

    public override void RefreshInventory() {

        base.RefreshInventory();

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
