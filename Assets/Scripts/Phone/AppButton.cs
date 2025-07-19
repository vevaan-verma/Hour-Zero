using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AppButton : MonoBehaviour {

    [Header("UI References")]
    [SerializeField] private TMP_Text appNameText;
    [SerializeField] private Image appIcon;

    public void Initialize(string appName, Sprite appIcon) {

        appNameText.text = appName;
        this.appIcon.sprite = appIcon;

    }
}
