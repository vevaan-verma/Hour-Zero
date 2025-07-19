using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhoneManager : MonoBehaviour {

    [Header("References")]
    private TimeManager timeManager;
    private Animator animator;

    [Header("UI References")]
    [SerializeField] private Button bunkaAppButton;
    [SerializeField] private CanvasGroup bunkaAppMenu;

    [Header("State")]
    private PhoneState phoneState;

    [Header("Time")]
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text dayText;

    private void Start() {

        timeManager = FindFirstObjectByType<TimeManager>();
        animator = GetComponent<Animator>();

        bunkaAppMenu.gameObject.SetActive(false); // hide the bunka app by default
        bunkaAppButton.onClick.AddListener(() => {

            bunkaAppMenu.gameObject.SetActive(true); // show the bunka app menu
            animator.SetTrigger("openBunkaApp"); // trigger the animation to open the bunka app

        });

        phoneState = PhoneState.PutAway; // initialize phone state to PutAway by default
        animator.SetTrigger("putAwayPhone"); // set the initial animation state to put away the phone

        UpdateTimeHUD(timeManager.GetDay(), timeManager.GetHour(), timeManager.GetMinute(), timeManager.IsAM());

    }

    private void Update() => UpdateTimeHUD(timeManager.GetDay(), timeManager.GetHour(), timeManager.GetMinute(), timeManager.IsAM());

    public void CyclePhoneState() {

        // cycle through phone states and loop around
        phoneState++;

        // reset to PutAway if it exceeds ToFace to loop through states
        if (phoneState > PhoneState.ToFace)
            phoneState = PhoneState.PutAway;

        switch (phoneState) {

            case PhoneState.PutAway:
                Cursor.lockState = CursorLockMode.Locked; // lock cursor when phone is put away
                Cursor.visible = false; // hide cursor when phone is put away
                animator.SetTrigger("putAwayPhone");
                break;

            case PhoneState.TakenOut:
                // no need to lock or hide cursor here, since it is guaranteed to be locked and hidden when the phone is put away
                animator.SetTrigger("takeOutPhone");
                break;

            case PhoneState.ToFace:
                animator.SetTrigger("toFacePhone");
                Cursor.lockState = CursorLockMode.None; // unlock cursor when phone is to face
                Cursor.visible = true; // make cursor visible when phone is to face
                break;

        }
    }

    private void UpdateTimeHUD(int day, int hour, int minute, bool isAM) {

        timeText.text = $"{hour:00}:{minute:00} " + (isAM ? "AM" : "PM");
        dayText.text = $"Day {day}";

    }

    public bool IsPhoneToFace() => phoneState == PhoneState.ToFace;

}

public enum PhoneState {

    PutAway, TakenOut, ToFace

}
