using UnityEngine;

public class Marker : MonoBehaviour {

    [Header("References")]
    private Transform player;

    private void Start() => player = FindFirstObjectByType<PlayerController>().transform;

    private void LateUpdate() => transform.LookAt(player.position); // rotate the marker to always face the player

}
