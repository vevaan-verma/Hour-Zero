using System;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour {

    [Header("References")]
    private TaskDatabase taskDatabase;
    private Hotbar hotbar;
    private List<Item> dropoffItems;
    private BaseTask activeTask;
    private List<BaseTask> completedTasks;

    [Header("Task Specifics")]
    private ItemStack dropoffItemStack; // the item stack that needs to be dropped off in the DoomsdayDropoff task

    [Header("Actions")]
    public Action onTaskAssigned; // event to notify when a task is assigned
    public Action onTaskCompleted; // event to notify when a task is completed

    private void Start() {

        taskDatabase = FindFirstObjectByType<TaskDatabase>();
        hotbar = FindFirstObjectByType<Hotbar>();

        completedTasks = new List<BaseTask>();

        // initialize the dropoff items by looking in the Resources/Items folder but only storing all the items that are marked as dropoff items
        Item[] allItems = Resources.LoadAll<Item>("Items");
        dropoffItems = new List<Item>();

        foreach (Item item in allItems)
            if (item.IsDropoffItem())
                dropoffItems.Add(item);

    }

    public bool AssignRandomTask(NPCData npcData) {

        if (activeTask != null)
            return false;

        TaskData randomTaskData = taskDatabase.GetRandomTaskData(); // get a random task data from the task database

        switch (randomTaskData.GetTaskType()) {

            case TaskType.DoomsdayDropoff:

                Item item = dropoffItems[UnityEngine.Random.Range(0, dropoffItems.Count)]; // randomly select a dropoff item from the list
                dropoffItemStack = new ItemStack(item, item.GetDropoffCount()); // create a new item stack with the selected item and its dropoff count
                activeTask = new DoomsdayDropoffTask(npcData, randomTaskData, dropoffItemStack, hotbar);
                break;

            case TaskType.LastMinuteRepairs:
                activeTask = new LastMinuteRepairsTask(npcData, randomTaskData);
                break;

            case TaskType.DanceOff:
                activeTask = new DanceOffTask(npcData, randomTaskData);
                break;

            case TaskType.CrowbarTherapy:
                activeTask = new CrowbarTherapyTask(npcData, randomTaskData);
                break;

            case TaskType.AcademicFraud:
                activeTask = new AcademicFraudTask(npcData, randomTaskData);
                break;

            default:
                Debug.LogError($"Unknown task type: {randomTaskData.GetTaskType()}.");
                break;

        }

        onTaskAssigned?.Invoke(); // invoke the task assigned event to notify any listeners that a task has been assigned
        return true;

    }

    public bool CheckTaskCompletion() {

        if (activeTask == null)
            return false;

        bool isCompleted = activeTask.CheckCompletion();

        if (isCompleted) {

            completedTasks.Add(activeTask); // add the completed task to the list of completed tasks
            activeTask = null; // reset the active task after completion
            onTaskCompleted?.Invoke(); // invoke the task completed event to notify any listeners that a task has been completed

        }

        return isCompleted;

    }

    public BaseTask GetActiveTask() => activeTask;

    public List<BaseTask> GetCompletedTasks() => completedTasks;

}

public enum TaskType {

    DoomsdayDropoff, // bring item to NPC (TODO: should it be called LastMinuteDropoff if LastMinuteRepairs isn't used?)
    LastMinuteRepairs, // hammer an object in the world (TODO: should it be called BangBeforeTheBoom?)
    DanceOff,
    CrowbarTherapy, // attack the NPC with a crowbar
    AcademicFraud

}

public abstract class BaseTask {

    [Header("Data")]
    private readonly NPCData npcData; // data of the NPC associated with the task
    private readonly TaskData taskData; // data associated with the task

    public BaseTask(NPCData npcData, TaskData taskData) {

        this.npcData = npcData;
        this.taskData = taskData;

    }

    public NPCData GetNPCData() => npcData; // getter for the NPC data

    public TaskData GetTaskData() => taskData; // getter for the task data

    public abstract bool CheckCompletion(); // check if the task is completed

    public abstract TaskType GetTaskType(); // get the type of the task

    public abstract Sprite GetTaskIcon(); // get the icon of the task, if applicable

}

public class DoomsdayDropoffTask : BaseTask {

    [Header("References")]
    private readonly Hotbar hotbar; // reference to the hotbar to check if the player is holding the correct item

    [Header("Data")]
    private readonly ItemStack dropoffItemStack;

    public DoomsdayDropoffTask(NPCData npcData, TaskData taskData, ItemStack dropoffItemStack, Hotbar hotbar) : base(npcData, taskData) {

        this.dropoffItemStack = dropoffItemStack;
        this.hotbar = hotbar;

    }

    public override bool CheckCompletion() {

        ItemStack heldItemStack = hotbar.GetItemStack(hotbar.GetSelectedIndex());

        if (heldItemStack == null || heldItemStack.GetItem() == null)
            return false; // no item held, task not completed

        // check if player is holding the correct item and enough of it
        if (heldItemStack.GetItem().Equals(dropoffItemStack.GetItem()) && heldItemStack.GetCount() >= dropoffItemStack.GetCount()) {

            hotbar.RemoveItemStack(new ItemStack(heldItemStack.GetItem(), dropoffItemStack.GetCount())); // remove the item stack from the hotbar
            return true; // task completed

        }

        return false; // task not completed, player is not holding the correct item or not enough of it

    }

    public override TaskType GetTaskType() => TaskType.DoomsdayDropoff; // return the type of the task

    public override Sprite GetTaskIcon() => dropoffItemStack.GetItem().GetItemIcon(); // return the icon of the item to be dropped off

    public ItemStack GetDropoffItemStack() => dropoffItemStack; // getter for the item stack to be dropped off

}

public class LastMinuteRepairsTask : BaseTask {

    public LastMinuteRepairsTask(NPCData npcData, TaskData taskData) : base(npcData, taskData) { }

    public override bool CheckCompletion() {

        // implement the logic to check if the Last Minute Repairs task is completed
        return false; // placeholder return value

    }

    public override TaskType GetTaskType() => TaskType.LastMinuteRepairs;

    public override Sprite GetTaskIcon() {

        // return the icon for the Last Minute Repairs task
        return null; // placeholder return value

    }
}

public class DanceOffTask : BaseTask {

    public DanceOffTask(NPCData npcData, TaskData taskData) : base(npcData, taskData) { }

    public override bool CheckCompletion() {

        // implement the logic to check if the Dance Off task is completed
        return false; // placeholder return value

    }

    public override TaskType GetTaskType() => TaskType.DanceOff;

    public override Sprite GetTaskIcon() {

        // return the icon for the Dance Off task
        return null; // placeholder return value

    }
}

public class CrowbarTherapyTask : BaseTask {

    public CrowbarTherapyTask(NPCData npcData, TaskData taskData) : base(npcData, taskData) { }

    public override bool CheckCompletion() {

        // implement the logic to check if the Crowbar Therapy task is completed
        return false; // placeholder return value

    }

    public override TaskType GetTaskType() => TaskType.CrowbarTherapy;

    public override Sprite GetTaskIcon() {

        // return the icon for the Crowbar Therapy task
        return null; // placeholder return value

    }
}

public class AcademicFraudTask : BaseTask {

    public AcademicFraudTask(NPCData npcData, TaskData taskData) : base(npcData, taskData) { }

    public override bool CheckCompletion() {

        // implement the logic to check if the Academic Fraud task is completed
        return false; // placeholder return value

    }

    public override TaskType GetTaskType() => TaskType.AcademicFraud;

    public override Sprite GetTaskIcon() {

        // return the icon for the Academic Fraud task
        return null; // placeholder return value

    }
}
