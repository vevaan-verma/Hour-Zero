using System.Collections;
using UnityEngine;

public class OpenableInteractable : Interactable {

    [Header("References")]
    private Animator animator;
    private Coroutine interactCooldownCoroutine;

    [Header("Settings")]
    [SerializeField, Tooltip("Whether the interactable is open when the game starts")] private bool isInitiallyOpen;
    [SerializeField, Tooltip("Whether the item lock is intact by default, which requires the specified items to open or close the interactable when intact")] private bool itemLockIntact;
    [SerializeField, Tooltip("Whether the item lock should be broken when the interactable is interacted with")] private bool breakItemLockOnInteract;
    [SerializeField, Tooltip("Whether to wait for the animation to finish before allowing further interactions")] private bool waitForAnimation;
    private bool isOpen;

    [Header("Sounds")]
    [SerializeField] private SFXLib.Sounds openSound;
    [SerializeField] private SFXLib.Sounds closeSound;
    private AudioPlayer audioPlayer;

    [Header("Interact Indicator")]
    [SerializeField, Tooltip("Text shown on the indicator when the door state is set to open")] private string openedText;
    [SerializeField, Tooltip("Text shown on the indicator when the door state is set to closed")] private string closedText;

    private new void Start() {

        base.Start();
        animator = GetComponent<Animator>();
        audioPlayer = GetComponent<AudioPlayer>();

        if (!itemLockIntact && breakItemLockOnInteract)
            Debug.LogWarning($"Interactable {name} has item lock intact set to false but break item lock on interact is true. This will have no effect since the item lock is already broken.");

        // set the initial state of the interactable based on the isInitiallyOpen variable
        if (isInitiallyOpen)
            Open();
        else
            Close();

    }

    public override bool Interact() {

        if (!canInteract) return false;

        // if the item lock is still intact, check if the player has the required item to open or close the interactable
        if (itemLockIntact)
            if (!base.Interact()) return false; // if the base interaction fails, do not proceed

        // if the item lock should be broken on interaction, set the item lock intact state to false
        if (breakItemLockOnInteract)
            itemLockIntact = false;

        if (isOpen)
            Close();
        else
            Open();

        if (interactCooldownCoroutine != null) StopCoroutine(interactCooldownCoroutine); // stop any existing interact cooldown coroutine

        // check if the animation should be waited for before allowing further interactions
        if (waitForAnimation) {

            canInteract = false; // set canInteract to false to prevent further interactions until the animation is done
            interactCooldownCoroutine = StartCoroutine(HandleInteractCooldown()); // start the interact cooldown coroutine

        }

        return true;

    }

    private void Open() {

        animator.SetTrigger("open"); // trigger the open animation
        isOpen = true; // set the interactable as open

        audioPlayer.Play(openSound);

        indicator.SetText(openedText);

    }

    private void Close() {

        animator.SetTrigger("close"); // trigger the close animation
        isOpen = false; // set the interactable as closed

        audioPlayer.Play(closeSound);

        indicator.SetText(closedText);

    }

    private IEnumerator HandleInteractCooldown() {

        yield return null; // wait for the animation to start
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length); // wait for the interact animation to finish

        canInteract = true;

    }
}