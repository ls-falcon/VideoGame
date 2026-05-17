using UnityEngine;

public class ApplePickup : MonoBehaviour, IInteractable
{
    [Header("Heal")]
    [SerializeField] private int healAmount = 1;

    public System.Action OnCollected;

    public void Interact()
    {
        PlayerHealth playerHealth =
            FindAnyObjectByType<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.Heal(healAmount);
        }

        OnCollected?.Invoke();

        Destroy(gameObject);
    }
}