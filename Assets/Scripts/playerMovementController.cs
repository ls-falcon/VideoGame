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
    private float originalGravity;

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

    [Header("Sword Throw")]
    [SerializeField] private ThrownSword sword;
    [SerializeField] private Transform swordHolder;
    [SerializeField] private float pullSpeed = 15f;
    [SerializeField] private float momentumDecay = 15f; // Qué tan rápido pierde el impulso en el aire

    private Vector2 savedVelocity;
    private bool pullingToSword = false;
    private bool hasPullMomentum = false; // <--- NUEVA VARIABLE

    private void Awake()
    {
        playerInput = new PlayerInputSystem();
        playerInput.Player.Enable();

        playerInput.Player.Jump.performed += OnJump;
        playerInput.Player.Attack.performed += OnAttack;
        playerInput.Player.ThrowSword.performed += OnThrowSword;

        // Inicializamos los componentes
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        originalGravity = rb.gravityScale;
        sword.SetPlayerCollider(GetComponent<Collider2D>());

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
            spriteRenderer.flipX = false;
        }
        else if (moveX < 0)
        {
            spriteRenderer.flipX = true;
        }

        // Ajusta el punto de ataque segun la direccion del jugador
        float direction = spriteRenderer.flipX ? -1f : 1f;

        attackPoint.localPosition = new Vector3(
            direction * Mathf.Abs(originalAttackPointPos.x),
            originalAttackPointPos.y,
            originalAttackPointPos.z
        );
    }

    private void FixedUpdate()
    {
        if (pullingToSword)
        {
            Vector2 direction = (sword.transform.position - transform.position).normalized;
            rb.linearVelocity = direction * pullSpeed;

            float distance = Vector2.Distance(transform.position, sword.transform.position);
            if (distance < 1f)
            {
                RecoverSword();
            }
            return;
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundMask);

        // Si toca el suelo, el momentum se detiene inmediatamente
        if (isGrounded)
        {
            hasPullMomentum = false;
        }

        // --- MANEJO DE VELOCIDAD HORIZONTAL ---
        if (hasPullMomentum)
        {
            // El impulso del pull decae gradualmente hacia la velocidad objetivo del jugador (moveX * playerSpeed)
            float targetX = moveX * playerSpeed;
            float newX = Mathf.MoveTowards(rb.linearVelocity.x, targetX, momentumDecay * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
        }
        else
        {
            // Movimiento normal en suelo o aire sin momentum
            rb.linearVelocity = new Vector2(moveX * playerSpeed, rb.linearVelocity.y);
        }

        // --- LÓGICA DE SALTO ---
        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpPressed = false;
        }
    }

    void OnThrowSword(InputAction.CallbackContext ctx)
    {
        if (pullingToSword)
        {
            CancelPull();
            return;
        }

        if (!sword.IsThrown)
        {
            ThrowSword();
        }
        else
        {
            PullPlayerToSword();
        }
    }

    void CancelPull()
    {
        pullingToSword = false;
        rb.gravityScale = originalGravity;

        // Activamos el momentum para que no frene en seco
        hasPullMomentum = true;

        // BORRAMOS O COMENTAMOS ESTA LÍNEA:
        // rb.linearVelocity = savedVelocity; 
    }

    void RecoverSword()
    {
        pullingToSword = false;
        rb.gravityScale = originalGravity;
        animator.SetBool("NoSword", false);
        sword.AttachToPlayer(swordHolder);

        // OPCIONAL: Si también quieres que salga disparado con inercia 
        // al llegar con éxito a la espada, activa esto aquí también:
        hasPullMomentum = true;
    }

    void ThrowSword()
    {


        Vector2 direction = spriteRenderer.flipX
            ? Vector2.left
            : Vector2.right;

        sword.Throw(direction);
        animator.SetBool("NoSword", true);
    }

    void PullPlayerToSword()
    {
        if (sword == null || !sword.IsThrown) return;

        

        pullingToSword = true;

        rb.gravityScale = 0;
    }

   

    void OnJump(InputAction.CallbackContext ctx)
    {
        // Solo permitimos marcar el salto si estamos tocando el suelo
        if (pullingToSword) return;
        if (isGrounded)
        {
            jumpPressed = true;
        }
    }

    void OnAttack(InputAction.CallbackContext ctx)
    {
        if (pullingToSword) return;
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