using UnityEditor;

// <summary>
// custom editor for the Hotbar class to allow for a range slider for the slotStackLimit field
// since we need to constrain the slot count between 1 and 9
// this is because each slot needs to have a single digit key binding from 1 to 9
// </summary>
[CustomEditor(typeof(Hotbar))]
public class HotbarEditor : Editor {

    public override void OnInspectorGUI() {

        serializedObject.Update();

        // draw all fields except initialSlotCount
        DrawPropertiesExcluding(serializedObject, "initialSlotCount");

        // draw initialSlotCount with a range slider
        SerializedProperty initialSlotCount = serializedObject.FindProperty("initialSlotCount");

        if (initialSlotCount != null)
            initialSlotCount.intValue = EditorGUILayout.IntSlider("Initial Slot Count", initialSlotCount.intValue, 1, 9);

        serializedObject.ApplyModifiedProperties();

    }
}