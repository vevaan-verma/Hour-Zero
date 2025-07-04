using UnityEngine;

public class NPCInteractable : Interactable {

    [Header("References")]
    private TaskManager taskManager;
    private TaskType? assignedTask;

    private new void Start() {

        base.Start();
        taskManager = FindFirstObjectByType<TaskManager>();

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
}
