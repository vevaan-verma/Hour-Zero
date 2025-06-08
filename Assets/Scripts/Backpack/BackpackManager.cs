using UnityEngine;

public class BackpackManager : MonoBehaviour {

    [Header("References")]
    [SerializeField] private GameObject backpackSlotPrefab;
    [SerializeField] private Transform backpackContents;

    [Header("Settings")]
    [SerializeField] private int initialBackpackSize;
    private int currBackpackSize;

    private void Start() {

        currBackpackSize = initialBackpackSize;
        InitializeBackpack();

    }

    private void InitializeBackpack() {

        // delete all existing slots in the backpack contents
        foreach (Transform child in backpackContents)
            Destroy(child.gameObject);

        // instantiate the initial number of backpack slots
        for (int i = 0; i < currBackpackSize; i++) {

            GameObject slot = Instantiate(backpackSlotPrefab, backpackContents);
            slot.name = $"Slot {i + 1}";

        }
    }

    public void AddItem() {


    }
}
