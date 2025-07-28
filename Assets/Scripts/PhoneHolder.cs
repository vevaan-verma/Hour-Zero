using UnityEngine;

public class PhoneHolder : MonoBehaviour {

    [Header("References")]
    private PlayerController playerController;

    [Header("Settings")]
    [SerializeField] private float swayAmount;
    [SerializeField] private float swaySmoothness;
    [SerializeField] private float rotationSwayAmount;
    [SerializeField] private float rotationSwaySmoothness;
    [SerializeField] private float breathingAmplitude;
    [SerializeField] private float breathingFrequency;
    [SerializeField] private float mouseSwayDeadzone; // deadzone for mouse movement to prevent jittering in sway effect
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 swayVelocity;

    private void Start() {

        playerController = FindFirstObjectByType<PlayerController>();

        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;

    }

    public void HandleSway(float mouseX, float mouseY, bool enableBreathing, bool enableHeadbob, bool enableSway) {

        // if the mouse movement on either axis is less than the deadzone, set it to 0 to prevent jittering
        if (Mathf.Abs(mouseX) < mouseSwayDeadzone) mouseX = 0f;
        if (Mathf.Abs(mouseY) < mouseSwayDeadzone) mouseY = 0f;

        // invert the sway directions for a more natural feel
        if (enableSway) {

            mouseX *= -swayAmount;
            mouseY *= -swayAmount;

        } else {

            mouseX = 0f;
            mouseY = 0f;

        }

        float breathingOffset = enableBreathing ? Mathf.Sin(Time.time * breathingFrequency) * breathingAmplitude : 0f; // add breathing offset (smooth up and down bobbing)

        float headbobOffset = enableHeadbob ? -playerController.GetHeadbobOffset() : 0f; // add headbob offset from the camera movement and flip it since the phone is in the left hand, which is opposite to the camera movement

        // calculate the target position and rotation based on mouse movement
        Vector3 targetPosition = initialPosition + new Vector3(mouseX, mouseY + breathingOffset + headbobOffset, 0f);
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, targetPosition, ref swayVelocity, 1f / swaySmoothness);

        // calculate the rotations based on mouse movement
        float rotX = mouseY * rotationSwayAmount;
        float rotY = mouseX * rotationSwayAmount;

        // apply the rotations to the initial rotation
        Quaternion targetRotation = Quaternion.Euler(rotX, -rotY, 0);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, initialRotation * targetRotation, Time.deltaTime * rotationSwaySmoothness);

    }

    // smoothly returns the sway position and rotation to the initial position and rotation.
    public void SmoothReturnToCenter() => transform.SetLocalPositionAndRotation(Vector3.SmoothDamp(transform.localPosition, initialPosition, ref swayVelocity, 1f / swaySmoothness), Quaternion.Slerp(transform.localRotation, initialRotation, Time.deltaTime * rotationSwaySmoothness));

}
