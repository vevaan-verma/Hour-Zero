using System.Collections;
using UnityEngine;

public class ItemInteractable : Interactable {

    [Header("References")]
    [SerializeField] private Item item;

    [Header("Settings")]
    [SerializeField, Min(1)] private int itemCount;
    [SerializeField, Tooltip("Whether to drop the remainder of the item stack or keep the remainder in the interactable if the backpack is full")] private bool dropRemainder; // make sure this value is applied to the prefab because after dropping the item, the prefab's setting will be used for future interactions
    [SerializeField] private float destroyDuration;
    private int currCount;
    private bool destroyed; // flag to prevent multiple destruction calls

    private new void Start() {

        base.Start();
        currCount = itemCount;

    }

    public override bool Interact() {

        if (!base.Interact() || destroyed) return false; // if the base interaction fails or the interactable is already destroyed, do not proceed

        // if the backpack cannot hold even one item stack of the interactable item, do not proceed with the interaction
        if (!backpack.CanAddFullItemStacks(new ItemStack[] { new ItemStack(item, 1) }))
            return false;

        ItemStack stackToAdd = new ItemStack(item, itemCount);

        if (dropRemainder) { // check if we should drop the remainder of the item stack

            backpack.AddItemStack(stackToAdd, true); // add the item stack to the backpack and drop the remainder if the backpack is full
            currCount = 0; // update the current count of the item interactable to zero since we dropped the entire stack
            StartCoroutine(HandleDestruction()); // start the destruction coroutine to destroy the interactable

        } else {

            int remainder = backpack.AddItemStack(stackToAdd, false);
            currCount = remainder; // update the current count of the item interactable

            // destroy the item interactable if the count reaches zero
            if (currCount <= 0)
                StartCoroutine(HandleDestruction());

        }

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
