using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VehicleHUD : MonoBehaviour {

    [Header("UI References")]
    [SerializeField] private CanvasGroup hudPanel;
    [SerializeField] private Slider speedSlider;
    [SerializeField] private TMP_Text speedText;
    private Coroutine fadeCoroutine;

    [Header("Settings")]
    [SerializeField] private float hudFadeDuration;
    [SerializeField] private float speedSmoothTime;
    private float currentSliderVelocity;
    private bool isDrivingHUDOpen;

    private void Start() {

        hudPanel.alpha = 0f; // set initial alpha to 0
        hudPanel.gameObject.SetActive(false); // make sure the HUD is hidden by default

    }

    public void ShowHUD(float maxSpeedMPH) {

        isDrivingHUDOpen = true; // set the HUD state to open
        hudPanel.gameObject.SetActive(true); // make sure the HUD panel is active

        speedSlider.maxValue = maxSpeedMPH; // set the max value of the speed slider based on the vehicle's max speed (which is already in MPH, so no need to convert)

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine); // stop any ongoing fade coroutine
        fadeCoroutine = StartCoroutine(Fade(hudPanel, 1f, hudFadeDuration)); // fade in the HUD

    }

    public void HideHUD() {

        isDrivingHUDOpen = false; // set the HUD state to closed

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine); // stop any ongoing fade coroutine
        fadeCoroutine = StartCoroutine(Fade(hudPanel, 0f, hudFadeDuration)); // fade out the HUD

    }

    public void UpdateDrivingHUD(float speedInMPH) {

        speedSlider.value = Mathf.SmoothDamp(speedSlider.value, speedInMPH, ref currentSliderVelocity, speedSmoothTime); // smoothly update the slider value to reduce jitter
        speedText.text = $"{Mathf.Round(speedSlider.value)} MPH"; // set the speed text to the SLIDER VALUE rounded to the nearest whole number

    }

    public bool IsDrivingHUDOpen() => isDrivingHUDOpen;

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

        fadeCoroutine = null; // reset the coroutine reference

    }
}
