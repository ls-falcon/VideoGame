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

    [Header("Ataque a enemigos")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 0.5f;
    [SerializeField] private LayerMask enemyLayer;
    private Vector3 originalAttackPointPos;
    private bool isAttacking = false;


    private float moveX;

    private void Awake()
    {
        playerInput = new PlayerInputSystem();
        playerInput.Player.Enable();

        playerInput.Player.Jump.performed += OnJump;
        playerInput.Player.Attack.performed += OnAttack;

        // Inicializamos los componentes
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        originalAttackPointPos = attackPoint.localPosition;
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

        // Ajusta el punto de ataque según la dirección del jugador
        float direction = spriteRenderer.flipX ? -1f : 1f;

        attackPoint.localPosition = new Vector3(
            direction * Mathf.Abs(originalAttackPointPos.x),
            originalAttackPointPos.y,
            originalAttackPointPos.z
        );
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

    void OnAttack(InputAction.CallbackContext ctx)
    {
        Attack();
    }

    public void DealDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRadius,
            enemyLayer
        );

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            enemyMovement enemy = enemyCollider.GetComponent<enemyMovement>();
            if (enemy != null)
            {
                enemy.Die();
            }
        }
    }

    void Attack()
    {
        if (isAttacking) return;

        isAttacking = true;
        animator.SetTrigger("Attack");
    }

    public void EndAttack()
    {
        isAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}