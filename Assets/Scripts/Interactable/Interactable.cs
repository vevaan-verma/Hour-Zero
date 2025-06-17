using UnityEngine;

public abstract class Interactable : MonoBehaviour {

    protected void Start() => transform.tag = "Interactable"; // ensure the object is tagged as Interactable

    public abstract void Interact();

}
