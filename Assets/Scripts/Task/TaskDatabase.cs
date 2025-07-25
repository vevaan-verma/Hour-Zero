using System;
using UnityEngine;

public class TaskDatabase : MonoBehaviour {

    [Header("Data")]
    [SerializeField] private TaskData[] taskData;

    private void Start() {

        #region VALIDATION
        TaskType[] taskTypes = (TaskType[]) Enum.GetValues(typeof(TaskType)); // get all task types

        // make sure exactly one task data exists for each task type
        foreach (TaskType taskType in taskTypes) {

            int count = 0;

            foreach (TaskData data in taskData)
                if (data.GetTaskType() == taskType)
                    count++;

            if (count != 1)
                Debug.LogError($"Expected exactly one TaskData for task type {taskType}, but found {count}.");

        }
        #endregion

    }

    public TaskData GetRandomTaskData() => taskData[UnityEngine.Random.Range(0, taskData.Length)];

}

[Serializable]
public class TaskData {

    [Header("Data")]
    [SerializeField] private TaskType taskType;
    [SerializeField] private DialogueSequence[] taskDialogueSequences;
    [SerializeField, Tooltip("Text to display in the task list UI.\nThe following placeholders are supported:\n- {npcName} for the NPC name\n- {taskType} for the task type\n- {itemName}* for the required item name\n- {itemCount}* for the required item count\n* onnly supported by tasks that require items to be returned")] private string[] todoTaskTexts; // text to display in the Todo app task list UI

    public TaskType GetTaskType() => taskType;

    public DialogueSequence GetRandomDialogueSequence() => taskDialogueSequences[UnityEngine.Random.Range(0, taskDialogueSequences.Length)]; // returns a random dialogue sequence from the task dialogue sequences array

    public string GetRandomTodoTaskText() => todoTaskTexts[UnityEngine.Random.Range(0, todoTaskTexts.Length)]; // returns a random text from the todo task texts array

}
