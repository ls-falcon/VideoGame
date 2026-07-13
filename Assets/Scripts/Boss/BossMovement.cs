using UnityEngine;

public class BossMovement : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform playerPosition;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;

    [SerializeField] private float minAttackDistance = 4f;

    [SerializeField] private float maxAttackDistance = 6f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (playerPosition == null)
            return;

        float distance =
            Vector2.Distance(
                transform.position,
                playerPosition.position);

        Vector2 direction =
            (playerPosition.position - transform.position).normalized;

        // Muy lejos → acercarse
        if (distance > maxAttackDistance)
        {
            transform.position +=
                (Vector3)(direction * moveSpeed * Time.deltaTime);

            animator.SetFloat("Movs", 1f);
        }
        // Muy cerca → alejarse
        else if (distance < minAttackDistance)
        {
            transform.position -=
                (Vector3)(direction * moveSpeed * Time.deltaTime);

            animator.SetFloat("Movs", 1f);
        }
        // Distancia correcta → quedarse quieto
        else
        {
            animator.SetFloat("Movs", 0f);
        }

        // Mirar al jugador
        spriteRenderer.flipX =
            playerPosition.position.x < transform.position.x;
    }

    public void SetTarget(Transform target)
    {
        playerPosition = target;
    }
}