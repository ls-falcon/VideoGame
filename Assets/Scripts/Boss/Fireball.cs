using UnityEngine;

public class Fireball : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private float lifeTime = 5f;

    [Header("Damage")]
    [SerializeField] private int damage = 2;

    private Vector2 direction;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    public void SetTarget(Transform target)
    {
        direction = (target.position - transform.position).normalized;

        // Rotar el sprite para mirar hacia donde viaja
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth health =
                collision.GetComponent<PlayerHealth>();

            if (health != null)
            {
                health.TakeDamage(damage, transform.position);
            }

            Destroy(gameObject);
        }

        if (collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}