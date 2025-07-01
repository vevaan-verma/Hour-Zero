using System.Collections;
using UnityEngine;

public class HeldItem : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Item item;
    private Animator animator;
    private Coroutine attackCoroutine;

    private void Start() => animator = GetComponent<Animator>();

    public void Use() {

        if (attackCoroutine != null) return; // if an attack is already in progress, do nothing
        attackCoroutine = StartCoroutine(HandleAttack());

    }

    private IEnumerator HandleAttack() {

        animator.SetTrigger("attackWindUp"); // trigger the attack wind up animation

        yield return null; // wait for the animation to start
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length); // wait for the wind up animation to finish

        // perform a raycast from the camera to check if there is an object in front of the player within the attack distance & apply impact force if it has a rigidbody
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, item.GetAttackDistance()) && hit.rigidbody)
            hit.rigidbody.AddForceAtPosition(Camera.main.transform.forward * item.GetAttackForce(), hit.point, ForceMode.Impulse);

        animator.SetTrigger("attackWindDown"); // trigger the attack wind down animation

        yield return null; // wait for the animation to start
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length); // wait for the wind down animation to finish

        yield return new WaitForSeconds(item.GetAttackCooldown()); // wait for the attack cooldown before allowing another attack

        attackCoroutine = null; // reset the attack coroutine to allow for another attack

    }

    private void OnDrawGizmos() {

        // draw a ray from the camera to visualize the attack distance
        if (item != null) {

            Gizmos.color = Color.red;
            Gizmos.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * item.GetAttackDistance());

        }
    }
}
