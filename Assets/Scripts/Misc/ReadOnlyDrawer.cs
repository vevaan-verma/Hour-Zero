#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Allows [SerializeField, ReadOnly] to make a variable display in the inspector without being modifiable
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label);
        GUI.enabled = true;

    }
}
#endif
