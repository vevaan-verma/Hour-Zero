using UnityEngine;

public class Backpack : MonoBehaviour {

    [Header("References")]
    [SerializeField] private BackpackSlot slotPrefab;
    [SerializeField] private Transform backpackContents;
    private BackpackSlot[] backpackSlots;
    private UIManager uiManager;

    [Header("Settings")]
    [SerializeField] private int initialBackpackCapacity;
    private int currBackpackCapacity;

    private void Start() {

        uiManager = FindFirstObjectByType<UIManager>();
        backpackSlots = new BackpackSlot[initialBackpackCapacity];

        currBackpackCapacity = initialBackpackCapacity;
        InitializeBackpack();

    }

    private void InitializeBackpack() {

        // delete all existing slots in the backpack contents
        foreach (Transform child in backpackContents)
            Destroy(child.gameObject);

        // instantiate the initial number of backpack slots
        for (int i = 0; i < currBackpackCapacity; i++) {

            BackpackSlot slot = Instantiate(slotPrefab, backpackContents);
            slot.transform.name = $"Slot{i + 1}";
            slot.Initialize(); // initialize the slot
            backpackSlots[i] = slot; // store the slot in the array for later reference

        }
    }

    // returns the amount of items that could not be added to the backpack
    public int AddItem(Item item, int count) {

        if (item == null) {

            Debug.LogWarning("Cannot add null item to backpack."); // log a warning if the item is null
            return 0; // return 0 to prevent adding null items

        }

        if (count <= 0) {

            Debug.LogWarning("Count must be greater than zero for adding to backpack."); // log a warning if the count is less than or equal to zero
            return 0; // return 0 to prevent adding invalid counts

        }

        // check if the item is already in the backpack, if so, stack it as much as possible
        foreach (BackpackSlot slot in backpackSlots) {

            if (slot.HasItem() && slot.GetItem().Equals(item)) {

                int remainder = slot.AddItem(item, count);
                if (remainder == 0) return 0; // item added successfully, exit the method and return that 0 items could not be added

                count = remainder; // update the count to the remainder that could not be added

            }
        }

        // find first available slot in the backpack
        for (int i = 0; i < backpackSlots.Length; i++) {

            if (!backpackSlots[i].HasItem()) {

                int remainder = backpackSlots[i].SetItem(item, count);
                if (remainder == 0) return 0; // item added successfully, exit the method and return that 0 items could not be added

                count = remainder; // update the count to the remainder that could not be added

            }
        }

        // if we reach here, it means no slots were available or the item could not be fully added

        uiManager.SendAlert($"Backpack is full! Could not add {count}x {item.GetName()} to backpack."); // send alert to UI manager
        return count; // return the count of items that could not be added

    }

    public void RemoveItem(Item item, int count) {

        // find the slot containing the item to remove
        for (int i = 0; i < backpackSlots.Length; i++) {

            if (backpackSlots[i].HasItem() && backpackSlots[i].GetItem() == item) {

                Destroy(backpackSlots[i].gameObject); // destroy the slot item holder
                backpackSlots[i] = null; // set the slot to null to indicate it's now empty
                return; // item removed successfully, exit the method

            }
        }

        Debug.LogWarning("Item not found in backpack: " + item.GetName()); // log a warning if the item is not found

    }
}
