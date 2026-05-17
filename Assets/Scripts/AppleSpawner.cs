using System.Collections;
using UnityEngine;

public class AppleSpawner : MonoBehaviour
{
    [Header("Apple")]
    [SerializeField] private GameObject applePrefab;

    [Header("Spawn Area")]
    [SerializeField] private float minX = -20f;

    [SerializeField] private float maxX = 20f;

    [SerializeField] private float raycastHeight = 20f;

    [SerializeField] private LayerMask groundLayer;

    [Header("Settings")]
    [SerializeField] private int maxApples = 2;

    [SerializeField] private float respawnDelay = 10f;

    private int currentApples = 0;

    private void Start()
    {
        for (int i = 0; i < maxApples; i++)
        {
            SpawnApple();
        }
    }

    void SpawnApple()
    {
        Vector2 spawnPosition = GetRandomGroundPosition();

        GameObject apple =
            Instantiate(
                applePrefab,
                spawnPosition,
                Quaternion.identity
            );

        currentApples++;

        ApplePickup applePickup =
            apple.GetComponent<ApplePickup>();

        if (applePickup != null)
        {
            applePickup.OnCollected += OnAppleCollected;
        }
    }

    Vector2 GetRandomGroundPosition()
    {
        for (int attempt = 0; attempt < 20; attempt++)
        {
            float randomX = Random.Range(minX, maxX);

            Vector2 rayOrigin =
                new Vector2(randomX, raycastHeight);

            RaycastHit2D hit =
                Physics2D.Raycast(
                    rayOrigin,
                    Vector2.down,
                    100f,
                    groundLayer
                );

            if (hit.collider != null)
            {
                return hit.point + Vector2.up * 0.5f;
            }
        }

        Debug.LogWarning("No se encontró suelo válido");

        return Vector2.zero;
    }

    void OnAppleCollected()
    {
        currentApples--;

        StartCoroutine(RespawnAppleRoutine());
    }

    IEnumerator RespawnAppleRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        if (currentApples < maxApples)
        {
            SpawnApple();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Vector3 left =
            new Vector3(minX, 0, 0);

        Vector3 right =
            new Vector3(maxX, 0, 0);

        Gizmos.DrawLine(left, right);
    }
}