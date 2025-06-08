using UnityEngine;

public abstract class Interactable : MonoBehaviour {

    protected void Start() {

        if (!transform.CompareTag("Interactable"))
            Debug.LogWarning("ItemInteractable object is not tagged as Interactable! Interacting will not work for this object.");

    }

    public abstract void Interact();

}
