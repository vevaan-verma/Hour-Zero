using UnityEngine;

[RequireComponent(typeof(CarController))]
public class VehicleInteractable : Interactable {

    [Header("References")]
    private CarController carController;

    private void Awake() => carController = GetComponent<CarController>();

    public override bool Interact() {

        if (!base.Interact()) return false; // if the base interaction fails, do not proceed

        carController.enabled = true; // enable the car controller
        player.gameObject.SetActive(false); // hide the player model when in the vehicle

        return true;

    }
}
