using UnityEngine;

public class enemyMovement : MonoBehaviour
{
    [SerializeField] private Transform playerPosition;
    [SerializeField] private float enemySpeed = 2f;
    [SerializeField] private float stoppingDistance = 0.1f; // Distancia mínima para detenerse

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
        if (playerPosition == null) return; // Seguridad por si el jugador no está asignado

        // Calculamos la distancia entre el enemigo y el jugador
        float distanceToPlayer = Vector2.Distance(transform.position, playerPosition.position);

        if (distanceToPlayer > stoppingDistance)
        {
            // --- MOVIMIENTO ---
            transform.position = Vector2.MoveTowards(transform.position, playerPosition.position, enemySpeed * Time.deltaTime);

            // --- ANIMACIÓN ---
            // Si se está moviendo, ponemos Movs en 1 (o podrías usar enemySpeed)
            animator.SetFloat("Movs", 1f);

            // --- GIRO (FLIP) ---
            // Si la X del jugador es mayor que la del enemigo, el jugador está a la derecha
            if (playerPosition.position.x > transform.position.x)
            {
                spriteRenderer.flipX = false; // Mira a la derecha (default)
            }
            // Si la X del jugador es menor, el jugador está a la izquierda
            else if (playerPosition.position.x < transform.position.x)
            {
                spriteRenderer.flipX = true; // Mira a la izquierda
            }
        }
        else
        {
            // Si llegó al jugador, ponemos Movs en 0 para que pase a Idle
            animator.SetFloat("Movs", 0f);
        }
    }
}