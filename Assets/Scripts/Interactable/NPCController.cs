using Pathfinding;
using System;
using System.Collections;
using UnityEngine;

public class NPCController : Interactable {

    [Header("References")]
    private NameDatabase nameDatabase;
    private TaskManager taskManager;
    private UIManager uiManager;
    private AIPath aiPath;
    private Animator animator;
    private Transform player;
    private Coroutine lookCoroutine;

    [Header("Settings")]
    [SerializeField] private NPCData npcData;
    [SerializeField, Tooltip("How quickly the NPC should look at the player when interacting")] private float lookSpeed;
    [SerializeField, Tooltip("How high above the ground the foot should be positioned")] private float footHeightOffset;
    [SerializeField] private LayerMask environmentMask;
    private bool isInteracting; // whether the NPC is currently interacting with the player

    [Header("Ground Check")]
    [SerializeField] private float raycastDistance; // distance to raycast down from the foot position

    private new void Start() {

        base.Start();
        nameDatabase = FindFirstObjectByType<NameDatabase>();
        taskManager = FindFirstObjectByType<TaskManager>();
        uiManager = FindFirstObjectByType<UIManager>();
        aiPath = GetComponent<AIPath>();
        animator = GetComponent<Animator>();
        player = FindFirstObjectByType<PlayerController>().transform;

        npcData.SetName(nameDatabase.GetRandomName(npcData.GetSex())); // set the name of the NPC using the name database and sex from the NPC data

    }

    private void Update() => animator.SetFloat("speed", aiPath.canMove ? aiPath.velocity.magnitude : 0f); // update the animator speed based on the NPC's velocity

    private void OnAnimatorIK(int layerIndex) {

        AdjustFootIK(AvatarIKGoal.LeftFoot);
        AdjustFootIK(AvatarIKGoal.RightFoot);

    }

    public override bool Interact() {

        if (taskManager.GetActiveTask() is DoomsdayDropoffTask doomsdayDropoffTask) {

            // could use the base method to check here, but instead we use the task manager to directly check if the task is completed
            // check if the task is completed when interacting with the NPC and return true if it is completed, so the NPC menu isn't opened
            if (taskManager.CheckTaskCompletion())
                return true;

        }

        isInteracting = true;
        aiPath.canMove = false; // stop the NPC from moving while the menu is open
        uiManager.OpenNPCMenu(this); // open the NPC menu with the current NPC controller and data
        lookCoroutine = StartCoroutine(LookAtPlayer()); // start looking at the player

        return true;

    }

    private void AdjustFootIK(AvatarIKGoal foot) {

        Vector3 footPos = animator.GetIKPosition(foot); // get the current IK position of the foot
        Vector3 rayOrigin = footPos + Vector3.up * 0.1f; // start raycast slightly above the foot position

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastDistance, environmentMask)) { // use a raycast to check if the foot is on the ground

            Vector3 footTargetPos = hit.point + Vector3.up * footHeightOffset; // adjust the target position to be above the ground hit point

            Quaternion footRot = animator.GetIKRotation(foot); // get the current IK rotation of the foot
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

    private IEnumerator LookAtPlayer() {

        while (isInteracting) {

            Vector3 direction = player.position - transform.position;
            direction.y = 0; // keep rotation on horizontal plane

            if (direction.sqrMagnitude > 0.01f) {

                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * lookSpeed);

            }

            yield return null;

        }

        lookCoroutine = null; // reset coroutine reference

    }

    public NPCData GetNPCData() => npcData;

}

#if UNITY_EDITOR
// using UnityEditor prefix to avoid needing to hide the import in the final build
[UnityEditor.CustomEditor(typeof(NPCController), true)]
public class NPCControllerEditor : UnityEditor.Editor {

    public override void OnInspectorGUI() {

        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, "requireHeldItem", "requiredHeldItem", "consumeHeldItem", "requireBackpackItems", "requiredBackpackItems", "consumeBackpackItems");

        serializedObject.ApplyModifiedProperties();

    }
}
#endif

[Serializable]
public class NPCData {

    [Header("Data")]
    [SerializeField] private NPCType npcType;
    [SerializeField] private Sex sex;
    private string npcName;

    public NPCType GetNPCType() => npcType;

    public Sex GetSex() => sex;

    public void SetName(string name) => npcName = name;

    public string GetName() => npcName;

}

public enum NPCType {

    Scavenger,
    Medic,
    Guard

}
