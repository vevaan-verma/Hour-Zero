using UnityEngine;

public class OpenableInteractable : Interactable {

    [Header("References")]
    private Animator animator;

    [Header("Settings")]
    [SerializeField, Tooltip("Whether the interactable is open when the game starts")] private bool isInitiallyOpen;
    [SerializeField, Tooltip("Whether the item lock is intact by default, which requires the specified items to open or close the interactable when intact")] private bool itemLockIntact;
    [SerializeField, Tooltip("Whether the item lock should be broken when the interactable is interacted with")] private bool breakItemLockOnInteract;
    [SerializeField, Tooltip("The linkedOpenable will be triggered when ")] private OpenableInteractable linkedOpenable;
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

        isOpen = isInitiallyOpen; // set the initial state of the interactable

        print(gameObject.name + " " + isInitiallyOpen);

        if (isOpen) {

            animator.SetTrigger("open"); // trigger the open animation if the interactable is initially open

            indicator.SetText(openedText);

        }
        else
            indicator.SetText(closedText);

    }

    public override bool Interact() {

        // if the item lock is still intact, check if the player has the required item to open or close the interactable
        if (itemLockIntact)
            if (!base.Interact()) return false; // if the base interaction fails, do not proceed

        // if the item lock should be broken on interaction, set the item lock intact state to false
        if (breakItemLockOnInteract)
            itemLockIntact = false;

        if (isOpen) {

            animator.SetTrigger("close"); // trigger the close animation
            isOpen = false; // set the interactable as closed

            audioPlayer.Play(closeSound);

            indicator.SetText(closedText);

        }
        else {

            animator.SetTrigger("open"); // trigger the open animation
            isOpen = true; // set the interactable as open

            audioPlayer.Play(openSound);

            indicator.SetText(openedText);

        }

        return true;

    }
}