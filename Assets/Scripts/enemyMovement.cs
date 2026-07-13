using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private Transform playerPosition;
    [SerializeField] private float enemySpeed = 2f;
    [SerializeField] private float stoppingDistance = 0.1f; // Distancia m nima para detenerse
    // Cdigo agregado para dao a personaje
    [SerializeField] private int damage = 1;
    [SerializeField] private float damageCooldown = 1f;
    private float lastDamageTime;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Transform lowestPlatformTransform;
    private bool reachedLowestPlatform = false;

    void Awake()
    {
        // Obtenemos los componentes necesarios
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        FindLowestPlatform();
    }

    private void FindLowestPlatform()
    {
        GameObject[] platforms = GameObject.FindGameObjectsWithTag("Ground");
        float lowestY = float.MaxValue;
        GameObject lowestPlat = null;
        foreach (GameObject plat in platforms)
        {
            // Filtrar sólo las plataformas generadas y no el suelo principal
            if (plat.name.Contains("Platform") && plat.transform.position.y < lowestY)
            {
                lowestY = plat.transform.position.y;
                lowestPlat = plat;
            }
        }
        if (lowestPlat != null)
        {
            lowestPlatformTransform = lowestPlat.transform;
            Debug.Log($"[EnemyMovement] Enemigo '{gameObject.name}' fijó como destino inicial la plataforma más baja en Y: {lowestY}");
        }
    }

    void Update()
    {
        if (playerPosition == null) return; // Seguridad por si el jugador no est  asignado

        Vector3 targetPos = playerPosition.position;

        // Lógica de descenso ordenada: si el jugador está abajo y el enemigo está arriba de la plataforma más baja
        if (lowestPlatformTransform != null && !reachedLowestPlatform && playerPosition.position.y < transform.position.y)
        {
            // El objetivo temporal es la plataforma de más abajo
            targetPos = lowestPlatformTransform.position + Vector3.up * 0.5f;

            // Si el enemigo ya bajó por debajo de la altura de la plataforma más baja o está muy cerca, persigue al jugador
            if (transform.position.y <= lowestPlatformTransform.position.y + 0.1f || Vector2.Distance(transform.position, targetPos) < 1.2f)
            {
                reachedLowestPlatform = true;
            }
        }

        // Calculamos la distancia al objetivo dinámico
        float distanceToTarget = Vector2.Distance(transform.position, targetPos);
        if (distanceToTarget > stoppingDistance)
        {
            // --- MOVIMIENTO ---
            transform.position = Vector2.MoveTowards(transform.position, targetPos, enemySpeed * Time.deltaTime);
            // --- ANIMACI N ---
            // Si se est  moviendo, ponemos Movs en 1 (o podr as usar enemySpeed)
            animator.SetFloat("Movs", 1f);
            // --- GIRO (FLIP) ---
            // Si la X del objetivo es mayor que la del enemigo, el objetivo est  a la derecha
            if (targetPos.x > transform.position.x)
            {
                spriteRenderer.flipX = false; // Mira a la derecha (default)
            }
            // Si la X del objetivo es menor, el objetivo est  a la izquierda
            else if (targetPos.x < transform.position.x)
            {
                spriteRenderer.flipX = true; // Mira a la izquierda
            }
        }
        else
        {
            // Si lleg  al objetivo, ponemos Movs en 0 para que pase a Idle
            animator.SetFloat("Movs", 0f);
        }
    }
    public void setTarget(Transform target)
    {
        playerPosition = target;
    }
    //CODIGO AGREGADO PARA DAO A PERSONAJE
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Si el enemigo está en llamas (tiene BurnEffect), no hace daño al jugador
            if (GetComponent<BurnEffect>() != null) return;

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