using UnityEngine;

public class ExitZone : MonoBehaviour
{
    [Header("clear condition")]
    [SerializeField] private int requiredStolenValue = 1;
    [SerializeField] private bool requireNotInteracting = true;

    [Header("message")]
    [SerializeField] private string noLootMessage = "You need to steal something first";
    [SerializeField] private string clearMessage = "Escaped with the loot!";

    private void OnTriggerEnter(Collider other)
    {
        TryExit(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryExit(other);
    }

    private void TryExit(Collider other)
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
        {
            return;
        }

        PlayerInventory inventory = other.GetComponentInParent<PlayerInventory>();
        PlayerInteractor interactor = other.GetComponentInParent<PlayerInteractor>();

        if (inventory == null)
        {
            return;
        }

        if (requireNotInteracting && interactor != null && interactor.IsInteracting)
        {
            return;
        }

        if (inventory.TotalStolenValue < requiredStolenValue)
        {
            if (interactor != null)
            {
                interactor.ShowNotice(noLootMessage);
            }

            return;
        }

        if (PlayerProfile.Instance != null)
        {
            PlayerProfile.Instance.AddMoney(inventory.TotalStolenValue);
        }

        if (GameManager.Instance != null)
        {
            string message = $"{clearMessage}\nReward: {inventory.TotalStolenValue} coins";
            GameManager.Instance.ClearGame(message);
        }
    }

    private void Reset()
    {
        Collider zoneCollider = GetComponent<Collider>();

        if (zoneCollider != null)
        {
            zoneCollider.isTrigger = true;
        }
    }
}