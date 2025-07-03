using UnityEngine;
using UnityEngine.UI;

public abstract class InventoryUI : MonoBehaviour {

    [Header("References")]
    [SerializeField] protected Slot slotPrefab;
    protected Inventory inventory;
    protected Slot[] inventorySlots;

    [Header("UI References")]
    [SerializeField] protected CanvasGroup uiPanel; // the panel that contains the inventory UI (used to allow the script to remain active while the UI is hidden)
    [SerializeField] protected GridLayoutGroup inventoryContents;

    [Header("Settings")]
    [SerializeField] private bool quickTransferEnabled;
    [SerializeField, Tooltip("The inventory that can be used for quick transfering items between inventories.")] private Inventory quickTransferInventory;
    [SerializeField, Tooltip("Whether to show the item info widget when hovering over an item in the inventory")] protected bool showItemInfoWidgetOnHover;
    protected bool isInventoryOpen;

    // runs before Initialize
    private void OnEnable() {

        // if the inventory is set, subscribe to the inventory's contents update event to refresh the UI when the contents change (done here too to ensure the event is always subscribed to; initialize only subscribes the first time)
        // inventory isn't set the first time (when Initialize is called) so we check if it is set here
        if (inventory) {

            RefreshInventory(); // refresh the inventory slots to ensure they are up to date
            inventory.onContentsUpdated += RefreshInventory; // subscribe to the inventory's contents update event to refresh the UI when the contents change

        }
    }

    public virtual void Initialize() {

        #region VALIDATION
        if (quickTransferEnabled && quickTransferInventory == null)
            Debug.LogError("Quick transfer is enabled but no quick transfer inventory is set on " + gameObject.name);
        #endregion

        uiPanel.gameObject.SetActive(inventory.IsVisibleByDefault());

        RefreshInventory();
        inventory.onContentsUpdated += RefreshInventory; // subscribe to the inventory's contents update event to refresh the UI when the contents change

    }

    private void OnDisable() {

        if (inventory)
            inventory.onContentsUpdated -= RefreshInventory;

    }

    public virtual void RefreshInventory() {

        RectTransform rectTransform = slotPrefab.GetComponent<RectTransform>();
        inventoryContents.cellSize = new Vector2(rectTransform.rect.width, rectTransform.rect.height); // set the cell size of the grid layout group to match the size of the slot prefab
        inventoryContents.constraint = GridLayoutGroup.Constraint.FixedColumnCount; // set the constraint to fixed column count
        inventoryContents.constraintCount = inventory.GetSlotsPerRow(); // set the number of columns in the inventory contents grid layout group

        inventorySlots = new Slot[inventory.GetCurrentSlotCount()];

        // delete all existing slots in the inventory contents
        foreach (Transform child in inventoryContents.transform)
            Destroy(child.gameObject);

        // instantiate the slots based on the current capacity of the inventory
        for (int i = 0; i < inventorySlots.Length; i++) {

            Slot slot = Instantiate(slotPrefab, inventoryContents.transform);
            slot.transform.name = $"Slot{i + 1}";
            ItemStack itemStack = inventory.GetItemStack(i); // get the item stack from the inventory at the corresponding index
            slot.Initialize(inventory, this, i, new ItemStack(itemStack.GetItem(), itemStack.GetCount()), showItemInfoWidgetOnHover); // initialize the slot
            inventorySlots[i] = slot; // store the slot in the array for later reference

        }
    }

    public abstract void OpenInventory();

    public abstract void CloseInventory();

    public bool IsInventoryOpen() => isInventoryOpen;

    public Inventory GetQuickTransferInventory() => quickTransferInventory;

}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(InventoryUI), true)]
// using UnityEditor prefix to avoid needing to hide the import in the final build
public class InventoryUIEditor : UnityEditor.Editor {

    public override void OnInspectorGUI() {

        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, "quickTransferInventory"); // draw all properties except quickTransferInventory

        // conditionally draw the quick transfer inventory field based on the quickTransferEnabled property
        if (serializedObject.FindProperty("quickTransferEnabled").boolValue == true)
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("quickTransferInventory"), new GUIContent("Quick Transfer Inventory"));

        serializedObject.ApplyModifiedProperties();

    }
}
#endif
