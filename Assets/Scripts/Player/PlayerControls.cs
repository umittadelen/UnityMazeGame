using UnityEngine;

using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;

    // Slope handling removed: all surfaces are flat

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    public Transform orientation;


    float horizontalInput;
    float verticalInput;
    Vector2 moveInput;
    Vector3 moveDirection;
    Rigidbody rb;

    private PlayerInputActions inputActions;
    private bool jumpPressed;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();
        inputActions.Player.Jump.performed += ctx => jumpPressed = true;
    }

    private void OnDestroy()
    {
        inputActions.Player.Jump.performed -= ctx => jumpPressed = true;
        inputActions.Dispose();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        // Slope handling removed
    }

    private void Update()
    {
        if (Time.timeScale == 0) return;

        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

        MyInput();
        SpeedControl();

        if (grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;

        if (grounded && moveInput == Vector2.zero)
        {
            Vector3 vel = rb.linearVelocity;
            vel.x = Mathf.Lerp(vel.x, 0f, 10f * Time.deltaTime);
            vel.z = Mathf.Lerp(vel.z, 0f, 10f * Time.deltaTime);
            rb.linearVelocity = vel;
        }
    }

    private void FixedUpdate()
    {
        if (jumpPressed)
        {
            Jump();
            jumpPressed = false;
        }
        MovePlayer();
    }

    private void MyInput()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        horizontalInput = moveInput.x;
        verticalInput = moveInput.y;
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        rb.useGravity = true;
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        Vector3 vel = rb.linearVelocity;

        // Only reset downward velocity
        if (vel.y < 0f)
            vel.y = 0f;

        rb.linearVelocity = vel;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    // Slope-related methods removed: OnSlope, GetSlopeMoveDirection
}