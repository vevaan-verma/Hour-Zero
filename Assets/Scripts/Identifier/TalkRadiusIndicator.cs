using UnityEngine;

public class TalkRadiusIndicator : MonoBehaviour {

    [Header("References")]
    [SerializeField] private LayerMask groundMask;
    private Transform target;

    [Header("Settings")]
    [SerializeField, Tooltip("Degrees per second to spin around the ground-normal")] private float spinSpeed;
    [SerializeField, Tooltip("Max distance for ground raycast; begins at talk radius indicator position")] private float groundRaycastDistance;
    private float currSpinAngle;

    public void Initialize(Transform target, float radius) {

        this.target = target;
        transform.localScale = new Vector3(radius * 2, radius * 2, 1f); // set the scale based on the radius * 2 for diameter

    }

    private void Update() {

        // raycast down from the target position to find the ground to make sure the indicator can be positioned correctly
        if (Physics.Raycast(target.position, Vector3.down, out RaycastHit hit, groundRaycastDistance, groundMask)) {

            Vector3 flatForward = Vector3.ProjectOnPlane(Vector3.forward, hit.normal).normalized; // project the world forward vector onto the ground plane

            // if the flat forward vector is too small, use a default direction to avoid NaN issues
            if (flatForward.sqrMagnitude < 0.001f)
                flatForward = Vector3.ProjectOnPlane(Vector3.right, hit.normal).normalized;

            // calculate the current spin angle and apply it to the rotation, keeping it within 0 - 360 degrees
            currSpinAngle += spinSpeed * Time.deltaTime;
            currSpinAngle %= 360f;

            Quaternion spinOffset = Quaternion.AngleAxis(currSpinAngle, hit.normal); // create a rotation around the ground normal based on the spin speed and time
            transform.rotation = Quaternion.LookRotation(flatForward, hit.normal) * spinOffset * Quaternion.Euler(-90f, 0f, 0f); // look in the flat forward direction, apply the spin offset, and tilt it downwards so it faces the right way

        }
    }
}