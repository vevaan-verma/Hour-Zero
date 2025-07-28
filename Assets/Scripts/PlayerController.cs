using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Transform dropPoint;
    [SerializeField] private LayerMask nonPlayerMask; // mask for raycasts that should not hit the player
    private UIManager uiManager;
    private Rigidbody rb;
    private new Collider collider;

    [Header("Camera")]
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private Transform cameraPos;

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

    [Header("Interacting")]
    [SerializeField] private float interactRange;

    [Header("Holding")]
    [SerializeField] private LayerMask heldItemMask;
    private ItemHolder itemHolder;
    private HeldItem currHeldItem;

    [Header("Grabbing")]
    [SerializeField] private float grabRange;
    [SerializeField] private float grabStrength;
    [SerializeField] private LineRenderer grabLine;
    private Rigidbody currGrabbedObject;
    private float currGrabbedObjectDistance;
    private Dictionary<GameObject, LayerMask> currGrabbedObjectLayers;
    private Vector3 grabOffset; // offset from the grab point to the grabbed object's position

    [Header("Headbob")]
    [SerializeField] private float walkBobSpeed;
    [SerializeField] private float walkBobAmount;
    [SerializeField] private float sprintBobSpeed;
    [SerializeField] private float sprintBobAmount;
    [SerializeField] private float bobMovementThreshold; // minimum velocity to start headbob
    private float defaultYPos;
    private float timer;

    [Header("Phone")]
    private PhoneHolder phoneHolder;

    [Header("Ground Check")]
    [SerializeField] private Transform feet;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask environmentMask;
    private bool isGrounded;

    [Header("Drag")]
    [SerializeField] private float groundDrag;
    [SerializeField] private float airDrag;

    [Header("Developer Only")]
    [SerializeField] private KeyCode flightToggleKey;
    [SerializeField] private KeyCode flightAscendKey;
    [SerializeField] private KeyCode flightDescendKey;
    [SerializeField] private float flightSpeed;
    [SerializeField] private float verticalFlightForce;
    [SerializeField] private bool noClipFlight;
    private bool flightModeActive;

    private void Start() {

        uiManager = FindFirstObjectByType<UIManager>();
        rb = GetComponent<Rigidbody>();
        hotbar = FindFirstObjectByType<Hotbar>();
        itemHolder = FindFirstObjectByType<ItemHolder>();
        phoneHolder = FindFirstObjectByType<PhoneHolder>();
        collider = GetComponent<Collider>();

        defaultYPos = cameraPos.localPosition.y; // for headbob

    }

    private void Update() {

        // these are placed before the menu check because they need to occur regardless of whether a menu is open or not
        #region GROUND CHECK
        isGrounded = Physics.CheckSphere(feet.position, groundCheckDistance, environmentMask);
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

        // prevent player from doing other actions while a menu is open
        if (uiManager.IsMenuOpen()) {

            // if a menu is open, drop any grabbed object to prevent it from being stuck in the player's hand
            if (currGrabbedObject)
                DropGrabbedItem();

            return;

        }

        #region INPUT
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

        // if the player is falling, apply a fall multiplier to increase the fall speed
        if (rb.linearVelocity.y < 0f)
            rb.linearVelocity += (fallMultiplier - 1) * Physics.gravity.y * Time.deltaTime * Vector3.up;
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
        if (currHeldItem) // check if there is a currently held item
            if (Input.GetMouseButtonDown(0)) // check if the left mouse button is pressed
                currHeldItem.Attack(); // call the attack method on the held item
            else if (Input.GetMouseButtonDown(1)) // check if the right mouse button is pressed
                currHeldItem.Use(); // call the use method on the held item
        #endregion

        #region GRABBING
        // check if player is looking at a rigidbody within grab range and right mouse button is pressed; also make sure the rigidbody is not kinematic (so it can be grabbed); use the nonPlayerMask to prevent the player from grabbing themselves
        if (Physics.Raycast(cameraPos.position, cameraPos.forward, out RaycastHit hit, grabRange, nonPlayerMask) && hit.rigidbody) {

            RigidbodyGrabTrigger rbTrigger = hit.rigidbody.gameObject.GetComponent<RigidbodyGrabTrigger>();

            // if the object has an untriggered RigidbodyGrabTrigger, bypass the isKinematic check
            if (Input.GetMouseButtonDown(1) && (!hit.rigidbody.isKinematic || rbTrigger != null && !rbTrigger.IsTriggered())) {

                if (rbTrigger != null)
                    rbTrigger.Trigger(); // make the object not kinematic

                SetGrabbedItem(hit.rigidbody, hit.distance);

            }
        }

        if (currGrabbedObject)
            if (Input.GetMouseButtonUp(1)) // check if there is a currently grabbed object and the right mouse button is released and drop the grabbed object if so
                DropGrabbedItem();
            else if (Vector3.Distance(cameraPos.position, currGrabbedObject.position) > grabRange) // check if the grabbed object is still within grab range and if not, drop it (use else if here because if the other condition is true, the currGrabbedObject will be dropped anyway)
                DropGrabbedItem();

        UpdateGrabLine();
        #endregion

        #region INTERACTING
        if (Physics.Raycast(cameraPos.position, cameraPos.forward, out hit, interactRange, nonPlayerMask) && hit.collider.CompareTag("Interactable")) { // check if player is looking at interactable object within interact distance and is tagged as interactable; use the nonPlayerMask to prevent the player from interacting with themselves

            Interactable interactable = hit.collider.GetComponentInParent<Interactable>(); // make sure to check parent for interactable component since that is how some interactables are set up; use hit.collider not hit.transform to ensure the object is the one with the collider

            if (interactable) {

                if (Input.GetKeyDown(KeyCode.E))
                    interactable.Interact();

                interactable.ShowInteractIndicator();

            }
        }
        #endregion

        #region HEADBOB
        HandleHeadbob();
        #endregion

        #region CROSSHAIR
        SetCrosshair();
        #endregion

        #region DEVELOPER ONLY
#if UNITY_EDITOR
        if (Input.GetKeyDown(flightToggleKey)) {

            flightModeActive = !flightModeActive; // toggle flight mode when flight key is pressed

            if (flightModeActive) {

                rb.useGravity = false; // disable gravity when flight mode is activated
                collider.enabled = !noClipFlight; // disable collider when noclip flight is enabled to allow passing through objects

            } else {

                // no need to set the move speed here since it is already set to walk or sprint speed based on the input
                rb.useGravity = true; // enable gravity when flight mode is deactivated
                collider.enabled = true; // enable collider when flight mode is deactivated or noclip flight is disabled

            }
        }

        if (flightModeActive) {

            moveSpeed = flightSpeed; // set move speed to flight speed when flight mode is activated

            if (Input.GetKey(flightAscendKey))
                rb.AddForce(Vector3.up * verticalFlightForce, ForceMode.Acceleration); // apply upward force when the ascend key is held
            else if (Input.GetKey(flightDescendKey))
                rb.AddForce(Vector3.down * verticalFlightForce, ForceMode.Acceleration); // apply downward force when the descend key is held
            else
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // reset vertical velocity to keep the player at the same height when flight mode is activated and they aren't moving up or down

        }
#endif
        #endregion

    }

    private void FixedUpdate() {

        // prevent player from moving or grabbing while a menu is open
        if (uiManager.IsMenuOpen()) return;

        if (isGrounded) // check if the player is grounded or flying and apply movement accordingly
            rb.AddForce((transform.forward * verticalInput + transform.right * horizontalInput).normalized * moveSpeed, ForceMode.Force);
        else
            rb.AddForce(airMultiplier * moveSpeed * (transform.forward * verticalInput + transform.right * horizontalInput).normalized, ForceMode.Force);

        if (currGrabbedObject) { // check if the player is grabbing an object

            Vector3 targetPos = cameraPos.position + cameraPos.forward * currGrabbedObjectDistance + grabOffset;
            Vector3 toTarget = targetPos - currGrabbedObject.position;

            float grabVelocityMultiplier = grabStrength / currGrabbedObject.mass; // make the grab velocity multiplier inversely proportional to the mass of the grabbed object, so lighter objects are easier to grab and throw

            currGrabbedObject.linearVelocity = grabVelocityMultiplier * toTarget.magnitude * toTarget;

        }
    }

    private void LateUpdate() {

        bool menuOpen = uiManager.IsMenuOpen();

        // if a menu is open, smoothly return the held item position to the center point
        if (menuOpen) {

            itemHolder.SmoothReturnToCenter(); // smoothly return the item holder to the center position when a menu is open
            phoneHolder.SmoothReturnToCenter(); // smoothly return the phone holder to the center position when a menu is open

        }

        itemHolder.HandleSway(mouseX, mouseY, true, !menuOpen, !menuOpen); // handle the sway effect for the held item based on mouse movement; use LateUpdate to calculate sway to ensure the sway happens after all other updates, preventing jittering; the breathe effect is always enabled, headbob is enabled when not in a menu, and sway is enabled when not in a menu
        phoneHolder.HandleSway(mouseX, mouseY, !menuOpen, !menuOpen, !menuOpen); // handle the sway effect for the phone holder based on mouse movement; use LateUpdate to calculate sway to ensure the sway happens after all other updates, preventing jittering; the breathe effect is enabled when not in a menu, headbob is enabled when not in a menu, and sway is enabled when not in a menu

        cameraHolder.SetPositionAndRotation(cameraPos.position, cameraPos.rotation);

    }

    private void Jump() => rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpHeight, rb.linearVelocity.z);

    public void SetHeldItem(HeldItem heldItemPrefab) {

        // destroy any existing held item prefab at the held item position
        foreach (Transform child in itemHolder.transform)
            Destroy(child.gameObject);

        // instantiate the new held item prefab at the held item position if the heldItemPrefab is not null (a null parameter would clear the held item)
        if (heldItemPrefab) {

            currHeldItem = Instantiate(heldItemPrefab, itemHolder.transform.position, itemHolder.transform.rotation, itemHolder.transform); // instantiate the held item prefab at the held item position

            // set the layer of the held item and its children to the held item layer so it can be shown over the other objects in the scene to prevent clipping
            foreach (Transform child in currHeldItem.GetComponentsInChildren<Transform>())
                child.gameObject.layer = LayerMask.NameToLayer("HeldItem");

        } else {

            currHeldItem = null; // clear the held item

        }
    }

    public void SetGrabbedItem(Rigidbody grabbedObject, float hitDistance) {

        currGrabbedObject = grabbedObject; // store the grabbed rigidbody

        Vector3 grabPoint = cameraPos.position + cameraPos.forward * hitDistance; // calculate the grab point based on the camera position and the distance to the hit point
        grabOffset = currGrabbedObject.position - grabPoint; // calculate the offset from the grab point to the grabbed object's center (which is what rigidbody.position returns)

        currGrabbedObject.useGravity = false; // disable gravity on the grabbed object
        currGrabbedObject.freezeRotation = true;
        currGrabbedObjectDistance = hitDistance; // store the distance at which the object was grabbed

        currGrabbedObjectLayers = new Dictionary<GameObject, LayerMask>();

        // loop through the grabbed object and its children
        foreach (Transform child in currGrabbedObject.GetComponentsInChildren<Transform>()) {

            currGrabbedObjectLayers[child.gameObject] = child.gameObject.layer; // store the original layer of the grabbed object and its children
            child.gameObject.layer = LayerMask.NameToLayer("Grabbed"); // change the layer of the grabbed object and its children to prevent the player from jumping on the grabbed object to fly; this layer doesn't collide with the player

        }
    }

    private void UpdateGrabLine() {

        if (currGrabbedObject) { // check if the player is grabbing an object

            Vector3 crosshairPoint = cameraPos.position + cameraPos.forward * grabRange; // calculate the crosshair point based on the camera position and the grab range
            grabLine.SetPosition(0, crosshairPoint); // start the line at the crosshair point

            grabLine.SetPosition(1, currGrabbedObject.worldCenterOfMass); // end the line at the center of the grabbed object

            grabLine.enabled = true; // enable the grab line to show the grab range

        } else {

            grabLine.enabled = false; // disable the grab line if there is no grabbed object

        }
    }

    public void DropGrabbedItem() {

        if (!currGrabbedObject) return; // if there is no grabbed object, do nothing

        // restore the original layers of the grabbed object and its children
        foreach (KeyValuePair<GameObject, LayerMask> kvp in currGrabbedObjectLayers)
            kvp.Key.layer = kvp.Value;

        currGrabbedObject.freezeRotation = false; // allow rotation again
        currGrabbedObject.useGravity = true; // enable gravity on the grabbed object
        currGrabbedObject = null; // clear the grabbed object

    }

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

    public float GetHeadbobOffset() => cameraPos.localPosition.y - defaultYPos; // returns the headbob offset from the default position

    public void DropItemStack(ItemStack itemStack) {

        Item item = itemStack.GetItem();

        // instantiate each item in the item stack at the drop point position
        for (int i = 0; i < itemStack.GetCount(); i++)
            Instantiate(item.GetDroppedItemPrefab(), dropPoint.position, Quaternion.identity);

    }

    private void SetCrosshair() {

        // order of priority:
        // 1. grabbing crosshair
        // 2. interact crosshair
        // 3. grabbable crosshair
        // 4. default crosshair

        if (currGrabbedObject)
            uiManager.SetCrosshairType(CrosshairType.Grabbing);
        else if (Physics.Raycast(cameraPos.position, cameraPos.forward, out RaycastHit hit, interactRange, nonPlayerMask) && hit.collider.CompareTag("Interactable"))
            uiManager.SetCrosshairType(CrosshairType.Interact);
        else if (Physics.Raycast(cameraPos.position, cameraPos.forward, out hit, interactRange, nonPlayerMask) && hit.rigidbody && !hit.rigidbody.isKinematic)
            uiManager.SetCrosshairType(CrosshairType.Grabbable);
        else
            uiManager.SetCrosshairType(CrosshairType.Default);

    }

    public Transform GetCameraTransform() => cameraPos;

    public bool IsLookingAt(GameObject target) => Physics.Raycast(cameraPos.position, cameraPos.forward, out RaycastHit hit, interactRange) && hit.transform.gameObject == target;

}
