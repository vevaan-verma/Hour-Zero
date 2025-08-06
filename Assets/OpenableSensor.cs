using UnityEngine;

[RequireComponent(typeof(Collider))]
public class OpenableSensor : MonoBehaviour {

    private Collider trigger;
    [Header("Linked To")]
    [SerializeField, Tooltip("The openable to send a signal to")] private OpenableInteractable[] openables;

    [Header("Settings")]
    [SerializeField, Tooltip("Pause auto close timer when player/object is standing in the sensor")] private bool holdOpen;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {

        trigger = GetComponent<Collider>();

        foreach (OpenableInteractable openable in openables)
            openable.SetAutoOpenable();

    }

    private void OnTriggerEnter(Collider other) {

        // player only
        if (other.gameObject.layer == 6)
            foreach (OpenableInteractable openable in openables)
                openable.SensorInteract();

    }

    private void OnTriggerStay(Collider other) {

        if (other.gameObject.layer == 6 && holdOpen)
            foreach (OpenableInteractable openable in openables)
                openable.SetHeldOpen(true);


    }

    private void OnTriggerExit(Collider other) {

        // player only
        if (other.gameObject.layer == 6) {
            if (holdOpen)
                foreach (OpenableInteractable openable in openables)
                    openable.SetHeldOpen(false);
            else
                foreach (OpenableInteractable openable in openables)
                    openable.SensorInteract();
        }

    }

}
