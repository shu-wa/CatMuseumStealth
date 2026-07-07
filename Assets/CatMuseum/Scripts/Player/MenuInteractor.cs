using UnityEngine;

public class MenuInteractor : MonoBehaviour
{
    [Header("interaction")]
    [SerializeField] private float interactRadius = 2.0f;
    [SerializeField] private Vector3 interactCenterOffset = new Vector3(0f, 0.8f, 0f);
    [SerializeField] private LayerMask interactLayer;

    public IMenuInteractable CurrentInteractable { get; private set; }
    public string CurrentPrompt { get; private set; }

    private void Update()
    {
        if (BackpackMenuUI.IsAnyBackpackOpen)
        {
            CurrentPrompt = "";
            return;
        }

        UpdateCurrentInteractable();

        if (CurrentInteractable != null)
        {
            CurrentInteractable.HandleFocusedInput(this);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            TrySecondaryInteract();
        }
    }

    private void UpdateCurrentInteractable()
    {
        CurrentInteractable = FindNearestInteractable();

        if (CurrentInteractable != null)
        {
            CurrentPrompt = CurrentInteractable.GetPrompt();
        }
        else
        {
            CurrentPrompt = "";
        }

        if (!string.IsNullOrEmpty(CurrentPrompt))
        {
            MenuNoticeUI.Instance?.ShowNotice(CurrentPrompt, 0.1f);
        }
    }

    private void TryInteract()
    {
        if (CurrentInteractable == null)
        {
            MenuNoticeUI.Instance?.ShowNotice("Nothing to interact with");
            return;
        }

        CurrentInteractable.Interact(this);
    }

    private void TrySecondaryInteract()
    {
        if (CurrentInteractable == null)
        {
            return;
        }

        CurrentInteractable.SecondaryInteract(this);
    }

    private IMenuInteractable FindNearestInteractable()
    {
        Vector3 center = transform.position + interactCenterOffset;

        Collider[] hits = Physics.OverlapSphere(
            center,
            interactRadius,
            interactLayer,
            QueryTriggerInteraction.Collide
        );

        IMenuInteractable nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            IMenuInteractable interactable = hit.GetComponentInParent<IMenuInteractable>();

            if (interactable == null)
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