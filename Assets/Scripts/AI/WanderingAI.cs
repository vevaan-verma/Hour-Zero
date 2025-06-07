using Pathfinding;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(IAstarAI))]
public class WanderingAI : AI {

    [Header("References")]
    private Coroutine waitCoroutine;
    private bool isWaiting;

    [Header("Settings")]
    [SerializeField][Tooltip("Minimum wait time at each destination")] private float minWaitTime;
    [SerializeField][Tooltip("Maximum wait time at each destination")] private float maxWaitTime;

    private new void Update() {

        base.Update();

        #region WAITING/PATHFINDING
        if (!isWaiting && !ai.pathPending && (ai.reachedEndOfPath || !ai.hasPath)) {

            if (waitCoroutine != null) StopCoroutine(waitCoroutine); // stop any existing wait coroutine
            waitCoroutine = StartCoroutine(WaitAndSetNewDestination());

        }
        #endregion
    }

    private IEnumerator WaitAndSetNewDestination() {

        isWaiting = true;
        float waitTime = Random.Range(minWaitTime, maxWaitTime);

        yield return new WaitForSeconds(waitTime);

        ai.destination = GetRandomPoint();
        ai.SearchPath();
        isWaiting = false;

        waitCoroutine = null; // reset coroutine reference

    }

    private Vector3 GetRandomPoint() {

        NNInfo sample = AstarPath.active.graphs[0].RandomPointOnSurface(NNConstraint.Walkable);
        return sample.position;

    }
}
