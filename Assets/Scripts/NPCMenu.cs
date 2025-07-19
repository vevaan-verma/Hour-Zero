using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NPCMenu : MonoBehaviour {

    [Header("References")]
    private PlayerController playerController;
    private UIManager uiManager;
    private NameDatabase nameDatabase;
    private TaskDatabase taskDatabase;
    private NPCController currNPCController; // reference to the currently interacted NPC controller
    private Animator animator;

    [Header("UI References")]
    [SerializeField] private CanvasGroup menuPanel;
    [SerializeField] private TMP_Text npcNameText;
    private bool isMenuOpen;
    private Coroutine fadeCoroutine;

    [Header("Interact Section")]
    [SerializeField] private GameObject interactSection;
    [SerializeField] private Button teamButton;
    [SerializeField] private Button talkButton;
    [SerializeField] private Button tradeButton;
    [SerializeField] private Button trackButton;

    [Header("Dialogue Section")]
    [SerializeField] private GameObject dialogueSection;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button okayButton;
    [SerializeField] private Button closeMenuButton;
    private DialogueDatabase dialogueDatabase;
    private string[] currDialogueSequence; // current dialogue sequence being displayed
    private int currDialogueIndex; // index of the current dialogue line being displayed
    private Coroutine dialogueCoroutine;

    [Header("Settings")]
    [SerializeField] private float menuFadeDuration;
    [SerializeField] private float dialogueCharactersPerSecond;
    [SerializeField] private DialogueSequence[] alreadyAssignedTaskSequences; // predefined dialogue sequences for already assigned tasks

    private void Start() {

        playerController = FindFirstObjectByType<PlayerController>();
        uiManager = FindFirstObjectByType<UIManager>();
        animator = GetComponent<Animator>();
        dialogueDatabase = FindFirstObjectByType<DialogueDatabase>();
        nameDatabase = FindFirstObjectByType<NameDatabase>();
        taskDatabase = FindFirstObjectByType<TaskDatabase>();

        teamButton.onClick.AddListener(() => {

            interactSection.SetActive(false); // hide the interact section
            dialogueSection.SetActive(true); // show the dialogue section

            if (playerController.AssignRandomTask())
                currDialogueSequence = taskDatabase.GetTaskData((TaskType) playerController.GetAssignedTask()).GetRandomDialogueSequence().GetDialogueLines();
            else
                currDialogueSequence = alreadyAssignedTaskSequences[Random.Range(0, alreadyAssignedTaskSequences.Length)].GetDialogueLines(); // get a random dialogue sequence for already assigned tasks

            NextDialogueText(); // display the next dialogue text (this starts the dialogue sequence)

        }); // assign a task to the player

        talkButton.onClick.AddListener(() => {

            interactSection.SetActive(false); // hide the interact section
            dialogueSection.SetActive(true); // show the dialogue section
            currDialogueSequence = dialogueDatabase.GetRandomDialogueSequence().GetDialogueLines();
            NextDialogueText(); // display the next dialogue text (this starts the dialogue sequence)

        });

        okayButton.onClick.AddListener(() => {

            NextDialogueText();
            EventSystem.current.SetSelectedGameObject(null); // deselect the button to allow for re-clicking

        });

        closeMenuButton.onClick.AddListener(() => uiManager.CloseNPCMenu()); // add listener to close menu button; call the UIManager method to close the NPC menu rather than this class directly to ensure the extra logic is executed (e.g., re-opening the hotbar UI)

        dialogueSection.SetActive(false); // ensure the dialogue section is hidden by default
        interactSection.SetActive(true); // ensure the interact section is active by default

        menuPanel.gameObject.SetActive(false); // make sure the menu is hidden by default

    }

    public void OpenMenu(NPCController npcController) {

        this.currNPCController = npcController; // store the current NPC controller

        isMenuOpen = true; // set the menu state to open
        menuPanel.gameObject.SetActive(true); // make sure the menu is active

        dialogueSection.SetActive(false); // ensure the dialogue section is hidden by default
        interactSection.SetActive(true); // ensure the interact section is active by default

        npcNameText.text = nameDatabase.GetRandomName(npcController.GetGender()) + " (" + npcController.GetNPCType() + ")"; // set the NPC name text based on the NPC's gender and type

        animator.SetTrigger("openMenu"); // trigger the open menu animation

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine); // stop any ongoing fade coroutine
        fadeCoroutine = StartCoroutine(Fade(menuPanel, 1f, menuFadeDuration)); // fade in the menu

    }

    public void CloseMenu() {

        isMenuOpen = false; // set the menu state to closed

        animator.SetTrigger("closeMenu"); // trigger the close menu animation

        currDialogueIndex = 0; // reset the index
        if (dialogueCoroutine != null) StopCoroutine(dialogueCoroutine); // stop any existing dialogue typing coroutine

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine); // stop any ongoing fade coroutine
        fadeCoroutine = StartCoroutine(Fade(menuPanel, 0f, menuFadeDuration)); // fade out the menu

    }

    private void NextDialogueText() {

        if (currDialogueSequence == null || currDialogueSequence.Length == 0) return; // if there is no dialogue sequence, do nothing

        if (currDialogueIndex >= currDialogueSequence.Length) { // if we have reached the end of the dialogue sequence

            uiManager.CloseNPCMenu(); // close the NPC menu; call the UIManager method to close the NPC menu rather than this class directly to ensure the extra logic is executed (e.g., re-opening the hotbar UI)
            return;

        }

        // at this point, we have a valid dialogue line to display

        dialogueText.text = ""; // clear the current dialogue text

        if (dialogueCoroutine != null) StopCoroutine(dialogueCoroutine); // stop any existing dialogue typing coroutine
        dialogueCoroutine = StartCoroutine(TypeDialogue(currDialogueSequence[currDialogueIndex++])); // start typing the new dialogue text

    }

    private IEnumerator TypeDialogue(string text) {

        float waitTime = 1f / dialogueCharactersPerSecond; // calculate the wait time based on characters per second

        foreach (char c in text) {

            dialogueText.text += c; // append each character to the dialogue text
            yield return new WaitForSeconds(waitTime); // wait for the specified time before adding the next character

        }
    }

    private IEnumerator Fade(CanvasGroup ui, float targetAlpha, float duration) {

        float currentTime = 0f;
        float startAlpha = ui.alpha;

        ui.gameObject.SetActive(true); // ensure UI is active before fading

        while (currentTime < duration) {

            currentTime += Time.deltaTime;
            ui.alpha = Mathf.Lerp(startAlpha, targetAlpha, currentTime / duration);
            yield return null;

        }

        ui.alpha = targetAlpha; // ensure final alpha is set

        // if the target alpha is 0, disable the UI
        if (targetAlpha == 0f)
            ui.gameObject.SetActive(false);

        fadeCoroutine = null; // reset the coroutine reference

    }

    public bool IsMenuOpen() => isMenuOpen;

}
