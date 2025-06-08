using UnityEngine;

[CreateAssetMenu(menuName = "Item")]
public class Item : ScriptableObject {

    [Header("Properties")]
    [SerializeField] private string itemName;
    [SerializeField] private Sprite icon;

    public string GetName() => itemName;

    public Sprite GetIcon() => icon;

}
