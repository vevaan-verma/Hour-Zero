using System.Collections;
using UnityEngine;

public class ItemInteractable : Interactable {

    [Header("References")]
    [SerializeField] private Item item;
    private Backpack backpack;
    private Hotbar hotbar;

    [Header("Settings")]
    [SerializeField, Min(1)] private int itemCount;
    [SerializeField] private float destroyDuration;
    private int currCount;
    private bool destroyed; // flag to prevent multiple destruction calls

    private new void Start() {

        base.Start();

        backpack = FindFirstObjectByType<Backpack>();
        hotbar = FindFirstObjectByType<Hotbar>();
        currCount = itemCount;

    }

    public override void Interact() {

        if (destroyed) return; // if already destroyed, do nothing

        int remainder = hotbar.AddItemStack(new ItemStack(item, itemCount)); // try to add the item to the hotbar first
        remainder = backpack.AddItemStack(new ItemStack(item, remainder)); // then try to add the remainder to the backpack

        currCount = remainder; // update the current count of the item interactable

        // destroy the item interactable if the count reaches zero
        if (currCount <= 0)
            StartCoroutine(HandleDestruction());

    }

    private IEnumerator HandleDestruction() {

        destroyed = true; // set the flag to true to prevent multiple destruction calls

        float currentTime = 0f;
        Vector3 startScale = transform.localScale;

        while (currentTime < destroyDuration) {

            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, currentTime / destroyDuration);
            currentTime += Time.deltaTime;
            yield return null;

        }

        transform.localScale = Vector3.zero; // ensure the scale is exactly zero at the end
        Destroy(gameObject); // destroy the game object after scaling down

    }
}
