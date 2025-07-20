using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhoneManager : MonoBehaviour {

    [Header("References")]
    private TimeManager timeManager;
    private UIManager uiManager;
    private Animator animator;
    private Coroutine refreshLayoutCoroutine;

    [Header("UI References")]
    [SerializeField] private Transform homeMenu;
    [SerializeField] private AppButton appButtonPrefab;
    [SerializeField] private Button homeButton;

    [Header("Apps")]
    [SerializeField] private PhoneApp[] phoneApps;
    private PhoneApp openedApp; // reference to the currently opened app; null signifies home menu is open

    [Header("State")]
    [SerializeField] private KeyCode phoneCycleKey;
    private PhoneState phoneState;

    [Header("Time")]
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text dayText;

    private void Start() {

        timeManager = FindFirstObjectByType<TimeManager>();
        uiManager = FindFirstObjectByType<UIManager>();
        animator = GetComponent<Animator>();

        // clear all children of the home menu to avoid duplicates
        foreach (Transform child in homeMenu)
            Destroy(child.gameObject);

        // create a button for each app and initialize it
        foreach (PhoneApp app in phoneApps) {

            AppButton appButton = Instantiate(appButtonPrefab, homeMenu);
            appButton.transform.name = app.GetAppName() + "AppButton"; // set the name of the button to the app name
            appButton.Initialize(app.GetAppName(), app.GetAppIcon());
            app.Initialize(this, appButton);
            app.ForceCloseApp(); // ensure the app is closed initially

        }

        RefreshLayout(homeMenu.GetComponent<RectTransform>()); // refresh the layout of the home menu to fit the new buttons

        homeButton.onClick.AddListener(() => {

            openedApp?.CloseApp();
            animator.SetTrigger("pressHomeButton"); // trigger the animation to press the home button

        });

        phoneState = PhoneState.PutAway; // initialize phone state to PutAway by default
        animator.SetTrigger("putAwayPhone"); // set the initial animation state to put away the phone

        UpdateTimeHUD(timeManager.GetDay(), timeManager.GetHour(), timeManager.GetMinute(), timeManager.IsAM());

    }

    private void Update() {

        if (Input.GetKeyDown(phoneCycleKey)) {

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

            uiManager.OnPhoneStateCycle(); // notify UIManager of phone state change

        }

        UpdateTimeHUD(timeManager.GetDay(), timeManager.GetHour(), timeManager.GetMinute(), timeManager.IsAM());

    }

    public void OnAppOpened(PhoneApp app) => openedApp = app; // set the currently opened app

    private void UpdateTimeHUD(int day, int hour, int minute, bool isAM) {

        timeText.text = $"{hour:00}:{minute:00} " + (isAM ? "AM" : "PM");
        dayText.text = $"Day {day}";

    }

    public bool IsPhoneToFace() => phoneState == PhoneState.ToFace;

    private void RefreshLayout(RectTransform root) {

        if (refreshLayoutCoroutine != null) StopCoroutine(refreshLayoutCoroutine); // stop any existing layout refresh coroutine
        refreshLayoutCoroutine = StartCoroutine(HandleRefreshLayout(root));

    }

    private IEnumerator HandleRefreshLayout(RectTransform root) {

        yield return null; // wait for the end of the frame to ensure all UI elements are properly initialized

        foreach (LayoutGroup layoutGroup in root.GetComponentsInChildren<LayoutGroup>())
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());

        refreshLayoutCoroutine = null; // reset the coroutine reference after completion

    }
}

[Serializable]
public class PhoneApp {

    [Header("References")]
    private PhoneManager phoneManager;
    private MonoBehaviour coroutineHost; // host for coroutines so they can be run through this class
    private readonly Coroutine appCloseCoroutine;

    [Header("UI References")]
    [SerializeField] private CanvasGroup appMenu;
    private Animator appAnimator;

    [Header("Data")]
    [SerializeField] private string appName;
    [SerializeField] private Sprite appIcon;

    public void Initialize(PhoneManager phoneManager, AppButton appButton) {

        this.phoneManager = phoneManager;
        appAnimator = appMenu.GetComponent<Animator>();
        coroutineHost = phoneManager; // Use PhoneManager as the host for coroutines

        appButton.GetComponent<Button>().onClick.AddListener(OpenApp); // add listener to the app button to open the app when clicked

    }

    public void OpenApp() {

        if (appCloseCoroutine != null) coroutineHost.StopCoroutine(appCloseCoroutine); // stop any existing close coroutine

        appMenu.gameObject.SetActive(true); // show the app menu
        appAnimator.SetTrigger("openApp"); // trigger the animation to open the app
        phoneManager.OnAppOpened(this); // notify the PhoneManager that this app is opened

    }

    public void CloseApp() {

        if (appCloseCoroutine != null) coroutineHost.StopCoroutine(appCloseCoroutine); // stop any existing close coroutine
        coroutineHost.StartCoroutine(HandleAppClose()); // start the coroutine to handle app close animation

    }

    public void ForceCloseApp() {

        if (appCloseCoroutine != null) coroutineHost.StopCoroutine(appCloseCoroutine); // stop any existing close coroutine
        appMenu.gameObject.SetActive(false); // hide the app menu immediately without animation
        phoneManager.OnAppOpened(null); // notify the PhoneManager that no app is opened (home screen is open)

    }

    private IEnumerator HandleAppClose() {

        appAnimator.SetTrigger("closeApp"); // trigger the animation to close the app
        yield return null; // wait for the next frame to ensure the animation starts
        yield return new WaitForSeconds(appAnimator.GetCurrentAnimatorStateInfo(0).length); // wait for the animation to finish
        appMenu.gameObject.SetActive(false); // hide the app menu after the animation is done
        phoneManager.OnAppOpened(null); // notify the PhoneManager that no app is opened (home screen is open)

    }

    public string GetAppName() => appName;

    public Sprite GetAppIcon() => appIcon;

}

public enum PhoneState {

    PutAway, TakenOut, ToFace

}
