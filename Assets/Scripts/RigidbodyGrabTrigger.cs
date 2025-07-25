using UnityEngine;

public class RigidbodyGrabTrigger : MonoBehaviour {

    private Rigidbody rb;
    private bool triggered = false;

    /// <summary>
    /// 
    /// Makes an object with an rb be kinematic until it is grabbed for the first time
    /// 
    /// </summary>


    private void Start() {

        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

    }

    public void Trigger() {

        if (!triggered) {

            rb.isKinematic = false;
            triggered = true;

        }

    }

    public bool IsTriggered() => triggered;

}
