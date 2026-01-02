using UnityEngine;

public class CameraFollow : MonoBehaviour {

    [Header("References")]
    private Transform targetPivot;

    [Header("Settings")]
    private bool freecam;

    [Header("Freecam")]
    [SerializeField] private float sensitivity;
    [SerializeField] private float pitchMin;
    [SerializeField] private float pitchMax;
    [SerializeField] private bool invertY;
    private float yaw;
    private float pitch;

    [Header("Zooming")]
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float defaultDistance;
    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;
    [SerializeField] private float zoomSmoothTime; // lower = snappier, higher = slower
    private float targetDistance;
    private float distance;
    private float distanceVelocity;

    private void Start() {

        // output a warning if defaultDistance isn't within min/max bounds
        if (defaultDistance < minDistance || defaultDistance > maxDistance)
            Debug.LogWarning($"[CameraFollow] Default Distance ({defaultDistance}) is out of bounds! It should be between Min Distance ({minDistance}) and Max Distance ({maxDistance}). Clamping to valid range.");

    }

    private void LateUpdate() {

        if (freecam) {

            // get mouse input
            float mouseX = Input.GetAxisRaw("Mouse X") * sensitivity;
            float mouseY = Input.GetAxisRaw("Mouse Y") * sensitivity * (invertY ? 1f : -1f);

            // update yaw/pitch
            yaw += mouseX;
            pitch += mouseY;
            pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f); // construct rotation from yaw/pitch

            float scroll = Input.GetAxis("Mouse ScrollWheel"); // get scroll input for zooming

            // update targetDistance based on scroll input
            if (Mathf.Abs(scroll) > 0.0001f)
                targetDistance = Mathf.Clamp(targetDistance - scroll * zoomSpeed, minDistance, maxDistance);

            // smoothly interpolate current distance to targetDistance
            if (zoomSmoothTime > 0f)
                distance = Mathf.SmoothDamp(distance, targetDistance, ref distanceVelocity, zoomSmoothTime);
            else
                distance = targetDistance; // immediate snap if smooth time is zero

            Vector3 desiredPos = targetPivot.position - (rotation * Vector3.forward) * distance; // calculate desired camera position
            transform.SetPositionAndRotation(desiredPos, rotation); // set camera position and rotation

        } else {

            transform.SetPositionAndRotation(targetPivot.position, targetPivot.rotation); // lock to pivot position/rotation because freecam is off

        }
    }

    public void SetFollowTarget(Transform targetPivot, bool freecam) {

        this.targetPivot = targetPivot;
        this.freecam = freecam;

        if (freecam) {

            Vector3 offset = transform.position - targetPivot.position; // get the offset from pivot to camera
            targetDistance = distance = defaultDistance; // set target distance and current distance to the default distance

            // convert offset to spherical angles (degrees)
            // yaw: angle around Y, pitch: angle above/below horizontal
            // rot.forward should equal -offset.normalized because we place the camera at:
            // pivotPos - rot * Vector3.forward * distance
            // so add 180 degrees (or negate the offset) to get the correct direction
            yaw = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg + 180f;
            pitch = Mathf.Asin(Mathf.Clamp(offset.y / distance, -1f, 1f)) * Mathf.Rad2Deg;

        }
    }
}
