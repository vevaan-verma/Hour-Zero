using Pathfinding;
using UnityEngine;

[RequireComponent(typeof(IAstarAI))] // scripts like AIPath and RichAI are IAstarAI components, so this ensures that the AI component is always present
public class AI : MonoBehaviour {

    [Header("References")]
    protected IAstarAI ai;

    [Header("Settings")]
    [SerializeField] private float speedSmoothing; // smoothing factor for speed transitions
    private float smoothedSpeed; // used for smoothing speed transitions

    protected void Start() => ai = GetComponent<IAstarAI>();

    protected void Update() {

        float targetSpeed = ai.canMove ? ai.velocity.magnitude : 0f; // target speed is the magnitude of the AI's velocity if it can move, otherwise it's 0
        smoothedSpeed = Mathf.Lerp(smoothedSpeed, targetSpeed, Time.deltaTime * speedSmoothing);

    }
}
