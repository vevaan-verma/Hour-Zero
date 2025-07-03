using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Transform cameraPos;
    private UIManager uiManager;
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
    private float mouseX;
    private float mouseY;
    private float xRotation;
    private float yRotation;

    [Header("Jumping")]
    [SerializeField] private float jumpHeight;
    [SerializeField] private float fallMultiplier;

    [Header("Hotbar")]
    private Hotbar hotbar;

    [Header("Headbob")]
    [SerializeField] private float walkBobSpeed;
    [SerializeField] private float walkBobAmount;
    [SerializeField] private float sprintBobSpeed;
    [SerializeField] private float sprintBobAmount;
    [SerializeField] private float bobMovementThreshold; // minimum velocity to start headbob
    private float defaultYPos;
    private float timer;

    [Header("Holding")]
    [SerializeField] private float swayAmount;
    [SerializeField] private float swaySmoothness;
    [SerializeField] private float rotationSwayAmount;
    [SerializeField] private float rotationSwaySmoothness;
    [SerializeField] private float breathingAmplitude;
    [SerializeField] private float breathingFrequency;
    [SerializeField, Tooltip("Deadzone for mouse movement to prevent jittering in sway effect")] private float mouseSwayDeadzone;
    private ItemHolder itemHolder;
    private HeldItem currHeldItem;

    [Header("Grabbing")]
    [SerializeField] private float grabRange;
    [SerializeField] private float followStiffness;
    private Rigidbody currGrabbedObject;
    private float currGrabbedObjectDistance;
    private LayerMask currGrabbedObjectLayer;
    private Vector3 grabOffset; // offset from the grab point to the grabbed object's position

    [Header("Interacting")]
    [SerializeField] private float interactRange;

    [Header("Ground Check")]
    [SerializeField] private Transform feet;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private LayerMask environmentMask;
    private bool isGrounded;

    [Header("Drag")]
    [SerializeField] private float groundDrag;
    [SerializeField] private float airDrag;

    private void Start() {

        uiManager = FindFirstObjectByType<UIManager>(); // find the UI manager in the scene
        rb = GetComponent<Rigidbody>();
        hotbar = FindFirstObjectByType<Hotbar>();
        itemHolder = FindFirstObjectByType<ItemHolder>();

        itemHolder.Initialize(swayAmount, swaySmoothness, rotationSwayAmount, rotationSwaySmoothness, breathingAmplitude, breathingFrequency, mouseSwayDeadzone); // initialize the held item sway with the settings

        defaultYPos = cameraPos.localPosition.y; // for headbob

    }

    private void Update() {

        // prevent player from doing other actions while a menu is open
        if (uiManager.IsMenuOpen()) {

            if (currGrabbedObject)
                DropGrabbedItem(); // if a menu is open, drop any grabbed object to prevent it from being stuck in the player's hand

            return;

        }

        #region GROUND CHECK
        isGrounded = Physics.CheckSphere(feet.position, groundCheckRadius, environmentMask);
        #endregion

        #region MOVEMENT INPUT
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        moveSpeed = Input.GetKey(KeyCode.LeftShift) && verticalInput > 0f ? sprintSpeed : walkSpeed; // set move speed to sprint speed if shift is held and player has a forward movement component, otherwise set to walk speed
        #endregion

        #region LOOKING
        mouseX = Input.GetAxisRaw("Mouse X") * xSensitivity * Time.fixedDeltaTime;
        mouseY = Input.GetAxisRaw("Mouse Y") * ySensitivity * Time.fixedDeltaTime;

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

        #region HOTBAR
        if (Input.mouseScrollDelta.y != 0f)
            hotbar.CycleSlot(Input.mouseScrollDelta.y < 0f ? 1 : -1);

        // check for number keys 1-9 to select hotbar slots
        for (int i = 0; i < 9; i++)
            if (Input.GetKeyDown((i + 1).ToString()))
                hotbar.SelectSlot(i);
        #endregion

        #region TOOL USAGE
        if (Input.GetMouseButtonDown(0) && currHeldItem) // check for left mouse button press and if there is a currently held item
            currHeldItem.Use(); // call the use method on the held item
        #endregion

        #region HEADBOB
        HandleHeadbob();
        #endregion

        #region GRABBING
        // check if player is looking at a rigidbody within grab range and right mouse button is pressed; also make sure the rigidbody is not kinematic (so it can be grabbed)
        if (Input.GetMouseButtonDown(1) && Physics.Raycast(cameraPos.position, cameraPos.forward, out RaycastHit hit, grabRange) && hit.rigidbody && !hit.rigidbody.isKinematic)
            SetGrabbedItem(hit.rigidbody, hit.distance);

        if (currGrabbedObject) {

            // check if there is a currently grabbed object and the right mouse button is released and drop the grabbed object if so
            if (Input.GetMouseButtonUp(1))
                DropGrabbedItem();

            // check if the grabbed object is still within grab range and if not, drop it
            if (currGrabbedObject && Vector3.Distance(cameraPos.position, currGrabbedObject.position) > grabRange) {

                DropGrabbedItem();
                uiManager.SetCrosshairType(CrosshairType.Default); // reset crosshair to default when dropping the grabbed object

            } else {

                uiManager.SetCrosshairType(CrosshairType.Grab); // set crosshair to grab crosshair when holding an object

            }
        }
        #endregion

        #region INTERACTING
        if (Physics.Raycast(cameraPos.position, cameraPos.forward, out hit, interactRange) && hit.collider.CompareTag("Interactable")) { // check if player is looking at interactable object within interact distance and is tagged as interactable

            Interactable interactable = hit.transform.GetComponentInParent<Interactable>(); // make sure to check parent for interactable component since that is how some interactables are set up

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

        if (currGrabbedObject) {

            Vector3 targetPos = cameraPos.position + cameraPos.forward * currGrabbedObjectDistance + grabOffset; // calculate the target position for the grabbed object based on the camera position, forward direction, distance at which it was grabbed, and the grab offset
            Vector3 toTarget = targetPos - currGrabbedObject.position; // calculate the vector from the grabbed object's position to the target position
            currGrabbedObject.linearVelocity = toTarget * toTarget.magnitude * 10f; // set the grabbed object's velocity to move it towards the target position; this is a simple way to make the object follow the player without using physics forces

        }
    }

    private void LateUpdate() {

        bool menuOpen = uiManager.IsMenuOpen();

        // if a menu is open, smoothly return the held item position to the center point
        if (menuOpen)
            itemHolder.SmoothReturnToCenter();

        itemHolder.HandleSway(mouseX, mouseY, true, !menuOpen, !menuOpen); // handle the sway effect for the held item based on mouse movement; use LateUpdate to calculate sway to ensure the sway happens after all other updates, preventing jittering; the breathe effect is always enabled, headbob is enabled when not in a menu, and sway is enabled when not in a menu

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

    public void SetHeldItem(HeldItem heldItemPrefab) {

        // destroy any existing held item prefab at the held item position
        foreach (Transform child in itemHolder.transform)
            Destroy(child.gameObject);

        // instantiate the new held item prefab at the held item position if the heldItemPrefab is not null (a null parameter would clear the held item)
        if (heldItemPrefab)
            currHeldItem = Instantiate(heldItemPrefab, itemHolder.transform.position, itemHolder.transform.rotation, itemHolder.transform); // instantiate the held item prefab at the held item position
        else
            currHeldItem = null; // clear the held item

    }

    public void SetGrabbedItem(Rigidbody grabbedObject, float hitDistance) {

        currGrabbedObject = grabbedObject; // store the grabbed rigidbody
        Vector3 grabPoint = cameraPos.position + cameraPos.forward * hitDistance; // calculate the grab point based on the camera position and the distance to the hit point
        grabOffset = currGrabbedObject.position - grabPoint; // calculate the offset from the grab point to the grabbed object's center (which is what rigidbody.position returns)
        currGrabbedObject.useGravity = false; // disable gravity on the grabbed object
        currGrabbedObject.freezeRotation = true;
        currGrabbedObjectDistance = hitDistance; // store the distance at which the object was grabbed
        currGrabbedObjectLayer = currGrabbedObject.gameObject.layer; // store the layer of the grabbed object
        currGrabbedObject.gameObject.layer = LayerMask.NameToLayer("Default"); // change the layer of the grabbed object to prevent the player from jumping on the grabbed object to fly

    }

    public void DropGrabbedItem() {

        if (!currGrabbedObject) return; // if there is no grabbed object, do nothing

        currGrabbedObject.gameObject.layer = currGrabbedObjectLayer; // restore the original layer of the grabbed object
        currGrabbedObject.freezeRotation = false; // allow rotation again
        currGrabbedObject.useGravity = true; // enable gravity on the grabbed object
        currGrabbedObject = null; // clear the grabbed object

    }

    public float GetHeadbobOffset() => cameraPos.localPosition.y - defaultYPos; // returns the headbob offset from the default position

}
