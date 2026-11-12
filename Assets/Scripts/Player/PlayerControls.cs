using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float groundDrag;
    public float jumpForce;
    public float airMultiplier;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    public Transform orientation;

    private Vector2 moveInput;
    private Rigidbody rb;

    private PlayerInputActions inputActions;
    private bool jumpPressed;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.Jump.performed += OnJump;
    }

    private void OnEnable() => inputActions.Player.Enable();
    private void OnDisable() => inputActions.Player.Disable();

    private void OnDestroy()
    {
        inputActions.Player.Jump.performed -= OnJump;
        inputActions.Dispose();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        jumpPressed = true;
    }

    private void Update()
    {
        if (Time.timeScale == 0) return;

        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        SpeedControl();

        rb.linearDamping = grounded ? groundDrag : 0f;

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

    private void MovePlayer()
    {
        Vector3 moveDirection = orientation.forward * moveInput.y + orientation.right * moveInput.x;
        float mult = grounded ? 1f : airMultiplier;
        rb.AddForce(moveDirection.normalized * moveSpeed * 10f * mult, ForceMode.Force);
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
        if (vel.y < 0f) vel.y = 0f;
        rb.linearVelocity = vel;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}
