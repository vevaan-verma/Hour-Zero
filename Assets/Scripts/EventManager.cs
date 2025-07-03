using UnityEngine;

public class EventManager : MonoBehaviour {

    public void StartEvent(EventType eventType) {

        switch (eventType) {

            case EventType.BringItem:

                break;

            case EventType.HammerObject:

                break;

            case EventType.DanceOff:

                break;

            case EventType.CrowbarTherapy:

                break;

            case EventType.AcademicFraud:

                break;

            default:
                Debug.LogError("Unknown event type: " + eventType);
                break;

        }
    }
}

public enum EventType {

    BringItem, HammerObject, DanceOff, CrowbarTherapy, AcademicFraud

}
