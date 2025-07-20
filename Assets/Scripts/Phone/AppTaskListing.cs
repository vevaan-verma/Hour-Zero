using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AppTaskListing : MonoBehaviour {

    [Header("UI References")]
    [SerializeField] private TMP_Text taskText;
    [SerializeField] private Image taskIcon;

    public void Initialize(string taskText, Sprite taskIcon) {

        this.taskText.text = taskText;
        this.taskIcon.sprite = taskIcon;

    }
}
