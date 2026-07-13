using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 5f;

    private Vector2 targetDirection;

    private void Start()
    {
        // Destruir la flecha después de unos segundos para no saturar la memoria
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Mover la flecha en la dirección asignada
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    public void SetDirection(Vector2 direction)
    {
        // Rotar la flecha hacia la dirección del movimiento
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Aquí conectas con tu sistema de vida del jugador (PlayerHealth)
        if (other.CompareTag("Player"))
        {
            // Ejemplo: other.GetComponent<PlayerHealth>().TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}