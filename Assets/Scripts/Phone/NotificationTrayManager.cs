using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NotificationTrayManager : ViewManager {

    [Header("References")]
    private PhoneManager phoneManager;
    private RectTransform rectTransform;

    [Header("UI References")]
    [SerializeField] private Transform notificationTrayContents;
    [SerializeField] private TrayNotification trayNotificationPrefab;
    [SerializeField] private TMP_Text noNotificationsTextPrefab; // text to display when there are no notifications

    private void Awake() {

        phoneManager = FindFirstObjectByType<PhoneManager>();
        rectTransform = GetComponent<RectTransform>();

    }

    private new void OnEnable() {

        base.OnEnable();
        phoneManager.onSendNotification += RefreshApp; // subscribe to the notification event to refresh the app when a new notification is sent

    }

    private void OnDisable() => phoneManager.onSendNotification -= RefreshApp; // unsubscribe from the notification event to avoid memory leaks

    public override void RefreshApp() {

        // clear the notifications in the tray
        foreach (Transform child in notificationTrayContents)
            Destroy(child.gameObject);

        List<NotificationData> notifications = phoneManager.GetTrayNotifications();

        if (notifications == null || notifications.Count == 0) { // check if there are no notifications (either null or empty list)

            TMP_Text noNotificationsText = Instantiate(noNotificationsTextPrefab, notificationTrayContents); // instantiate the no active task text prefab since there are no active tasks

        } else { // there are notifications

            // populate the notification tray with the current tray notifications
            foreach (NotificationData notification in notifications) {

                TrayNotification trayNotification = Instantiate(trayNotificationPrefab, notificationTrayContents);
                trayNotification.Initialize(notification);

            }
        }

        RefreshLayout(rectTransform); // refresh the layout of the app UI

    }
}
