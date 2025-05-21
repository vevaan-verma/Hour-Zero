using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HotbarSlot : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Image fillImage;
    [SerializeField] private Image icon;
    private Coroutine selectCoroutine;

    [Header("Slot")]
    private ItemProperties slotItem;

    [Header("Selection")]
    [SerializeField] private Color selectedColor;
    [SerializeField] private float selectFadeDuration;
    private Color startColor;
    private bool isSelected;

    private void Awake() => startColor = fillImage.color;

    public void SetSlotItem(ItemProperties item) {

        slotItem = item;
        icon.sprite = item.GetIcon();

    }

    public void SetSelected(bool selected) {

        isSelected = selected;

        if (selectCoroutine != null) StopCoroutine(selectCoroutine); // stop any active fade coroutines
        selectCoroutine = StartCoroutine(HandleColorFade(isSelected ? selectedColor : startColor));

    }

    private IEnumerator HandleColorFade(Color targetColor) {

        float elapsedTime = 0f;

        while (elapsedTime < selectFadeDuration) {

            fillImage.color = Color.Lerp(startColor, targetColor, elapsedTime / selectFadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;

        }

        fillImage.color = targetColor;
        selectCoroutine = null;

    }

    public bool IsEmpty() => slotItem == null;

    public ItemProperties GetItem() => slotItem;

}
