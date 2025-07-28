using System.Collections;
using UnityEngine;

public abstract class Interactable : MonoBehaviour {

    [Header("References")]
    protected Hotbar hotbar;
    protected Backpack backpack;
    protected PlayerController player;

    [Header("Settings")]
    [SerializeField, Tooltip("Whether to require an item stack to be held to interact with this object")] protected bool requireHeldItem;
    [SerializeField, Tooltip("The item stack that must be held to interact with this object")] protected ItemStack requiredHeldItem;
    [SerializeField, Tooltip("Whether to consume the specified held item stack after interaction")] protected bool consumeHeldItem;
    [SerializeField, Tooltip("Whether to require specific item stacks in the backpack to interact with this object")] protected bool requireBackpackItems;
    [SerializeField, Tooltip("The item stacks that must be in the backpack to interact with this object")] protected ItemStack[] requiredBackpackItems;
    [SerializeField, Tooltip("Whether to consume the backpack item stacks after interaction")] protected bool consumeBackpackItems;
    protected bool canInteract;

    [Header("Indicator")]
    protected InteractIndicator indicator;
    private Vector3 indicatorDefaultSize;
    private Coroutine indicatorLerpCoroutine;
    private bool isIndicatorVisible;

    [Header("Constants")]
    private const float interactIndicatorLerpDuration = 0.1f;

    protected void Start() {

        #region VALIDATION
        // if requireHeldItem is true, requiredHeldItem must not be null or have a count of 0
        if (requireHeldItem)
            if (requiredHeldItem.GetItem() == null)
                Debug.LogError($"Interactable {name} requires a held item but none was specified. Please assign a required item in the inspector.");
            else if (requiredHeldItem.GetCount() <= 0)
                Debug.LogError($"Interactable {name} requires a held item with a count greater than 0 but the specified item has a count of {requiredHeldItem.GetCount()}. Please assign a valid item in the inspector.");

        // if requireBackpackItems is true, requiredBackpackItems must not be null or empty
        if (requireBackpackItems && requiredBackpackItems.Length == 0)
            Debug.LogError($"Interactable {name} requires backpack items but none were specified. Please assign required items in the inspector.");
        #endregion

        hotbar = FindFirstObjectByType<Hotbar>();
        backpack = FindFirstObjectByType<Backpack>();
        player = FindFirstObjectByType<PlayerController>();
        indicator = GetComponentInChildren<InteractIndicator>(true); // find the interact indicator in the children of this object, even if inactive

        indicatorDefaultSize = indicator.transform.localScale;
        indicator.transform.localScale = Vector3.zero;
        indicator.gameObject.SetActive(true); // ensure the indicator is active so it can be shown when needed

        // ensure the object and all its children are tagged as Interactable
        foreach (Transform child in GetComponentsInChildren<Transform>())
            child.gameObject.tag = "Interactable";

        canInteract = true;

    }

    protected void Update() {

        indicator.transform.LookAt(player.GetCameraTransform());

        // the indicator is set active (shown) by the player, then set inactive (hidden) by the interactable itself

        if (isIndicatorVisible && !player.IsLookingAt(gameObject)) {

            // go from current size to hidden
            if (indicatorLerpCoroutine != null) StopCoroutine(indicatorLerpCoroutine);
            indicatorLerpCoroutine = StartCoroutine(LerpIndicatorSize(indicator.transform.localScale, Vector3.zero));

            isIndicatorVisible = false;

        }
    }

    public virtual bool Interact() {

        if (!canInteract) return false; // if the interactable is not allowed to be interacted with, return false

        // if the interactable requires a held item, check if the player is holding the required item and enough of it
        if (requireHeldItem) {

            ItemStack selectedItemStack = hotbar.GetItemStack(hotbar.GetSelectedIndex()); // get the item stack in the currently selected hotbar slot

            // if the required item is not held or not enough of it is held, don't follow through with the interaction
            if (selectedItemStack.GetItem() == null || !selectedItemStack.GetItem().Equals(requiredHeldItem.GetItem()))
                return false;

            // if the held item should be consumed, use the backpack inventory to remove as much of the item stack from the current selected hotbar slot as possible, then remove the remainder as normal
            if (consumeHeldItem)
                backpack.RemoveItemStack(new ItemStack(requiredHeldItem.GetItem(), requiredHeldItem.GetCount()), hotbar.GetSelectedIndex());

        }

        // if the interactable required items in the backpack, check if the player has those items and enough of them
        if (requireBackpackItems) {

            foreach (ItemStack requiredStack in requiredBackpackItems) {

                // if the backpack does not contain the required item stack, don't follow through with the interaction
                if (!backpack.ContainsItemStack(requiredStack))
                    return false;

                // if the backpack items should be consumed, remove the required amount from the backpack
                if (consumeBackpackItems)
                    backpack.RemoveItemStack(new ItemStack(requiredStack.GetItem(), requiredStack.GetCount()));

            }
        }

        return true;

    }

    public void ShowInteractIndicator() {

        // if the indicator is inactive, it is not currently being displayed
        if (!isIndicatorVisible) {

            // go from hidden to normal size
            if (indicatorLerpCoroutine != null) StopCoroutine(indicatorLerpCoroutine);
            indicatorLerpCoroutine = StartCoroutine(LerpIndicatorSize(Vector3.zero, indicatorDefaultSize));

            isIndicatorVisible = true;

        }
    }

    private IEnumerator LerpIndicatorSize(Vector3 start, Vector3 end) {

        float currentTime = 0f;
        indicator.transform.localScale = start;

        while (currentTime < interactIndicatorLerpDuration) {

            indicator.transform.localScale = Vector3.Lerp(start, end, currentTime / interactIndicatorLerpDuration);
            currentTime += Time.deltaTime;
            yield return null;

        }

        indicator.transform.localScale = end;

    }
}

#if UNITY_EDITOR
// using UnityEditor prefix to avoid needing to hide the import in the final build
[UnityEditor.CustomEditor(typeof(Interactable), true)]
public class InteractableEditor : UnityEditor.Editor {

    public override void OnInspectorGUI() {

        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, "requireHeldItem", "requiredHeldItem", "consumeHeldItem", "requireBackpackItems", "requiredBackpackItems", "consumeBackpackItems");

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
