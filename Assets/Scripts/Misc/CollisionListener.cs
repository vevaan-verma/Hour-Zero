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

    [Header("Collision Sound")]
    [SerializeField] private SFXLib.Sounds colSound;

    [Header("Breakable Prop")]
    [SerializeField] private bool linkedToBreakableProp;
    [SerializeField] private BreakableProp breakableProp;

    [Header("Misc")]
    [SerializeField] private bool ignorePlayerCollisions;

    private Rigidbody rb;
    private AudioPlayer audioPlayer;

    #region

    void Start() {

        rb = GetComponent<Rigidbody>();
        audioPlayer = GetComponent<AudioPlayer>();

        cooldown = collisionRegisterDelay;

        if (linkedToBreakableProp == true && breakableProp == null)
            Debug.LogError("CollisionListener " + gameObject.name + " (Parent: " + transform.parent.gameObject.name + ") is set as linked to a breakable prop, but no breakable prop has been assigned to it.");

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

            if (linkedToBreakableProp)
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

        if (linkedToBreakableProp)
            breakableProp.ReportHit(attackForce);

    }

    #endregion

    #region Custom Editor

#if UNITY_EDITOR

    // hide the breakableProp field if 
    // using UnityEditor prefix to avoid needing to hide the import in the final build -vv
    [UnityEditor.CustomEditor(typeof(CollisionListener), true)]
    public class InteractableEditor : UnityEditor.Editor {

        public override void OnInspectorGUI() {

            serializedObject.Update();

            // make sure its in the right order
            UnityEditor.SerializedProperty _colSound = serializedObject.FindProperty("colSound");
            UnityEditor.SerializedProperty _linkedToBreakableProp = serializedObject.FindProperty("linkedToBreakableProp");
            UnityEditor.SerializedProperty _breakableProp = serializedObject.FindProperty("breakableProp");
            UnityEditor.SerializedProperty _ignorePlayerCollisions = serializedObject.FindProperty("ignorePlayerCollisions");

            UnityEditor.EditorGUILayout.PropertyField(_colSound);

            UnityEditor.EditorGUILayout.PropertyField(_linkedToBreakableProp);
            if (_linkedToBreakableProp.boolValue)
                UnityEditor.EditorGUILayout.PropertyField(_breakableProp);

            UnityEditor.EditorGUILayout.PropertyField(_ignorePlayerCollisions);

            serializedObject.ApplyModifiedProperties();

        }
    }
#endif

    #endregion


}
