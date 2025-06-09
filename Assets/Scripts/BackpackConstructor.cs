using UnityEngine;

public class BackpackConstructor : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Transform backpackContents;

    private void ConstructBackpack(Slot slotPrefab, int currBackpackCapacity) {

        // delete all existing slots in the backpack contents
        foreach (Transform child in backpackContents)
            Destroy(child.gameObject);

        // instantiate the initial number of backpack slots
        for (int i = 0; i < currBackpackCapacity; i++) {

            Slot slot = Instantiate(slotPrefab, backpackContents);
            slot.transform.name = $"Slot{i + 1}";
            slot.Initialize(); // initialize the slot

        }
    }
}
