using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

[CustomEditor(typeof(ItemSpawnGroup))]
public class ItemSpawnGroupSetOwnerButton : Editor {
    public override void OnInspectorGUI() {

        DrawDefaultInspector();

        ItemSpawnGroup group = (ItemSpawnGroup)target;

        if (GUILayout.Button("Find SpawnPoints in children (and autoassign groups)"))
            group.FindSpawnpointsInChildren(group.transform);

        if (GUILayout.Button("Auto assign ItemSpawnPoint groups"))
            group.ApplyGroupToSpawnPoints();

        if (GUILayout.Button("Validate group"))
            group.ValidateGroup(true);

        if (GUILayout.Button("Clear group and all assignments"))
            group.ClearGroup();


    }

}

#endif
