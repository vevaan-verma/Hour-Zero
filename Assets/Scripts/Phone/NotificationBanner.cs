using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationBanner : MonoBehaviour {

    [Header("References")]
    private Animator animator;
    private Coroutine displayCoroutine;
    private Coroutine dismissCoroutine;

    [Header("UI References")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text appNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button dismissButton;
    [SerializeField, Tooltip("Slider to show the time remaining for the notification")] private Slider timeSlider;

    [Header("Settings")]
    private float notificationDisplayDuration;

    [Header("Actions")]
    public Action onNotificationDismiss;

    public void Initialize(NotificationData notificationData, float notificationDisplayDuration) {

        //this.icon.sprite = notificationData.GetIcon();
        this.appNameText.text = notificationData.GetAppName();
        this.descriptionText.text = notificationData.GetDescription();
        this.notificationDisplayDuration = notificationDisplayDuration;

        dismissButton.onClick.AddListener(Dismiss); // add listener to dismiss button to call the Dismiss method when clicked

        animator = GetComponent<Animator>();

    }

    public void Display() {

        if (displayCoroutine != null) return; // if a display coroutine is already running, do not start a new one
        displayCoroutine = StartCoroutine(HandleDisplay());

    }

    private IEnumerator HandleDisplay() {

        animator.SetTrigger("slideIn");

        yield return null; // wait for the slide in animation to start
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length); // wait for the slide in animation to finish

        timeSlider.minValue = 0f; // set the min value of the slider to 0
        timeSlider.maxValue = notificationDisplayDuration; // set the max value of the slider to the notification display duration

        float currentTime = notificationDisplayDuration; // initialize the current time to the notification display duration

        while (currentTime > 0f) { // loop while the current time is greater than 0

            timeSlider.value = currentTime; // update the slider value to the current time
            currentTime -= Time.deltaTime; // decrease the current time by the time since the last frame
            yield return null; // wait for the next frame

        }

        timeSlider.value = 0f; // ensure the slider ends at the target value

        Dismiss(); // call the Dismiss method to slide out the notification

    }

    public void Dismiss() {

        if (dismissCoroutine != null) return;
        dismissCoroutine = StartCoroutine(HandleDismiss());

    }

    private IEnumerator HandleDismiss() {

        animator.SetTrigger("slideOut");

        yield return null; // wait for the slide out animation to start
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length); // wait for the slide out animation to finish

        onNotificationDismiss?.Invoke(); // invoke the dismiss action if set

        // no need to destroy the banner, as this is handled by the PhoneManager when the notification is dismissed

    }
}

public class NotificationData {

    [Header("Data")]
    [SerializeField, Tooltip("Icon for the notification")] private Sprite icon;
    [SerializeField, Tooltip("Name of the app sending the notification")] private string appName;
    [SerializeField] private string description;

    public NotificationData(Sprite icon, string appName, string description) {

        this.icon = icon;
        this.appName = appName;
        this.description = description;

    }

    public Sprite GetIcon() => icon;

    public string GetAppName() => appName;

    public string GetDescription() => description;

}
