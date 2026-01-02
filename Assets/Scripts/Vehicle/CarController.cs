using System;
using UnityEngine;

public class CarController : MonoBehaviour {

    [Header("References")]
    [SerializeField] private DriveType driveType;
    [SerializeField] private Wheel[] wheels;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Transform exitPosition;
    private PlayerController playerController;
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
    [SerializeField] private float maxSpeed;
    [SerializeField] private float exitHoldTime;
    private float throttleInput;
    private float steerInput;
    private float brakeInput;

    private void OnEnable() {

        cameraFollow = FindFirstObjectByType<CameraFollow>();
        cameraFollow.SetFollowTarget(cameraPivot, true);

    }

    private void Awake() {

        enabled = false;

        playerController = FindFirstObjectByType<PlayerController>();
        player = playerController.transform;
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass;

    }

    private void Update() {

        // get player input
        throttleInput = Input.GetAxisRaw("Vertical");
        steerInput = Input.GetAxis("Horizontal");
        brakeInput = Input.GetKey(KeyCode.Space) ? 1f : 0f;

        // check if the player wants to exit the vehicle (holding down the 'E' key for a specific amount of seconds)
        if (Input.GetKeyDown(KeyCode.E))
            Invoke(nameof(ExitVehicle), exitHoldTime);
        else if (Input.GetKeyUp(KeyCode.E))
            CancelInvoke(nameof(ExitVehicle));

    }

    private void ExitVehicle() {

        player.SetPositionAndRotation(exitPosition.position + new Vector3(0f, player.localScale.y / 2f, 0f), Quaternion.Euler(0f, transform.eulerAngles.y, 0f)); // position the player at the exit point of the vehicle, facing the same direction as the vehicle
        playerController.ResetCameraPos(); // reset the player camera position to ensure the player is facing the same direction as the vehicle
        player.gameObject.SetActive(true); // activate the player model

        // reset all wheel torques and apply brake torque to stop the vehicle when exited
        foreach (Wheel wheel in wheels) {

            wheel.GetCollider().motorTorque = 0f;
            wheel.GetCollider().brakeTorque = exitBrakeTorque;

        }

        enabled = false; // disable the car controller

    }

    private void FixedUpdate() {

        AnimateWheels();
        HandleEngine();

        // enforce max speed (both forward and reverse)
        Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity); // convert the velocity to local space

        if (Mathf.Abs(localVel.z) > maxSpeed) { // check if the local forward velocity exceeds max speed

            localVel.z = Mathf.Sign(localVel.z) * maxSpeed; // clamp the local forward velocity to max speed
            rb.linearVelocity = transform.TransformDirection(localVel); // convert back to world space and apply

        }
    }

    private void AnimateWheels() {

        // loop through each wheel and update its model position and rotation based on the collider
        foreach (Wheel wheel in wheels) {

            float rotationAngle = (Vector3.Dot(rb.linearVelocity, transform.forward) / wheel.GetCollider().radius) * Mathf.Rad2Deg * Time.fixedDeltaTime; // calculate the rotation angle based on the car's forward velocity and wheel radius
            wheel.GetModel().transform.Rotate(rotationAngle, 0f, 0f); // rotate the wheel model around its local X axis according to the calculated rotation angle (spins the wheels at the correct speed)

        }
    }

    private void HandleEngine() {

        foreach (Wheel wheel in wheels) {

            // apply steering to front wheels only
            if (wheel.GetAxel() == Axel.Front)
                wheel.GetCollider().steerAngle = steerInput * steerSensitivity * maxSteerAngle;

            float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward); // get the car's forward speed

            bool reversingAgainstMotion = (forwardSpeed > 1f && throttleInput < 0f) || (forwardSpeed < -1f && throttleInput > 0f);

            if (reversingAgainstMotion) {

                // hard brake instead of reverse torque
                wheel.GetCollider().motorTorque = 0f;
                wheel.GetCollider().brakeTorque = brakeTorque;

            } else {

                // apply motor torque based on drive type
                if ((driveType == DriveType.FWD && wheel.GetAxel() == Axel.Front) ||
                    (driveType == DriveType.RWD && wheel.GetAxel() == Axel.Rear) ||
                    driveType == DriveType.AWD)
                    wheel.GetCollider().motorTorque = throttleInput * driveTorque;
                else
                    wheel.GetCollider().motorTorque = 0f; // no torque for non-driven wheels

                wheel.GetCollider().brakeTorque = brakeInput * brakeTorque; // apply brake torque to all wheels (only has an effect if actually braking)

            }
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
