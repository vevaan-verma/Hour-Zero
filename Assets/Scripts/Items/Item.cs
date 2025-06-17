using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Item")]
public class Item : ScriptableObject {

    [Header("Properties")]
    [SerializeField] private string itemName;
    [SerializeField] private Sprite icon;
    [SerializeField, Tooltip("The maximum number of items that can be stacked in this item. If set to 0, it will either use the slot's stack limit, or if that is set to 0, an infinite limit"), Min(0)] private int stackLimit;
    [SerializeField] private ItemType itemType;
    [SerializeField] private GameObject heldToolPrefab;

    public string GetName() => itemName;

    public Sprite GetIcon() => icon;

    public int GetStackSize() => stackLimit;

    public ItemType GetItemType() => itemType;

    public GameObject GetHeldToolPrefab() => heldToolPrefab;

    public override bool Equals(object other) => other is Item item && itemName == item.itemName && icon == item.icon && stackLimit == item.stackLimit;

    public override int GetHashCode() => itemName.GetHashCode() ^ icon.GetHashCode() ^ stackLimit.GetHashCode(); // combine hash codes of item properties for uniqueness

}

[Serializable]
public class ItemStack {

    [Header("Data")]
    [SerializeField] private Item item;
    [SerializeField] private int count;

    public ItemStack(Item item, int count) {

        this.item = item;
        this.count = count;

    }

    public Item GetItem() => item;

    public int GetCount() => count;

}

public enum ItemType {

    Consumable,
    Tool

}

#if UNITY_EDITOR
// using UnityEditor prefix to avoid needing to hide the import in the final build
[UnityEditor.CustomEditor(typeof(Item))]
public class ItemEditor : UnityEditor.Editor {

    public override void OnInspectorGUI() {

        serializedObject.Update();

        UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("itemName"));
        UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"));
        UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("stackLimit"));
        UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("itemType"));

        // hides the grip position field if the item type is not a tool
        if ((ItemType) serializedObject.FindProperty("itemType").enumValueIndex == ItemType.Tool)
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("heldToolPrefab"), new GUIContent("Held Tool Prefab"));

        serializedObject.ApplyModifiedProperties();

    }
}
#endif
