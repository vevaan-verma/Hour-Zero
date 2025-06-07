using UnityEngine;

// <summary>
// This class is responsible for handling the AI joint collision with the player.
// It detects when the AI collides with the player and notifies the parent AI component.
// This is important because the parent AI component cannot detect the collision itself due to the rigidbody placements on the joints, preventing the parent from receiving the collision event.
// </summary>
public class AIJoint : MonoBehaviour {

    private void OnCollisionEnter(Collision collision) {

        // check if the collision is with the player
        if (collision.gameObject.CompareTag("Player")) {

            // check if the parent has an AI component
            var parentAI = GetComponentInParent<AI>();

            // call the OnPlayerCollision method on the parent AI component if it exists
            if (parentAI != null)
                parentAI.OnPlayerCollision();

        }
    }
}
