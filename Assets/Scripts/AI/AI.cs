using Pathfinding;
using System.Collections;
using UnityEngine;

public class AI : MonoBehaviour {

    [Header("References")]
    [SerializeField] private TalkRadiusIndicator talkRadiusIndicator;
    [SerializeField] private Transform talkRadiusIndicatorPos;
    protected Animator animator;
    protected IAstarAI ai;
    private Transform player;
    private TalkRadiusIndicator currTalkRadiusIndicator;
    private Coroutine lookCoroutine;
    private bool isTalking;

    [Header("Settings")]
    [SerializeField] private float speedSmoothing; // smoothing factor for speed transitions
    [SerializeField] private bool ragdollCollisionEnabled; // whether to enable ragdoll physics on player collision
    [SerializeField] private float talkRadius;
    [SerializeField] private float lookSmoothing;
    private float smoothedSpeed; // used for smoothing speed transitions

    protected void Start() {

        ai = GetComponent<IAstarAI>();
        animator = GetComponent<Animator>();
        player = FindFirstObjectByType<PlayerController>().transform;

        currTalkRadiusIndicator = Instantiate(talkRadiusIndicator, transform.position + new Vector3(0f, 0.1f, 0f), Quaternion.Euler(-90f, 0f, 0f), transform);
        currTalkRadiusIndicator.Initialize(talkRadiusIndicatorPos, talkRadius); // initialize the talk radius indicator with the talk radius value

    }

    protected void Update() {

        #region TALKING
        float distToPlayer = Vector3.Distance(transform.position, player.position);

        if (distToPlayer < talkRadius && !isTalking)
            StartTalking();
        else if (distToPlayer >= talkRadius && isTalking)
            StopTalking();
        #endregion

        float targetSpeed = ai.canMove ? ai.velocity.magnitude : 0f; // target speed is the magnitude of the AI's velocity if it can move, otherwise it's 0
        smoothedSpeed = Mathf.Lerp(smoothedSpeed, targetSpeed, Time.deltaTime * speedSmoothing);
        animator.SetFloat("speed", smoothedSpeed);

    }

    private void StartTalking() {

        isTalking = true;
        ai.canMove = false; // disable AI movement
        ai.isStopped = true; // stop AI movement to set velocity to zero

        if (lookCoroutine != null) StopCoroutine(lookCoroutine); // stop any existing look coroutine
        lookCoroutine = StartCoroutine(LookAtPlayer()); // start looking at player

    }

    private void StopTalking() {

        isTalking = false;
        ai.canMove = true; // re-enable AI movement
        ai.isStopped = false; // allow AI to move again

    }

    private IEnumerator LookAtPlayer() {

        while (isTalking) {

            Vector3 dir = player.position - transform.position;
            dir.y = 0; // keep rotation on horizontal plane

            if (dir.sqrMagnitude > 0.01f) {

                Quaternion targetRotation = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * lookSmoothing);

            }

            yield return null;

        }

        lookCoroutine = null; // reset coroutine reference

    }

    public void OnPlayerCollision() {

        if (!ragdollCollisionEnabled) return; // if ragdoll collision is disabled, do nothing

        ai.canMove = false; // disable AI movement
        animator.enabled = false; // disable animator to allow for ragdoll physics

    }
}
