using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SFXLib))]
public class SFXLibValidateButton : Editor {
    public override void OnInspectorGUI() {

        DrawDefaultInspector();

        SFXLib lib = (SFXLib)target;

        if (GUILayout.Button("Validate Dictionary"))
            lib.ValidateDict(modifyDict: true);

    }

}
