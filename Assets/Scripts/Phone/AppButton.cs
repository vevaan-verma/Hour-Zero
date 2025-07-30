using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AppButton : MonoBehaviour {

    [Header("UI References")]
    [SerializeField] private TMP_Text appNameText;
    [SerializeField] private Image appIcon;
    [SerializeField] private Transform notificationBadge;
    [SerializeField] private TMP_Text notificationBadgeText;

    public void Initialize(string appName, Sprite appIcon) {

        appNameText.text = appName;
        this.appIcon.sprite = appIcon;
        SetNotificationCount(0); // initialize the notification badge count to 0

    }

    public void SetNotificationCount(int count) {

        if (count > 0) {

            notificationBadge.gameObject.SetActive(true); // show the badge if count is greater than 0

            // if the count is greater than 9, display "9+" to indicate more than 9 notifications
            if (count > 9)
                notificationBadgeText.text = "9+";
            else // otherwise, display the actual count
                notificationBadgeText.text = count.ToString();

        } else {

            notificationBadge.gameObject.SetActive(false); // hide the badge if count is 0

        }
    }
}
