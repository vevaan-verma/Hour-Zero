using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    [Header("References")]
    private TimeManager timeManager;
    private Animator animator;

    [Header("UI References")]
    [SerializeField] private TMP_Text timeText;

    [Header("Backpack")]
    [SerializeField] private CanvasGroup backpackUI;
    [SerializeField] private Button closeBackpackButton;
    private Coroutine backpackCoroutine;
    private bool isBackpackOpen;

    [Header("Settings")]
    [SerializeField] private float backpackFadeDuration;
    [SerializeField] private KeyCode backpackKey;

    private void Start() {

        // hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        timeManager = FindFirstObjectByType<TimeManager>();
        animator = GetComponent<Animator>();

        UpdateTimeText(timeManager.GetHour(), timeManager.GetMinute(), timeManager.IsAM());

        backpackUI.gameObject.SetActive(false); // ensure backpack UI is inactive at start
        closeBackpackButton.onClick.AddListener(ToggleBackpack); // add listener to close backpack button (toggle can be used since the button can only be clicked when backpack is open)

    }

    private void Update() {

        UpdateTimeText(timeManager.GetHour(), timeManager.GetMinute(), timeManager.IsAM());

        // toggle backpack UI
        if (Input.GetKeyDown(backpackKey))
            ToggleBackpack();

        // close backpack if escape is pressed and backpack is open
        if (Input.GetKeyDown(KeyCode.Escape) && isBackpackOpen)
            ToggleBackpack();

    }

    private void UpdateTimeText(int hour, int minute, bool isAM) => timeText.text = $"{hour:00}:{minute:00} " + (isAM ? "AM" : "PM");

    public void ToggleBackpack() {

        if (backpackCoroutine != null) StopCoroutine(backpackCoroutine); // stop any existing backpack animation coroutine
        backpackCoroutine = StartCoroutine(HandleBackpackAnim(backpackUI)); // start backpack animation coroutine

    }

    private IEnumerator HandleBackpackAnim(CanvasGroup ui) {

        if (isBackpackOpen) {

            // close backpack
            isBackpackOpen = false; // set the state to closed before waiting for animation because it feels better if the player can move and look around while the backpack is closing
            closeBackpackButton.interactable = false; // disable close button to prevent multiple clicks (as this could mess with the toggle logic)
            backpackUI.gameObject.SetActive(true); // ensure UI is active before fading

            animator.SetTrigger("closeBackpack"); // trigger close animation

            Cursor.lockState = CursorLockMode.Locked; // lock cursor
            Cursor.visible = false; // hide cursor

            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length); // wait for the animation to finish
            ui.gameObject.SetActive(false); // disable UI after animation

        } else {

            // open backpack
            isBackpackOpen = true;
            closeBackpackButton.interactable = true; // enable close button to allow closing backpack
            backpackUI.gameObject.SetActive(true); // ensure UI is active before fading

            animator.SetTrigger("openBackpack"); // trigger open animation

            Cursor.lockState = CursorLockMode.None; // unlock cursor
            Cursor.visible = true; // make cursor visible

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

    }

    public bool IsBackpackOpen() => isBackpackOpen;

}
