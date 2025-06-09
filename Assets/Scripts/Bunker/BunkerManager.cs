using System.Collections.Generic;
using UnityEngine;

public class BunkerManager : MonoBehaviour {

    [Header("References")]
    private Dictionary<BunkerSystemType, BunkerSystem> bunkerSystems;

    [Header("Settings")]
    [SerializeField] private BunkerType bunkerType;

    private void Start() {

        bunkerSystems = new Dictionary<BunkerSystemType, BunkerSystem>();

        #region VALIDATION
        // make sure each bunker system type has a bunker system component in the scene
        BunkerSystemType[] systemTypes = (BunkerSystemType[]) System.Enum.GetValues(typeof(BunkerSystemType));
        BunkerSystem[] systems = FindObjectsByType<BunkerSystem>(FindObjectsSortMode.None);

        foreach (BunkerSystemType systemType in systemTypes) {

            bool found = false;

            foreach (BunkerSystem system in systems) {

                if (system.GetSystemType() == systemType) {

                    found = true;
                    break;

                }
            }

            if (!found)
                Debug.LogError($"BunkerController: No BunkerSystem component found for {systemType} in the scene. Please add it to the scene.");

        }
        #endregion

        // add all bunker systems to the dictionary
        foreach (BunkerSystem system in systems)
            if (!bunkerSystems.ContainsKey(system.GetSystemType()))
                bunkerSystems.Add(system.GetSystemType(), system);

    }

    public void RepairSystem(BunkerSystemType systemType, int repairAmount) {

        BunkerSystem system = bunkerSystems[systemType];
        system.Repair(repairAmount);
        Debug.Log($"Repaired {systemType} by {repairAmount}. Current durability: {system.GetCurrentDurability()}.");

    }

    public BunkerSystem GetBunkerSystem(BunkerSystemType systemType) => bunkerSystems[systemType];

}

public enum BunkerType {

    City, Mountain

}