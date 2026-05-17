using System.Collections;
using TMPro;
using UnityEngine;

public class waveSpawner : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform playerPosition;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("UI")]
    [SerializeField] private TMP_Text waveText;

    private WaveData[] waves;

    private int currentWaveIndex = 0;
    private int enemiesAlive = 0;

    private void Start()
    {
        DifficultySettings difficulty =
            GameManager.Instance.currentDifficulty;

        waves = difficulty.waves;

        StartCoroutine(StartWaveRoutine());
    }

    IEnumerator StartWaveRoutine()
    {
        DifficultySettings difficulty =
            GameManager.Instance.currentDifficulty;

        int maxRounds = Mathf.Min(
            difficulty.numberOfRounds,
            waves.Length
        );

        while (currentWaveIndex < maxRounds)
        {
            waveText.text = "Oleada " + (currentWaveIndex + 1);

            WaveData currentWave = waves[currentWaveIndex];

            Debug.Log("Iniciando Wave " + (currentWaveIndex + 1));

            yield return StartCoroutine(SpawnWave(currentWave));

            yield return new WaitUntil(() => enemiesAlive == 0);

            currentWaveIndex++;

            yield return new WaitForSeconds(2f);
        }
    }

    IEnumerator SpawnWave(WaveData wave)
    {
        for (int i = 0; i < wave.enemyCount; i++)
        {
            GameObject enemyPrefab = GetRandomEnemy(wave);

            SpawnEnemy(enemyPrefab);

            yield return new WaitForSeconds(1f / wave.spawnRate);
        }
    }

    GameObject GetRandomEnemy(WaveData wave)
    {
        float totalWeight = 0f;

        foreach (WaveEnemyEntry enemy in wave.enemies)
        {
            totalWeight += enemy.spawnWeight;
        }

        float randomValue = Random.Range(0f, totalWeight);

        float currentWeight = 0f;

        foreach (WaveEnemyEntry enemy in wave.enemies)
        {
            currentWeight += enemy.spawnWeight;

            if (randomValue <= currentWeight)
            {
                return enemy.enemyData.enemyPrefab;
            }
        }

        return wave.enemies[0].enemyData.enemyPrefab;
    }

    void SpawnEnemy(GameObject enemyPrefab)
    {
        Transform spawnPoint =
            spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject enemy =
            Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

        enemiesAlive++;

        EnemyMovement enemyScript =
            enemy.GetComponent<EnemyMovement>();

        if (enemyScript != null)
        {
            enemyScript.setTarget(playerPosition);

            enemyScript.OnDeath += OnEnemyDeath;
        }
    }

    void OnEnemyDeath()
    {
        enemiesAlive--;
    }
}