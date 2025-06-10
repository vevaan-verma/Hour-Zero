using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Transform cameraPos;
    private GeneralUIManager uiManager;
    private Rigidbody rb;

    [Header("Movement")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float airMultiplier;
    private float moveSpeed;
    private float horizontalInput;
    private float verticalInput;

    [Header("Looking")]
    [SerializeField] private float xSensitivity;
    [SerializeField] private float ySensitivity;
    [SerializeField] private float topCameraClamp;
    [SerializeField] private float bottomCameraClamp;
    private float xRotation;
    private float yRotation;

    [Header("Jumping")]
    [SerializeField] private float jumpHeight;
    [SerializeField] private float fallMultiplier;

    [Header("Headbob")]
    [SerializeField] private float walkBobSpeed;
    [SerializeField] private float walkBobAmount;
    [SerializeField] private float sprintBobSpeed;
    [SerializeField] private float sprintBobAmount;
    [SerializeField] private float bobMovementThreshold; // minimum velocity to start headbob
    private float defaultYPos;
    private float timer;

    [Header("Interacting")]
    [SerializeField] private float interactDistance;

    [Header("Ground Check")]
    [SerializeField] private Transform feet;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private LayerMask environmentMask;
    private bool isGrounded;

    [Header("Drag")]
    [SerializeField] private float groundDrag;
    [SerializeField] private float airDrag;

    private void Start() {

        uiManager = FindFirstObjectByType<GeneralUIManager>(); // find the UI manager in the scene
        rb = GetComponent<Rigidbody>();

        defaultYPos = cameraPos.localPosition.y; // for headbob

    }

    private void Update() {

        // prevent player from doing other actions while a menu is open
        if (uiManager.IsMenuOpen()) return;

        #region GROUND CHECK
        isGrounded = Physics.CheckSphere(feet.position, groundCheckRadius, environmentMask);
        #endregion

        #region MOVEMENT INPUT
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        moveSpeed = Input.GetKey(KeyCode.LeftShift) && verticalInput > 0f ? sprintSpeed : walkSpeed; // set move speed to sprint speed if shift is held and player has a forward movement component, otherwise set to walk speed
        #endregion

        #region LOOKING
        float mouseX = Input.GetAxisRaw("Mouse X") * xSensitivity * Time.fixedDeltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * ySensitivity * Time.fixedDeltaTime;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -topCameraClamp, bottomCameraClamp);

        rb.MoveRotation(Quaternion.Euler(0f, yRotation, 0f));
        cameraPos.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        #endregion

        #region JUMPING
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded) Jump();

        if (rb.linearVelocity.y < 0f)
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        #endregion

        #region HEADBOB
        HandleHeadbob();
        #endregion

        #region INTERACTING
        RaycastHit hit;

        if (Physics.Raycast(cameraPos.position, cameraPos.forward, out hit, interactDistance) && hit.collider.CompareTag("Interactable")) { // check if player is looking at interactable object within interact distance and is tagged as interactable

            Interactable interactable = hit.collider.GetComponent<Interactable>();

            if (interactable) {

                uiManager.SetCrosshairType(CrosshairType.Interact); // set crosshair to interact crosshair

                if (Input.GetKeyDown(KeyCode.E))
                    interactable.Interact();

            } else {

                uiManager.SetCrosshairType(CrosshairType.Default); // set crosshair to default since interactable component was not found

            }
        } else {

            uiManager.SetCrosshairType(CrosshairType.Default); // set crosshair to default since player is not looking at interactable object

        }
        #endregion

        #region SPEED & DRAG CONTROL
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // get flat velocity (no y value)

        // limit flat velocity
        if (flatVel.magnitude > moveSpeed) {

            Vector3 controlledVel = flatVel.normalized * moveSpeed; // get controlled velocity
            rb.linearVelocity = new Vector3(controlledVel.x, rb.linearVelocity.y, controlledVel.z); // set controlled velocity

        }

        if (isGrounded) rb.linearDamping = groundDrag;
        else rb.linearDamping = airDrag;
        #endregion

    }

    private void FixedUpdate() {

        // prevent player from moving while a menu is open
        if (uiManager.IsMenuOpen()) return;

        if (isGrounded)
            rb.AddForce((transform.forward * verticalInput + transform.right * horizontalInput).normalized * moveSpeed, ForceMode.Force);
        else
            rb.AddForce(airMultiplier * moveSpeed * (transform.forward * verticalInput + transform.right * horizontalInput).normalized, ForceMode.Force);

    }

    private void Jump() => rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpHeight, rb.linearVelocity.z);

    private void HandleHeadbob() {

        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // get flat velocity (no y value)

        if (!isGrounded || flatVel.magnitude < bobMovementThreshold) return; // make sure player is grounded and moving fast enough to bob

        if (horizontalInput != 0f || verticalInput != 0f) {

            timer += Time.deltaTime * (moveSpeed == walkSpeed ? walkBobSpeed : sprintBobSpeed);
            cameraPos.localPosition = new Vector3(cameraPos.localPosition.x, defaultYPos + Mathf.Sin(timer) * (moveSpeed == walkSpeed ? walkBobAmount : sprintBobAmount), cameraPos.localPosition.z);

        } else {

            timer = 0f;
            cameraPos.localPosition = new Vector3(cameraPos.localPosition.x, Mathf.Lerp(cameraPos.localPosition.y, defaultYPos, Time.deltaTime * (moveSpeed == walkSpeed ? walkBobSpeed : sprintBobSpeed)), cameraPos.localPosition.z);

        }
    }
}
