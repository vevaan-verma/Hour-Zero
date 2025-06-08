using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BunkerSystemUI : MonoBehaviour {

    [Header("References")]
    private BunkerPanelManager bunkerPanelManager;
    private Coroutine sliderLerpCoroutine;

    [Header("UI References")]
    [SerializeField] private Slider durabilitySlider;
    [SerializeField] private TMP_Text durabilityText;
    [SerializeField] private TMP_Text statusText;

    [Header("Settings")]
    [SerializeField] private BunkerSystemType systemType;
    [SerializeField] private float sliderLerpDuration;

    public void Initialize(BunkerPanelManager bunkerPanelController, BunkerSystem bunkerSystem) {

        this.bunkerPanelManager = bunkerPanelController;

        // min and max are 0 and 100 respectively since durability is a percentage
        durabilitySlider.minValue = 0f;
        durabilitySlider.maxValue = 100f;
        durabilitySlider.value = bunkerSystem.GetCurrentDurability();

    }

    public void UpdateSystemStatus(int durability) {

        if (sliderLerpCoroutine != null) StopCoroutine(sliderLerpCoroutine); // stop any existing lerp coroutine
        sliderLerpCoroutine = StartCoroutine(HandleSliderLerp(durability));

        BunkerSystemStatusProperties systemStatusProperties = bunkerPanelManager.GetSystemStatusFromDurability(durability);
        statusText.text = systemStatusProperties.GetStatus().ToString(); // update the status text based on the status
        statusText.color = systemStatusProperties.GetColor(); // update the text color based on the status

    }

    private IEnumerator HandleSliderLerp(float targetValue) {

        float currentTime = 0f;
        float startValue = durabilitySlider.value;

        while (currentTime < sliderLerpDuration) {

            durabilitySlider.value = Mathf.Lerp(startValue, targetValue, currentTime / sliderLerpDuration);
            durabilityText.text = Mathf.Round(durabilitySlider.value).ToString(); // update the text to match the slider value
            currentTime += Time.deltaTime;
            yield return null;

        }

        durabilitySlider.value = targetValue; // ensure the slider ends at the target value
        durabilityText.text = Mathf.Round(targetValue).ToString(); // update the text to match the final slider value

        sliderLerpCoroutine = null; // reset the coroutine reference

    }

    public BunkerSystemType GetSystemType() => systemType;

}
