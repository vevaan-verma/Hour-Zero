using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour {

    [Header("UI References")]
    [SerializeField] private TMP_Text timeText;

    [Header("Time")]
    [SerializeField, Tooltip("How many real seconds per in-game minute. 1 = real time, 0.5 = 2x speed, etc."), Min(0.01f)] private float realSecondsPerGameMinute;
    private int hour;
    private int minute;
    private bool isAM;
    private float timer;

    private void Start() {

        // start the time at 12:00 AM
        hour = 12;
        isAM = true;

        UpdateTimeText();

    }

    private void Update() {

        timer += Time.deltaTime;

        while (timer >= realSecondsPerGameMinute) {

            timer -= realSecondsPerGameMinute;
            UpdateTime();
            UpdateTimeText();

        }
    }

    private void UpdateTime() {

        minute++;

        if (minute >= 60f) {

            minute = 0;
            hour++;

            if (hour > 12f)
                hour = 1;

            if (hour == 12f)
                isAM = !isAM;

        }
    }

    private void UpdateTimeText() {

        string ampm = isAM ? "AM" : "PM";
        timeText.text = $"{hour:00}:{minute:00} {ampm}";

    }
}
