using UnityEngine;

public class CameraFollow : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Transform target;
    private Vector3 offset;

    private void Start() => offset = transform.position - target.position;

    private void LateUpdate() => transform.SetPositionAndRotation(target.position + offset, target.rotation);

}
