using UnityEngine;

public class NPCFootIKController : MonoBehaviour {

    [Header("References")]
    private Animator animator;

    [Header("Settings")]
    [SerializeField] private float raycastDistance; // distance to raycast down from the foot position
    [SerializeField] private float footHeightOffset; // how high above the ground the foot should be positioned
    [SerializeField] private LayerMask groundLayers; // layers considered as ground for foot placement

    private void Start() => animator = GetComponent<Animator>();

    private void OnAnimatorIK(int layerIndex) {

        AdjustFootIK(AvatarIKGoal.LeftFoot);
        AdjustFootIK(AvatarIKGoal.RightFoot);

    }

    private void AdjustFootIK(AvatarIKGoal foot) {

        Vector3 footPos = animator.GetIKPosition(foot); // get the current IK position of the foot
        Vector3 rayOrigin = footPos + Vector3.up * 0.1f; // start raycast slightly above the foot position

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastDistance, groundLayers)) { // use a raycast to check if the foot is on the ground

            Vector3 footTargetPos = hit.point + Vector3.up * footHeightOffset; // adjust the target position to be above the ground hit point

            Quaternion footRot = animator.GetIKRotation(foot); // get the current IK rotation of the foot
            Vector3 footForward = footRot * Vector3.forward; // get the forward direction of the foot in world space
            Vector3 projectedForward = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized; // project the body's forward direction onto the ground plane to align the foot with the ground normal (using the body's forward direction is more accurate than the foot's forward direction)
            Quaternion targetRot = Quaternion.LookRotation(projectedForward, hit.normal); // create a rotation that aligns the foot's forward direction with the ground normal

            float distanceToGround = hit.distance - footHeightOffset; // calculate the distance from the foot to the ground hit point, adjusted by the foot height offset
            float ikWeight = Mathf.Clamp01(1f - (distanceToGround / raycastDistance)); // calculate the IK weight based on the distance to the ground, clamped between 0 and 1; close = 1, far = 0

            // set the IK position and rotation for the foot with weighting
            animator.SetIKPositionWeight(foot, ikWeight);
            animator.SetIKRotationWeight(foot, ikWeight);
            animator.SetIKPosition(foot, Vector3.Lerp(footPos, footTargetPos, ikWeight));
            animator.SetIKRotation(foot, Quaternion.Slerp(footRot, targetRot, ikWeight));

        } else {

            // if the raycast did not hit the ground, reset the IK position and rotation weights to zero
            animator.SetIKPositionWeight(foot, 0f);
            animator.SetIKRotationWeight(foot, 0f);

        }
    }
}
