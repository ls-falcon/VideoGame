using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 5f;

    // Variable para guardar la dirección exacta hacia el jugador
    private Vector2 moveDirection;

    private void Start()
    {
        // Destruir la flecha después de unos segundos para no saturar la memoria
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Nos movemos directamente en la dirección calculada (inmune a errores de rotación)
        transform.position += (Vector3)moveDirection * speed * Time.deltaTime;
    }

    public void SetDirection(Vector2 direction)
    {
        // 1. Guardamos la dirección que nos dio el arquero
        moveDirection = direction;

        // 2. Rotamos el dibujo de la flecha para que apunte visualmente hacia el jugador
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Verificamos si la flecha chocó con el Jugador
        if (other.CompareTag("Player"))
        {
            // 2. Buscamos el script PlayerHealth en el jugador
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            // 3. Si lo encuentra, aplicamos el daño y le pasamos la posición de la flecha para el knockback
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage, transform.position);
            }

            // 4. Destruimos la flecha al impactar contra el jugador
            Destroy(gameObject);
        }
    }
}