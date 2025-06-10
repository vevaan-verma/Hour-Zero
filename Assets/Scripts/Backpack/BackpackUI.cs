using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BackpackUI : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Slot slotPrefab;
    private Backpack backpack;
    private Animator animator;
    private Slot[] backpackSlots;
    private Coroutine backpackCoroutine;
    private bool isBackpackOpen;

    [Header("UI References")]
    [SerializeField] private GameObject backpackPanel; // the panel that contains the backpack UI (used to allow the script to remain active while the UI is hidden)
    [SerializeField] private Transform backpackContents;
    [SerializeField] private Button closeBackpackButton;

    public void Initialize() {

        backpack = FindFirstObjectByType<Backpack>(); // find the backpack in the scene
        animator = GetComponent<Animator>();

        backpackSlots = new Slot[backpack.GetInitialCapacity()];
        closeBackpackButton.onClick.AddListener(CloseBackpack); // add listener to close backpack button
        backpackPanel.SetActive(false); // make sure the backpack panel is hidden by default

    }

    public void RefreshBackpack() {

        // delete all existing slots in the backpack contents
        foreach (Transform child in backpackContents)
            Destroy(child.gameObject);

        // instantiate the slots based on the current capacity of the backpack
        for (int i = 0; i < backpackSlots.Length; i++) {

            Slot slot = Instantiate(slotPrefab, backpackContents);
            slot.transform.name = $"Slot{i + 1}";
            slot.Initialize(this); // initialize the slot
            slot.SetItem(backpack.GetItemStack(i).GetItem(), backpack.GetItemStack(i).GetCount()); // set the item and count in the slot
            backpackSlots[i] = slot; // store the slot in the array for later reference

        }
    }

    public void OpenBackpack() {

        if (isBackpackOpen) return; // do nothing if the backpack is already open

        RefreshBackpack(); // refresh the backpack slots to ensure they are up to date

        isBackpackOpen = true;
        closeBackpackButton.interactable = true; // enable close button to allow closing backpack
        backpackPanel.SetActive(true); // make sure the backpack panel is active while opening

        animator.SetTrigger("openBackpack"); // trigger open animation

        Cursor.lockState = CursorLockMode.None; // unlock cursor
        Cursor.visible = true; // make cursor visible

    }

    public void CloseBackpack() {

        if (!isBackpackOpen) return; // do nothing if the backpack is already closed

        RefreshBackpack(); // refresh the backpack slots to ensure they are up to date

        isBackpackOpen = false; // set the state to closed before waiting for animation because it feels better if the player can move and look around while the backpack is closing
        closeBackpackButton.interactable = false; // disable close button to prevent multiple clicks (as this could mess with the toggle logic)
        backpackPanel.SetActive(true); // make sure the backpack panel is active while closing

        animator.SetTrigger("closeBackpack"); // trigger close animation

        Cursor.lockState = CursorLockMode.Locked; // lock cursor
        Cursor.visible = false; // hide cursor

        StartCoroutine(WaitForBackpackCloseAnim()); // start coroutine to wait for the close animation to finish

    }

    private IEnumerator WaitForBackpackCloseAnim() {

        yield return null; // wait for the next frame to ensure the animation has started
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length); // wait for the animation to finish

        backpackPanel.SetActive(false); // hide the backpack panel after closing

    }

    public bool IsBackpackOpen() => isBackpackOpen;

}
