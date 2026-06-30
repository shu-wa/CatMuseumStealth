using UnityEngine;

public class ExhibitionRoomZone : MonoBehaviour
{
    [Header("room information")]
    [SerializeField] private string roomName = "Normal Room";
    [SerializeField] private string roomDescription = "No special effect";

    [Header("alert multiplier")]
    [SerializeField] private float paintingAlertMultiplier = 1.0f;
    [SerializeField] private float sculptureAlertMultiplier = 1.0f;
    [SerializeField] private float forceStealAlertMultiplier = 1.0f;

    public string RoomName => roomName;
    public string RoomDescription => roomDescription;

    public float GetAlertMultiplier(ArtData artData, bool forceSteal)
    {
        if (artData == null)
        {
            return 1.0f;
        }

        float categoryMultiplier = 1.0f;

        if (artData.category == ArtCategory.Painting)
        {
            categoryMultiplier = paintingAlertMultiplier;
        }
        else if (artData.category == ArtCategory.Sculpture)
        {
            categoryMultiplier = sculptureAlertMultiplier;
        }

        float modeMultiplier = forceSteal ? forceStealAlertMultiplier : 1.0f;

        return categoryMultiplier * modeMultiplier;
    }

    public string GetRoomText()
    {
        return $"{roomName}: {roomDescription}";
    }

    private void Reset()
    {
        Collider roomCollider = GetComponent<Collider>();

        if (roomCollider != null)
        {
            roomCollider.isTrigger = true;
        }
    }
}