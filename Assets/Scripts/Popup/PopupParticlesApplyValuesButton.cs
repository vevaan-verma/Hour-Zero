#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PopupParticles))]
public class PopupParticlesApplyValuesButton : Editor {

    public override void OnInspectorGUI() {

        DrawDefaultInspector();

        PopupParticles popup = (PopupParticles)target;

        if (GUILayout.Button("Apply Values To Particle System"))
            popup.ApplyValues();

    }

}
#endif
