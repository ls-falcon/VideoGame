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
    private Vector3 stuckOffset;
    public bool IsThrown { get; private set; }
    public float ThrowForce => throwForce;
    public float GravityScale => rb != null ? rb.gravityScale : 1f;
    private float currentRotationDirection = 1f;

    private void Update()
    {
        if (stuckInEnemy)
        {
            if (stuckTarget != null)
            {
                transform.position = stuckTarget.position + stuckOffset;
            }
            else
            {
                // El enemigo fue destruido
                stuckInEnemy = false;

                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.simulated = true;
            }
        }

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

        rb.freezeRotation = true;

        swordCollider = GetComponent<Collider2D>();
    }

    public void SetPlayerCollider(Collider2D collider)
    {
        playerCollider = collider;
    }

    public void Throw(Vector2 direction)
    {
        Throw(direction, throwForce);
    }

    public void Throw(Vector2 direction, float force)
    {
        IsThrown = true;

        sr.enabled = true;

        transform.parent = null;

        transform.rotation = Quaternion.identity;

        rb.simulated = true;
        rb.bodyType = RigidbodyType2D.Dynamic;

        rb.constraints = RigidbodyConstraints2D.None;

        rb.linearVelocity = Vector2.zero;

        rb.AddForce(direction * force, ForceMode2D.Impulse);

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
            Rigidbody2D enemyRb = collision.gameObject.GetComponent<Rigidbody2D>();

            if (enemyRb != null)
            {
                Vector2 pushDirection = rb.linearVelocity.normalized;
                enemyRb.AddForce(pushDirection * 1f, ForceMode2D.Impulse);
            }

            stuckInEnemy = true;
            stuckTarget = collision.transform;

            stuckOffset = collision.GetContact(0).point - (Vector2)stuckTarget.position;

            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = false;

            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }
    public void DetachFromEnemy()
    {
        stuckInEnemy = false;
        stuckTarget = null;

        rb.bodyType = RigidbodyType2D.Dynamic;
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
