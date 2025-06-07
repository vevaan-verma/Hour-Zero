using UnityEngine;

public class BunkerController : MonoBehaviour {

    [Header("Settings")]
    [SerializeField] private BunkerType bunkerType;

    private void Start() {


    }
}

public enum BunkerType {

    City, Mountain

}