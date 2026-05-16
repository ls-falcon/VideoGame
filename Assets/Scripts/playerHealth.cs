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
    }

    public void TakeDamage(int amount = 1)
    {
        if (isDead)
            return;

        currentHearts = Mathf.Max(0, currentHearts - amount);

        Debug.Log("Vida actual: " + currentHearts);

        OnHealthChanged?.Invoke(currentHearts, maxHearts);

        if (currentHearts <= 0)
        {
            Die();
        }
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