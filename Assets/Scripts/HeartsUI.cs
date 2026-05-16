using UnityEngine;
using UnityEngine.UI;

public class HeartsUI : MonoBehaviour
{
    [SerializeField] private playerHealth health;
    [SerializeField] private Image heartPrefab;
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;
    [SerializeField] private Transform heartsContainer;

    private Image[] heartImages;

    private void Start()
    {
        int maxHearts = gameManager.Instance.currentDifficulty.maxInitialHearts;

        heartImages = new Image[maxHearts];

        for (int i = 0; i < maxHearts; i++)
        {
            Image heart = Instantiate(heartPrefab, heartsContainer);
            heart.sprite = fullHeartSprite;
            heartImages[i] = heart;
        }

        health.OnHealthChanged += UpdateHearts;
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
}
