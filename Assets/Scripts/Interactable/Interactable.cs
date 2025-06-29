using UnityEngine;

public abstract class Interactable : MonoBehaviour {

    [Header("References")]
    protected Hotbar hotbar;
    protected Backpack backpack;

    [Header("Settings")]
    [SerializeField, Tooltip("Whether to require an item stack to be held to interact with this object")] protected bool requireHeldItem;
    [SerializeField, Tooltip("The item stack that must be held to interact with this object")] protected ItemStack requiredHeldItem;
    [SerializeField, Tooltip("Whether to consume the specified held item stack after interaction")] protected bool consumeHeldItem;
    [SerializeField, Tooltip("Whether to require specific item stacks in the backpack to interact with this object")] protected bool requireBackpackItems;
    [SerializeField, Tooltip("The item stacks that must be in the backpack to interact with this object")] protected ItemStack[] requiredBackpackItems;
    [SerializeField, Tooltip("Whether to consume the backpack item stacks after interaction")] protected bool consumeBackpackItems;

    protected void Start() {

        hotbar = FindFirstObjectByType<Hotbar>();
        backpack = FindFirstObjectByType<Backpack>();

        transform.tag = "Interactable"; // ensure the object is tagged as Interactable

    }

    public abstract void Interact();

}

#if UNITY_EDITOR
// using UnityEditor prefix to avoid needing to hide the import in the final build
[UnityEditor.CustomEditor(typeof(Interactable), true)]
public class InteractableEditor : UnityEditor.Editor {

    public override void OnInspectorGUI() {

        serializedObject.Update();

        // only show the requiredHeldItem and consumeHeldItem fields if requireHeldItem is true
        UnityEditor.SerializedProperty requireHeldItemProp = serializedObject.FindProperty("requireHeldItem");
        UnityEditor.EditorGUILayout.PropertyField(requireHeldItemProp);

        if (requireHeldItemProp.boolValue) {

            UnityEditor.SerializedProperty requiredHeldItemProp = serializedObject.FindProperty("requiredHeldItem");
            UnityEditor.EditorGUILayout.PropertyField(requiredHeldItemProp);

            UnityEditor.SerializedProperty consumeHeldItemsProp = serializedObject.FindProperty("consumeHeldItem");
            UnityEditor.EditorGUILayout.PropertyField(consumeHeldItemsProp);

        }

        // only show the requiredBackpackItems and consumeBackpackItems fields if requireBackpackItems is true
        UnityEditor.SerializedProperty requireBackpackItemsProp = serializedObject.FindProperty("requireBackpackItems");
        UnityEditor.EditorGUILayout.PropertyField(requireBackpackItemsProp);

        if (requireBackpackItemsProp.boolValue) {

            UnityEditor.SerializedProperty requiredBackpackItemsProp = serializedObject.FindProperty("requiredBackpackItems");
            UnityEditor.EditorGUILayout.PropertyField(requiredBackpackItemsProp, true); // true to show children since it's an array

            UnityEditor.SerializedProperty consumeBackpackItemsProp = serializedObject.FindProperty("consumeBackpackItems");
            UnityEditor.EditorGUILayout.PropertyField(consumeBackpackItemsProp);

        }

        serializedObject.ApplyModifiedProperties();

    }
}
#endif

