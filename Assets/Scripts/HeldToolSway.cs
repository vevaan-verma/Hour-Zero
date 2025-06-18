using UnityEngine;

public class HeldToolSway : MonoBehaviour {

    [Header("References")]
    private PlayerController playerController;

    [Header("Settings")]
    [SerializeField] private float swayAmount;
    [SerializeField] private float swaySmoothness;
    [SerializeField] private float rotationSwayAmount;
    [SerializeField] private float rotationSwaySmoothness;
    [SerializeField] private float breathingAmplitude = 0.02f;
    [SerializeField] private float breathingFrequency = 1.5f;
    [SerializeField, Tooltip("Deadzone for mouse movement to prevent jittering")] private float mouseDeadzone;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 swayVelocity;

    private void Start() {

        playerController = FindFirstObjectByType<PlayerController>();

        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;

    }

    // LateUpdate is used to ensure the sway happens after all other updates, preventing jittering
    private void LateUpdate() {

        float moveX = Input.GetAxis("Mouse X");
        float moveY = Input.GetAxis("Mouse Y");

        if (Mathf.Abs(moveX) < mouseDeadzone) moveX = 0f; // if the mouse movement is less than the deadzone, set it to 0 to prevent jittering
        if (Mathf.Abs(moveY) < mouseDeadzone) moveY = 0f; // if the mouse movement is less than the deadzone, set it to 0 to prevent jittering

        // invert the sway directions for a more natural feel
        moveX *= -swayAmount;
        moveY *= -swayAmount;

        float breathingOffset = Mathf.Sin(Time.time * breathingFrequency) * breathingAmplitude; // add breathing offset (smooth up and down bobbing)

        float headbobOffset = playerController ? playerController.GetHeadbobOffset() : 0f; // add headbob offset from the camera movement

        // calculate the target position and rotation based on mouse movement
        Vector3 targetPosition = initialPosition + new Vector3(moveX, moveY + breathingOffset + headbobOffset, 0);
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, targetPosition, ref swayVelocity, 1f / swaySmoothness);

        // calculate the rotations based on mouse movement
        float rotX = moveY * rotationSwayAmount;
        float rotY = moveX * rotationSwayAmount;

        // apply the rotations to the initial rotation
        Quaternion targetRotation = Quaternion.Euler(rotX, -rotY, 0);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, initialRotation * targetRotation, Time.deltaTime * rotationSwaySmoothness);

    }
}
