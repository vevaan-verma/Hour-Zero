using System.Collections.Generic;
using UnityEngine;

public class ItemSpawnGroup : MonoBehaviour {

    [SerializeField, Tooltip("All the ItemSpawnPoints this spawn group can use to spawn items at")] private List<ItemSpawnPoint> spawnPoints = new List<ItemSpawnPoint>();
    [SerializeField, Tooltip("All the items this SpawnGroup can spawn")] private List<Item> spawnableItems = new List<Item>();
    [SerializeField, Tooltip("Chance for each spawner to spawn something"), Range(0, 1)] private float spawnChance;


    // TODO: add restocking
    // have a pool of items to spawn
    // spawn items at a certain time every day or after a time interval (use the time manager)
    // wait for player to be outside a certain range of the group or not facing it before spawning items
    // considerations:
    //      destroy existing items upon spawn (if they are within a certain range of the spawner)?
    //          each spawner could remember the last item it spawned and check if it needs to be destroyed by using a spherecast (check if "populated")
    //          alternatively, only restock in "unpopulated" spawnpoints?
    //          instead of destroying items, maybe have npcs randomly "take" items? (approach them and deactivate them) 
    //              this can be what they do all the time as an idle behavior, besides wandering
    //              best solution ^^
    //      figure out pool size...
    //      centralized item pool in Systems prefab? 

    void Start() {

        ValidateGroup(false);

        foreach (ItemSpawnPoint spawnPoint in spawnPoints)
            if (Random.Range(0f, 1f) < spawnChance)
                spawnPoint.SpawnItem(spawnableItems[Random.Range(0, spawnableItems.Count)]);

    }

    #region Util

    public void ApplyGroupToSpawnPoints() {

        foreach (ItemSpawnPoint point in spawnPoints)
            point.SetGroup(this);

    }

    // also assigns groups
    // skips duplicates
    public void FindSpawnpointsInChildren(Transform parent) {

        // for proper organization, the spawnpoints should all be the direct children of the spawn group
        // however, this method allows for any sort of weird structure

        for (int i = 0; i < parent.childCount; i++) {

            Transform child = parent.GetChild(i);

            // recurse
            if (child.childCount > 0)
                FindSpawnpointsInChildren(child);

            if (child.gameObject.activeSelf && child.GetComponent<ItemSpawnPoint>() != null) {

                ItemSpawnPoint spawnPoint = child.GetComponent<ItemSpawnPoint>();

                // skip duplicates
                if (!spawnPoints.Contains(spawnPoint)) {

                    spawnPoints.Add(spawnPoint);

                    // assign group
                    spawnPoint.SetGroup(this);

                }

            }

        }

    }

    public void ValidateGroup(bool printSuccessMessage) {

        HashSet<ItemSpawnPoint> uniqueSpawns = new HashSet<ItemSpawnPoint>();
        HashSet<ItemSpawnPoint> duplicateSpawns = new HashSet<ItemSpawnPoint>();

        foreach (ItemSpawnPoint spawnPoint in spawnPoints) {

            if (!uniqueSpawns.Add(spawnPoint) && !duplicateSpawns.Contains(spawnPoint)) {

                Debug.LogWarning("ItemSpawnPoint \"" + spawnPoint.gameObject.name + "\" appears multiple times in the spawn group " + gameObject.name);

                duplicateSpawns.Add(spawnPoint);

            }

        }

        if (duplicateSpawns.Count == 0 && printSuccessMessage)
            Debug.Log("No errors in this spawn group");

    }

    public void ClearGroup() {

        foreach (ItemSpawnPoint point in spawnPoints)
            point.SetGroup(null);

        spawnPoints.Clear();

        Debug.Log("Cleared item spawn group \"" + gameObject.name + "\"");

    }

    public void VerifyLists() {

        if (spawnPoints == null)
            spawnPoints = new List<ItemSpawnPoint>();

        if (spawnableItems == null)
            spawnableItems = new List<Item>();

    }

    #endregion

    #region Gizmos

    void OnDrawGizmos() {

        Gizmos.DrawIcon(transform.position, "ItemSpawnGroupGizmo.psd", true);

    }

    private void OnDrawGizmosSelected() {

        Gizmos.color = new Color(0.3f, 0.49f, 0.68f);

        foreach (ItemSpawnPoint point in spawnPoints)
            Gizmos.DrawLine(transform.position, point.transform.position);

    }

    #endregion

}

