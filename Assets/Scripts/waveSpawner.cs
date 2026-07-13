using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class waveSpawner : MonoBehaviour
{
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
        // Estirar el fondo y configurar color de cámara para eliminar el azul
        ExpandBackgroundAndSetCameraColor();

        // Instanciar el gestor de plataformas dinámicas automáticamente
        if (FindAnyObjectByType<DynamicPlatformManager>() == null)
        {
            gameObject.AddComponent<DynamicPlatformManager>();
        }

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

            // Decidir si aparece un Santuario de Valor en esta oleada
            bool shouldSpawnAltar = (currentWaveIndex > 0) && (currentWaveIndex % 2 == 1 || Random.value > 0.5f);
            if (shouldSpawnAltar)
            {
                StartCoroutine(DelayedAltarSpawn(6f));
            }

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

    private IEnumerator DelayedAltarSpawn(float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnRandomAltar();
    }

    private void SpawnRandomAltar()
    {
        float spawnX = Random.Range(-5f, 5f);
        float groundY = -3.2f;

        // Intentar encontrar el suelo real
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer == -1) groundLayer = 0;
        
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(spawnX, 10f), Vector2.down, 25f, 1 << groundLayer);
        if (hit.collider != null)
        {
            groundY = hit.point.y;
        }
        else
        {
            GameObject groundObj = GameObject.FindWithTag("Ground");
            if (groundObj != null)
            {
                groundY = groundObj.transform.position.y;
            }
        }

        Vector2 spawnPos = new Vector2(spawnX, groundY + 0.7f); // Altura corregida para colocarse en el suelo de la base

        GameObject altarObj = new GameObject("AltarOfValor");
        altarObj.transform.position = spawnPos;

        AltarOfValor altar = altarObj.AddComponent<AltarOfValor>();
        altar.blessingType = (AltarBlessingType)Random.Range(0, 3);

        StartCoroutine(ShowAltarAnnouncement(altar.blessingType));
    }

    private IEnumerator ShowAltarAnnouncement(AltarBlessingType type)
    {
        if (waveBannerText != null)
        {
            waveBannerText.gameObject.SetActive(true);
            Color origColor = waveBannerText.color;
            waveBannerText.color = new Color(1f, 0.85f, 0f); // Color oro medieval

            string blessingName = type == AltarBlessingType.DragonFire ? "Fuego" :
                                   type == AltarBlessingType.CelestialStorm ? "Tormenta" : "Viento";

            waveBannerText.text = $"Altar: {blessingName}";
            yield return new WaitForSeconds(1.5f);

            waveBannerText.gameObject.SetActive(false);
            waveBannerText.color = origColor;
        }

        // Mostrar la explicación pausando el juego
        ShowAltarExplanation(type);
    }

    private void ShowAltarExplanation(AltarBlessingType type)
    {
        // Pausar juego
        Time.timeScale = 0f;

        // Encontrar Canvas en la escena
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Time.timeScale = 1f;
            return;
        }

        // Crear contenedor principal del panel (más grande para acomodar el texto)
        GameObject panelObj = new GameObject("AltarExplanationPanel");
        panelObj.transform.SetParent(canvas.transform, false);
        
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(480f, 340f);
        panelRect.anchoredPosition = Vector2.zero;

        // Fondo oscuro
        Image bgImage = panelObj.AddComponent<Image>();
        bgImage.color = new Color(0.12f, 0.12f, 0.16f, 0.95f);

        // Añadir borde dorado
        GameObject borderObj = new GameObject("Border");
        borderObj.transform.SetParent(panelObj.transform, false);
        RectTransform borderRect = borderObj.AddComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.sizeDelta = new Vector2(-10f, -10f);
        Image borderImg = borderObj.AddComponent<Image>();
        borderImg.color = new Color(0.85f, 0.7f, 0.2f, 1f);

        // Contenedor interno
        GameObject contentBgObj = new GameObject("ContentBg");
        contentBgObj.transform.SetParent(borderObj.transform, false);
        RectTransform contentBgRect = contentBgObj.AddComponent<RectTransform>();
        contentBgRect.anchorMin = Vector2.zero;
        contentBgRect.anchorMax = Vector2.one;
        contentBgRect.sizeDelta = new Vector2(-6f, -6f);
        Image contentBgImg = contentBgObj.AddComponent<Image>();
        contentBgImg.color = new Color(0.15f, 0.15f, 0.2f, 1f);

        // Título del Altar
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(contentBgObj.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 0.83f);
        titleRect.anchorMax = new Vector2(1f, 0.98f);
        titleRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.fontSize = 20;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(1f, 0.85f, 0f);
        
        string blessingName = type == AltarBlessingType.DragonFire ? "Fuego de Dragón" :
                               type == AltarBlessingType.CelestialStorm ? "Tormenta Celestial" : "Botas de Viento";
        titleText.text = $"Santuario del Reino: {blessingName}";

        // Descripción
        GameObject descObj = new GameObject("DescriptionText");
        descObj.transform.SetParent(contentBgObj.transform, false);
        RectTransform descRect = descObj.AddComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0.04f, 0.22f);
        descRect.anchorMax = new Vector2(0.96f, 0.82f);
        descRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
        descText.fontSize = 11;
        descText.alignment = TextAlignmentOptions.Left;
        descText.color = Color.white;
        
        string recompensaText = "";
        switch (type)
        {
            case AltarBlessingType.DragonFire:
                recompensaText = "<b><color=#FFA500>Bendición de Fuego de Dragón:</color></b> Tu espada normal y arrojadiza prenderá fuego a los enemigos, causándoles daño continuo en el tiempo (DoT).";
                break;
            case AltarBlessingType.CelestialStorm:
                recompensaText = "<b><color=#00FFFF>Bendición de Tormenta Celestial:</color></b> La espada arrojadiza encadenará rayos a rivales cercanos. Al volar hacia tu espada, electrocutarás a los enemigos en el trayecto.";
                break;
            case AltarBlessingType.WindBoots:
                recompensaText = "<b><color=#00FF7F>Bendición de Botas de Viento:</color></b> Podrás impulsarte rápido en el aire (Air-Dash) presionando la tecla <b>Shift Izquierdo o la tecla F</b>. Apuntar la espada en el aire ralentizará el tiempo.";
                break;
        }

        string explanation = $"<b><color=#FFD700>EL MOTIVO:</color></b> Los espíritus ancestrales del reino han enviado un Altar del Valor para bendecir tu arma y ayudarte a sobrevivir.\n\n" +
                             $"<b><color=#FFD700>EL RETO:</color></b> Sacrifica <b>1 de tus corazones</b> interactuando con el Altar (tecla E) y <b>sobrevive durante 10 segundos</b> al ataque de los guardianes rápidos.\n\n" +
                             $"<b><color=#FFD700>LA RECOMPENSA:</color></b> {recompensaText}";

        descText.text = explanation;

        // Botón OK
        GameObject buttonObj = new GameObject("OkButton");
        buttonObj.transform.SetParent(contentBgObj.transform, false);
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.38f, 0.04f);
        buttonRect.anchorMax = new Vector2(0.62f, 0.18f);
        buttonRect.sizeDelta = Vector2.zero;
        
        Image buttonImg = buttonObj.AddComponent<Image>();
        buttonImg.color = new Color(0.85f, 0.7f, 0.2f, 1f);
        
        Button button = buttonObj.AddComponent<Button>();
        
        // Texto del Botón
        GameObject btnTextObj = new GameObject("ButtonText");
        btnTextObj.transform.SetParent(buttonObj.transform, false);
        RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.fontSize = 12;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = Color.black;
        btnText.text = "Aceptar";

        // Acción al hacer clic
        button.onClick.AddListener(() => {
            Time.timeScale = 1f;
            Destroy(panelObj);
        });
    }

    private void ExpandBackgroundAndSetCameraColor()
    {
        // 1. Cambiar el color de fondo de la cámara a un tono de bosque oscuro medieval
        if (Camera.main != null)
        {
            Camera.main.backgroundColor = new Color(0.04f, 0.05f, 0.07f);
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
        }

        // 2. Buscar y ampliar horizontalmente los fondos
        SpriteRenderer[] allSprites = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        foreach (SpriteRenderer sr in allSprites)
        {
            if (sr.gameObject.name.ToLower().Contains("background") || 
                sr.gameObject.name.ToLower().Contains("fondo") || 
                (sr.sprite != null && sr.sprite.name.ToLower().Contains("background")))
            {
                // Incrementar escala en X para estirar el fondo
                Vector3 localScale = sr.transform.localScale;
                sr.transform.localScale = new Vector3(localScale.x * 1.6f, localScale.y, localScale.z);
                
                // Desplazar un poco a la derecha para cubrir el hueco azul de la derecha
                sr.transform.position += new Vector3(2.5f, 0f, 0f);
            }
        }
    }
}