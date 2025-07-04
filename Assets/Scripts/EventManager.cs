using UnityEngine;

public class EventManager : MonoBehaviour {

    public void StartEvent(EventType eventType) {

        switch (eventType) {

            case EventType.DanceOff:
                break;

            default:
                Debug.LogError("Unknown event type: " + eventType);
                break;

        }
    }
}

public enum EventType {

    DanceOff

}
