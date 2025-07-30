using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class TodoAppManager : ViewManager {

    [Header("References")]
    private TaskManager taskManager;
    private RectTransform rectTransform;

    [Header("UI References")]
    [SerializeField] private AppTaskListing taskListingPrefab;
    [SerializeField] private TMP_Text noActiveTaskTextPrefab; // text to display when there are no active tasks
    [SerializeField] private TMP_Text noCompletedTasksTextPrefab; // text to display when there are no completed tasks
    [SerializeField] private Transform currentTaskSection;
    [SerializeField] private Transform completedTaskSection;
    [SerializeField] private Sprite emptyIcon; // icon to show when there is no task icon

    private void Awake() {

        taskManager = FindFirstObjectByType<TaskManager>();
        rectTransform = GetComponent<RectTransform>();

    }

    private new void OnEnable() {

        base.OnEnable();

        // subscribe to events from the task manager to update the app UI
        taskManager.onTaskAssigned += RefreshApp; // subscribe to task assignment event to refresh the app UI
        taskManager.onTaskCompleted += RefreshApp; // subscribe to task completion event to refresh the app UI

    }

    private void OnDisable() {

        // unsubscribe from events to prevent memory leaks
        taskManager.onTaskAssigned -= RefreshApp;
        taskManager.onTaskCompleted -= RefreshApp;

    }

    public override void RefreshApp() {

        // clear the current task section
        foreach (Transform child in currentTaskSection)
            Destroy(child.gameObject);

        // clear the completed task section
        foreach (Transform child in completedTaskSection)
            Destroy(child.gameObject);

        BaseTask activeTask = taskManager.GetActiveTask();

        if (activeTask == null) {

            TMP_Text noTaskText = Instantiate(noActiveTaskTextPrefab, currentTaskSection); // instantiate the no active task text prefab since there are no active tasks

            List<BaseTask> completedTasks = taskManager.GetCompletedTasks();

            if (completedTasks == null || completedTasks.Count == 0)
                Instantiate(noCompletedTasksTextPrefab, completedTaskSection); // instantiate the no completed tasks text prefab since there are no completed tasks
            else
                foreach (BaseTask completedTask in completedTasks)
                    CreateTaskListing(completedTask, completedTaskSection); // create a task listing for each completed task

        } else {

            CreateTaskListing(activeTask, currentTaskSection); // create a task listing for the active task

            List<BaseTask> completedTasks = taskManager.GetCompletedTasks();

            if (completedTasks == null || completedTasks.Count == 0)
                Instantiate(noCompletedTasksTextPrefab, completedTaskSection); // instantiate the no completed tasks text prefab since there are no completed tasks
            else
                foreach (BaseTask completedTask in completedTasks)
                    CreateTaskListing(completedTask, completedTaskSection); // create a task listing for each completed task

        }

        RefreshLayout(rectTransform); // refresh the layout of the app UI

    }

    private void CreateTaskListing(BaseTask task, Transform parent) {

        AppTaskListing taskListing = Instantiate(taskListingPrefab, parent);
        string taskText = $"{task.GetTaskData().GetRandomTodoTaskText()}";

        // replace placeholders with actual values
        taskText = taskText.Replace("{npcName}", task.GetNPCData().GetName());
        taskText = taskText.Replace("{taskType}", Regex.Replace(task.GetTaskType().ToString(), "(\\B[A-Z])", " $1"));

        if (task is DoomsdayDropoffTask doomsdayTask) {

            taskText = taskText.Replace("{itemName}", doomsdayTask.GetDropoffItemStack().GetItem().GetName());
            taskText = taskText.Replace("{itemCount}", doomsdayTask.GetDropoffItemStack().GetCount().ToString());

        }

        taskListing.Initialize(taskText, task.GetTaskIcon() ?? emptyIcon);

    }
}
