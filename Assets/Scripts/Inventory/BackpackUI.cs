using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BackpackUI : InventoryUI {

    [Header("References")]
    private Animator animator;
    private Coroutine backpackCloseCoroutine;

    [Header("UI References")]
    [SerializeField] private Button closeBackpackButton;

    [Header("Settings")]
    [SerializeField] private BackpackType backpackType;

    public override void Initialize() {

        inventory = FindFirstObjectByType<Backpack>(FindObjectsInactive.Include); // find the backpack in the scene
        animator = GetComponent<Animator>();

        closeBackpackButton.onClick.AddListener(CloseInventory); // add listener to close backpack button
        closeBackpackButton.gameObject.SetActive(backpackType == BackpackType.Primary); // only show the close button if this is the primary backpack

        base.Initialize();

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

}
