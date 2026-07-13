using System.Collections;
using UnityEngine;

public enum AltarBlessingType
{
    DragonFire,
    CelestialStorm,
    WindBoots
}

public class AltarOfValor : MonoBehaviour, IInteractable
{
    [Header("Configuración del Altar")]
    public AltarBlessingType blessingType;
    [SerializeField] private float trialDuration = 10f;
    [SerializeField] private int enemiesToSpawn = 3;
    [SerializeField] private float spawnRadius = 4f;

    private bool trialActive = false;
    private bool trialCompleted = false;
    private float trialTimer;
    
    private PlayerHealth playerHealth;
    private PlayerMovementController playerMovement;
    private SpriteRenderer spriteRenderer;

    private GameObject interactionPopup;
    private Transform popupAnchor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        spriteRenderer.sortingOrder = 16;

        // Crear una colisión Trigger para interactuar
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.2f, 1.5f);
            col.isTrigger = true;
        }

        // Configurar capa interactuable
        gameObject.layer = LayerMask.NameToLayer("Interactable");
        if (gameObject.layer == -1)
        {
            gameObject.layer = 0; // Default
        }

        // Crear anchor para el popup de interacción
        GameObject anchorObj = new GameObject("PopupAnchor");
        anchorObj.transform.parent = this.transform;
        anchorObj.transform.localPosition = new Vector3(0, 1.2f, 0);
        popupAnchor = anchorObj.transform;

        // Generar el aspecto procedimental según el tipo de bendición
        CreateProceduralAltarSprite();
    }

    private void Start()
    {
        playerHealth = FindAnyObjectByType<PlayerHealth>();
        playerMovement = FindAnyObjectByType<PlayerMovementController>();
    }

    private void CreateProceduralAltarSprite()
    {
        int width = 32;
        int height = 32;
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;

        Color stoneColor = new Color(0.4f, 0.4f, 0.4f);
        Color darkStone = new Color(0.25f, 0.25f, 0.25f);
        Color lightStone = new Color(0.6f, 0.6f, 0.6f);
        
        Color energyColor = Color.white;
        switch (blessingType)
        {
            case AltarBlessingType.DragonFire:
                energyColor = new Color(0.9f, 0.2f, 0.1f); // Fuego (Rojo)
                break;
            case AltarBlessingType.CelestialStorm:
                energyColor = new Color(0.1f, 0.6f, 0.9f); // Tormenta (Azul eléctrico)
                break;
            case AltarBlessingType.WindBoots:
                energyColor = new Color(0.1f, 0.8f, 0.5f); // Viento (Verde esmeralda)
                break;
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Dibujar forma de altar: base ancha, pilar delgado, copa superior
                bool isBase = y < 6 && x >= 4 && x < width - 4;
                bool isPillar = y >= 6 && y < 20 && x >= 10 && x < width - 10;
                bool isTop = y >= 20 && y < 26 && x >= 6 && x < width - 6;
                bool isEnergy = y >= 26 && y < 32 && x >= 8 && x < width - 8;

                if (isEnergy)
                {
                    // Patrón de energía/fuego parpadeante
                    if ((x + y + (int)blessingType) % 3 == 0)
                    {
                        texture.SetPixel(x, y, energyColor);
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
                else if (isBase || isPillar || isTop)
                {
                    // Bordes oscuros
                    if (x == 4 || x == width - 5 || x == 10 || x == width - 11 || x == 6 || x == width - 7 || y == 0 || y == 25)
                    {
                        texture.SetPixel(x, y, darkStone);
                    }
                    // Runas de energía grabadas en el pilar
                    else if (isPillar && y >= 10 && y < 16 && x == 16)
                    {
                        texture.SetPixel(x, y, energyColor);
                    }
                    else if ((x + y) % 7 == 0)
                    {
                        texture.SetPixel(x, y, lightStone);
                    }
                    else
                    {
                        texture.SetPixel(x, y, stoneColor);
                    }
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }

        texture.Apply();
        spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.1f), 24f);
    }

    public void Interact()
    {
        if (trialActive || trialCompleted) return;

        if (playerHealth == null || playerHealth.CurrentHearts <= 1)
        {
            Debug.LogWarning("[AltarOfValor] No tienes suficiente vida para sacrificarte.");
            return;
        }

        // Sacrificar 1 corazón
        playerHealth.TakeDirectDamage(1);
        
        // Iniciar reto
        StartTrial();
    }

    private void StartTrial()
    {
        trialActive = true;
        trialTimer = trialDuration;
        Debug.Log($"[AltarOfValor] ¡Reto iniciado! Sobrevive durante {trialDuration} segundos.");

        // Spawner de enemigos de prueba
        StartCoroutine(SpawnTrialEnemiesRoutine());
    }

    private IEnumerator SpawnTrialEnemiesRoutine()
    {
        // Esperar 3 segundos para dar tiempo de preparación al jugador
        yield return new WaitForSeconds(3.0f);

        // Obtener el prefab del primer enemigo disponible en la dificultad actual
        GameObject enemyPrefab = null;
        if (GameManager.Instance != null && GameManager.Instance.currentDifficulty != null)
        {
            var difficulty = GameManager.Instance.currentDifficulty;
            if (difficulty.waves != null && difficulty.waves.Length > 0)
            {
                var wave = difficulty.waves[0];
                if (wave.enemies != null && wave.enemies.Length > 0)
                {
                    enemyPrefab = wave.enemies[0].enemyData.enemyPrefab;
                }
            }
        }

        if (enemyPrefab != null)
        {
            for (int i = 0; i < enemiesToSpawn; i++)
            {
                if (!trialActive) break;

                // Generar posición aleatoria alrededor de la plataforma
                Vector2 spawnPos = (Vector2)transform.position + new Vector2(
                    Random.Range(-spawnRadius, spawnRadius),
                    Random.Range(1f, 3f)
                );

                GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                
                // Hacer al enemigo más rápido para el reto
                EnemyMovement enemyMovement = enemyObj.GetComponent<EnemyMovement>();
                if (enemyMovement != null)
                {
                    enemyMovement.setTarget(playerMovement.transform);
                    // Aumentar velocidad temporalmente mediante reflexión o simplemente clonando su comportamiento
                    // pero para simplificar, aceleramos su avance
                    enemyObj.transform.localScale *= 0.9f; // Un poco más pequeños y rápidos
                }

                yield return new WaitForSeconds(0.5f);
            }
        }
        else
        {
            Debug.LogError("[AltarOfValor] No se encontró prefab de enemigo para el reto.");
        }
    }

    private void Update()
    {
        if (trialActive)
        {
            trialTimer -= Time.deltaTime;
            
            // Añadir efectos de partículas visuales de color
            if (Random.value < 0.15f)
            {
                CreateVisualEffect();
            }

            if (trialTimer <= 0)
            {
                CompleteTrial();
            }
        }
    }

    private void CreateVisualEffect()
    {
        GameObject particle = new GameObject("AltarParticle");
        particle.transform.position = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0f, 1.5f), 0);
        
        SpriteRenderer pr = particle.AddComponent<SpriteRenderer>();
        
        // Crear sprite de mini estrella
        Texture2D tex = new Texture2D(4, 4);
        Color pColor = blessingType == AltarBlessingType.DragonFire ? Color.red :
                       blessingType == AltarBlessingType.CelestialStorm ? Color.cyan : Color.green;
        for (int y = 0; y < 4; y++)
            for (int x = 0; x < 4; x++)
                tex.SetPixel(x, y, pColor);
        tex.Apply();
        
        pr.sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
        pr.sortingOrder = 10;

        Destroy(particle, 0.5f);
        StartCoroutine(FloatAndFade(particle.transform, pr));
    }

    private IEnumerator FloatAndFade(Transform t, SpriteRenderer sr)
    {
        float elapsed = 0f;
        while (elapsed < 0.5f && t != null)
        {
            t.position += Vector3.up * 1f * Time.deltaTime;
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f - (elapsed / 0.5f));
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void CompleteTrial()
    {
        trialActive = false;
        trialCompleted = true;
        Debug.Log("[AltarOfValor] ¡Reto superado! Bendición otorgada.");

        // Otorgar bendición
        if (playerMovement != null)
        {
            switch (blessingType)
            {
                case AltarBlessingType.DragonFire:
                    playerMovement.UnlockedDragonFire = true;
                    playerMovement.activeBlessing = PlayerMovementController.ActiveBlessingType.DragonFire;
                    break;
                case AltarBlessingType.CelestialStorm:
                    playerMovement.UnlockedCelestialStorm = true;
                    playerMovement.activeBlessing = PlayerMovementController.ActiveBlessingType.CelestialStorm;
                    break;
                case AltarBlessingType.WindBoots:
                    playerMovement.UnlockedWindBoots = true;
                    playerMovement.activeBlessing = PlayerMovementController.ActiveBlessingType.WindBoots;
                    break;
            }
        }

        // Auto-destrucción con efecto
        StartCoroutine(DestructionRoutine());
    }

    private IEnumerator DestructionRoutine()
    {
        float elapsed = 0f;
        Vector3 origScale = transform.localScale;
        while (elapsed < 0.8f)
        {
            transform.localScale = Vector3.Lerp(origScale, Vector3.zero, elapsed / 0.8f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
