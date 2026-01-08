using System;
using UnityEngine;

[RequireComponent(typeof(VehicleInteractable))]
public class CarController : MonoBehaviour {

    [Header("References")]
    [SerializeField] private DriveType driveType;
    [SerializeField] private Wheel[] wheels;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Transform exitPosition;
    private VehicleInteractable vehicleInteractable;
    private PlayerController playerController;
    private UIManager uiManager;
    private Transform player;
    private CameraFollow cameraFollow;
    private Rigidbody rb;
    private Vector3 centerOfMass;

    [Header("Settings")]
    [SerializeField] private float steerSensitivity;
    [SerializeField] private float maxSteerAngle;
    [SerializeField] private float driveTorque;
    [SerializeField] private float brakeTorque;
    [SerializeField] private float exitBrakeTorque;
    [SerializeField] private float coastDrag;
    [SerializeField][Tooltip("In miles per hour (mph)")] private float maxSpeed;
    [SerializeField] private float exitHoldTime;
    private const float MPS_TO_MPH = 2.23694f; // conversion factor for meters per second to miles per hour
    private const float MPH_TO_MPS = 0.44704f; // conversion factor for miles per hour to meters per second
    private float throttleInput;
    private float steerInput;
    private float brakeInput;
    private float initialDrag;
    private bool inVehicle;

    private void Start() {

        vehicleInteractable = GetComponent<VehicleInteractable>();
        playerController = FindFirstObjectByType<PlayerController>();
        uiManager = FindFirstObjectByType<UIManager>();
        cameraFollow = FindFirstObjectByType<CameraFollow>();
        player = playerController.transform;
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass;

        initialDrag = rb.linearDamping;

    }

    private void Update() {

        if (!inVehicle) return; // do not process input if not in vehicle

        // get player input
        throttleInput = Input.GetAxisRaw("Vertical");
        steerInput = Input.GetAxis("Horizontal");
        brakeInput = Input.GetKey(KeyCode.Space) ? 1f : 0f;

        // check if the player wants to exit the vehicle (holding down the 'E' key for a specific amount of seconds)
        if (Input.GetKeyDown(KeyCode.E))
            Invoke(nameof(ExitVehicle), exitHoldTime);
        else if (Input.GetKeyUp(KeyCode.E))
            CancelInvoke(nameof(ExitVehicle));

        float speedInMPS = Mathf.Abs(Vector3.Dot(rb.linearVelocity, transform.forward)); // get the car's forward speed in m/s
        float speedInMPH = speedInMPS * MPS_TO_MPH;

        uiManager.UpdateVehicleHUD(Mathf.Min(speedInMPH, maxSpeed)); // update the vehicle HUD with the minimum of current speed and max speed to avoid displaying speeds higher than max speed (avoiding little jitters at max speed)

    }

    private void FixedUpdate() {

        HandleEngine();
        AnimateWheels();

        // apply drag when no throttle or brake input is given to gradually slow down the car
        if (Mathf.Approximately(throttleInput, 0f) && Mathf.Approximately(brakeInput, 0f))
            rb.linearDamping = coastDrag;
        else
            rb.linearDamping = initialDrag;

        // do not limit speed if not in vehicle
        if (!inVehicle)
            return;

        // limit the car's maximum speed (assuming maximum speed is in miles per hour)
        float currentSpeedMPS = Mathf.Abs(Vector3.Dot(rb.linearVelocity, transform.forward)); // get the car's forward speed in m/s
        float maxSpeedMPS = maxSpeed * MPH_TO_MPS; // convert max speed from mph to m/s

        // clamp the car's speed if it exceeds the maximum speed
        if (currentSpeedMPS > maxSpeedMPS)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeedMPS;

    }

    public void EnterVehicle() {

        inVehicle = true; // set the inVehicle flag to true
        vehicleInteractable.SetInteractable(false); // disable interaction while in vehicle

        cameraFollow.SetFollowTarget(cameraPivot, true); // set the camera to follow the vehicle's camera pivot in freecam mode
        uiManager.ShowDrivingHUD(maxSpeed); // show the driving HUD

    }

    private void ExitVehicle() {

        uiManager.HideDrivingHUD(); // hide the driving HUD

        player.SetPositionAndRotation(exitPosition.position + new Vector3(0f, player.localScale.y / 2f, 0f), Quaternion.Euler(0f, transform.eulerAngles.y, 0f)); // position the player at the exit point of the vehicle, facing the same direction as the vehicle
        playerController.ResetCameraPos(); // reset the player camera position to ensure the player is facing the same direction as the vehicle
        player.gameObject.SetActive(true); // activate the player model

        // reset all wheel torques and apply brake torque to stop the vehicle when exited
        foreach (Wheel wheel in wheels) {

            wheel.GetCollider().motorTorque = 0f;
            wheel.GetCollider().brakeTorque = exitBrakeTorque;

        }

        inVehicle = false; // set the inVehicle flag to false
        vehicleInteractable.SetInteractable(true); // re-enable interaction

    }

    private void HandleEngine() {

        float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward); // current forward speed in m/s
        float maxSpeedMPS = maxSpeed * MPH_TO_MPS; // convert max speed to m/s

        foreach (Wheel wheel in wheels) {

            // apply steering to front wheels only
            if (wheel.GetAxel() == Axel.Front)
                wheel.GetCollider().steerAngle = steerInput * steerSensitivity * maxSteerAngle;

            bool reversingAgainstMotion = (forwardSpeed > 1f && throttleInput < 0f) || (forwardSpeed < -1f && throttleInput > 0f);

            if (reversingAgainstMotion) {

                // hard brake instead of reverse torque
                wheel.GetCollider().motorTorque = 0f;
                wheel.GetCollider().brakeTorque = brakeTorque;

            } else {

                // determine if we should apply torque
                bool applyTorque = true;

                // forward throttle: stop torque if over max speed
                if (throttleInput > 0f && forwardSpeed >= maxSpeedMPS)
                    applyTorque = false;

                // reverse throttle: stop torque if below -max speed
                if (throttleInput < 0f && forwardSpeed <= -maxSpeedMPS)
                    applyTorque = false;

                // apply torque based on drive type and speed limit
                if (applyTorque)
                    // apply motor torque based on drive type
                    if ((driveType == DriveType.FWD && wheel.GetAxel() == Axel.Front) ||
                    (driveType == DriveType.RWD && wheel.GetAxel() == Axel.Rear) ||
                    driveType == DriveType.AWD)
                        wheel.GetCollider().motorTorque = throttleInput * driveTorque;
                    else
                        wheel.GetCollider().motorTorque = 0f; // no torque for non-driven wheels
                else
                    wheel.GetCollider().motorTorque = 0f; // no torque if speed limit reached

                wheel.GetCollider().brakeTorque = brakeInput * brakeTorque; // apply brake torque to all wheels

            }
        }
    }

    private void AnimateWheels() {

        // loop through each wheel and update its model position and rotation based on the collider
        foreach (Wheel wheel in wheels) {

            float rotationAngle = (Vector3.Dot(rb.linearVelocity, transform.forward) / wheel.GetCollider().radius) * Mathf.Rad2Deg * Time.fixedDeltaTime; // calculate the rotation angle based on the car's forward velocity and wheel radius
            wheel.GetModel().transform.Rotate(rotationAngle, 0f, 0f); // rotate the wheel model around its local X axis according to the calculated rotation angle (spins the wheels at the correct speed)

        }
    }
}

public enum Axel {

    Front,
    Rear

}

public enum DriveType {

    FWD,
    RWD,
    AWD

}

[Serializable]
public struct Wheel {

    [Header("References")]
    [SerializeField] private GameObject model;
    [SerializeField] private WheelCollider collider;
    [SerializeField] private Axel axel;

    public GameObject GetModel() => model;

    public WheelCollider GetCollider() => collider;

    public Axel GetAxel() => axel;

}
