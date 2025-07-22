using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NPCMenu : MonoBehaviour {

    [Header("References")]
    private TaskManager taskManager;
    private UIManager uiManager;
    private Animator animator;
    private NPCData currNPCData; // the current NPC data for this menu, set when the menu is opened

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
    [SerializeField] private Button untrackButton;

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

        taskManager = FindFirstObjectByType<TaskManager>();
        uiManager = FindFirstObjectByType<UIManager>();
        animator = GetComponent<Animator>();
        dialogueDatabase = FindFirstObjectByType<DialogueDatabase>();

        // we can use currNpcData in the button events because they will only be clicked when the menu is open, which means currNpcData will always be set

        teamButton.onClick.AddListener(() => {

            interactSection.SetActive(false); // hide the interact section
            dialogueSection.SetActive(true); // show the dialogue section

            if (taskManager.AssignRandomTask(currNPCData))
                currDialogueSequence = taskManager.GetActiveTask().GetTaskData().GetRandomDialogueSequence().GetDialogueLines();
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

        tradeButton.onClick.AddListener(() => uiManager.OpenTradeMenu(currNPCData.GetTradeData())); // open the trade menu for this NPC

        trackButton.onClick.AddListener(() => {

            currNPCData.GetNPCController().StartGeneralTracking();
            uiManager.CloseNPCMenu(); // close the NPC menu; call the UIManager method to close the NPC menu rather than this class directly to ensure the extra logic is executed (e.g., re-opening the hotbar UI)

        }); // start general tracking the NPC

        untrackButton.onClick.AddListener(() => {

            currNPCData.GetNPCController().StopGeneralTracking();
            uiManager.CloseNPCMenu(); // close the NPC menu; call the UIManager method to close the NPC menu rather than this class directly to ensure the extra logic is executed (e.g., re-opening the hotbar UI)

        }); // stop general tracking the NPC

        okayButton.onClick.AddListener(() => {

            NextDialogueText();
            EventSystem.current.SetSelectedGameObject(null); // deselect the button to allow for re-clicking

        });

        closeMenuButton.onClick.AddListener(() => uiManager.CloseNPCMenu()); // add listener to close menu button; call the UIManager method to close the NPC menu rather than this class directly to ensure the extra logic is executed (e.g., re-opening the hotbar UI)

        // activate the default interact section buttons
        teamButton.gameObject.SetActive(true); // team button is active by default
        talkButton.gameObject.SetActive(true); // talk button is active by default
        tradeButton.gameObject.SetActive(true); // trade button is active by default
        trackButton.gameObject.SetActive(true); // track button is active by default
        untrackButton.gameObject.SetActive(false); // untrack button is not active by default

        dialogueSection.SetActive(false); // ensure the dialogue section is hidden by default
        interactSection.SetActive(true); // ensure the interact section is active by default

        menuPanel.gameObject.SetActive(false); // make sure the menu is hidden by default

    }

    public void OpenMenu(NPCData npcData) {

        currNPCData = npcData; // set the NPC data for this menu

        isMenuOpen = true; // set the menu state to open
        menuPanel.gameObject.SetActive(true); // make sure the menu is active

        TradeData tradeData = npcData.GetTradeData(); // get the trade data for this NPC

        teamButton.gameObject.SetActive(!npcData.IsTeamMember());
        tradeButton.gameObject.SetActive(tradeData.GetInputItems().Length > 0 && tradeData.GetOutputItems().Length > 0); // only show the trade button if there is at least one input and output item in the trade data

        BaseTask activeTask = taskManager.GetActiveTask(); // get the current active task from the task manager
        bool trackingForTask = activeTask != null && activeTask.GetNPCData().Equals(npcData); // check if the active task is for this NPC, which would mean the player is already tracking this NPC for a task
        bool generalTracking = npcData.GetNPCController().IsGeneralTracking(); // check if the NPC is being tracked generally (not for a task)

        //                        tracking for task && general tracking         just tracking for task         just general tracking               none
        // trackButton                         not active                               active                      not active                    active
        // untrackButton                         active                               not active                      active                    not active

        if (trackingForTask && generalTracking) { // tracking for task && general tracking

            trackButton.gameObject.SetActive(false);
            untrackButton.gameObject.SetActive(true);

        } else if (trackingForTask && !generalTracking) { // just tracking for task

            trackButton.gameObject.SetActive(true);
            untrackButton.gameObject.SetActive(false);

        } else if (!trackingForTask && generalTracking) { // just general tracking

            trackButton.gameObject.SetActive(false);
            untrackButton.gameObject.SetActive(true);

        } else { // none

            trackButton.gameObject.SetActive(true);
            untrackButton.gameObject.SetActive(false);

        }

        dialogueSection.SetActive(false); // ensure the dialogue section is hidden by default
        interactSection.SetActive(true); // ensure the interact section is active by default

        npcNameText.text = npcData.GetName() + " (" + npcData.GetNPCType() + ")"; // set the NPC name text based on the NPC's sex and type

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
