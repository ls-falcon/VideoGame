using UnityEngine;
using UnityEngine.UI;

public class HeartsUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth health;
    [SerializeField] private Image heartPrefab;
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;
    [SerializeField] private Transform heartsContainer;

    private Image[] heartImages;

    private void Start()
    {
        if (health == null)
        {
            health = FindAnyObjectByType<PlayerHealth>();
        }

        if (health == null || heartPrefab == null || heartsContainer == null)
        {
            Debug.LogError("HeartsUI is missing required references.", this);
            enabled = false;
            return;
        }

        int maxHearts = GetMaxHearts();

        heartImages = new Image[maxHearts];

        for (int i = 0; i < maxHearts; i++)
        {
            Image heart = Instantiate(heartPrefab, heartsContainer);
            heart.sprite = fullHeartSprite;
            heartImages[i] = heart;
        }

        health.OnHealthChanged += UpdateHearts;
        UpdateHearts(health.CurrentHearts, maxHearts);
    }

    private void OnDestroy()
    {
        if (health != null)
            health.OnHealthChanged -= UpdateHearts;
    }

    private void UpdateHearts(int currentHearts, int maxHearts)
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            heartImages[i].sprite = i < currentHearts ? fullHeartSprite : emptyHeartSprite;
        }
    }

    private int GetMaxHearts()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.currentDifficulty != null)
        {
            return GameManager.Instance.currentDifficulty.maxInitialHearts;
        }

        Debug.LogWarning(
            "No difficulty was selected before loading the game scene. " +
            "Using PlayerHealth maxHearts as a fallback.",
            this
        );

        return Mathf.Max(1, health.MaxHearts);
    }
}
