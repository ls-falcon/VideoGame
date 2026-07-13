using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class BreakablePlatform : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float timeUntilWarning = 2f;
    [SerializeField] private float timeUntilBreak = 3f;
    [SerializeField] private float respawnTime = 6f;
    [SerializeField] private float topContactTolerance = 0.08f;

    [Header("Visuals")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite crackedSprite;
    [SerializeField] private Color warningColor = new Color(1f, 0.75f, 0.45f, 1f);

    [Header("One Way Platform")]
    [SerializeField] private bool configureAsOneWayPlatform = true;
    [SerializeField] private PlatformEffector2D platformEffector;

    [Header("Break Feedback")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip breakSound;
    [SerializeField] private ParticleSystem breakParticles;

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D platformCollider;

    private float playerStandingTime;
    private bool playerOnPlatform;
    private bool isBroken;
    private bool isWarning;
    private Color originalColor;
    private PlayerMovementController standingPlayer;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (platformCollider == null)
        {
            platformCollider = GetComponent<Collider2D>();
        }

        if (platformEffector == null)
        {
            platformEffector = GetComponent<PlatformEffector2D>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        ConfigureOneWayPlatform();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;

            if (normalSprite == null)
            {
                normalSprite = spriteRenderer.sprite;
            }
        }

        if (timeUntilBreak < timeUntilWarning)
        {
            timeUntilWarning = timeUntilBreak;
        }
    }

    private void Update()
    {
        if (isBroken || !playerOnPlatform)
        {
            return;
        }

        playerStandingTime += Time.deltaTime;

        if (playerStandingTime >= timeUntilWarning)
        {
            ShowWarningState();
        }

        if (playerStandingTime >= timeUntilBreak)
        {
            Break();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryUpdatePlayerStandingState(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryUpdatePlayerStandingState(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        PlayerMovementController player = GetPlayer(collision);

        if (player == null || player != standingPlayer)
        {
            return;
        }

        ClearStandingPlayer();
        ResetTimer();
    }

    private void TryUpdatePlayerStandingState(Collision2D collision)
    {
        if (isBroken)
        {
            return;
        }

        PlayerMovementController player = GetPlayer(collision);

        if (player == null)
        {
            return;
        }

        if (IsPlayerStandingOnTop(collision))
        {
            standingPlayer = player;
            playerOnPlatform = true;
            return;
        }

        if (player == standingPlayer)
        {
            ClearStandingPlayer();
            ResetTimer();
        }
    }

    private bool IsPlayerStandingOnTop(Collision2D collision)
    {
        if (GetPlayer(collision) == null)
        {
            return false;
        }

        if (platformCollider == null)
        {
            return false;
        }

        Bounds platformBounds = platformCollider.bounds;
        Bounds playerBounds = collision.collider.bounds;

        if (playerBounds.min.y >= platformBounds.max.y - topContactTolerance)
        {
            return true;
        }

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.point.y >= platformBounds.max.y - topContactTolerance)
            {
                return true;
            }
        }

        return false;
    }

    private PlayerMovementController GetPlayer(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerMovementController player))
        {
            return player;
        }

        return collision.gameObject.GetComponentInParent<PlayerMovementController>();
    }

    private void ShowWarningState()
    {
        if (isWarning || spriteRenderer == null)
        {
            return;
        }

        isWarning = true;

        if (crackedSprite != null)
        {
            spriteRenderer.sprite = crackedSprite;
        }
        else
        {
            spriteRenderer.color = warningColor;
        }
    }

    private void Break()
    {
        isBroken = true;
        ClearStandingPlayer();

        if (audioSource != null && breakSound != null)
        {
            audioSource.PlayOneShot(breakSound);
        }

        if (breakParticles != null)
        {
            breakParticles.Play();
        }

        if (platformCollider != null)
        {
            platformCollider.enabled = false;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnTime);

        isBroken = false;
        ResetTimer();

        if (platformCollider != null)
        {
            platformCollider.enabled = true;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.sprite = normalSprite;
            spriteRenderer.color = originalColor;
        }
    }

    private void ResetTimer()
    {
        playerStandingTime = 0f;
        isWarning = false;

        if (spriteRenderer == null || isBroken)
        {
            return;
        }

        spriteRenderer.sprite = normalSprite;
        spriteRenderer.color = originalColor;
    }

    private void ClearStandingPlayer()
    {
        standingPlayer = null;
        playerOnPlatform = false;
    }

    private void ConfigureOneWayPlatform()
    {
        if (!configureAsOneWayPlatform ||
            platformCollider == null ||
            platformEffector == null)
        {
            return;
        }

        platformCollider.usedByEffector = true;
        platformEffector.useOneWay = true;
        platformEffector.useSideFriction = false;
        platformEffector.useSideBounce = false;
    }
}
