using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("information")]
    [SerializeField] private float interactRadius = 2.0f;
    [SerializeField] private Vector3 interactCenterOffset = new Vector3(0f, 0.8f, 0f);
    [SerializeField] private LayerMask interactLayer;

    public PlayerInventory Inventory { get; private set; }
    public IInteractable CurrentInteractable { get; private set; }
    public string CurrentPrompt { get; private set; }

    public string NoticeMessage { get; private set; }

    public bool IsInteracting => isInteracting;
    public float InteractionProgress => currentInteractionDuration <= 0f ? 0f : currentInteractionTimer / currentInteractionDuration;
    public string InteractionText => currentInteractionText;

    private float noticeTimer = 0f;

    private bool isInteracting = false;
    private float currentInteractionTimer = 0f;
    private float currentInteractionDuration = 0f;
    private string currentInteractionText = "";

    private IInteractable interactionTarget;
    private InteractionMode currentMode;

    private void Awake()
    {
        Inventory = GetComponent<PlayerInventory>();

        if (Inventory == null)
        {
            Debug.LogError("PlayerInventory is not found");
        }
    }

    private void Update()
    {
        UpdateNoticeTimer();

        if (isInteracting)
        {
            UpdateInteraction();
            return;
        }

        UpdateCurrentInteractable();

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E key pressed");
            TryStartInteraction(InteractionMode.SwapIfPossible);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("F key pressed");
            TryStartInteraction(InteractionMode.ForceSteal);
        }
    }

    private void UpdateNoticeTimer()
    {
        if (noticeTimer <= 0f)
        {
            NoticeMessage = "";
            return;
        }

        noticeTimer -= Time.deltaTime;

        if (noticeTimer <= 0f)
        {
            NoticeMessage = "";
        }
    }

    public void ShowNotice(string message, float duration = 2.0f)
    {
        NoticeMessage = message;
        noticeTimer = duration;
    }

    public void ForceCancelInteraction(string reason)
    {
        if (!isInteracting)
        {
            return;
        }

        CancelInteraction(reason);
    }

    private void UpdateCurrentInteractable()
    {
        CurrentInteractable = FindNearestInteractable();

        if (CurrentInteractable != null)
        {
            CurrentPrompt = CurrentInteractable.GetPrompt(this);
        }
        else
        {
            CurrentPrompt = "";
        }
    }

    private void TryStartInteraction(InteractionMode mode)
    {
        if (CurrentInteractable == null)
        {
            Debug.Log("No interactable object nearby");
            ShowNotice("No interactable object nearby");
            return;
        }

        if (!CurrentInteractable.CanStartInteraction(this, mode))
        {
            return;
        }

        interactionTarget = CurrentInteractable;
        currentMode = mode;

        currentInteractionDuration = interactionTarget.GetInteractionDuration(this, mode);
        currentInteractionTimer = 0f;
        currentInteractionText = interactionTarget.GetInteractionText(this, mode);

        isInteracting = true;

        Debug.Log("Interaction started: " + currentInteractionText);
    }

    private void UpdateInteraction()
    {
        if (interactionTarget == null)
        {
            CancelInteraction("Interaction target lost");
            return;
        }

        currentInteractionTimer += Time.deltaTime;

        if (currentInteractionTimer >= currentInteractionDuration)
        {
            CompleteCurrentInteraction();
        }
    }

    private void CompleteCurrentInteraction()
    {
        isInteracting = false;

        Debug.Log("Interaction completed: " + currentInteractionText);

        interactionTarget.CompleteInteraction(this, currentMode);

        interactionTarget = null;
        currentInteractionTimer = 0f;
        currentInteractionDuration = 0f;
        currentInteractionText = "";

        UpdateCurrentInteractable();
    }

    private void CancelInteraction(string reason)
    {
        isInteracting = false;

        Debug.Log("Interaction canceled: " + reason);
        ShowNotice("Interaction canceled");

        interactionTarget = null;
        currentInteractionTimer = 0f;
        currentInteractionDuration = 0f;
        currentInteractionText = "";
    }

    private IInteractable FindNearestInteractable()
    {
        Vector3 center = transform.position + interactCenterOffset;
        Collider[] hits = Physics.OverlapSphere(center, interactRadius, interactLayer);

        IInteractable nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            IInteractable interactable = hit.GetComponentInParent<IInteractable>();

            if (interactable == null)
            {
                continue;
            }

            if (!interactable.CanInteract)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, hit.transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = interactable;
            }
        }

        return nearest;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = transform.position + interactCenterOffset;
        Gizmos.DrawWireSphere(center, interactRadius);
    }
}