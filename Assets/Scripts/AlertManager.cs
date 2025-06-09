using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AlertManager : MonoBehaviour {

    [Header("UI References")]
    [SerializeField] private TMP_Text alertText;
    private Queue<Alert> alertQueue;
    private Coroutine alertCoroutine;

    [Header("Settings")]
    [SerializeField, Tooltip("The amount of characters to type per second in the alert text")] private int charactersPerSecond;
    [SerializeField, Tooltip("Duration for the alert text to stay visible after fully typing but before fading out")] private float alertDuration;
    [SerializeField, Tooltip("Duration for the alert text to fade out")] private float alertFadeDuration;
    [SerializeField] private AlertColor[] alertColors;

    private void Start() {

        #region VALIDATION
        // make sure each alert type has a corresponding color
        AlertType[] alertTypes = (AlertType[]) Enum.GetValues(typeof(AlertType));

        foreach (AlertType type in alertTypes) {

            bool hasColor = false;

            foreach (AlertColor color in alertColors) {

                if (color.GetAlertType() == type) {

                    hasColor = true;
                    break;

                }
            }

            if (!hasColor)
                Debug.LogError($"No color defined for alert type: {type}");

        }
        #endregion

        alertQueue = new Queue<Alert>(); // initialize the alert queue
        alertText.gameObject.SetActive(false); // ensure alert text is inactive at start

    }

    public void SendAlert(Alert alert) {

        // TODO: decide if alerts should be queued or existing alerts should be canceled/replaced (or maybe if specific types of alerts should be queued while others replace existing ones)

        // if an alert is already being processed, add the new alert to the queue
        if (alertCoroutine != null) {

            alertQueue.Enqueue(alert);
            return;

        }

        //if (alertCoroutine != null) StopCoroutine(alertCoroutine); // stop any existing alert coroutine
        alertText.color = GetAlertColor(alert.GetAlertType()).GetColor(); // set alert text color based on alert type
        alertCoroutine = StartCoroutine(HandleAlert(alert.GetMessage())); // start alert animation coroutine

    }

    private IEnumerator HandleAlert(string line) {

        // ensure alert text is fully visible
        alertText.gameObject.SetActive(true);
        CanvasGroup canvasGroup = alertText.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;

        string textBuffer = null;
        alertText.text = ""; // clear previous alert text

        foreach (char c in line) {

            textBuffer += c;
            alertText.text = textBuffer;
            yield return new WaitForSeconds(1f / charactersPerSecond); // wait for the specified time before typing the next character

        }

        yield return new WaitForSeconds(alertDuration); // wait for the alert to stay visible after typing
        yield return Fade(canvasGroup, 0f, alertFadeDuration); // fade out the alert text

        alertCoroutine = null;

        if (alertQueue.Count > 0) { // if there are more alerts in the queue

            Alert nextAlert = alertQueue.Dequeue(); // get the next alert from the queue
            alertText.color = GetAlertColor(nextAlert.GetAlertType()).GetColor(); // set the color for the next alert
            alertCoroutine = StartCoroutine(HandleAlert(nextAlert.GetMessage())); // start handling the next alert

        }
    }

    private IEnumerator Fade(CanvasGroup ui, float targetAlpha, float duration) {

        float currentTime = 0f;
        float startAlpha = ui.alpha;

        ui.gameObject.SetActive(true); // ensure UI is active before fading

        while (currentTime < duration) {

            currentTime += Time.deltaTime;
            ui.alpha = Mathf.Lerp(startAlpha, targetAlpha, currentTime / duration);
            yield return null;

        }

        ui.alpha = targetAlpha; // ensure final alpha is set

        // if the target alpha is 0, disable the UI
        if (targetAlpha == 0f)
            ui.gameObject.SetActive(false);

    }

    private AlertColor GetAlertColor(AlertType alertType) {

        foreach (AlertColor color in alertColors)
            if (color.GetAlertType() == alertType)
                return color;

        return null; // return null if no color found for the alert type

    }
}

public enum AlertType {

    Success,
    Warning,
    Failure,
    Info

}

public class Alert {

    [Header("Data")]
    [SerializeField] private string message;
    [SerializeField] private AlertType alertType;

    public Alert(string message, AlertType alertType) {

        this.message = message;
        this.alertType = alertType;

    }

    public string GetMessage() => message;

    public AlertType GetAlertType() => alertType;

}

[Serializable]
public class AlertColor {

    [Header("Settings")]
    [SerializeField] private AlertType alertType;
    [SerializeField] private Color color;

    public AlertType GetAlertType() => alertType;

    public Color GetColor() => color;

}