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

        // Efecto visual elemental de color en la espada
        UpdateElementalVisuals();
    }

    private void UpdateElementalVisuals()
    {
        PlayerMovementController pm = FindAnyObjectByType<PlayerMovementController>();
        if (pm != null && IsThrown)
        {
            if (pm.HasDragonFire)
            {
                sr.color = new Color(1f, 0.3f, 0f); // Fuego naranja
            }
            else if (pm.HasCelestialStorm)
            {
                sr.color = new Color(0f, 0.8f, 1f); // Tormenta cian
            }
            else if (pm.HasWindBoots)
            {
                sr.color = new Color(0.2f, 1f, 0.5f); // Viento verde
            }
            else
            {
                sr.color = Color.white;
            }
        }
        else
        {
            sr.color = Color.white;
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

        // Comprobar si golpeó enemigo o boss
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Boss") || collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            
            Rigidbody2D enemyRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                Vector2 pushDirection = rb.linearVelocity.normalized;
                enemyRb.AddForce(pushDirection * 1.5f, ForceMode2D.Impulse);
            }

            // Aplicar daño
            int damageToDeal = 1;
            PlayerMovementController pm = FindAnyObjectByType<PlayerMovementController>();
            if (pm != null)
            {
                damageToDeal = pm.SwordDamage;
            }

            EnemyHealth eh = collision.gameObject.GetComponent<EnemyHealth>();
            if (eh != null)
            {
                if (pm != null && pm.HasDragonFire)
                {
                    ApplyElementalSwordHit(collision.gameObject, pm);
                }
                else
                {
                    eh.TakeDamage(damageToDeal);
                }
            }
            else
            {
                BossHealth bh = collision.gameObject.GetComponent<BossHealth>();
                if (bh != null)
                {
                    bh.TakeDamage(damageToDeal);
                    ApplyElementalSwordHit(collision.gameObject, pm);
                }
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
        else if (collision.gameObject.CompareTag("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // Se clava en el suelo
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    private void ApplyElementalSwordHit(GameObject hitObj, PlayerMovementController pm)
    {
        if (pm == null) return;

        if (pm.HasDragonFire)
        {
            // Aplicar efecto de quemadura DoT
            BurnEffect burn = hitObj.GetComponent<BurnEffect>();
            if (burn == null)
            {
                hitObj.AddComponent<BurnEffect>();
            }
        }
        else if (pm.HasCelestialStorm)
        {
            // Cadena de rayos a enemigos cercanos
            Collider2D[] potentialTargets = Physics2D.OverlapCircleAll(hitObj.transform.position, 5f);
            int strikes = 0;
            foreach (Collider2D targetCol in potentialTargets)
            {
                if (targetCol.gameObject == hitObj) continue;
                if (strikes >= 2) break; // Máximo 2 objetivos extra

                bool validEnemy = targetCol.CompareTag("Enemy") || targetCol.CompareTag("Boss");
                if (validEnemy)
                {
                    EnemyHealth eh = targetCol.GetComponent<EnemyHealth>();
                    if (eh != null)
                    {
                        eh.TakeDamage(1);
                        CreateLightningArc(hitObj.transform.position, targetCol.transform.position);
                        strikes++;
                    }
                    else
                    {
                        BossHealth bh = targetCol.GetComponent<BossHealth>();
                        if (bh != null)
                        {
                            bh.TakeDamage(1);
                            CreateLightningArc(hitObj.transform.position, targetCol.transform.position);
                            strikes++;
                        }
                    }
                }
            }
        }
    }

    private void CreateLightningArc(Vector3 start, Vector3 end)
    {
        GameObject arc = new GameObject("LightningArc");
        LineRenderer lr = arc.AddComponent<LineRenderer>();
        lr.startWidth = 0.08f;
        lr.endWidth = 0.08f;
        
        if (sr != null)
        {
            lr.sharedMaterial = sr.sharedMaterial;
        }
        else
        {
            lr.material = new Material(Shader.Find("Sprites/Default"));
        }

        lr.startColor = Color.cyan;
        lr.endColor = Color.white;
        lr.positionCount = 3;

        // Crear una trayectoria quebrada para simular el rayo
        Vector3 middle = Vector3.Lerp(start, end, 0.5f) + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
        lr.SetPosition(0, start);
        lr.SetPosition(1, middle);
        lr.SetPosition(2, end);

        Destroy(arc, 0.12f);
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
