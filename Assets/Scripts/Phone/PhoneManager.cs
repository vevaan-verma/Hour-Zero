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
    private PhoneView openedPhoneView; // reference to the currently opened view; null signifies home menu is open

    [Header("Tray Notifications")]
    [SerializeField] private NotificationTrayView notificationTrayView;
    private NotificationTrayManager notificationTrayManager;
    private List<NotificationData> trayNotifications;

    [Header("Banner Notifications")]
    [SerializeField] private Transform bannerNotificationSection;
    [SerializeField] private BannerNotification bannerNotificationPrefab;
    [SerializeField] private float notificationDisplayDuration;
    private BannerNotification currBannerNotification; // reference to the currently displayed banner notification; null signifies no notification is being displayed
    private Queue<NotificationData> notificationQueue;

    [Header("State")]
    [SerializeField] private KeyCode phoneCycleKey;
    private PhoneState[] phoneStateOrder;
    private PhoneState phoneState;

    [Header("Time")]
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text dayText;

    [Header("Actions")]
    public Action onSendNotification; // action to notify when a notification is sent

    private void Start() {

        timeManager = FindFirstObjectByType<TimeManager>();
        uiManager = FindFirstObjectByType<UIManager>();
        animator = GetComponent<Animator>();
        notificationTrayManager = FindFirstObjectByType<NotificationTrayManager>();

        phoneStateOrder = (PhoneState[]) Enum.GetValues(typeof(PhoneState)); // get all phone states in order (order is defined by the enum declaration)

        trayNotifications = new List<NotificationData>(); // initialize the list of tray notifications
        notificationQueue = new Queue<NotificationData>(); // initialize the notification queue

        // clear all children of the home menu to avoid duplicates
        foreach (Transform child in homeMenu)
            Destroy(child.gameObject);

        // create a button for each app and initialize it
        foreach (AppView appView in phoneAppViews) {

            AppButton appButton = Instantiate(appButtonPrefab, homeMenu);
            appButton.transform.name = appView.GetName() + "AppButton"; // set the name of the button to the app name
            appButton.Initialize(appView.GetName(), appView.GetIcon());
            appView.Initialize(this, appButton);
            appView.ForceCloseView(); // ensure the app is closed initially

        }

        notificationTrayView.Initialize(this);
        notificationTrayView.ForceCloseView(); // ensure the notification tray is closed initially

        RefreshLayout(homeMenu.GetComponent<RectTransform>()); // refresh the layout of the home menu to fit the new buttons

        homeButton.onPressBegin += () => animator.SetTrigger("pressHomeButton"); // trigger the animation to press the home button when the press begins

        homeButton.onClickReleased += () => {

            openedPhoneView?.CloseView(); // close the currently opened view if there is one
            animator.SetTrigger("releaseHomeButton"); // trigger the animation to release the home button

        };

        homeButton.onLongPressReleased += () => {

            notificationQueue.Clear(); // clear the notification queue when the notification tray is opened
            currBannerNotification?.Dismiss(); // dismiss the currently displayed banner notification if there is one

            openedPhoneView?.CloseView(); // close the currently opened view if there is one
            notificationTrayView.OpenView(); // open the notification tray view when the home button is long-pressed

            animator.SetTrigger("releaseHomeButton"); // trigger the animation to release the home button

        };

        phoneState = PhoneState.Pocket; // initialize phone state to PutAway by default
        animator.SetTrigger("phoneToPocket"); // set the initial animation state to put away the phone

        homeMenu.gameObject.SetActive(true); // make sure the home menu is active by default

        UpdateTimeHUD(timeManager.GetDay(), timeManager.GetHour(), timeManager.GetMinute(), timeManager.IsAM());

    }

    private void Update() {

        if (Input.GetKeyDown(KeyCode.U))
            SendNotification(AppType.Bunka, "This is a test notification for the Bunka app!"); // test notification for Bunka app

        if (Input.GetKeyDown(phoneCycleKey) && !(uiManager.IsMenuOpen() && phoneState != PhoneState.Face)) { // check if the phone cycle key is pressed and no menu, other than the phone being to the player's face, is open (because the phone should not be cycled through when a non-phone-to-face menu is open)

            // cycle through phone states and loop around
            phoneState++;

            // reset to first state if it exceeds the last state
            if (phoneState > phoneStateOrder[^1])
                phoneState = phoneStateOrder[0];

            switch (phoneState) {

                case PhoneState.Pocket:
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

    }

    public void OnViewOpened(PhoneView phoneView) => openedPhoneView = phoneView; // set the currently opened view

    #region BANNER NOTIFICATIONS
    public void SendNotification(AppType appType, string description) {

        AppView appView = Array.Find(phoneAppViews, app => app.GetAppType() == appType); // find the app view for the specified app type
        appView.IncrementNotificationCount(); // increment the notification count for the app

        NotificationData notificationData = new NotificationData(appView.GetIcon(), appView.GetName(), description, appType);
        trayNotifications.Add(notificationData);
        notificationQueue.Enqueue(notificationData);

        // if there is no current banner notification being displayed, display the next notification
        if (currBannerNotification == null)
            DisplayNextNotification();

        onSendNotification?.Invoke(); // invoke the action to notify that a notification has been sent

    }

    private void DisplayNextNotification() {

        if (notificationQueue.Count <= 0 || openedPhoneView is NotificationTrayView) return; // if there are no notifications in the queue, do nothing

        NotificationData nextNotificationData = notificationQueue.Peek(); // get the next notification data from the queue

        currBannerNotification = Instantiate(bannerNotificationPrefab, bannerNotificationSection);
        currBannerNotification.Initialize(nextNotificationData, notificationDisplayDuration); // initialize the banner notification with the notification data and display duration
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

    public void ClearAppTrayNotifications(AppType appType) {

        AppView appView = Array.Find(phoneAppViews, app => app.GetAppType() == appType); // find the app view for the specified app type

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

public enum AppType {

    Bunka, Todo, Notes, FindAWay

}

public enum PhoneState {

    Pocket, Face, Hand

}
