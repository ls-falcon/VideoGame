using UnityEngine;

public class EnemyArcher : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform firePoint; // Punto desde donde sale la flecha

    [Header("Estadísticas de Combate")]
    [SerializeField] private float attackRange = 6f; // Distancia a la que se detiene a disparar
    [SerializeField] private float fireRate = 2f;     // Tiempo en segundos entre disparos

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

        // Instanciar la flecha
        GameObject arrowObj = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);

        // Calcular dirección hacia el jugador
        Vector2 direction = (player.position - firePoint.position).normalized;

        // Pasarle la dirección al script de la flecha
        Arrow arrowScript = arrowObj.GetComponent<Arrow>();
        if (arrowScript != null)
        {
            arrowScript.SetDirection(direction);
        }
    }
}