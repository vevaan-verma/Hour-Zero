using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

[CustomEditor(typeof(SFXLib))]
public class SFXLibValidateButton : Editor {
    public override void OnInspectorGUI() {

        DrawDefaultInspector();

        SFXLib lib = (SFXLib)target;

        if (GUILayout.Button("Validate and auto-organize dictionary"))
            lib.ValidateDict(modifyDict: true, printSuccessMessage: true);

    }

}

#endif
