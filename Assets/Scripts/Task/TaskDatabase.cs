using System;
using UnityEngine;

public class TaskDatabase : MonoBehaviour {

    [Header("Data")]
    [SerializeField] private TaskData[] taskData;

    public TaskData GetTaskData(TaskType taskType) {

        foreach (var data in taskData)
            if (data.GetTaskType() == taskType)
                return data;

        Debug.LogWarning($"TaskData for {taskType} not found.");
        return null;

    }
}

[Serializable]
public class TaskData {

    [Header("Data")]
    [SerializeField] private TaskType taskType;
    [SerializeField, Tooltip("Use ___ as a placeholder for the item name")] private DialogueSequence[] taskDialogueSequences;

    public TaskType GetTaskType() => taskType;

    public DialogueSequence GetRandomDialogueSequence() => taskDialogueSequences[UnityEngine.Random.Range(0, taskDialogueSequences.Length)];

}
