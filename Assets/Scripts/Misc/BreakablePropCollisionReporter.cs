using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BreakablePropCollisionReporter : MonoBehaviour {

    /// This script is placed on the intact version of a breakable prop (the defaultObject) 
    /// it reports collision data to the BreakableProp, so the prop can break when collided with
    /// it also is the thing that gets hit by the player and then reports that to the prop 

    [Header("Constants")]
    // prevents spammy and broken collision detections and makes the behavior more reliable
    private const float collisionRegisterDelay = 0.1f;

    [Header("References")]
    [SerializeField] private BreakableProp prop;
    private Rigidbody rb;

    [Header("Settings")]
    [SerializeField] private bool ignorePlayerCollisions;

    private bool registerCollision;

    void Start() {

        rb = GetComponent<Rigidbody>();

        StartCoroutine(WaitForCollisionRegisterDelay());

        if (prop == null) {

            // try to get the prop component from the parent
            prop = transform.parent.gameObject.GetComponent<BreakableProp>();

            // if it is still missing, error
            if (prop == null)
                throw new UnassignedReferenceException("(Null Reference) A BreakableObjectCollisionReporter is missing a reference to a BreakableObject GameObject");

        }

    }

    void OnCollisionEnter(Collision collision) {

        string collisionTag = collision.transform.tag;

        // if the BreakableProp is defined and the collision cooldown is up
        //     and, if ignorePlayerCollisions, then this isnt a player collision
        if (prop != null && registerCollision && !(collisionTag == "Player" && ignorePlayerCollisions)) {

            // start cd
            StartCoroutine(WaitForCollisionRegisterDelay());

            prop.ReportCollision(collision.relativeVelocity);

        }

    }

    private IEnumerator WaitForCollisionRegisterDelay() {

        registerCollision = false;

        yield return new WaitForSeconds(collisionRegisterDelay);

        registerCollision = true;

    }

    // called when this gets hit
    public void Hit(float attackForce) => prop.ReportHit(attackForce);

}
