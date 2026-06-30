using UnityEngine;

public class GuardVision : MonoBehaviour
{
    [Header("view setting")]
    [SerializeField] private float viewDistance = 6.0f;
    [SerializeField] private float viewAngle = 80.0f;
    [SerializeField] private float eyeHeight = 1.2f;
    [SerializeField] private float targetHeight = 1.0f;

    [Header("layers")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("caught setting")]
    [SerializeField] private float alertWhenCaughtInteracting = 40.0f;
    [SerializeField] private float caughtCooldown = 2.0f;

    [Header("debug")]
    [SerializeField] private bool showDebugRay = true;
    [SerializeField] private bool showDebugLog = false;

    private float caughtTimer = 0f;

    private void Update()
    {
        UpdateCooldown();

        PlayerInteractor player = FindVisiblePlayer();

        if (player == null)
        {
            return;
        }

        if (player.IsInteracting)
        {
            CatchPlayerInteracting(player);
        }
    }

    private void UpdateCooldown()
    {
        if (caughtTimer > 0f)
        {
            caughtTimer -= Time.deltaTime;
        }
    }

    private PlayerInteractor FindVisiblePlayer()
    {
        Vector3 eyePosition = GetEyePosition();

        Collider[] hits = Physics.OverlapSphere(
            eyePosition,
            viewDistance,
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
            Vector3 directionToPlayer = targetPosition - eyePosition;

            float distanceToPlayer = directionToPlayer.magnitude;

            if (distanceToPlayer > viewDistance)
            {
                continue;
            }

            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer.normalized);

            if (angleToPlayer > viewAngle * 0.5f)
            {
                continue;
            }

            bool blocked = Physics.Raycast(
                eyePosition,
                directionToPlayer.normalized,
                out RaycastHit hitInfo,
                distanceToPlayer,
                obstacleLayer,
                QueryTriggerInteraction.Ignore
            );

            if (showDebugRay)
            {
                Color rayColor = blocked ? Color.red : Color.green;
                Debug.DrawRay(eyePosition, directionToPlayer.normalized * distanceToPlayer, rayColor);
            }

            if (blocked)
            {
                if (showDebugLog)
                {
                    Debug.Log("Guard vision blocked by: " + hitInfo.collider.gameObject.name);
                }

                continue;
            }

            return player;
        }

        return null;
    }

    private void CatchPlayerInteracting(PlayerInteractor player)
    {
        if (caughtTimer > 0f)
        {
            return;
        }

        caughtTimer = caughtCooldown;

        Debug.Log("Player was caught while interacting");

        player.ForceCancelInteraction("Seen by guard while interacting");
        player.ShowNotice("Caught while interacting!");

        if (AlertManager.Instance != null)
        {
            AlertManager.Instance.AddAlert(alertWhenCaughtInteracting);
        }
    }

    private Vector3 GetEyePosition()
    {
        return transform.position + Vector3.up * eyeHeight;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 eyePosition = transform.position + Vector3.up * eyeHeight;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyePosition, viewDistance);

        Vector3 leftDirection = Quaternion.Euler(0f, -viewAngle * 0.5f, 0f) * transform.forward;
        Vector3 rightDirection = Quaternion.Euler(0f, viewAngle * 0.5f, 0f) * transform.forward;

        Gizmos.DrawLine(eyePosition, eyePosition + leftDirection * viewDistance);
        Gizmos.DrawLine(eyePosition, eyePosition + rightDirection * viewDistance);
    }
}