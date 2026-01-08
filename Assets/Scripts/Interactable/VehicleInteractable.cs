using UnityEngine;

public class VehicleInteractable : Interactable {

    [Header("References")]
    private CarController carController;

    private new void Start() {

        base.Start();
        carController = GetComponent<CarController>();

    }

    public override bool Interact() {

        if (!base.Interact()) return false; // if the base interaction fails, do not proceed

        playerController.DropGrabbedItem(); // make the player drop any grabbed item before entering the vehicle
        carController.EnterVehicle(); // enter the vehicle
        playerController.gameObject.SetActive(false); // hide the player model when in the vehicle

        return true;

    }
}
