using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour {

    [Header("References")]
    private Hotbar hotbar;
    private List<Item> dropoffItems;
    private BaseTask activeTask;

    [Header("Task Specifics")]
    private ItemStack dropoffItemStack; // the item stack that needs to be dropped off in the DoomsdayDropoff task

    private void Start() {

        hotbar = FindFirstObjectByType<Hotbar>();

        // initialize the dropoff items by looking in the Resources/Items folder but only storing all the items that are marked as dropoff items
        Item[] allItems = Resources.LoadAll<Item>("Items");
        dropoffItems = new List<Item>();

        foreach (Item item in allItems)
            if (item.IsDropoffItem())
                dropoffItems.Add(item);

    }

    public void AssignTask(TaskType taskType) {

        if (activeTask != null) {

            Debug.LogWarning("A task is already active. Completing the current task before assigning a new one.");
            return;

        }

        switch (taskType) {

            case TaskType.DoomsdayDropoff:

                Item item = dropoffItems[Random.Range(0, dropoffItems.Count)]; // randomly select a dropoff item from the list
                dropoffItemStack = new ItemStack(item, item.GetDropoffCount()); // create a new item stack with the selected item and its dropoff count
                Debug.Log("Assigned DoomsdayDropoff task with item: " + item.GetName());
                activeTask = new DoomsdayDropoffTask(dropoffItemStack, hotbar);
                break;

            case TaskType.LastMinuteRepairs:

                break;

            case TaskType.DanceOff:

                break;

            case TaskType.CrowbarTherapy:

                break;

            case TaskType.AcademicFraud:

                break;

            default:
                Debug.LogError("Unknown event type: " + taskType);
                break;

        }
    }

    public bool CheckTaskCompletion() {

        if (activeTask == null) {

            Debug.LogWarning("No active task to check completion for.");
            return false;

        }

        bool isCompleted = activeTask.CheckCompletion();

        if (isCompleted)
            activeTask = null; // reset the active task after completion
        else
            Debug.Log("Task not completed yet");

        return isCompleted;

    }
}

public enum TaskType {

    DoomsdayDropoff, // bring item to NPC (TODO: should it be called LastMinuteDropoff if LastMinuteRepairs isn't used?)
    LastMinuteRepairs, // hammer an object in the world (TODO: should it be called BangBeforeTheBoom?)
    DanceOff,
    CrowbarTherapy, // attack the NPC with a crowbar
    AcademicFraud

}

public abstract class BaseTask {

    public abstract bool CheckCompletion(); // check if the task is completed

}

public class DoomsdayDropoffTask : BaseTask {

    [Header("References")]
    private readonly Hotbar hotbar; // reference to the hotbar to check if the player is holding the correct item

    [Header("Data")]
    private readonly ItemStack dropoffItemStack;

    public DoomsdayDropoffTask(ItemStack dropoffItemStack, Hotbar hotbar) {

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
}
