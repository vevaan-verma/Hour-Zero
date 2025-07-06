

using System.Collections.Generic;
using UnityEngine;

public class BreakableProp : MonoBehaviour {

    /// <summary>
    /// A BreakableProp is a GameObject that, after being hit (by the player) or collided with (like if dropped) enough times,]
    ///     will be swapped with a broken version of itself
    ///     
    /// The operation is permanent and this script does not support the reconstruction of broken props, although the intact prop's
    ///     GameObject is merely deactivated, not actually destroyed
    /// </summary>

    [Header("Constants")]
    // (damage = durability to subtract)
    // multiply horizontal collision velocity by this to calculate the damage to deal 
    private const float horizontalVelocityDamageCoefficient = 0.65f;
    // multiply vertical collision velocity by this to calculate the damage to deal 
    private const float verticalVelocityDamageCoefficient = 1.25f;
    // multiply the attack force applied by the player onto the object by this to calculate the damage to deal 
    private const float attackForceDamageCoefficient = 0.045f;


    [Header("References")]
    [SerializeField][Tooltip("Undestroyed object. \n!!! KEEP AT 0, 0, 0 !!!")] private GameObject defaultObject;
    [SerializeField][Tooltip("Empty parent of the broken pieces. \n!!! KEEP AT 0, 0, 0 !!!")] private GameObject brokenObject;
    private List<Rigidbody> brokenObjectRbs = new List<Rigidbody>();
    private Rigidbody defaultObjectRb;
    private AudioPlayer audioPlayer;

    [Header("Prop Sturdiness")]
    [SerializeField] private bool breakable;
    [SerializeField] private bool startBroken;
    [SerializeField][Min(0.01f)][Tooltip("An abstract value that determines how much of a beating the prop can take before breaking.")] private float totalDurability;
    [SerializeField][Min(0)][Tooltip("In a collision, if the x+z impact speed is below this amount, the prop will not take damage")] private float minColSpeedHorizontal;
    [SerializeField][Min(0)][Tooltip("In a collision, if the y impact speed is below this amount, the prop will not take damage")] private float minColSpeedVertical;
    private float durability;
    private bool broken;

    [Header("\"Explosion\" Config")]
    [SerializeField][Range(0, 1)][Tooltip("When the prop breaks, the broken object pieces inherent the velocity of the intact object. This sets the min % of the velocity to inherit")] private float minInheritedVelocity;
    [SerializeField][Range(0, 1)][Tooltip("When the prop breaks, the broken object pieces inherent the velocity of the intact object. This sets the max % of the velocity to inherit")] private float maxInheritedVelocity;
    [SerializeField][Min(0)][Tooltip("Higher = more explosive. Tends to be more chaotic on small, light objects")] private float explosionMaxTorque;
    [SerializeField][Min(0)][Tooltip("Higher = more explosive")] private float explosionMaxForce;
    //[SerializeField][Tooltip("Make destructon effect force application consistent across the different broken parts by disregarding the masses of the objects")] private bool explosionIgnoresMass;

    [Header("Sound Effects")]
    [SerializeField] private SFXLib.Sounds breakSound;

    [Header("Debug")]
    [SerializeField] private bool logDamage;
    [SerializeField] private bool logCollisionSpeed;

    void Start() {

        // TODO: replace this with a live updating thing like used in kyiv
        if (maxInheritedVelocity < minInheritedVelocity) {

            Debug.LogWarning("BreakableProp \"" + gameObject.name + "\" has a max inherited velocity below its min inherited velocity. The max has been set to the min.");
            maxInheritedVelocity = minInheritedVelocity;

        }

        defaultObjectRb = defaultObject.GetComponent<Rigidbody>();
        audioPlayer = GetComponent<AudioPlayer>();

        // get the rb's of the broken pieces
        for (int c = 0; c < brokenObject.transform.childCount; c++)
            brokenObjectRbs.Add(brokenObject.transform.GetChild(c).GetComponent<Rigidbody>());

        if (!breakable && startBroken)
            Debug.LogWarning("BreakableProp \"" + gameObject.name + "\" is set to start broken, but is also marked as indestructible. Was this a mistake?");

        durability = totalDurability;

        broken = startBroken;
        brokenObject.SetActive(startBroken);
        defaultObject.SetActive(!startBroken);

        if (broken)
            HandleExplosion();
    }

    void Update() {

        if (durability <= 0 && breakable && !broken)
            BreakProp();

    }

    // replace default prop with broken version, set state to broken
    private void BreakProp() {

        if (!broken) {

            broken = true;

            defaultObject.SetActive(false);

            brokenObject.SetActive(true);

            // copy the kinematic state of the object onto the broken pieces
            brokenObject.transform.position = defaultObject.transform.position;
            brokenObject.transform.rotation = defaultObject.transform.rotation;
            foreach (Rigidbody brokenPart in brokenObjectRbs)
                brokenPart.linearVelocity = defaultObjectRb.linearVelocity * Random.Range(minInheritedVelocity, maxInheritedVelocity);

            HandleExplosion();

            audioPlayer.Play(breakSound, false, brokenObject);

            this.enabled = false;

        }
        else
            Debug.Log("BreakableProp \"" + gameObject.name + "\" is already broken!");

    }

    // apply a random torque and force to each object to help it break apart and for visual effect, instead of the broken pieces sitting in place
    // TODO: when an prop breaks due to a collision, apply a force relative to the impact speed in the explosion
    private void HandleExplosion() {

        if (broken)
            foreach (Rigidbody rb in brokenObjectRbs) {

                Vector3 torqueToApply = new Vector3(Random.Range(-explosionMaxTorque, explosionMaxTorque),
                                                    Random.Range(-explosionMaxTorque, explosionMaxTorque),
                                                    Random.Range(-explosionMaxTorque, explosionMaxTorque));

                Vector3 forceToApply = new Vector3(Random.Range(-explosionMaxForce, explosionMaxForce),
                                                    Random.Range(-explosionMaxForce, explosionMaxForce),
                                                    Random.Range(-explosionMaxForce, explosionMaxForce));

                rb.AddTorque(torqueToApply);
                rb.AddForce(forceToApply);

            }
        else
            print("Cannot play effect, object must be broken");
    }

    // called by a BreakablePropCollisionReporter (on the default object) to say:
    //      "you got hit, heres how hard you got hit"
    public void ReportCollision(Vector3 impactVel) {

        // abs value of the impact velocity
        Vector3 impactSpeed = new Vector3(Mathf.Abs(impactVel.x),
                                       Mathf.Abs(impactVel.y),
                                       Mathf.Abs(impactVel.z));

        float damage = 0f;

        string damageLog = "";
        string speedLog = "";

        // apply damage based on veritcal speed
        if (impactSpeed.y > minColSpeedVertical) {

            damage += impactSpeed.y * verticalVelocityDamageCoefficient;

            damageLog += "Vertical damage = " + impactSpeed.y * verticalVelocityDamageCoefficient + ". ";
            speedLog += "Vertical speed = " + impactSpeed.y + ". ";

        }

        float horizontalSpeed = new Vector2(impactSpeed.x, impactSpeed.z).magnitude;

        // apply damage based on horizontal speed
        if (horizontalSpeed > minColSpeedVertical) {

            damage += horizontalSpeed * horizontalVelocityDamageCoefficient;

            damageLog += "Horizontal damage = " + horizontalSpeed * horizontalVelocityDamageCoefficient + ". ";
            speedLog += "Horizontal speed = " + horizontalSpeed + ". ";

        }

        damageLog += "\n \t Total damage = " + damage;
        if (logDamage)
            Debug.Log(damageLog);
        if (logCollisionSpeed)
            Debug.Log(speedLog);

        durability -= damage;

    }

    // Register a strike from the player, i.e., a crowbar strike, and deal damage based on the
    //      striking tool's attack force
    public void ReportHit(float attackForce) {

        float damage = attackForce * attackForceDamageCoefficient;

        if (logDamage)
            Debug.Log("Player-dealt damage = " + damage + ". ");

        durability -= damage;

    }

}


