using System.Collections;
using UnityEngine;

public class ItemInteractable : Interactable {

    [Header("References")]
    [SerializeField] private Item item;

    [Header("Settings")]
    [SerializeField, Min(1)] private int itemCount;
    [SerializeField] private float destroyDuration;
    private int currCount;
    private bool destroyed; // flag to prevent multiple destruction calls

    private new void Start() {

        base.Start();
        currCount = itemCount;

    }

    public override bool Interact() {

        if (!base.Interact() || destroyed) return false; // if the base interaction fails or the interactable is already destroyed, do not proceed

        int remainder = backpack.AddItemStack(new ItemStack(item, itemCount)); // add the item stack to the backpack

        currCount = remainder; // update the current count of the item interactable

        // destroy the item interactable if the count reaches zero
        if (currCount <= 0)
            StartCoroutine(HandleDestruction());

        return true;

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
