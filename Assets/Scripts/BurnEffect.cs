using System.Collections;
using UnityEngine;

public class BurnEffect : MonoBehaviour
{
    private EnemyHealth enemyHealth;
    private BossHealth bossHealth;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    [SerializeField] private float burnDuration = 2f;
    private float elapsed = 0f;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        bossHealth = GetComponent<BossHealth>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // Iniciar la animación de fuego y el temporizador de muerte lenta (2 segundos)
        StartCoroutine(BurnTimerRoutine());
        StartCoroutine(SpawnFlameParticlesRoutine());
    }

    private IEnumerator BurnTimerRoutine()
    {
        float timer = 0f;
        while (timer < burnDuration)
        {
            // Hacer parpadear al enemigo entre rojo y naranja para indicar quemadura
            if (spriteRenderer != null)
            {
                spriteRenderer.color = (int)(timer * 10) % 2 == 0 ? new Color(1f, 0.3f, 0f) : new Color(1f, 0.6f, 0.1f);
            }
            
            timer += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        // Restaurar color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        // Aplicar daño mortal después de 2 segundos
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(10); // Daño letal
        }
        else if (bossHealth != null)
        {
            bossHealth.TakeDamage(10);
        }

        Destroy(this);
    }

    private IEnumerator SpawnFlameParticlesRoutine()
    {
        float timer = 0f;
        while (timer < burnDuration)
        {
            CreateFlameParticle();
            timer += 0.08f;
            yield return new WaitForSeconds(0.08f);
        }
    }

    private void CreateFlameParticle()
    {
        if (this == null) return;
        
        // Crear una pequeña flama procedimental
        GameObject flame = new GameObject("FlameParticle");
        // Posicionar en la base/centro del enemigo
        flame.transform.position = transform.position + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.2f, 0.5f), 0);
        
        SpriteRenderer fr = flame.AddComponent<SpriteRenderer>();
        
        // Textura pequeña de 4x4
        Texture2D tex = new Texture2D(4, 4);
        Color flameColor = Random.value > 0.5f ? new Color(1f, 0.2f, 0f, 0.8f) : new Color(1f, 0.7f, 0.1f, 0.8f);
        for (int y = 0; y < 4; y++)
            for (int x = 0; x < 4; x++)
                tex.SetPixel(x, y, flameColor);
        tex.Apply();
        
        fr.sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
        fr.sortingOrder = 20; // Delante del follaje y plataformas

        Destroy(flame, 0.4f);
        
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(AnimateFlame(flame.transform, fr));
        }
    }

    private IEnumerator AnimateFlame(Transform t, SpriteRenderer sr)
    {
        float elapsedFlame = 0f;
        float duration = 0.4f;
        Vector3 startPos = t.position;
        float speed = Random.Range(1.2f, 2.0f);
        float waveSpeed = Random.Range(5f, 10f);
        float waveAmp = Random.Range(0.05f, 0.15f);

        while (elapsedFlame < duration && t != null)
        {
            // Movimiento hacia arriba y ondulante lateralmente
            float yOffset = elapsedFlame * speed;
            float xOffset = Mathf.Sin(elapsedFlame * waveSpeed) * waveAmp;
            t.position = startPos + new Vector3(xOffset, yOffset, 0);

            // Desvanecer color
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, (1f - (elapsedFlame / duration)) * 0.8f);
            
            elapsedFlame += Time.deltaTime;
            yield return null;
        }
    }
}
