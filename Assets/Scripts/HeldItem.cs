using System.Collections;
using UnityEngine;

public class HeldItem : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Item item;
    private Backpack backpack;
    private Hotbar hotbar;
    private AudioPlayer audioPlayer;
    private Animator animator;
    private Coroutine attackCoroutine;
    private Coroutine useCoroutine;

    [Header("Settings")]
    private bool canAttack;
    private bool canUse;

    private void Awake() {

        backpack = FindFirstObjectByType<Backpack>();
        hotbar = FindFirstObjectByType<Hotbar>();
        audioPlayer = GetComponent<AudioPlayer>();
        animator = GetComponent<Animator>();

        canAttack = true; // allow attacking by default
        canUse = true; // allow using by default

    }

    public void Attack() {

        if (animator == null || !item.CanAttack()) return; // prevent attempts to attack with the item before the animator is set or if the item cannot be attacked with

        if (!canAttack) return; // if the item cannot be attacked with, do nothing
        attackCoroutine = StartCoroutine(HandleAttack());

    }

    private IEnumerator HandleAttack() {

        // by setting canUse in this coroutine, we can prevent the item from being used while the attack is in progress, but allow it to be used again after the attack is finished but before the cooldown begins

        canAttack = false; // prevent further attacks while the current attack is in progress
        canUse = false; // prevent using the item while the current attack is in progress

        animator.SetTrigger("attackWindUp"); // trigger the attack wind up animation

        yield return null; // wait for the wind up animation to start
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length); // wait for the wind up animation to finish

        // perform a raycast from the camera to check if there is an object in front of the player within the attack distance & apply impact force if it has a rigidbody
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, item.GetAttackDistance())) {

            if (hit.rigidbody) { // check if the hit object has a rigidbody

                // both of the below lines require the hit object to have a rigidbody

                hit.rigidbody.AddForceAtPosition(Camera.main.transform.forward * item.GetAttackForce(), hit.point, ForceMode.Impulse);
                hit.rigidbody.GetComponent<CollisionListener>()?.Hit(item.GetAttackForce()); // if the hit object has a CollisionListener component, call its Hit method with the attack force
                audioPlayer.Play(item.GetHitSound());

            }

            hit.transform.GetComponent<Strikable>()?.Strike(); // if the hit object has a Strikable component, call its Strike method; this doesn't require a rigidbody to be present on the hit object

        }

        yield return null; // wait for the wind down animation to start (it naturally begins after the wind up animation due to how the animator is set up)
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length); // wait for the wind down animation to finish

        canUse = true; // allow using the item again after the attack is finished but before the cooldown begins

        yield return new WaitForSeconds(item.GetAttackCooldown()); // wait for the attack cooldown before allowing another attack

        canAttack = true; // allow attacking again after the cooldown is finished
        attackCoroutine = null; // reset the attack coroutine to allow for another attack

    }

    public void Use() {

        if (animator == null || !item.CanUse()) return; // prevent attempts to use the item before the animator is set or if the item cannot be used

        if (!canUse) return; // if the item cannot be used, do nothing
        useCoroutine = StartCoroutine(HandleUse());

    }

    private IEnumerator HandleUse() {

        // by setting canAttack in this coroutine, we can prevent the item from being attacked while the use is in progress, but allow it to be attacked again after the use is finished but before the cooldown begins

        canUse = false; // prevent further uses while the current use is in progress
        canAttack = false; // prevent attacking the item while the current use is in progress

        animator.SetTrigger("useWindUp"); // trigger the use wind up animation

        yield return null; // wait for the wind up animation to start
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length); // wait for the wind up animation to finish

        yield return null; // wait for the wind down animation to start (it naturally begins after the wind up animation due to how the animator is set up)
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length); // wait for the wind down animation to finish

        if (item.ConsumeOnUse())
            backpack.RemoveItemStack(new ItemStack(item, 1), hotbar.GetSelectedIndex());

        canAttack = true; // allow attacking again after the use is finished but before the cooldown begins

        yield return new WaitForSeconds(item.GetUseCooldown()); // wait for the use cooldown before allowing another use

        canUse = true; // allow using again after the cooldown is finished
        useCoroutine = null; // reset the use coroutine to allow for another use

    }

    private void OnDrawGizmos() {

        // draw a ray from the camera to visualize the attack distance
        if (item != null) {

            Gizmos.color = Color.red;
            Gizmos.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * item.GetAttackDistance());

        }
    }
}
