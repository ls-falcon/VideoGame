using UnityEngine;

public class LightningEffect : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip strikeSound;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Start()
    {
        if (audioSource != null && strikeSound != null)
        {
            audioSource.PlayOneShot(strikeSound);
        }
    }

    // Animation Event al final de la animación
    public void DestroyEffect()
    {
        Destroy(gameObject);
    }
}