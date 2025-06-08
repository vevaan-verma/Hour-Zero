using System.Collections;
using TMPro;
using UnityEngine;

public class Hotbar : MonoBehaviour {

    [Header("References")]
    private PlayerController playerController;

    [Header("Slots")]
    [SerializeField] private HotbarSlot slotPrefab;
    [SerializeField][Range(0, 9)] private int slotCount;
    private int currSlotIndex;
    private HotbarSlot[] slots;

    [Header("Item Text")]
    [SerializeField] private TMP_Text itemText;
    [SerializeField] private float itemTextFadeDelayDuration;
    [SerializeField] private float itemTextFadeDuration;
    private Coroutine itemTextFadeCoroutine;

    private void Start() {

        playerController = FindFirstObjectByType<PlayerController>();

        // destroy all children
        for (int i = transform.childCount - 1; i >= 0; i--)
            if (transform.GetChild(i).GetComponent<HotbarSlot>()) // make sure the child is a hotbar slot
                Destroy(transform.GetChild(i).gameObject);

        slots = new HotbarSlot[slotCount];

        for (int i = 0; i < slotCount; i++) {

            slots[i] = Instantiate(slotPrefab, transform);
            slots[i].name = "Slot" + (i + 1);

        }

        SelectSlot(0); // select first slot

    }

    public void AddItem(Item item) {

        for (int i = 0; i < slots.Length; i++) {

            if (slots[i].IsEmpty()) {

                slots[i].SetSlotItem(item);
                SelectSlot(currSlotIndex); // reselect the current slot to update the holding item
                return;

            }
        }

        // if all slots are full, replace the current slot with the new item
        slots[currSlotIndex].SetSlotItem(item);
        SelectSlot(currSlotIndex); // reselect the current slot to update the holding item

        // TODO: drop the replaced item

    }

    public void SelectSlot(int slotIndex) { // slot index is the number key that was clicked minus one

        if (slotIndex < 0 || slotIndex >= slots.Length) return; // make sure slot index is within bounds

        //if (slots[slotIndex].IsEmpty()) return; // make sure slot is not empty (no item in slot)

        //// if the same slot is clicked, deselect it
        //if (currSlotIndex == slotIndex) {

        //    slots[currSlotIndex].SetSelected(false);
        //    playerController.RemoveHoldingItem();
        //    currSlotIndex = -1;
        //    return;

        //}

        // deselect current slot and remove holding item
        slots[currSlotIndex].SetSelected(false);
        playerController.RemoveHoldingItem();

        // if slot is not empty, hold item
        if (!slots[currSlotIndex].IsEmpty())
            playerController.SetHoldingItem(slots[currSlotIndex].GetItem());

        // select new slot
        currSlotIndex = slotIndex;
        slots[currSlotIndex].SetSelected(true);
        SetItemText(); // set the item text to the current slot item

    }

    public void CycleSlot(int cycleAmount) {

        int slotIndex = currSlotIndex;
        slotIndex = (slotIndex + cycleAmount) % slots.Length; // cycle the slot index forward
        slotIndex = slotIndex < 0 ? slots.Length - 1 : slotIndex; // if the index is negative, set it to the last index

        SelectSlot(slotIndex); // select the new slot

    }

    private void SetItemText() {

        if (!slots[currSlotIndex].IsEmpty()) {

            itemText.text = slots[currSlotIndex].GetItem().GetName();

            if (itemTextFadeCoroutine != null) StopCoroutine(itemTextFadeCoroutine); // stop any active fade coroutines
            itemTextFadeCoroutine = StartCoroutine(HandleItemTextFade());

        } else {

            itemText.text = ""; // clear the text
            itemText.gameObject.SetActive(false); // hide the text if the slot is empty

            if (itemTextFadeCoroutine != null) StopCoroutine(itemTextFadeCoroutine); // stop any active fade coroutines
            itemTextFadeCoroutine = null; // reset the coroutine reference

        }
    }

    private IEnumerator HandleItemTextFade() {

        CanvasGroup canvasGroup = itemText.GetComponent<CanvasGroup>();

        // fade in the item text
        itemText.gameObject.SetActive(true); // show the text
        canvasGroup.alpha = 0f; // set alpha to 0 to start the fade in

        float elapsedTime = 0f;

        while (elapsedTime < itemTextFadeDuration) {

            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / itemTextFadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;

        }

        canvasGroup.alpha = 1f; // set alpha to 1

        yield return new WaitForSeconds(itemTextFadeDelayDuration); // wait for the delay duration

        // fade out the item text
        elapsedTime = 0f;

        while (elapsedTime < itemTextFadeDuration) {

            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / itemTextFadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;

        }

        canvasGroup.alpha = 0f; // set alpha to 0
        itemText.gameObject.SetActive(false); // hide the text after fading out
        itemTextFadeCoroutine = null;

    }

    public int GetSlotCount() => slotCount;

}
