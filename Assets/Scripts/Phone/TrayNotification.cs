using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrayNotification : MonoBehaviour {

    [Header("UI References")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text appNameText;
    [SerializeField] private TMP_Text descriptionText;

    public void Initialize(NotificationData notificationData) {

        this.icon.sprite = notificationData.GetIcon();
        this.appNameText.text = notificationData.GetAppName();
        this.descriptionText.text = notificationData.GetDescription();

    }
}
