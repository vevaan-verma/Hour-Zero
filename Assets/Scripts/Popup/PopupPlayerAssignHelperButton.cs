using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

[CustomEditor(typeof(PopupSound))]
public class PopupPlayerAssignHelperButton : Editor {
    public override void OnInspectorGUI() {

        DrawDefaultInspector();

        PopupSound popup = (PopupSound)target;

        if (GUILayout.Button("Auto Assign Clip By Prefab Name"))
            popup.TryAutoAssignClip();

    }

}

#endif