using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private HomeButton homeButton;

    [Header("Apps")]
    [SerializeField] private AppView[] phoneAppViews;
    private PhoneView[] phoneViews; // array of phone views for easy access by view type
    private PhoneView openedView; // reference to the currently opened view; null signifies home menu is open

    [Header("Tray Notifications")]
    [SerializeField] private NotificationTrayView notificationTrayView;
    private List<NotificationData> trayNotifications = new List<NotificationData>(); // list of notifications in the tray; used to display notifications in the notification tray view

    [Header("Banner Notifications")]
    [SerializeField] private Transform bannerNotificationSection;
    [SerializeField] private BannerNotification bannerNotificationPrefab;
    [SerializeField] private float notificationDisplayDuration;
    private BannerNotification currBannerNotification; // reference to the currently displayed banner notification; null signifies no notification is being displayed
    private Queue<NotificationData> notificationQueue = new Queue<NotificationData>(); // queue of notifications to be displayed; used to display notifications in the banner notification section

    [Header("State")]
    [SerializeField] private KeyCode phoneCycleKey;
    private PhoneState[] phoneStateOrder;
    private PhoneState phoneState;

    [Header("Time")]
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text dayText;

    [Header("Actions")]
    public Action onSendNotification; // action to notify when a notification is sent

    [Header("Flashlight")]
    [SerializeField] private GameObject flashlight;
    [SerializeField] private KeyCode flashlightKey;

    private void Start() {

        timeManager = FindFirstObjectByType<TimeManager>();
        uiManager = FindFirstObjectByType<UIManager>();
        animator = GetComponent<Animator>();

        phoneStateOrder = (PhoneState[]) Enum.GetValues(typeof(PhoneState)); // get all phone states in order (order is defined by the enum declaration)

        // duplicate phoneAppViews array to phoneViews for easy access by view type
        phoneViews = new PhoneView[phoneAppViews.Length + 1]; // create a new array with one extra slot for the notification tray view

        for (int i = 0; i < phoneAppViews.Length; i++)
            phoneViews[i] = phoneAppViews[i]; // copy each app view to the phone views array

        phoneViews[^1] = notificationTrayView; // add the notification tray view to the end of the phone views array

        // clear all children of the home menu to avoid duplicates
        foreach (Transform child in homeMenu)
            Destroy(child.gameObject);

        // create a button for each app and initialize it
        foreach (AppView appView in phoneAppViews) {

            ViewType viewType = appView.GetViewType(); // get the view type of the app

            // ensure the app view is not of type NotificationTray
            if (viewType == ViewType.NotificationTray)
                Debug.LogError("NotificationTray view type cannot have an app button.");

            AppButton appButton = Instantiate(appButtonPrefab, homeMenu);
            appButton.transform.name = appView.GetName() + "AppButton"; // set the name of the button to the app name
            appButton.Initialize(appView.GetName(), appView.GetIcon());
            appButton.GetComponent<Button>().onClick.AddListener(() => OpenView(viewType)); // add listener to the app button to open the app when clicked; use the PhoneManager OpenView method, not the one in PhoneView, to ensure the PhoneManager logic is executed (e.g., closing other views)
            appView.Initialize(this, appButton);
            ForceCloseView(viewType); // ensure the app is closed initially

        }

        notificationTrayView.Initialize(this);
        ForceCloseView(ViewType.NotificationTray); // ensure the notification tray is closed initially

        RefreshLayout(homeMenu.GetComponent<RectTransform>()); // refresh the layout of the home menu to fit the new buttons

        homeButton.onPressBegin += () => animator.SetTrigger("pressHomeButton"); // trigger the animation to press the home button when the press begins

        homeButton.onClickReleased += () => {

            CloseView(); // close the currently opened view if there is one
            animator.SetTrigger("releaseHomeButton"); // trigger the animation to release the home button

        };

        homeButton.onLongPressReleased += () => {

            notificationQueue.Clear(); // clear the notification queue when the notification tray is opened
            currBannerNotification?.Dismiss(); // dismiss the currently displayed banner notification if there is one

            CloseView(); // close the currently opened view if there is one
            OpenView(ViewType.NotificationTray); // open the notification tray view

            animator.SetTrigger("releaseHomeButton"); // trigger the animation to release the home button

        };

        phoneState = PhoneState.Pocket; // initialize phone state to PutAway by default
        animator.SetTrigger("phoneToPocket"); // set the initial animation state to put away the phone

        homeMenu.gameObject.SetActive(true); // make sure the home menu is active by default

        UpdateTimeHUD(timeManager.GetDay(), timeManager.GetHour(), timeManager.GetMinute(), timeManager.IsAM());

        flashlight.SetActive(false); // ensure flashlight is off by default

    }

    private void Update() {

        if (Input.GetKeyDown(phoneCycleKey) && !(uiManager.IsMenuOpen() && phoneState != PhoneState.Face)) { // check if the phone cycle key is pressed and no menu, other than the phone being to the player's face, is open (because the phone should not be cycled through when a non-phone-to-face menu is open)

            // cycle through phone states and loop around
            phoneState++;

            // reset to first state if it exceeds the last state
            if (phoneState > phoneStateOrder[^1])
                phoneState = phoneStateOrder[0];

            switch (phoneState) {

                case PhoneState.Pocket:
                    flashlight.SetActive(false); // turn off flashlight when phone is put away
                    // it is not necessarily required to lock or hide the cursor here, but it is done for consistency
                    Cursor.lockState = CursorLockMode.Locked; // lock cursor when phone is put away
                    Cursor.visible = false; // hide cursor when phone is put away
                    animator.SetTrigger("phoneToPocket");
                    break;

                case PhoneState.Face:
                    Cursor.lockState = CursorLockMode.None; // unlock cursor when phone is to face
                    Cursor.visible = true; // make cursor visible when phone is to face
                    animator.SetTrigger("phoneToFace");
                    break;

                case PhoneState.Hand:
                    Cursor.lockState = CursorLockMode.Locked; // lock cursor when phone is in hand
                    Cursor.visible = false; // hide cursor when phone is in hand
                    animator.SetTrigger("phoneToHand");
                    break;

            }

            uiManager.OnPhoneStateCycle(); // notify UIManager of phone state change

        }

        UpdateTimeHUD(timeManager.GetDay(), timeManager.GetHour(), timeManager.GetMinute(), timeManager.IsAM());

        // toggle flashlight state when flashlight key is pressed and phone is not in pocket
        if (phoneState != PhoneState.Pocket && Input.GetKeyDown(flashlightKey))
            flashlight.SetActive(!flashlight.activeSelf);

    }

    public void OpenView(ViewType viewType) {

        if (openedView != null) {

            if (openedView.GetViewType() == viewType) return; // if the view is already opened, do nothing
            openedView.CloseView(); // close the currently opened view before opening a new one

        }

        PhoneView viewToOpen = Array.Find(phoneViews, app => app.GetViewType() == viewType); // find the view to open based on the specified view type
        viewToOpen.OpenView(); // open the view

        openedView = viewToOpen; // set the currently opened view to the opened view

    }

    public void CloseView() {

        if (openedView == null) return; // if no view is opened, do nothing

        openedView.CloseView(); // close the currently opened view
        openedView = null; // reset the currently opened view reference

    }

    public void ForceCloseView(ViewType viewType) {

        PhoneView viewToClose = Array.Find(phoneViews, app => app.GetViewType() == viewType); // find the view to close based on the specified view type

        if (viewToClose != null) { // check if the view to close exists

            viewToClose.ForceCloseView(); // force close the view
            openedView = null; // reset the currently opened view reference

        }
    }

    #region BANNER NOTIFICATIONS
    public void SendNotification(ViewType viewType, string description) {

        if (viewType == ViewType.NotificationTray) {

            Debug.LogWarning("Cannot send notification to NotificationTray view type.");
            return; // do not send the notification if the view type is NotificationTray because the notification tray cannot have notifications

        }

        // at this point, the viewType is guaranteed to be an AppView type

        AppView appView = (AppView) Array.Find(phoneViews, view => view.GetViewType() == viewType); // find the app view for the specified view type
        appView.IncrementNotificationCount(); // increment the notification count for the app

        NotificationData notificationData = new NotificationData(appView.GetIcon(), appView.GetName(), description, viewType);
        trayNotifications.Add(notificationData);
        notificationQueue.Enqueue(notificationData);

        // if there is no current banner notification being displayed, display the next notification
        if (currBannerNotification == null)
            DisplayNextNotification();

        onSendNotification?.Invoke(); // invoke the action to notify that a notification has been sent

    }

    private void DisplayNextNotification() {

        if (notificationQueue.Count <= 0 || openedView is NotificationTrayView) return; // if there are no notifications in the queue, do nothing

        NotificationData nextNotificationData = notificationQueue.Peek(); // get the next notification data from the queue

        currBannerNotification = Instantiate(bannerNotificationPrefab, bannerNotificationSection);
        currBannerNotification.Initialize(nextNotificationData, this, notificationDisplayDuration); // initialize the banner notification with the notification data and display duration
        currBannerNotification.Display(); // display the banner notification
        currBannerNotification.onNotificationDismiss += OnNotificationDismiss; // add a listener to handle notification dismissal

    }

    private void OnNotificationDismiss() {

        if (notificationQueue.Count <= 0) return; // if there are no notifications in the queue, do nothing

        notificationQueue.Dequeue(); // remove the current notification from the queue
        currBannerNotification.onNotificationDismiss -= DisplayNextNotification; // remove the listener from the current banner notification
        Destroy(currBannerNotification.gameObject); // destroy the current banner notification to free up resources
        currBannerNotification = null; // reset the current banner notification reference

        DisplayNextNotification(); // display the next notification if available

    }

    public void ClearAppTrayNotifications(ViewType viewType) {

        if (viewType == ViewType.NotificationTray) {

            Debug.LogWarning("Cannot clear notifications for NotificationTray view type.");
            return; // do not clear notifications if the view type is NotificationTray because the notification tray cannot have notifications

        }

        // at this point, the viewType is guaranteed to be an AppView type

        AppView appView = (AppView) Array.Find(phoneViews, view => view.GetViewType() == viewType); // find the app view for the specified view type

        appView.ResetNotificationCount(); // reset the notification count for the app
        trayNotifications.RemoveAll(notification => notification.GetAppName() == appView.GetName()); // remove all notifications for the app from the tray notifications

    }
    #endregion

    private void UpdateTimeHUD(int day, int hour, int minute, bool isAM) {

        timeText.text = $"{hour:00}:{minute:00} " + (isAM ? "AM" : "PM");
        dayText.text = $"Day {day}";

    }

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

    public PhoneState GetPhoneState() => phoneState;

    public List<NotificationData> GetTrayNotifications() => trayNotifications;

}

public enum ViewType {

    Bunka, Todo, Notes, FindAWay, NotificationTray

}

public enum PhoneState {

    Pocket, Face, Hand

}
