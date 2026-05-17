using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class playerHealth : MonoBehaviour
{
    [SerializeField] private int maxHearts;
    [SerializeField] private int currentHearts;

    private bool isDead = false;
    private Rigidbody2D rb; //1
    private SpriteRenderer spriteRenderer; //2

    [SerializeField] private float invulnerabilityTime = 0.7f;

    private bool isInvulnerable = false;
    [SerializeField] private GameObject gameOverPanel;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    private void Start()
    {
        if (gameManager.Instance != null && gameManager.Instance.currentDifficulty != null)
        {
            maxHearts = gameManager.Instance.currentDifficulty.maxInitialHearts;
        }

        currentHearts = maxHearts;
        OnHealthChanged?.Invoke(currentHearts, maxHearts);
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void TakeDamage(int amount, Vector2 damageSource)
    {
        if (isDead)
            return;

        if (isInvulnerable)
            return;

        currentHearts = Mathf.Max(0, currentHearts - amount);

        Debug.Log("Vida actual: " + currentHearts);
        ApplyKnockback(damageSource);
        StartCoroutine(FlashDamage());
        StartCoroutine(InvulnerabilityCoroutine());

        OnHealthChanged?.Invoke(currentHearts, maxHearts);



        if (currentHearts <= 0)
        {
            Die();
        }
    }

    private void ApplyKnockback(Vector2 damageSource)
    {
        if (rb == null)
            return;

        Vector2 direction =
            ((Vector2)transform.position - damageSource).normalized;

        rb.linearVelocity = Vector2.zero;

        rb.AddForce(direction * 7f, ForceMode2D.Impulse);
    }

    private System.Collections.IEnumerator FlashDamage()
    {
        if (spriteRenderer == null)
            yield break;

        spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(0.15f);

        spriteRenderer.color = Color.white;
    }

    private System.Collections.IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;

        float elapsed = 0f;

        while (elapsed < invulnerabilityTime)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
            }

            yield return new WaitForSeconds(0.1f);

            elapsed += 0.1f;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        isInvulnerable = false;
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log("Jugador muerto");

        // Desactivar movimiento
        playerMovementController movement =
            GetComponent<playerMovementController>();

        if (movement != null)
            movement.enabled = false;

        // Desactivar input
        PlayerInput input =
            GetComponent<PlayerInput>();

        if (input != null)
            input.enabled = false;

        // Mostrar Game Over
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        OnDeath?.Invoke();

        Invoke(nameof(LoadMainMenu), 3f);
    }

    private void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}