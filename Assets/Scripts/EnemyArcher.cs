using UnityEngine;

public class EnemyArcher : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform firePoint; // Punto desde donde sale la flecha

    [Header("Estadísticas de Combate")]
    [SerializeField] private float attackRange = 6f; // Distancia a la que se detiene a disparar
    [SerializeField] private float fireRate = 2f;     // Tiempo en segundos entre disparos

    [Header("Configuración de Precisión")]
    [SerializeField][Range(0f, 100f)] private float accuracy = 70f; // 70% de probabilidad de tiro perfecto
    [SerializeField] private float missOffset = 2f; // Qué tanto se desvía la flecha si falla (en unidades)

    private Transform player;
    private EnemyMovement movementScript;
    private float nextFireTime = 0f;

    private void Start()
    {
        // Obtenemos el script de movimiento para poder pausarlo cuando dispare
        movementScript = GetComponent<EnemyMovement>();

        // Buscamos al jugador por su Tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void Update()
    {
        if (player == null) return;

        // Calcular la distancia entre el arquero y el jugador
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            // 1. Si está en rango, detenemos el movimiento
            if (movementScript != null)
            {
                movementScript.enabled = false; // O un método como movementScript.StopMoving() si lo tienen
            }

            // 2. Temporizador para disparar periódicamente
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
        else
        {
            // Si el jugador se aleja, reactivamos el movimiento para que lo vuelva a perseguir
            if (movementScript != null)
            {
                movementScript.enabled = true;
            }
        }
    }

    private void Shoot()
    {
        if (arrowPrefab == null || firePoint == null) return;

        // 1. Guardamos la posición original del jugador
        Vector3 targetPosition = player.position;

        // 2. Calculamos una probabilidad de fallo (tiramos un dado de 0 a 100)
        float randomChance = Random.Range(0f, 100f);

        // Si el número aleatorio es MAYOR que nuestra precisión, el arquero FALLA
        if (randomChance > accuracy)
        {
            // Generamos un desvío aleatorio hacia arriba o hacia abajo
            float randomYOffset = Random.Range(-missOffset, missOffset);

            // Se lo sumamos a la posición objetivo (desviando el punto de mira)
            targetPosition += new Vector3(0, randomYOffset, 0);

            Debug.Log("¡El arquero falló el tiro a propósito!"); // Puedes borrar esto después
        }

        // 3. Instanciamos la flecha
        GameObject arrowObj = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);

        // 4. Calculamos la dirección hacia la posición (sea la perfecta o la desviada)
        Vector2 direction = (targetPosition - firePoint.position).normalized;

        Arrow arrowScript = arrowObj.GetComponent<Arrow>();
        if (arrowScript != null)
        {
            arrowScript.SetDirection(direction);
        }
    }
}