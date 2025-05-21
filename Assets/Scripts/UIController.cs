using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour {

    [Header("References")]
    private TimeManager timeManager;

    [Header("UI References")]
    [SerializeField] private TMP_Text timeText;

    private void Start() {

        timeManager = FindFirstObjectByType<TimeManager>();
        UpdateTimeText(timeManager.GetHour(), timeManager.GetMinute(), timeManager.IsAM());

    }

    private void Update() => UpdateTimeText(timeManager.GetHour(), timeManager.GetMinute(), timeManager.IsAM());

    private void UpdateTimeText(int hour, int minute, bool isAM) => timeText.text = $"{hour:00}:{minute:00} " + (isAM ? "AM" : "PM");

}
