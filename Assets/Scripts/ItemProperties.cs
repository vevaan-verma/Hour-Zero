using UnityEngine;

[CreateAssetMenu(menuName = "Item")]
public class ItemProperties : ScriptableObject {

    [Header("Properties")]
    [SerializeField] private string itemName;
    [SerializeField] private GameObject heldItemPrefab;
    [SerializeField] private Sprite icon;

    public string GetName() => itemName;

    public GameObject GetHeldItemPrefab() => heldItemPrefab;

    public Sprite GetIcon() => icon;

}
