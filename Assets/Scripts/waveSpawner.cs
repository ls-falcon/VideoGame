using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class waveSpawner : MonoBehaviour
{
    [SerializeField] private Transform playerPosition;

    [System.Serializable]
    public class EnemyType
    {
        public GameObject prefab;
        public int count;
    }

    [System.Serializable]
    public class Wave
    {
        public EnemyType[] enemies;
        public float spawnRate; // enemigos por segundo
    }

    public Wave[] waves;
    public Transform[] spawnPoints;

    private int currentWaveIndex = 0;
    private int enemiesAlive = 0;

    void Start()
    {
        StartCoroutine(StartWave());
    }

    IEnumerator StartWave()
    {
        while (currentWaveIndex < waves.Length)
        {
            Wave wave = waves[currentWaveIndex];

            yield return StartCoroutine(SpawnWave(wave));

            // Esperar a que todos mueran
            yield return new WaitUntil(() => enemiesAlive == 0);

            currentWaveIndex++;
            yield return new WaitForSeconds(2f);
        }

        Debug.Log("¡Todas las oleadas completadas!");
    }

    IEnumerator SpawnWave(Wave wave)
    {
        // Crear lista con todos los enemigos de la oleada
        List<GameObject> pool = new List<GameObject>();

        foreach (EnemyType enemyType in wave.enemies)
        {
            for (int i = 0; i < enemyType.count; i++)
            {
                pool.Add(enemyType.prefab);
            }
        }

        // Mezclar enemigos (shuffle)
        for (int i = 0; i < pool.Count; i++)
        {
            GameObject temp = pool[i];
            int randomIndex = Random.Range(i, pool.Count);
            pool[i] = pool[randomIndex];
            pool[randomIndex] = temp;
        }

        // Spawnear
        foreach (GameObject prefab in pool)
        {
            SpawnEnemy(prefab);
            yield return new WaitForSeconds(1f / wave.spawnRate);
        }
    }

    void SpawnEnemy(GameObject enemyPrefab)
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

        enemiesAlive++;

        // Suscribirse al evento de muerte
        enemyMovement enemyScript = enemy.GetComponent<enemyMovement>();
        if (enemyScript != null)
        {
            Debug.Log("Entro al script enemy");
            enemyScript.setTarget(playerPosition);
            enemyScript.OnDeath += OnEnemyDeath;
        }
    }

    void OnEnemyDeath()
    {
        enemiesAlive--;
    }
}
