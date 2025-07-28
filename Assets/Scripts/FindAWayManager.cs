using UnityEngine;

public class FindAWayManager : MonoBehaviour {

    [Header("References")]
    private Transform player;

    [Header("Settings")]
    [SerializeField] private float cameraHeight;

    private void Start() {

        player = FindFirstObjectByType<PlayerController>().transform;
        GetComponent<Camera>().orthographicSize = cameraHeight / 2f; // set the orthographic size of the GPS camera based on the height above the player; divide by 2 because the orthographic size is half the height of the camera view

    }

    private void LateUpdate() => transform.SetPositionAndRotation(player.position + new Vector3(0f, cameraHeight, 0f), Quaternion.Euler(90f, player.eulerAngles.y, 0f)); // keep the camera above the player at a fixed height and set the rotation such that the player is always facing the top of the map

}
