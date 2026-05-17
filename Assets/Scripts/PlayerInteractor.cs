using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float interactRadius = 1.5f;

    [SerializeField] private LayerMask interactableLayer;

    [Header("UI")]
    [SerializeField] private GameObject interactPopup;

    private PlayerInputSystem input;

    private IInteractable currentInteractable;

    private Transform popupAnchor;

    private void Awake()
    {
        input = new PlayerInputSystem();
    }

    private void OnEnable()
    {
        input.Player.Enable();

        input.Player.InteractPickup.performed += OnInteract;
    }

    private void OnDisable()
    {
        input.Player.InteractPickup.performed -= OnInteract;

        input.Player.Disable();
    }

    private void Start()
    {
        if (interactPopup != null)
        {
            interactPopup.SetActive(false);
        }
    }

    private void Update()
    {
        DetectInteractable();

        UpdatePopupPosition();
    }

    void DetectInteractable()
    {
        Collider2D hit = Physics2D.OverlapCircle(
            transform.position,
            interactRadius,
            interactableLayer
        );

        if (hit != null)
        {
            currentInteractable =
                hit.GetComponent<IInteractable>();

            popupAnchor =
                hit.transform.Find("PopupAnchor");

            if (interactPopup != null)
            {
                interactPopup.SetActive(true);
            }
        }
        else
        {
            currentInteractable = null;

            popupAnchor = null;

            if (interactPopup != null)
            {
                interactPopup.SetActive(false);
            }
        }
    }

    void UpdatePopupPosition()
    {
        if (popupAnchor == null || interactPopup == null)
            return;

        Vector3 screenPosition =
            Camera.main.WorldToScreenPoint(
                popupAnchor.position
            );

        RectTransform rectTransform = interactPopup.GetComponent<RectTransform>();

        rectTransform.position = screenPosition;
    }

    void OnInteract(InputAction.CallbackContext ctx)
    {
        currentInteractable?.Interact();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            interactRadius
        );
    }
}