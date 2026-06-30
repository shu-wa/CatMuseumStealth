using UnityEngine;

public class PlayerRoomTracker : MonoBehaviour
{
    [Header("room check")]
    [SerializeField] private Vector3 checkOffset = new Vector3(0f, 0.5f, 0f);
    [SerializeField] private float checkRadius = 0.5f;
    [SerializeField] private LayerMask roomLayer;

    public ExhibitionRoomZone CurrentRoom { get; private set; }

    public string CurrentRoomText
    {
        get
        {
            if (CurrentRoom == null)
            {
                return "Room: None";
            }

            return $"Room: {CurrentRoom.GetRoomText()}";
        }
    }

    private void Update()
    {
        UpdateCurrentRoom();
    }

    private void UpdateCurrentRoom()
    {
        Vector3 center = transform.position + checkOffset;

        Collider[] hits = Physics.OverlapSphere(
            center,
            checkRadius,
            roomLayer,
            QueryTriggerInteraction.Collide
        );

        ExhibitionRoomZone nearestRoom = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            ExhibitionRoomZone room = hit.GetComponent<ExhibitionRoomZone>();

            if (room == null)
            {
                room = hit.GetComponentInParent<ExhibitionRoomZone>();
            }

            if (room == null)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, hit.transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestRoom = room;
            }
        }

        CurrentRoom = nearestRoom;
    }

    public float GetModifiedAlertAmount(ArtData artData, float baseAlertAmount, bool forceSteal)
    {
        if (CurrentRoom == null)
        {
            return baseAlertAmount;
        }

        float multiplier = CurrentRoom.GetAlertMultiplier(artData, forceSteal);
        return baseAlertAmount * multiplier;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = transform.position + checkOffset;
        Gizmos.DrawWireSphere(center, checkRadius);
    }
}