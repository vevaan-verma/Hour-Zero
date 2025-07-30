using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class HomeButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

    [Header("Settings")]
    [SerializeField] private float longPressDuration;
    private float pressStartTime;
    private bool isPressed;

    [Header("Actions")]
    public Action onPressBegin; // triggers when pressed down
    public Action onClickReleased; // triggers when released before long press duration
    public Action onLongPressReleased; // triggers when released after long press duration

    private void Update() {

        if (isPressed && Time.time - pressStartTime >= longPressDuration) { // check if the button is pressed and the duration exceeds long press duration

            isPressed = false; // reset to false to prevent double trigger
            onLongPressReleased?.Invoke(); // invoke the action for long press release

        }
    }

    public void OnPointerDown(PointerEventData eventData) {

        pressStartTime = Time.time; // store the time when the button was pressed
        isPressed = true;
        onPressBegin?.Invoke(); // invoke the action for press begin

    }

    public void OnPointerUp(PointerEventData eventData) {

        if (!isPressed) return; // if not pressed, do nothing

        float pressDuration = Time.time - pressStartTime; // calculate the duration of the press

        // invoke the action for click release if released before long press duration
        if (pressDuration < longPressDuration)
            onClickReleased?.Invoke();

        isPressed = false; // reset to false after handling the press

    }
}
