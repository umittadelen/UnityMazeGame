
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform orientation;
    public Transform cameraFollowTarget;

    public float sensitivityX = 100f;
    public float sensitivityY = 100f;
    public float maxPitch = 80f;
    public float minPitch = -80f;


    private float xRotation;
    private float yRotation;

    private InputAction lookAction;
    private InputAction altAction;


    private void OnEnable()
    {
        lookAction = new InputAction(type: InputActionType.Value, binding: "<Mouse>/delta");
        lookAction.Enable();

        altAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/leftAlt");
        altAction.Enable();

        LockCursor(true);
    }

    private void OnDisable()
    {
        lookAction.Disable();
        altAction.Disable();
    }

    void Update()
    {
        if (Time.timeScale == 0) return;
        // Handle LeftAlt for cursor lock/unlock
        if (altAction.WasPressedThisFrame()) LockCursor(false);
        if (altAction.WasReleasedThisFrame()) LockCursor(true);

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Vector2 lookDelta = lookAction.ReadValue<Vector2>();
            float mouseX = lookDelta.x * sensitivityX;
            float mouseY = lookDelta.y * sensitivityY;

            yRotation += mouseX;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, minPitch, maxPitch);
        }

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);

        if (orientation != null)
            orientation.rotation = Quaternion.Euler(0f, yRotation, 0f);

        if (cameraFollowTarget != null)
            transform.position = cameraFollowTarget.position;
    }

    // Make sure your Input Actions asset has a Jump action (type: Button) with binding <Keyboard>/space for jump to work in PlayerControls.
    void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}