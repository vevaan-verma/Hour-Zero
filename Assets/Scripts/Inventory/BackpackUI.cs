using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BackpackUI : InventoryUI {

    [Header("References")]
    private UIManager uiManager;
    private Animator animator;
    private Coroutine backpackCloseCoroutine;

    [Header("UI References")]
    [SerializeField] private Button closeBackpackButton;

    [Header("Settings")]
    [SerializeField] private BackpackType backpackType;
    // the first [slotsPerRow] amount of slots of the backpack are the hotbar slots

    public override void Initialize() {

        uiManager = FindFirstObjectByType<UIManager>();
        inventory = FindFirstObjectByType<Backpack>(FindObjectsInactive.Include); // find the backpack in the scene (must be done before base.Initialize() to ensure backpack is set)
        animator = GetComponent<Animator>();

        closeBackpackButton.onClick.AddListener(uiManager.ClosePrimaryBackpack); // add listener to close backpack button; call the UIManager method to close the primary backpack rather than this class directly to ensure the extra logic is executed too (e.g. closing the hotbar UI)
        closeBackpackButton.gameObject.SetActive(backpackType == BackpackType.Primary); // only show the close button if this is the primary backpack

        base.Initialize();

    }

    // overridden to allow for different colored hotbar slots in the backpack
    public override void RefreshInventory() {

        int slotsPerRow = inventory.GetSlotsPerRow();

        RectTransform rectTransform = slotPrefab.GetComponent<RectTransform>();
        inventoryContents.cellSize = new Vector2(rectTransform.rect.width, rectTransform.rect.height); // set the cell size of the grid layout group to match the size of the slot prefab
        inventoryContents.constraint = GridLayoutGroup.Constraint.FixedColumnCount; // set the constraint to fixed column count
        inventoryContents.constraintCount = slotsPerRow; // set the number of columns in the inventory contents grid layout group

        inventorySlots = new Slot[inventory.GetCurrentSlotCount()];

        // delete all existing slots in the inventory contents
        foreach (Transform child in inventoryContents.transform)
            Destroy(child.gameObject);

        // instantiate the slots based on the current capacity of the inventory
        for (int i = 0; i < inventorySlots.Length; i++) {

            Slot slot = Instantiate(slotPrefab, inventoryContents.transform);
            slot.transform.name = $"Slot{i + 1}";
            ItemStack itemStack = inventory.GetItemStack(i); // get the item stack from the inventory at the corresponding index
            slot.Initialize(inventory, this, i, new ItemStack(itemStack.GetItem(), itemStack.GetCount()), showItemInfoWidgetOnHover, i < slotsPerRow ? ((Backpack) inventory).GetHotbarSlotColor() : null); // initialize the slot; if the slot is a hotbar slot, set the color to the hotbar slot color, otherwise use the default color
            inventorySlots[i] = slot; // store the slot in the array for later reference

        }
    }

    public override void OpenInventory() {

        if (isInventoryOpen) return; // do nothing if the backpack is already open

        if (backpackCloseCoroutine != null) StopCoroutine(backpackCloseCoroutine); // stop any existing backpack close coroutine

        RefreshInventory(); // refresh the backpack inventory UI to ensure it is up to date before opening

        isInventoryOpen = true;
        closeBackpackButton.interactable = true; // enable close button to allow closing backpack
        uiPanel.gameObject.SetActive(true); // make sure the backpack panel is active while opening

        animator.SetTrigger("openMenu"); // trigger open animation

        Cursor.lockState = CursorLockMode.None; // unlock cursor
        Cursor.visible = true; // make cursor visible

    }

    public override void CloseInventory() {

        if (!isInventoryOpen) return; // do nothing if the backpack is already closed

        isInventoryOpen = false; // set the state to closed before waiting for animation because it feels better if the player can move and look around while the backpack is closing
        closeBackpackButton.interactable = false; // disable close button to prevent multiple clicks (as this could mess with the toggle logic)
        uiPanel.gameObject.SetActive(true); // make sure the backpack panel is active while closing

        animator.SetTrigger("closeMenu"); // trigger close animation

        Cursor.lockState = CursorLockMode.Locked; // lock cursor
        Cursor.visible = false; // hide cursor

        if (backpackCloseCoroutine != null) StopCoroutine(backpackCloseCoroutine); // stop any existing backpack close coroutine
        backpackCloseCoroutine = StartCoroutine(WaitForBackpackCloseAnim()); // start coroutine to wait for the close animation to finish

    }

    private IEnumerator WaitForBackpackCloseAnim() {

        yield return null; // wait for the next frame to ensure the animation has started
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length); // wait for the animation to finish

        uiPanel.gameObject.SetActive(false); // hide the backpack panel after closing

    }

    public BackpackType GetBackpackType() => backpackType;

}
