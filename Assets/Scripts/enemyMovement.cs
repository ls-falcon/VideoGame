using UnityEngine;
public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private Transform playerPosition;
    [SerializeField] private float enemySpeed = 2f;
    [SerializeField] private float stoppingDistance = 0.1f; // Distancia m nima para detenerse
    [SerializeField] private bool followTargetHeight = false;
    // C�digo agregado para da�o a personaje
    [SerializeField] private int damage = 1;
    [SerializeField] private float damageCooldown = 1f;
    private float lastDamageTime;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    void Awake()
    {
        // Obtenemos los componentes necesarios
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    void Update()
    {
        if (playerPosition == null) return; // Seguridad por si el jugador no est  asignado
        Vector2 targetPosition = followTargetHeight
            ? playerPosition.position
            : new Vector2(playerPosition.position.x, transform.position.y);

        // Calculamos la distancia entre el enemigo y el jugador
        float distanceToPlayer = Vector2.Distance(transform.position, targetPosition);
        if (distanceToPlayer > stoppingDistance)
        {
            // --- MOVIMIENTO ---
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, enemySpeed * Time.deltaTime);
            // --- ANIMACI N ---
            // Si se est  moviendo, ponemos Movs en 1 (o podr as usar enemySpeed)
            animator.SetFloat("Movs", 1f);
            // --- GIRO (FLIP) ---
            // Si la X del jugador es mayor que la del enemigo, el jugador est  a la derecha
            if (playerPosition.position.x > transform.position.x)
            {
                spriteRenderer.flipX = false; // Mira a la derecha (default)
            }
            // Si la X del jugador es menor, el jugador est  a la izquierda
            else if (playerPosition.position.x < transform.position.x)
            {
                spriteRenderer.flipX = true; // Mira a la izquierda
            }
        }
        else
        {
            // Si lleg  al jugador, ponemos Movs en 0 para que pase a Idle
            animator.SetFloat("Movs", 0f);
        }
    }
    public void setTarget(Transform target)
    {
        playerPosition = target;
    }
    //CODIGO AGREGADO PARA DA�O A PERSONAJE
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time >= lastDamageTime + damageCooldown)
            {
                PlayerHealth playerHealth =
                    collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage, transform.position);
                    lastDamageTime = Time.time;
                }
            }
        }
    }
}
