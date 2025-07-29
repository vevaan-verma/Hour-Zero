using UnityEngine;

public class ItemSpawnPoint : MonoBehaviour {

    [SerializeField] private ItemSpawnGroup groupOwner;

    public void SpawnItem(Item item) => Instantiate(item.GetDroppedItemPrefab(), transform.position, Quaternion.identity);

    public void SetGroup(ItemSpawnGroup group) => groupOwner = group;

    private void OnDrawGizmosSelected() {

        if (groupOwner == null)
            Gizmos.color = Color.red;
        else {

            Gizmos.color = new Color(0.3f, 0.49f, 0.68f);

            Gizmos.DrawLine(transform.position, groupOwner.transform.position);

        }

        Gizmos.DrawWireCube(transform.position, new Vector3(0.3f, 0.3f, 0.3f));

    }
}
