using System.Collections;
using UnityEngine;

public class BossAttack : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Cooldown")]
    [SerializeField] private float attackCooldown = 4f;

    [Header("Fireball")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform fireballSpawnPoint;

    [Header("Lightning")]
    [SerializeField] private GameObject lightningMarkerPrefab;

    [Header("Summon")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private Transform[] summonPoints;
    [SerializeField] private int enemiesToSummon = 2;

    [Header("Sounds")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip summonSound;
    [SerializeField] private AudioClip fireballSound;

    private bool attacking = false;

    private void Start()
    {
        StartCoroutine(AttackRoutine());
    }

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void SetTarget(Transform target)
    {
        player = target;
    }

    IEnumerator AttackRoutine()
    {
        while (true)
        {
            if (!attacking)
            {
                attacking = true;

                yield return new WaitForSeconds(attackCooldown);

                int attack =
                    Random.Range(0, 3);

                switch (attack)
                {
                    case 0:
                        Debug.Log("FIREBALL");
                        FireballAttack();
                        break;

                    case 1:
                        Debug.Log("LIGHTNING");
                        LightningAttack();
                        break;

                    case 2:
                        Debug.Log("ENEMIES");
                        SummonEnemies();
                        break;
                }

                attacking = false;
            }

            yield return null;
        }
    }

    void FireballAttack()
    {
        if (fireballPrefab == null || player == null)
            return;

        audioSource.PlayOneShot(fireballSound);

        GameObject fireball = Instantiate(
            fireballPrefab,
            fireballSpawnPoint.position,
            Quaternion.identity
        );

        Fireball projectile =
            fireball.GetComponent<Fireball>();

        if (projectile != null)
        {
            projectile.SetTarget(player);
        }
    }

    void LightningAttack()
    {
        if (player == null)
            return;

        Vector3 targetPosition = player.position;

        Instantiate(
            lightningMarkerPrefab,
            targetPosition,
            Quaternion.identity
        );
    }

    void SummonEnemies()
    {
        if (enemyPrefabs.Length == 0 ||
            summonPoints.Length == 0)
            return;

        audioSource.PlayOneShot(summonSound);

        for (int i = 0; i < enemiesToSummon; i++)
        {
            GameObject prefab =
                enemyPrefabs[
                    Random.Range(
                        0,
                        enemyPrefabs.Length)];

            Transform spawn =
                summonPoints[
                    Random.Range(
                        0,
                        summonPoints.Length)];

            GameObject enemy =
                Instantiate(
                    prefab,
                    spawn.position,
                    Quaternion.identity);

            EnemyMovement movement =
                enemy.GetComponent<EnemyMovement>();

            if (movement != null)
            {
                movement.setTarget(player);
            }
        }
    }
}