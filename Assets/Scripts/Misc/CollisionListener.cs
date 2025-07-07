using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CollisionListener : MonoBehaviour {

    /// This script is placed on the intact version of a breakable breakableProp (the defaultObject) 
    /// it reports collision data to the BreakablebreakableProp, so the breakableProp can break when collided with
    /// it also is the thing that gets hit by the player and then reports that to the breakableProp 

    /// This script is also used to play object collision sounds 

    /// TODO: 
    /// -make this a more multifunctional "CollsionListener" script that also plays sounds on collision
    /// -put the timer in FixedUpdate or Update instead of a Coroutine 

    [Header("Constants")]
    // prevents spammy and broken collision detections and makes the behavior more reliable
    private const float collisionRegisterDelay = 0.12f;
    // prevents audio spam 
    private const float minSpeedForSound = 2.5f;

    private float cooldown;

    [Header("Sound")]
    [SerializeField] private SFXLib.Sounds colSound;

    [Header("Breakable Prop")]
    [SerializeField] private BreakableProp breakableProp;

    [Header("Settings")]
    [SerializeField] private bool ignorePlayerCollisions;

    private Rigidbody rb;
    private AudioPlayer audioPlayer;

    void Start() {

        rb = GetComponent<Rigidbody>();
        audioPlayer = GetComponent<AudioPlayer>();

        cooldown = collisionRegisterDelay;

    }

    void OnCollisionEnter(Collision collision) {

        string collisionTag = collision.transform.tag;

        // if the BreakableProp is defined and the collision cooldown is up
        //     and, if ignorePlayerCollisions, then this isnt a player collision
        if (cooldown <= 0f && !(collisionTag == "Player" && ignorePlayerCollisions)) {

            // start cd
            cooldown = collisionRegisterDelay;

            if (audioPlayer != null && collision.relativeVelocity.magnitude > minSpeedForSound)
                audioPlayer.Play(colSound);

            if (breakableProp != null)
                breakableProp.ReportCollision(collision.relativeVelocity);

        }

    }

    private void FixedUpdate() {

        // do cd timer
        if (cooldown > 0f)
            cooldown -= Time.fixedDeltaTime;

    }

    // called when this gets hit
    public void Hit(float attackForce) {

        if (breakableProp != null)
            breakableProp.ReportHit(attackForce);

    }

}
