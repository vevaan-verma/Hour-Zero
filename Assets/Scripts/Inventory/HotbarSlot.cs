using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HotbarSlot : Slot {

    [Header("References")]
    private Image image;
    private Coroutine fadeCoroutine;

    [Header("Settings")]
    [SerializeField] private Color selectedColor;
    [SerializeField] private float fadeDuration;
    private Color initialColor;
    private bool isSelected;

    public override void Initialize(Inventory inventory, int index, Item item, int count) {

        base.Initialize(inventory, index, item, count);

        image = GetComponent<Image>();
        initialColor = image.color; // store the initial color of the slot

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