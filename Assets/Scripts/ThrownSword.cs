using UnityEngine;


public class ThrownSword : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private bool stuckInEnemy = false;
    private Transform stuckTarget;
    [SerializeField] private float rotationSpeed = 600f;
    [SerializeField] private float throwForce = 3f;
    private Collider2D playerCollider;
    private Collider2D swordCollider;
    public bool IsThrown { get; private set; }
    private float currentRotationDirection = 1f;

    private void Update()
    {
        if (
            IsThrown &&
            !stuckInEnemy &&
            rb.linearVelocity.magnitude > 0.5f
        )
                {
                    transform.Rotate(
                        0,
                        0,
                        rotationSpeed * currentRotationDirection * Time.deltaTime
                    );
        }
    }

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        swordCollider = GetComponent<Collider2D>();

        
    }

    public void SetPlayerCollider(Collider2D collider)
    {
        playerCollider = collider;
    }

    public void Throw(Vector2 direction)
    {
        IsThrown = true;

        sr.enabled = true;

        transform.parent = null;

        transform.rotation = Quaternion.identity;

        rb.simulated = true;
        rb.bodyType = RigidbodyType2D.Dynamic;

        rb.linearVelocity = Vector2.zero;

        rb.AddForce(direction * throwForce, ForceMode2D.Impulse);
        currentRotationDirection = direction.x < 0 ? -1f : 1f;
        Physics2D.IgnoreCollision(
            swordCollider,
            playerCollider,
            true
        );
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsThrown) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Buscamos el Rigidbody2D del enemigo para poder moverlo físicamente
            Rigidbody2D enemyRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                // Calculamos la dirección del impacto usando la velocidad que llevaba la espada
                Vector2 pushDirection = rb.linearVelocity.normalized;

                // Le aplicamos un impulso físico al enemigo (puedes cambiar el 5f para darle más o menos fuerza)
                enemyRb.AddForce(pushDirection * 1f, ForceMode2D.Impulse);
            }

            // --- Tu lógica original para clavar la espada ---
            stuckInEnemy = true;
            stuckTarget = collision.transform;

            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;

            transform.parent = stuckTarget;
        }
    }

    public void AttachToPlayer(Transform swordHolder)
    {
        IsThrown = false;
        sr.enabled = false;
        stuckInEnemy = false;
        stuckTarget = null;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = false;

        Physics2D.IgnoreCollision(
            swordCollider,
            playerCollider,
            false
        );

        transform.parent = swordHolder;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
}