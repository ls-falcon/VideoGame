using UnityEngine;
using UnityEngine.InputSystem;

public class playerMovementController : MonoBehaviour
{
    private PlayerInputSystem playerInput;
    private Rigidbody2D rb;

    // Agregamos estas dos referencias
    private Animator animator;
    private SpriteRenderer spriteRenderer;

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

        // Inicializamos los componentes
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        moveX = playerInput.Player.HorizontalMovement.ReadValue<float>();

        // --- Lógica de Animación y Flip ---

        // Usamos Mathf.Abs para que si moveX es -1, se convierta en 1.
        // Así el parámetro "Movs" siempre refleja si hay movimiento.
        float movsValue = Mathf.Abs(moveX);
        animator.SetFloat("Movs", movsValue);

        // Voltear el sprite según la dirección
        if (moveX > 0)
        {
            spriteRenderer.flipX = false; // Derecha (Default)
        }
        else if (moveX < 0)
        {
            spriteRenderer.flipX = true; // Izquierda
        }
    }

    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundMask);

        // Nota: En versiones recientes de Unity se usa rb.linearVelocity 
        // Si usas una versión anterior, cámbialo a rb.velocity
        rb.linearVelocity = new Vector2(moveX * playerSpeed, rb.linearVelocity.y);

        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpPressed = false;
        }
    }

    void OnJump(InputAction.CallbackContext ctx)
    {
        // Solo permitimos marcar el salto si estamos tocando el suelo
        if (isGrounded)
        {
            jumpPressed = true;
        }
    }
}