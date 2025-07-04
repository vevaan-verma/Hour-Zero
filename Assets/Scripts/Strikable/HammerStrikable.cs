using UnityEngine;

public class HammerStrikable : Strikable {

    [Header("References")]
    [SerializeField] private Item hammerItem;

    private new void Start() {

        // do all of the below before calling base.Start() to ensure the strikable is set up correctly (these values are used in the base Start method)
        requireHeldItem = true;
        requiredHeldItem = new ItemStack(hammerItem, 1); // set the required held item to the hammer with a count of 1
        consumeHeldItem = false; // do not consume the hammer after strike, as it is a tool that should be reusable

        requireBackpackItems = false; // no backpack items are required for this strikable

        base.Start();

    }

    public override bool Strike() {

        if (!base.Strike()) return false; // if the base strike fails, do not proceed

        // at this point, the hammer must be held if the required held item was set correctly
        Debug.Log("success");

        return true;

    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(HammerStrikable), true)]
// using UnityEditor prefix to avoid needing to hide the import in the final build
public class HammerStrikableEditor : StrikableEditor {

    public override void OnInspectorGUI() {

        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, "requireHeldItem", "requiredHeldItem", "consumeHeldItem", "requireBackpackItems", "requiredBackpackItems", "consumeBackpackItems"); // don't draw any of the required item properties, as they are not relevant for this strikable; only the hammer is required and that will be set in the code

        serializedObject.ApplyModifiedProperties();

    }
}
#endif
