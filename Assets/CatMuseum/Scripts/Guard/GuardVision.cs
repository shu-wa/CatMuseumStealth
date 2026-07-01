using UnityEngine;

public class GuardVision : MonoBehaviour
{
    [Header("view setting")]
    [SerializeField] private float viewDistance = 6.0f;
    [SerializeField] private float viewAngle = 80.0f;
    [SerializeField] private float eyeHeight = 1.2f;
    [SerializeField] private float targetHeight = 1.0f;

    [Header("alert modifier")]
    [SerializeField] private bool useAlertModifier = true;
    [SerializeField] private float maxViewAngle = 150.0f;

    [Header("layers")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask artLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("debug")]
    [SerializeField] private bool showDebugRay = true;
    [SerializeField] private bool showDebugLog = false;

    public PlayerInteractor VisiblePlayer { get; private set; }

    public float CurrentViewDistance
    {
        get
        {
            if (!useAlertModifier || AlertManager.Instance == null)
            {
                return viewDistance;
            }

            return viewDistance * AlertManager.Instance.GuardViewDistanceMultiplier;
        }
    }

    public float CurrentViewAngle
    {
        get
        {
            if (!useAlertModifier || AlertManager.Instance == null)
            {
                return viewAngle;
            }

            float modifiedAngle = viewAngle * AlertManager.Instance.GuardViewAngleMultiplier;
            return Mathf.Min(modifiedAngle, maxViewAngle);
        }
    }

    public PlayerInteractor CheckVisiblePlayer()
    {
        VisiblePlayer = FindVisiblePlayer();
        return VisiblePlayer;
    }

    public ArtPiece CheckVisibleEmptyArt()
    {
        Vector3 eyePosition = GetEyePosition();

        Collider[] hits = Physics.OverlapSphere(
            eyePosition,
            CurrentViewDistance,
            artLayer,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider hit in hits)
        {
            ArtPiece artPiece = hit.GetComponentInParent<ArtPiece>();

            if (artPiece == null)
            {
                continue;
            }

            if (!artPiece.CanReportEmpty)
            {
                continue;
            }

            Vector3 targetPosition = artPiece.GetLookPosition();

            if (!IsTargetVisible(targetPosition))
            {
                continue;
            }

            return artPiece;
        }

        return null;
    }

    private PlayerInteractor FindVisiblePlayer()
    {
        Vector3 eyePosition = GetEyePosition();

        Collider[] hits = Physics.OverlapSphere(
            eyePosition,
            CurrentViewDistance,
            playerLayer,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider hit in hits)
        {
            PlayerInteractor player = hit.GetComponentInParent<PlayerInteractor>();

            if (player == null)
            {
                continue;
            }

            Vector3 targetPosition = player.transform.position + Vector3.up * targetHeight;

            if (!IsTargetVisible(targetPosition))
            {
                continue;
            }

            return player;
        }

        return null;
    }

    private bool IsTargetVisible(Vector3 targetPosition)
    {
        Vector3 eyePosition = GetEyePosition();
        Vector3 directionToTarget = targetPosition - eyePosition;

        float distanceToTarget = directionToTarget.magnitude;

        if (distanceToTarget > CurrentViewDistance)
        {
            return false;
        }

        float angleToTarget = Vector3.Angle(transform.forward, directionToTarget.normalized);

        if (angleToTarget > CurrentViewAngle * 0.5f)
        {
            return false;
        }

        bool blocked = Physics.Raycast(
            eyePosition,
            directionToTarget.normalized,
            out RaycastHit hitInfo,
            distanceToTarget,
            obstacleLayer,
            QueryTriggerInteraction.Ignore
        );

        if (showDebugRay)
        {
            Color rayColor = blocked ? Color.red : Color.green;
            Debug.DrawRay(eyePosition, directionToTarget.normalized * distanceToTarget, rayColor);
        }

        if (blocked)
        {
            if (showDebugLog)
            {
                Debug.Log("Guard vision blocked by: " + hitInfo.collider.gameObject.name);
            }

            return false;
        }

        return true;
    }

    public Vector3 GetEyePosition()
    {
        return transform.position + Vector3.up * eyeHeight;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 eyePosition = transform.position + Vector3.up * eyeHeight;

        float drawDistance = Application.isPlaying ? CurrentViewDistance : viewDistance;
        float drawAngle = Application.isPlaying ? CurrentViewAngle : viewAngle;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyePosition, drawDistance);

        Vector3 leftDirection = Quaternion.Euler(0f, -drawAngle * 0.5f, 0f) * transform.forward;
        Vector3 rightDirection = Quaternion.Euler(0f, drawAngle * 0.5f, 0f) * transform.forward;

        Gizmos.DrawLine(eyePosition, eyePosition + leftDirection * drawDistance);
        Gizmos.DrawLine(eyePosition, eyePosition + rightDirection * drawDistance);
    }
}