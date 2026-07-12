using System;
using System.Collections;
using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 50;

    [Header("Damage Feedback")]
    [SerializeField] private float invulnerabilityTime = 0.2f;

    private int currentHealth;

    private bool isDead;
    private bool isInvulnerable;

    private SpriteRenderer spriteRenderer;

    public Action OnDeath;

    private void Start()
    {
        DifficultySettings difficulty =
            GameManager.Instance.currentDifficulty;

        switch (difficulty.difficultyName)
        {
            case "Easy":
                maxHealth = 10;
                break;
            case "Medium":
                maxHealth = 20;
                break;
            case "Hard":
                maxHealth = 30;
                break;
        }
    }

    private void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        if (isInvulnerable)
            return;

        currentHealth -= damage;

        currentHealth = Mathf.Max(0, currentHealth);

        StartCoroutine(FlashDamage());
        StartCoroutine(InvulnerabilityCoroutine());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        OnDeath?.Invoke();

        Destroy(gameObject);
    }

    IEnumerator FlashDamage()
    {
        if (spriteRenderer == null)
            yield break;

        Color originalColor = spriteRenderer.color;

        spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(0.15f);

        spriteRenderer.color = originalColor;
    }

    IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;

        float elapsed = 0f;

        while (elapsed < invulnerabilityTime)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
            }

            yield return new WaitForSeconds(0.08f);

            elapsed += 0.08f;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        isInvulnerable = false;
    }
}