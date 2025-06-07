using System;
using UnityEngine;

public class TimeManager : MonoBehaviour {

    [Header("Time")]
    [SerializeField, Tooltip("How many real seconds per in-game minute. 1 = real time, 0.5 = 2x speed, etc."), Min(0.01f)] private float realSecondsPerGameMinute;
    [SerializeField] private TimePreset startTimePreset; // preset time to start at
    private int hour;
    private int minute;
    private bool isAM;
    private float timer;

    [Header("Lighting")]
    [SerializeField] private Light sunLight;
    [SerializeField] private float sunSmoothing;

    private void Start() {

        // set time to preset time
        hour = startTimePreset.GetHour();
        minute = startTimePreset.GetMinute();
        isAM = startTimePreset.IsAM();

        UpdateSunRotation(); // set sun rotation to initial time

    }

    private void Update() {

        timer += Time.deltaTime;

        while (timer >= realSecondsPerGameMinute) {

            timer -= realSecondsPerGameMinute;
            UpdateTime();

        }

        UpdateSunRotation();

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

    private void UpdateSunRotation() {

        // convert 12-hour clock + AM/PM to 24-hour time
        int hour24 = hour % 12;
        if (!isAM) hour24 += 12;

        float totalMinutes = hour24 * 60f + minute; // calculate total minutes since midnight

        float timePercent = totalMinutes / 1440f; // convert to a 0-1 range (1440 minutes in a day)

        float sunAngle = timePercent * 360f; // rotate sun: 0° at midnight, 180° at noon, 360° at next midnight

        sunLight.transform.rotation = Quaternion.Slerp(sunLight.transform.rotation, Quaternion.Euler(new Vector3(sunAngle - 90f, 170f, 0)), Time.deltaTime * sunSmoothing);

    }

    public int GetHour() => hour;

    public int GetMinute() => minute;

    public bool IsAM() => isAM;

}

[Serializable]
public class TimePreset {

    [Header("Settings")]
    [SerializeField, Range(1, 12)] private int hour;
    [SerializeField, Range(0, 59)] private int minute;
    [SerializeField] private bool isAM;

    public int GetHour() => hour;

    public int GetMinute() => minute;

    public bool IsAM() => isAM;

}