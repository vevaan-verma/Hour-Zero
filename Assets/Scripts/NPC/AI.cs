using Pathfinding;
using UnityEngine;

public class AI : MonoBehaviour {

    [Header("References")]
    protected IAstarAI ai;

    protected void Start() => ai = GetComponent<IAstarAI>();

}
