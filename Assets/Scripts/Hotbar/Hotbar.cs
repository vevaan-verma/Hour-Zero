using UnityEngine;

public class Hotbar : MonoBehaviour {

    [Header("References")]
    private PlayerController playerController;

    [Header("Slots")]
    [SerializeField] private HotbarSlot slotPrefab;
    [SerializeField][Range(0, 9)] private int slotCount;
    private int currSlotIndex;
    private HotbarSlot[] slots;

    private void Start() {

        playerController = FindFirstObjectByType<PlayerController>();

        slots = new HotbarSlot[slotCount];

        for (int i = 0; i < slotCount; i++) {

            slots[i] = Instantiate(slotPrefab, transform);
            slots[i].name = "Slot " + (i + 1);

        }

        SelectSlot(0); // select first slot

    }

    public void AddItem(ItemProperties itemProperties) {

        for (int i = 0; i < slots.Length; i++) {

            if (slots[i].IsEmpty()) {

                slots[i].SetSlotItem(itemProperties);
                SelectSlot(currSlotIndex); // reselect the current slot to update the holding item
                break;

            }
        }
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

    }

    public void CycleSlot(int cycleAmount) {

        // deselect current slot and remove holding item
        slots[currSlotIndex].SetSelected(false);
        playerController.RemoveHoldingItem();

        currSlotIndex = (currSlotIndex + cycleAmount) % slots.Length; // cycle the slot index forward
        currSlotIndex = currSlotIndex < 0 ? slots.Length - 1 : currSlotIndex; // if the index is negative, set it to the last index

        // if slot is not empty, hold item
        if (!slots[currSlotIndex].IsEmpty())
            playerController.SetHoldingItem(slots[currSlotIndex].GetItem());

        // select new slot
        slots[currSlotIndex].SetSelected(true);

    }

    public int GetSlotCount() => slotCount;

}
