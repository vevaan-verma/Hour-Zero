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

    public override void Interact() {

        if (destroyed) return; // if already destroyed, do nothing

        // if the interactable requires a held item, check if the player is holding the required item and enough of it
        if (requireHeldItem) {

            ItemStack selectedItemStack = hotbar.GetSelectedItemStack(); // get the item stack in the currently selected hotbar slot

            // if the required item is not held or not enough of it is held, don't follow through with the interaction
            if (selectedItemStack.GetItem() == null || !selectedItemStack.GetItem().Equals(requiredHeldItem.GetItem()))
                return;

            // if the held item should be consumed, use the backpack inventory to remove as much of the item stack from the current selected hotbar slot as possible, then remove the remainder as normal
            if (consumeHeldItem)
                backpack.RemoveItemStack(new ItemStack(requiredHeldItem.GetItem(), requiredHeldItem.GetCount()), hotbar.GetSelectedIndex());

        }

        // if the interactable required items in the backpack, check if the player has those items and enough of them
        if (requireBackpackItems) {

            foreach (ItemStack requiredStack in requiredBackpackItems) {

                // if the backpack does not contain the required item stack, don't follow through with the interaction
                if (!backpack.ContainsItemStack(requiredStack))
                    return;

                // if the backpack items should be consumed, remove the required amount from the backpack
                if (consumeBackpackItems)
                    backpack.RemoveItemStack(new ItemStack(requiredStack.GetItem(), requiredStack.GetCount()));

            }
        }

        int remainder = backpack.AddItemStack(new ItemStack(item, itemCount)); // add the item stack to the backpack

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
