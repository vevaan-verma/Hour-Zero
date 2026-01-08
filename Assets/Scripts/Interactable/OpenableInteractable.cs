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

    [Header("Auto Close")]
    [SerializeField, Tooltip("Whether to automatically flip the state of the door after a duration")] private bool autoClose;
    [SerializeField, Tooltip("Time to wait for autoClose")] private float autoCloseTimer;
    private Coroutine autoCloseCoroutine;
    private float currAutoCloseTimer;
    private bool heldOpen;
    private bool autoMode; // whether the openable is in auto mode, which prevents manual interaction

    [Header("Animation")]
    [SerializeField, Tooltip("Instead of using an Animator, the script lerps the object to allow for a more dynamic openable. \n\nNOT IMPLEMENTED!!")] private bool useLerpAnimation;
    [SerializeField, Tooltip("When the openable is opened, its position will be displaced by this vector")] private Vector3 lerpPositionalDisplacement;
    [SerializeField, Tooltip("When the openable is opened, its rotation will be displaced by this vector")] private Vector3 lerpAngularDisplacement;

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

        if (!autoClose)
            autoCloseTimer = 0;

        // set the initial state of the interactable based on the isInitiallyOpen variable
        if (isInitiallyOpen)
            Open();
        else
            Close();

    }

    // tell this OpenableInteractable it cannot be manually opened
    public void SetAutoOpenable() => autoMode = true;

    public override bool Interact() {

        if (!base.Interact() || autoMode) return false; // if the base interaction fails or the interactable is in auto mode, do not proceed

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

            isInteractable = false; // set canInteract to false to prevent further interactions until the animation is done
            interactCooldownCoroutine = StartCoroutine(HandleInteractCooldown()); // start the interact cooldown coroutine

        }

        if (autoClose) {

            // if the auto close timer is running, cancel it, because the door is being closed manually prior to the expiration of the timer
            // otherwise, start a new timer

            if (autoCloseCoroutine != null) {

                StopCoroutine(autoCloseCoroutine);
                autoCloseCoroutine = null;

            } else {

                float animClipLength = animator.GetCurrentAnimatorStateInfo(0).length;
                currAutoCloseTimer = autoCloseTimer + animClipLength;

                autoCloseCoroutine = StartCoroutine(HandleAutoClose(animClipLength));

            }
        }

        return true;

    }

    // only for OpenableSensor 
    public void SensorInteract() {

        if (!autoMode) {

            Debug.LogError("The Interactable GameObject \"" + gameObject.name + "\" cannot have SensorInteract called on it because it does not have an OpenableSensor component.");
            return;

        }

        if (!isInteractable) return;

        if (isOpen && autoCloseCoroutine == null)
            Close();
        else if (autoCloseCoroutine == null)
            Open();

        if (interactCooldownCoroutine != null) StopCoroutine(interactCooldownCoroutine); // stop any existing interact cooldown coroutine

        // check if the animation should be waited for before allowing further interactions
        if (waitForAnimation) {

            isInteractable = false; // set canInteract to false to prevent further interactions until the animation is done
            interactCooldownCoroutine = StartCoroutine(HandleInteractCooldown()); // start the interact cooldown coroutine

        }

        if (autoClose) {

            // if the auto close timer is running, cancel it, because the door is being closed manually prior to the expiration of the timer
            // otherwise, start a new timer

            if (autoCloseCoroutine != null) {

                StopCoroutine(autoCloseCoroutine);
                autoCloseCoroutine = null;

            } else {

                float animClipLength = animator.GetCurrentAnimatorStateInfo(0).length;
                currAutoCloseTimer = autoCloseTimer + animClipLength;

                autoCloseCoroutine = StartCoroutine(HandleAutoClose(animClipLength));

            }
        }
    }

    private void Open() {

        // do open animation
        if (!useLerpAnimation)
            animator.SetTrigger("open");

        isOpen = true; // set the interactable as open

        audioPlayer.Play(openSound);

        indicator.SetText(openedText);

    }

    private void Close() {

        // do close animation
        if (!useLerpAnimation)
            animator.SetTrigger("close");

        isOpen = false; // set the interactable as closed

        audioPlayer.Play(closeSound);

        indicator.SetText(closedText);

    }

    private IEnumerator HandleInteractCooldown() {

        yield return null; // wait for the animation to start
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length); // wait for the interact animation to finish

        isInteractable = true;

    }

    // additionalWait is used to factor the interact animation duration into the auto close timer
    private IEnumerator HandleAutoClose(float additionalWait) {

        float time = 0;

        // wait for timer

        while (time < autoCloseTimer + additionalWait) {

            if (!heldOpen)
                time += Time.deltaTime;

            yield return new WaitForEndOfFrame();

        }

        // flip openable state

        if (isOpen) Close();
        else Open();

        autoCloseCoroutine = null;

    }

    public void SetHeldOpen(bool held) {

        heldOpen = held;

        if (!held) { // if ending held open (object/player leaves the sensor region), begin auto close coroutine

            if (autoCloseCoroutine != null) {

                StopCoroutine(autoCloseCoroutine);
                autoCloseCoroutine = null;

            }

            autoCloseCoroutine = StartCoroutine(HandleAutoClose(currAutoCloseTimer));

        }
    }

#if UNITY_EDITOR
    // using UnityEditor prefix to avoid needing to hide the import in the final build
    [UnityEditor.CustomEditor(typeof(OpenableInteractable), true)]
    public class InteractableEditor : UnityEditor.Editor {

        public override void OnInspectorGUI() {

            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, "lerpPositionalDisplacement", "lerpAngularDisplacement", "openSound", "closeSound", "openedText", "closedText");

            UnityEditor.SerializedProperty useLerpAnimation = serializedObject.FindProperty("useLerpAnimation");
            UnityEditor.SerializedProperty lerpPositionalDisplacement = serializedObject.FindProperty("lerpPositionalDisplacement");
            UnityEditor.SerializedProperty lerpAngularDisplacement = serializedObject.FindProperty("lerpAngularDisplacement");
            UnityEditor.SerializedProperty openSound = serializedObject.FindProperty("openSound");
            UnityEditor.SerializedProperty closeSound = serializedObject.FindProperty("closeSound");
            UnityEditor.SerializedProperty openedText = serializedObject.FindProperty("openedText");
            UnityEditor.SerializedProperty closedText = serializedObject.FindProperty("closedText");


            if (useLerpAnimation.boolValue) {

                UnityEditor.EditorGUILayout.PropertyField(lerpPositionalDisplacement);
                UnityEditor.EditorGUILayout.PropertyField(lerpAngularDisplacement);

            }

            UnityEditor.EditorGUILayout.PropertyField(openSound);
            UnityEditor.EditorGUILayout.PropertyField(closeSound);
            UnityEditor.EditorGUILayout.PropertyField(openedText);
            UnityEditor.EditorGUILayout.PropertyField(closedText);

            serializedObject.ApplyModifiedProperties();

        }
    }
#endif
}
