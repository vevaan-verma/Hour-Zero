using Pathfinding;
using UnityEngine;

public class NPCController : Interactable {

    [Header("References")]
    private TaskManager taskManager;
    private TaskType? assignedTask;
    private Animator animator;
    private NPCFootIKController footIKController;
    private Seeker seeker;
    private AIPath aiPath;
    private WanderingAI wanderingAI;

    private new void Start() {

        base.Start();
        taskManager = FindFirstObjectByType<TaskManager>();
        animator = GetComponent<Animator>();
        footIKController = GetComponent<NPCFootIKController>();
        seeker = GetComponent<Seeker>();
        aiPath = GetComponent<AIPath>();
        wanderingAI = GetComponent<WanderingAI>();

    }

    public override bool Interact() {

        if (!base.Interact()) return false; // if the base interaction fails, do not proceed

        if (assignedTask != null) {

            bool taskCompleted = taskManager.CheckTaskCompletion();

            if (taskCompleted)
                assignedTask = null;// reset the assigned task if it was completed

            return taskCompleted; // return whether the task was completed or not

        }

        // at this point, there is no assigned task, so we can assign a new one

        // assign a random task to the player
        TaskType[] taskTypes = (TaskType[]) System.Enum.GetValues(typeof(TaskType));
        TaskType randomTaskType = taskTypes[Random.Range(0, taskTypes.Length)];

        taskManager.AssignTask(randomTaskType);
        assignedTask = randomTaskType;

        return true;

    }

    private void OnCollisionEnter(Collision collision) {

        if (collision.gameObject.CompareTag("Player"))
            EnableRagdoll(); // enable ragdoll physics when colliding with the player

    }

    private void EnableRagdoll() {

        animator.enabled = false;
        footIKController.enabled = false;
        seeker.enabled = false;
        aiPath.enabled = false;
        wanderingAI.enabled = false;

    }
}
