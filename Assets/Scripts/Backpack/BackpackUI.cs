using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BackpackUI : InventoryUI {

    [Header("References")]
    [SerializeField] private Slot slotPrefab;
    private Backpack backpack;
    private Animator animator;
    private Slot[] backpackSlots;
    private Coroutine backpackCloseCoroutine;

    [Header("UI References")]
    [SerializeField] private GameObject uiPanel; // the panel that contains the backpack UI (used to allow the script to remain active while the UI is hidden)
    [SerializeField] private Transform backpackContents;
    [SerializeField] private Button closeBackpackButton;

    [Header("Settings")]
    [SerializeField] private BackpackType backpackType;

    public override void Initialize() {

        backpack = FindFirstObjectByType<Backpack>();
        animator = GetComponent<Animator>();

        backpackSlots = new Slot[backpack.GetInitialCapacity()];
        closeBackpackButton.onClick.AddListener(CloseInventory); // add listener to close backpack button

        uiPanel.SetActive(false); // make sure the backpack panel is hidden by default

    }

    public override void RefreshInventory() {

        // delete all existing slots in the backpack contents
        foreach (Transform child in backpackContents)
            Destroy(child.gameObject);

        // instantiate the slots based on the current capacity of the backpack
        for (int i = 0; i < backpackSlots.Length; i++) {

            Slot slot = Instantiate(slotPrefab, backpackContents);
            slot.transform.name = $"Slot{i + 1}";
            slot.Initialize(backpack, this, i); // initialize the slot
            slot.SetItem(backpack.GetItemStack(i).GetItem(), backpack.GetItemStack(i).GetCount()); // set the item and count in the slot
            backpackSlots[i] = slot; // store the slot in the array for later reference

        }
    }

    public override void OpenInventory() {

        if (isInventoryOpen) return; // do nothing if the backpack is already open

        if (backpackCloseCoroutine != null) StopCoroutine(backpackCloseCoroutine); // stop any existing backpack close coroutine

        RefreshInventory(); // refresh the backpack slots to ensure they are up to date

        isInventoryOpen = true;
        closeBackpackButton.interactable = true; // enable close button to allow closing backpack
        uiPanel.SetActive(true); // make sure the backpack panel is active while opening

        animator.SetTrigger("openBackpack"); // trigger open animation

        Cursor.lockState = CursorLockMode.None; // unlock cursor
        Cursor.visible = true; // make cursor visible

    }

    public override void CloseInventory() {

        if (!isInventoryOpen) return; // do nothing if the backpack is already closed

        RefreshInventory(); // refresh the backpack slots to ensure they are up to date

        isInventoryOpen = false; // set the state to closed before waiting for animation because it feels better if the player can move and look around while the backpack is closing
        closeBackpackButton.interactable = false; // disable close button to prevent multiple clicks (as this could mess with the toggle logic)
        uiPanel.SetActive(true); // make sure the backpack panel is active while closing

        animator.SetTrigger("closeBackpack"); // trigger close animation

        Cursor.lockState = CursorLockMode.Locked; // lock cursor
        Cursor.visible = false; // hide cursor

        if (backpackCloseCoroutine != null) StopCoroutine(backpackCloseCoroutine); // stop any existing backpack close coroutine
        backpackCloseCoroutine = StartCoroutine(WaitForBackpackCloseAnim()); // start coroutine to wait for the close animation to finish

    }

    private IEnumerator WaitForBackpackCloseAnim() {

        yield return null; // wait for the next frame to ensure the animation has started
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length); // wait for the animation to finish

        uiPanel.SetActive(false); // hide the backpack panel after closing

    }

    public BackpackType GetBackpackType() => backpackType;

    public override bool IsInventoryOpen() => isInventoryOpen;

}
