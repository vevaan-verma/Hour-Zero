using UnityEngine;

public class HeldToolSway : MonoBehaviour {

    [Header("References")]
    private PlayerController playerController;

    [Header("Settings")]
    private float swayAmount;
    private float swaySmoothness;
    private float rotationSwayAmount;
    private float rotationSwaySmoothness;
    private float breathingAmplitude;
    private float breathingFrequency;
    [Tooltip("Deadzone for mouse movement to prevent jittering in sway effect")] private float mouseSwayDeadzone;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 swayVelocity;

    public void Initialize(float swayAmount, float swaySmoothness, float rotationSwayAmount, float rotationSwaySmoothness, float breathingAmplitude, float breathingFrequency, float mouseSwayDeadzone) {

        this.swayAmount = swayAmount;
        this.swaySmoothness = swaySmoothness;
        this.rotationSwayAmount = rotationSwayAmount;
        this.rotationSwaySmoothness = rotationSwaySmoothness;
        this.breathingAmplitude = breathingAmplitude;
        this.breathingFrequency = breathingFrequency;
        this.mouseSwayDeadzone = mouseSwayDeadzone;

        playerController = FindFirstObjectByType<PlayerController>();

        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;

    }

    public void HandleSway(float mouseX, float mouseY, bool enableBreathing, bool enableHeadbob, bool enableSway) {

        if (Mathf.Abs(mouseX) < mouseSwayDeadzone) mouseX = 0f; // if the mouse movement is less than the deadzone, set it to 0 to prevent jittering
        if (Mathf.Abs(mouseY) < mouseSwayDeadzone) mouseY = 0f; // if the mouse movement is less than the deadzone, set it to 0 to prevent jittering

        // invert the sway directions for a more natural feel
        if (enableSway) {

            mouseX *= -swayAmount;
            mouseY *= -swayAmount;

        } else {

            mouseX = 0f;
            mouseY = 0f;

        }

        float breathingOffset = enableBreathing ? Mathf.Sin(Time.time * breathingFrequency) * breathingAmplitude : 0f; // add breathing offset (smooth up and down bobbing)

        float headbobOffset = (enableHeadbob && playerController) ? playerController.GetHeadbobOffset() : 0f; // add headbob offset from the camera movement

        // calculate the target position and rotation based on mouse movement
        Vector3 targetPosition = initialPosition + new Vector3(mouseX, mouseY + breathingOffset + headbobOffset, 0);
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
