using System.Collections;
using UnityEngine;

public class HotbarSlot : Slot {

    [Header("References")]
    private Coroutine fadeCoroutine;

    [Header("Settings")]
    [SerializeField] private Color selectedColor;
    [SerializeField] private float fadeDuration;
    private Color initialColor;
    private bool isSelected;

    public override void Initialize(Inventory inventory, InventoryUI inventoryUI, int index, ItemStack itemStack, bool showItemInfoWidgetOnHover, Color? slotColor = null) {

        base.Initialize(inventory, inventoryUI, index, itemStack, showItemInfoWidgetOnHover, slotColor);
        initialColor = image.color; // store the initial color of the slot; takes place after the base initialization to ensure the color is set correctly if a custom color is provided

    }

    public void SelectSlot() {

        if (isSelected) return; // if the slot is already selected, do nothing (slight optimization for when an already selected slot is selected again)

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine); // stop any existing fade coroutine
        fadeCoroutine = StartCoroutine(FadeColor(selectedColor, fadeDuration)); // start fading to the selected color

        isSelected = true; // mark the slot as selected

    }

    public void DeselectSlot() {

        if (!isSelected) return; // if the slot is already not selected, do nothing (slight optimization for when an already deselected slot is deselected again)

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine); // stop any existing fade coroutine
        fadeCoroutine = StartCoroutine(FadeColor(initialColor, fadeDuration)); // start fading back to the initial color

        isSelected = false; // mark the slot as not selected

    }

    private IEnumerator FadeColor(Color targetColor, float duration) {

        float currentTime = 0f;
        Color startAlpha = image.color;

        while (currentTime < duration) {

            currentTime += Time.deltaTime;
            image.color = Color.Lerp(startAlpha, targetColor, currentTime / duration);
            yield return null;

        }

        image.color = targetColor; // ensure final color is set

    }
}