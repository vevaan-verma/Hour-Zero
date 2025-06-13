using System.Collections;
using UnityEngine;

public class BunkerSystem : MonoBehaviour {

    [Header("Settings")]
    [SerializeField] private BunkerSystemType systemType;
    [SerializeField, Range(0, 100)] private int initialDurability;
    [SerializeField, Tooltip("The interval in seconds between each 1% durability loss tick")] private float durabilityLossInterval = 60f;
    [SerializeField] private Item[] repairItems;
    private int currDurability;

    [Header("Debug")]
    [SerializeField] private bool debugMode; // for debugging purposes

    private void Awake() {

        currDurability = initialDurability;
        StartCoroutine(HandleDurabilityLoss());

    }

    public void Repair(int repairPercent) => currDurability = Mathf.Clamp(currDurability + repairPercent, 0, initialDurability); // ensure current durability does not exceed initial durability (clamp it between 0 and initialDurability)

    private IEnumerator HandleDurabilityLoss() {

        while (currDurability > 0) {

            yield return new WaitForSeconds(durabilityLossInterval); // wait for the set interval
            currDurability--;

            if (currDurability < 0)
                currDurability = 0;

            if (debugMode)
                Debug.Log($"{systemType} durability decreased to {currDurability}/{initialDurability}");

        }

        // TODO: handle system failure

    }

    public BunkerSystemType GetSystemType() => systemType;

    public int GetCurrentDurability() => currDurability;

}

public enum BunkerSystemType {

    AirFiltration, WaterPurification, PowerSupply

}

public enum BunkerSystemStatus {

    Operational, Damaged, Critical, Offline

}