using Pathfinding;
using System;
using System.Collections;
using UnityEngine;

public class NPCController : Interactable {

    [Header("References")]
    [SerializeField] private Transform bunkerNPCPoint;
    private NameDatabase nameDatabase;
    private TaskManager taskManager;
    private UIManager uiManager;
    private Animator animator;
    private AIPath aiPath;
    private Coroutine interactCoroutine;
    private Coroutine moveCoroutine;

    [Header("Settings")]
    [SerializeField] private NPCData npcData;
    [SerializeField, Tooltip("How quickly the NPC should look at the player when interacting")] private float lookSpeed;
    [SerializeField, Tooltip("How high above the ground the foot should be positioned")] private float footHeightOffset;
    [SerializeField] private LayerMask environmentMask;
    private bool isInteracting; // whether the NPC is currently interacting with the player

    [Header("Pathfinding")]
    [SerializeField, Tooltip("Minimum wait time at each destination")] private float minWaitTime;
    [SerializeField, Tooltip("Maximum wait time at each destination")] private float maxWaitTime;

    [Header("Tracking")]
    [SerializeField] private Marker generalMarkerPrefab;
    [SerializeField] private Marker taskMarkerPrefab;
    private Marker generalMarker;
    private Marker taskMarker;
    private bool generalTrackedBeforeTask; // whether the general marker was active before a task was assigned; is also modified if the player general tracks/untracks the NPC while a task is assigned

    [Header("Ground Check")]
    [SerializeField] private float raycastDistance; // distance to raycast down from the foot position

    private void Awake() {

        nameDatabase = FindFirstObjectByType<NameDatabase>();
        taskManager = FindFirstObjectByType<TaskManager>();
        uiManager = FindFirstObjectByType<UIManager>();
        aiPath = GetComponent<AIPath>();
        animator = GetComponent<Animator>();

        npcData.Initialize(this, nameDatabase.GetRandomName(npcData.GetSex())); // initialize the NPC
        aiPath.canMove = false; // disable movement initially

        generalMarker = Instantiate(generalMarkerPrefab, transform); // instantiate the general marker and set it as a child of the NPC
        taskMarker = Instantiate(taskMarkerPrefab, transform); // instantiate the task marker and set it as a child of the NPC

        generalMarker.gameObject.SetActive(false); // hide the general marker by default
        taskMarker.gameObject.SetActive(false); // hide the task marker by default

        StartMovement(DestinationType.Random); // start moving the NPC to random points on the surface of the graph

    }

    private void OnEnable() {

        // subscribe to task events to update the NPC state when tasks are assigned or completed
        taskManager.onTaskAssigned += OnTaskAssigned;
        taskManager.onTaskCompleted += OnTaskCompleted;

    }

    private void OnDisable() {

        // unsubscribe from task events to prevent memory leaks
        taskManager.onTaskAssigned -= OnTaskAssigned;
        taskManager.onTaskCompleted -= OnTaskCompleted;

    }

    private new void Update() {

        base.Update();
        animator.SetFloat("speed", aiPath.canMove ? aiPath.velocity.magnitude : 0f); // update the animator speed based on the NPC's velocity

    }

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

        StopMovement(); // stop the NPC from moving while interacting
        isInteracting = true;
        uiManager.OpenNPCMenu(this); // open the NPC menu with the current NPC controller and data
        interactCoroutine = StartCoroutine(HandleInteraction()); // start interacting
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

    private IEnumerator HandleInteraction() {

        while (isInteracting) {

            Vector3 direction = player.transform.position - transform.position;
            direction.y = 0; // keep rotation on horizontal plane

            if (direction.sqrMagnitude > 0.01f) {

                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * lookSpeed);

            }

            yield return null;

        }

        interactCoroutine = null; // reset coroutine reference

    }

    public void OnEndInteraction() {

        isInteracting = false; // reset interaction state

        if (interactCoroutine != null) StopCoroutine(interactCoroutine); // stop any existing interaction coroutine
        interactCoroutine = null; // reset coroutine reference

        if (npcData.IsTeamMember())
            StartMovement(DestinationType.Bunker); // if the NPC is a team member, start moving to the bunker when interaction ends
        else
            StartMovement(DestinationType.Random); // if the NPC is not a team member, start moving to random points on the surface of the graph

    }

    private void OnTaskAssigned() {

        generalTrackedBeforeTask = generalMarker.gameObject.activeSelf; // store the state of the general marker before a task is assigned

        taskMarker.gameObject.SetActive(true); // show the task marker when a task is assigned
        StopGeneralTracking(); // stop tracking when a task is assigned

    }

    private void OnTaskCompleted() {

        taskMarker.gameObject.SetActive(false); // hide the task marker when the task is completed

        // if the general marker was active before the task was assigned, restore its state
        if (generalTrackedBeforeTask)
            StartGeneralTracking();

        StartMovement(DestinationType.Bunker); // start moving to the bunker when the task is completed
        npcData.SetTeamMember(true); // set the NPC as a team member when the task is completed

    }

    // there are no methods like these for the task marker because that is solely controlled by if a task is assigned or not; there is no other way to set it active or inactive
    // could combine these two methods into one, but keeping them separate gives more clarity
    public void StartGeneralTracking() {

        if (IsTaskTracking())
            generalTrackedBeforeTask = true; // if the task marker is active, set the general tracked state to true so it shows up after the task marker is hidden
        else
            generalMarker.gameObject.SetActive(true); // start general tracking the NPC by showing the general marker as normal

    }

    public void StopGeneralTracking() {

        if (IsTaskTracking())
            generalTrackedBeforeTask = false; // if the task marker is active, set the general tracked state to false so it doesn't show up after the task marker is hidden
        else
            generalMarker.gameObject.SetActive(false); // stop general tracking the NPC by hiding the general marker

    }

    public void StartMovement(DestinationType destinationType) {

        aiPath.SetPath(null); // clear the path as a new one will be set

        if (moveCoroutine != null) StopCoroutine(moveCoroutine); // stop any existing movement coroutine
        moveCoroutine = StartCoroutine(HandleMovement(destinationType)); // start the movement coroutine to make the NPC wander around

    }

    public void StopMovement() {

        if (moveCoroutine != null) StopCoroutine(moveCoroutine); // stop any existing movement coroutine
        moveCoroutine = null; // reset coroutine reference

        aiPath.canMove = false; // stop the NPC from moving

    }

    private IEnumerator HandleMovement(DestinationType destinationType) {

        while (true) {

            if (!aiPath.pathPending && (aiPath.reachedEndOfPath || !aiPath.hasPath)) { // check if there is no path pending and either the end of the path has been reached or there is no path

                aiPath.canMove = false; // stop the NPC from moving while setting a new destination
                yield return new WaitForSeconds(UnityEngine.Random.Range(minWaitTime, maxWaitTime)); // wait for a random amount of time before setting a new destination

                if (destinationType == DestinationType.Bunker)
                    aiPath.destination = bunkerNPCPoint.position; // set the destination to the bunker NPC point
                else if (destinationType == DestinationType.Random)
                    aiPath.destination = GetRandomPoint(); // set a random destination on the surface of the graph

                aiPath.SearchPath(); // search for a path to the new destination
                yield return new WaitUntil(() => !aiPath.pathPending); // wait until the path is ready
                aiPath.canMove = true; // allow the NPC to move again

            }

            yield return null; // wait for the next frame before checking again

        }
    }

    private Vector3 GetRandomPoint() {

        NNInfo sample = AstarPath.active.graphs[0].RandomPointOnSurface(NNConstraint.Walkable);
        return sample.position;

    }

    public NPCData GetNPCData() => npcData;

    public bool IsGeneralTracking() => generalMarker.gameObject.activeSelf || generalTrackedBeforeTask; // check if the general marker is active or if it was active before a task was assigned

    public bool IsTaskTracking() => taskMarker.gameObject.activeSelf;

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

    [Header("References")]
    private NPCController npcController;

    [Header("Data")]
    [SerializeField] private NPCType npcType;
    [SerializeField] private Sex sex;
    private string npcName;
    private bool isTeamMember; // whether the NPC is a team member (used for team button logic)

    public void Initialize(NPCController npcController, string npcName) {

        this.npcController = npcController;
        this.npcName = npcName;

    }

    public NPCController GetNPCController() => npcController;

    public NPCType GetNPCType() => npcType;

    public Sex GetSex() => sex;

    public string GetName() => npcName;

    public bool IsTeamMember() => isTeamMember;

    public void SetTeamMember(bool isTeamMember) => this.isTeamMember = isTeamMember;

}

public enum NPCType {

    Scavenger,
    Medic,
    Guard

}

public enum DestinationType {

    Random, // random point on the surface of the graph
    Bunker // the bunker NPC point where the NPC should go when the task is completed

}
