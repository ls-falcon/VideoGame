using System.Collections;
using UnityEngine;

public class LightningMarker : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private SpriteRenderer markerSprite;

    [Header("Timing")]
    [SerializeField] private float warningTime = 1.5f;

    [Header("Damage")]
    [SerializeField] private int damage = 2;

    [SerializeField] private Vector2 damageArea = new Vector2(1.2f, 6f);

    [Header("Effects")]
    [SerializeField] private GameObject lightningEffectPrefab;

    private void Start()
    {
        StartCoroutine(WarningRoutine());
    }

    IEnumerator WarningRoutine()
    {
        // Parpadeo
        float timer = 0f;

        while (timer < warningTime)
        {
            markerSprite.enabled = !markerSprite.enabled;

            yield return new WaitForSeconds(0.15f);

            timer += 0.15f;
        }

        markerSprite.enabled = true;

        Strike();
    }

    void Strike()
    {
        if (lightningEffectPrefab != null)
        {
            Instantiate(
                lightningEffectPrefab,
                transform.position,
                Quaternion.identity
            );
        }

        Collider2D[] hits =
            Physics2D.OverlapBoxAll(
                transform.position,
                damageArea,
                0f
            );

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Player"))
                continue;

            PlayerHealth health =
                hit.GetComponent<PlayerHealth>();

            if (health != null)
            {
                health.TakeDamage(
                    damage,
                    transform.position
                );
            }
        }

        //Destroy(gameObject);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireCube(
            transform.position,
            damageArea
        );
    }
}