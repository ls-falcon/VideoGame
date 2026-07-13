using UnityEngine;

public class DynamicPlatformManager : MonoBehaviour
{
    public static DynamicPlatformManager Instance;

    [Header("Configuración de Plataformas")]
    [SerializeField] private int platformCount = 4;
    [SerializeField] private float minPlatformWidth = 3f;
    [SerializeField] private float maxPlatformWidth = 6f;
    [SerializeField] private float platformThickness = 0.4f;

    [Header("Área de Generación")]
    [SerializeField] private float minX = -6.5f;
    [SerializeField] private float maxX = 6.5f;
    [SerializeField] private float minY = 1.2f;
    [SerializeField] private float maxY = 4.5f;

    [Header("Capa de Suelo")]
    [SerializeField] private string fallbackLayerName = "Ground";

    private int groundLayer;
    private Sprite platformSprite;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Generar textura y sprite procedimentalmente para evitar dependencias de archivos externos
        CreateProceduralPlatformSprite();
        
        // Detectar la capa de suelo automáticamente
        DetectGroundLayer();
    }

    private void Start()
    {
        // Detectar la altura del suelo usando Raycast
        float groundY = -3.2f; // Fallback
        
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(0f, 10f), Vector2.down, 25f, 1 << groundLayer);
        if (hit.collider != null)
        {
            groundY = hit.point.y;
            Debug.Log($"[DynamicPlatformManager] Suelo encontrado por Raycast en Y: {groundY}");
        }
        else
        {
            GameObject groundObj = GameObject.FindWithTag("Ground");
            if (groundObj != null)
            {
                groundY = groundObj.transform.position.y;
                Debug.Log($"[DynamicPlatformManager] Suelo encontrado por posición en Y: {groundY}");
            }
        }

        // Ajustar el rango de altura relativo al suelo real
        minY = groundY + 1.8f;
        maxY = groundY + 3.6f;

        // Deshabilitado por petición: "otro lo va a hacer"
        // GeneratePlatforms();
    }

    private void DetectGroundLayer()
    {
        // Intentar encontrar el jugador para obtener su groundMask
        PlayerMovementController player = FindAnyObjectByType<PlayerMovementController>();
        if (player != null)
        {
            GameObject groundObj = GameObject.FindWithTag("Ground");
            if (groundObj != null)
            {
                groundLayer = groundObj.layer;
                return;
            }
        }

        // Fallback a la capa por nombre
        groundLayer = LayerMask.NameToLayer(fallbackLayerName);
        if (groundLayer == -1)
        {
            groundLayer = 0;
        }
    }

    private void CreateProceduralPlatformSprite()
    {
        int width = 64;
        int height = 16;
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Repeat;

        Color woodColor = new Color(0.35f, 0.22f, 0.12f);
        Color darkBorder = new Color(0.20f, 0.11f, 0.05f);
        Color lightHighlight = new Color(0.50f, 0.32f, 0.18f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (y == 0 || y == height - 1 || x == 0 || x == width - 1)
                {
                    texture.SetPixel(x, y, darkBorder);
                }
                else if (y == height - 2 || x == 1)
                {
                    texture.SetPixel(x, y, lightHighlight);
                }
                else if ((x + y) % 12 == 0 || (x - y) % 16 == 0)
                {
                    texture.SetPixel(x, y, darkBorder);
                }
                else
                {
                    texture.SetPixel(x, y, woodColor);
                }
            }
        }

        texture.Apply();
        platformSprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 16f);
    }

    public void GeneratePlatforms()
    {
        Debug.Log("[DynamicPlatformManager] Generando plataformas flotantes ordenadamente...");
        
        // Dividir el espacio en columnas para asegurar que no se junten o solapen
        float stepX = (maxX - minX) / platformCount;
        for (int i = 0; i < platformCount; i++)
        {
            float colMinX = minX + i * stepX;
            float colMaxX = colMinX + stepX;

            // Posición X en el centro de la columna con ligero desfase
            float xPos = Mathf.Lerp(colMinX + 0.4f, colMaxX - 0.4f, 0.5f) + Random.Range(-0.2f, 0.2f);
            
            // Alternar alturas (baja, alta, media)
            float yPos = minY;
            if (i % 3 == 0)
            {
                yPos = minY;
            }
            else if (i % 3 == 1)
            {
                yPos = maxY;
            }
            else
            {
                yPos = Mathf.Lerp(minY, maxY, 0.5f);
            }

            // Desfase de altura sutil
            yPos += Random.Range(-0.1f, 0.1f);
            float width = Random.Range(minPlatformWidth, maxPlatformWidth);

            CreatePlatform(new Vector2(xPos, yPos), width);
        }
    }

    private void CreatePlatform(Vector2 position, float width)
    {
        GameObject platform = new GameObject($"Platform_{position.x:F1}_{position.y:F1}");
        platform.transform.position = position;
        platform.transform.parent = this.transform;
        
        // Añadir SpriteRenderer
        SpriteRenderer sr = platform.AddComponent<SpriteRenderer>();
        sr.sprite = platformSprite;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(width, platformThickness);
        sr.sortingOrder = 15;

        // Añadir BoxCollider2D
        BoxCollider2D col = platform.AddComponent<BoxCollider2D>();
        col.size = new Vector2(width, platformThickness);

        // Asignar capa de suelo
        platform.layer = groundLayer;
        platform.tag = "Ground"; // Para compatibilidad con otras comprobaciones
    }

    public Vector2 GetRandomPlatformPosition()
    {
        if (transform.childCount == 0)
        {
            return Vector2.zero;
        }

        int index = Random.Range(0, transform.childCount);
        Transform child = transform.GetChild(index);
        // Retornar un punto un poco por encima de la plataforma
        return (Vector2)child.position + Vector2.up * 0.8f;
    }

    public void RegeneratePlatforms()
    {
        // Guardar hijos actuales
        int childCount = transform.childCount;
        Transform[] children = new Transform[childCount];
        for (int i = 0; i < childCount; i++)
        {
            children[i] = transform.GetChild(i);
        }
        
        // Limpiar el parent y destruir para evitar que cuenten en el mismo frame
        foreach (Transform child in children)
        {
            child.SetParent(null);
            Destroy(child.gameObject);
        }

        // Generar nuevas plataformas
        GeneratePlatforms();
    }

    public void RaisePlatformsForBoss(float minHeightAboveGround)
    {
        // Encontrar la altura del suelo real
        float groundY = -3.2f;
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(0f, 10f), Vector2.down, 25f, 1 << groundLayer);
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

        // Mover cada plataforma hacia arriba para que no estorben al Boss
        int index = 0;
        foreach (Transform child in transform)
        {
            // Colocar escalonadamente arriba (ej. entre 4.2f y 5.4f sobre el suelo)
            float targetY = groundY + minHeightAboveGround + (index % 2) * 1.2f;
            child.position = new Vector3(child.position.x, targetY, child.position.z);
            index++;
        }
    }
}
