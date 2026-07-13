using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class waveSpawner : MonoBehaviour
{
    private enum BossPlatformBehavior
    {
        Keep,
        Hide,
        Destroy
    }

    [Header("Player")]
    [SerializeField] private Transform playerPosition;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("UI")]
    [SerializeField] private TMP_Text waveText;

    [Header("UI")]
    [SerializeField] private TMP_Text waveBannerText;

    [Header("Boss")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private BossPlatformBehavior bossPlatformBehavior =
        BossPlatformBehavior.Hide;
    private bool bossAlive = false;

    [Header("Music")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip bossMusic;
    [SerializeField] private AudioClip victoryMusic;

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
            waveText.gameObject.SetActive(false);

            //COLOCAR BANNER GRANDE SOBRE QUE RONDA ESTAMOS
            waveBannerText.gameObject.SetActive(true);

            waveBannerText.text =
                "===== OLEADA " + (currentWaveIndex + 1) + " =====";

            yield return new WaitForSeconds(2f);

            waveBannerText.gameObject.SetActive(false);

            waveText.gameObject.SetActive(true);

            waveText.text =
                "Oleada " + (currentWaveIndex + 1);

            waveText.text = "Oleada " + (currentWaveIndex + 1);

            WaveData currentWave = waves[currentWaveIndex];

            Debug.Log("Iniciando Wave " + (currentWaveIndex + 1));

            yield return StartCoroutine(SpawnWave(currentWave));

            yield return new WaitUntil(() => enemiesAlive == 0);

            currentWaveIndex++;

            yield return new WaitForSeconds(4f);
        }

        //FINAL BOSS

        yield return StartCoroutine(SpawnBoss());

        yield return new WaitUntil(() => bossAlive == false);

        //PANTALLA VICTORIA

        // Cambiar música
        if (musicSource != null)
        {
            musicSource.Stop();
            musicSource.clip = victoryMusic;
            musicSource.Play();
        }

        waveText.gameObject.SetActive(false);
        waveBannerText.gameObject.SetActive(true);

        waveBannerText.text = "¡VICTORIA!";
        waveBannerText.color = Color.green;

        yield return new WaitForSeconds(victoryMusic.length);

        SceneManager.LoadScene("MainMenu");
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

        EnemyMovement enemyMovement =
            enemy.GetComponent<EnemyMovement>();
        EnemyHealth enemyHealth =
            enemy.GetComponent<EnemyHealth>();

        if (enemyMovement != null)
        {
            enemyMovement.setTarget(playerPosition);
        }
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath += OnEnemyDeath;
        }
    }

    void OnEnemyDeath()
    {
        enemiesAlive--;
    }

    private void OnBossDeath()
    {
        bossAlive = false;
    }

    private IEnumerator SpawnBoss()
    {
        ApplyBossPlatformBehavior();

        waveText.gameObject.SetActive(false);

        waveBannerText.gameObject.SetActive(true);
        waveBannerText.color = Color.red;
        waveBannerText.text = "FINAL BOSS";

        // Cambiar música
        if(musicSource != null)
        {
            musicSource.Stop();
            musicSource.clip = bossMusic;
            musicSource.Play();
        }

        GameObject boss = Instantiate(
            bossPrefab,
            bossSpawnPoint.position,
            Quaternion.identity
        );

        BossMovement bossMovement = boss.GetComponent<BossMovement>();
        if (bossMovement != null)
        {
            bossMovement.SetTarget(playerPosition);
        }

        BossAttack bossAttack = boss.GetComponent<BossAttack>();
        if (bossAttack != null)
        {
            bossAttack.SetTarget(playerPosition);
        }

        BossHealth bossHealth = boss.GetComponent<BossHealth>();
        if (bossHealth != null)
        {
            bossAlive = true;
            bossHealth.OnDeath += OnBossDeath;
        }

        // Mostrar el banner durante 4 segundos
        yield return new WaitForSeconds(4f);

        waveBannerText.gameObject.SetActive(false);
    }

    private void ApplyBossPlatformBehavior()
    {
        switch (bossPlatformBehavior)
        {
            case BossPlatformBehavior.Hide:
                BreakablePlatform.HideAllPlatforms();
                break;

            case BossPlatformBehavior.Destroy:
                BreakablePlatform.DestroyAllPlatforms();
                break;
        }
    }
}
