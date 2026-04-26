using UnityEngine;
using UnityEngine.InputSystem;

public class playerMovementController : MonoBehaviour
{

    private PlayerInputSystem playerInput;
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool jumpPressed = false;

    [Header("Configuración de Movimiento")]
    [SerializeField] private float playerSpeed = 4f;
    [SerializeField] private float jumpForce = 6f;

    [Header("Detección de Suelo")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.2f;
    [SerializeField] private LayerMask groundMask;

    private float moveX;

    
    private void Awake()
    {
        playerInput = new PlayerInputSystem();
        playerInput.Player.Enable();

        playerInput.Player.Jump.performed += OnJump;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        moveX = playerInput.Player.HorizontalMovement.ReadValue<float>();
    }

    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundMask);

        rb.linearVelocity = new Vector2(moveX * playerSpeed, rb.linearVelocity.y);

        if(jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpPressed = false;
        }
    }

    void OnJump(InputAction.CallbackContext ctx)
    {
        jumpPressed = true;
    }
}
