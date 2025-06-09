using UnityEngine;

public class Backpack : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Slot slotPrefab;
    [SerializeField] private Transform backpackContents;
    private Slot[] backpackSlots;
    private AlertManager alertManager;

    [Header("Settings")]
    [SerializeField] private int initialBackpackCapacity;
    private int currBackpackCapacity;

    private void Start() {

        alertManager = FindFirstObjectByType<AlertManager>();
        backpackSlots = new Slot[initialBackpackCapacity];

        currBackpackCapacity = initialBackpackCapacity;
        InitializeBackpack();

    }

    private void InitializeBackpack() {

        // delete all existing slots in the backpack contents
        foreach (Transform child in backpackContents)
            Destroy(child.gameObject);

        // instantiate the initial number of backpack slots
        for (int i = 0; i < currBackpackCapacity; i++) {

            Slot slot = Instantiate(slotPrefab, backpackContents);
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
        foreach (Slot slot in backpackSlots) {

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

        alertManager.SendAlert(new Alert($"Backpack is full! Could not add {count}x {item.GetName()} to backpack", AlertType.Failure)); // send alert to UI manager
        return count; // return the count of items that could not be added

    }

    // returns the amount of items that were removed from the backpack
    public int RemoveItem(Item item, int count) {

        int removedCount = 0; // initialize the count of removed items

        // try to remove the item from the later slots first
        for (int i = backpackSlots.Length - 1; i >= 0; i--) {

            Slot slot = backpackSlots[i];

            if (slot.HasItem() && slot.GetItem().Equals(item)) {

                removedCount += slot.RemoveItem(item, count - removedCount); // remove the remaining count from the slot
                if (count <= 0) return removedCount; // if all items were removed, exit the method

            }
        }

        return removedCount; // return the total count of items that were removed

    }

    public bool ContainsItems(Item item, int count) {

        int totalCount = 0;

        // check if the item exists in the backpack and has enough count
        foreach (Slot slot in backpackSlots) {

            if (slot.HasItem() && slot.GetItem().Equals(item)) {

                totalCount += slot.GetCount();

                if (totalCount >= count) // if the total count is greater than or equal to the requested count, exit early (optimization)
                    return true; // return true as we have enough items

            }
        }

        return false; // return false if we did not find enough items in the backpack

    }
}
