using UnityEngine;

public class ArtPiece : MonoBehaviour, IInteractable
{
    private enum ArtPieceState
    {
        Real,
        Dummy,
        Empty
    }

    [Header("art data")]
    [SerializeField] private ArtData artData;

    [Header("visuals")]
    [SerializeField] private GameObject realVisual;
    [SerializeField] private GameObject dummyVisual;
    [SerializeField] private GameObject emptyPedestalVisual;

    [Header("interaction time")]
    [SerializeField] private float swapDuration = 2.0f;
    [SerializeField] private float stealDuration = 1.0f;
    [SerializeField] private float recoverDummyDuration = 0.8f;
    [SerializeField] private float placeDummyDuration = 1.2f;

    [Header("extra alert")]
    [SerializeField] private float alertWhenRecoverDummy = 0f;
    [SerializeField] private float alertWhenPlaceDummy = 0f;

    private ArtPieceState state = ArtPieceState.Real;
    private ArtData placedDummyData;

    public bool CanInteract => true;

    private void Start()
    {
        UpdateVisual();
    }

    public bool CanStartInteraction(PlayerInteractor player, InteractionMode mode)
    {
        if (artData == null)
        {
            Debug.LogWarning("ArtData is not set");
            player.ShowNotice("ArtData is not set");
            return false;
        }

        if (state == ArtPieceState.Real)
        {
            if (mode == InteractionMode.SwapIfPossible)
            {
                InventoryEntry dummy = player.Inventory.FindMatchingDummy(artData);

                if (dummy == null)
                {
                    Debug.Log("No matching dummy. Press F to steal by force");
                    player.ShowNotice("No matching dummy! Press F to steal by force");
                    return false;
                }

                return true;
            }

            if (mode == InteractionMode.ForceSteal)
            {
                if (!player.Inventory.CanAdd(artData))
                {
                    Debug.Log("Not enough capacity");
                    player.ShowNotice("Not enough capacity");
                    return false;
                }

                return true;
            }
        }

        if (state == ArtPieceState.Dummy)
        {
            if (mode == InteractionMode.ForceSteal)
            {
                if (placedDummyData == null)
                {
                    player.ShowNotice("No dummy data");
                    return false;
                }

                if (!player.Inventory.CanAdd(placedDummyData))
                {
                    player.ShowNotice("Not enough capacity");
                    return false;
                }

                return true;
            }

            player.ShowNotice("Dummy is already placed");
            return false;
        }

        if (state == ArtPieceState.Empty)
        {
            if (mode == InteractionMode.SwapIfPossible)
            {
                InventoryEntry dummy = player.Inventory.FindMatchingDummy(artData);

                if (dummy == null)
                {
                    player.ShowNotice("No matching dummy to place");
                    return false;
                }

                return true;
            }

            player.ShowNotice("Nothing to steal");
            return false;
        }

        return false;
    }

    public float GetInteractionDuration(PlayerInteractor player, InteractionMode mode)
    {
        if (state == ArtPieceState.Real)
        {
            if (mode == InteractionMode.SwapIfPossible)
            {
                return swapDuration;
            }

            return stealDuration;
        }

        if (state == ArtPieceState.Dummy)
        {
            return recoverDummyDuration;
        }

        if (state == ArtPieceState.Empty)
        {
            return placeDummyDuration;
        }

        return 1.0f;
    }

    public string GetInteractionText(PlayerInteractor player, InteractionMode mode)
    {
        if (state == ArtPieceState.Real)
        {
            if (mode == InteractionMode.SwapIfPossible)
            {
                return "Swapping dummy";
            }

            return "Stealing art";
        }

        if (state == ArtPieceState.Dummy)
        {
            return "Recovering dummy";
        }

        if (state == ArtPieceState.Empty)
        {
            return "Placing dummy";
        }

        return "Interacting";
    }

    public void CompleteInteraction(PlayerInteractor player, InteractionMode mode)
    {
        if (state == ArtPieceState.Real)
        {
            if (mode == InteractionMode.SwapIfPossible)
            {
                CompleteSwap(player);
            }
            else
            {
                CompleteSteal(player);
            }

            return;
        }

        if (state == ArtPieceState.Dummy)
        {
            if (mode == InteractionMode.ForceSteal)
            {
                CompleteRecoverDummy(player);
            }

            return;
        }

        if (state == ArtPieceState.Empty)
        {
            if (mode == InteractionMode.SwapIfPossible)
            {
                CompletePlaceDummy(player);
            }
        }
    }

    private void CompleteSwap(PlayerInteractor player)
    {
        InventoryEntry dummy = player.Inventory.FindMatchingDummy(artData);

        if (dummy == null)
        {
            player.ShowNotice("No matching dummy");
            return;
        }

        placedDummyData = dummy.data;

        bool success = player.Inventory.ReplaceItem(dummy, artData, false);

        if (!success)
        {
            return;
        }

        state = ArtPieceState.Dummy;
        UpdateVisual();

        if (AlertManager.Instance != null)
        {
            float alertAmount = GetModifiedAlertAmount(player, artData.suspicionWhenSwapped, false);
            AlertManager.Instance.AddAlert(alertAmount);
        }

        Debug.Log($"{artData.artName} stolen with dummy. Value: {artData.value}");
    }

    private void CompleteSteal(PlayerInteractor player)
    {
        bool success = player.Inventory.AddItem(artData, false);

        if (!success)
        {
            return;
        }

        placedDummyData = null;
        state = ArtPieceState.Empty;
        UpdateVisual();

        if (AlertManager.Instance != null)
        {
            float alertAmount = GetModifiedAlertAmount(player, artData.suspicionWhenStolen, true);
            AlertManager.Instance.AddAlert(alertAmount);
        }

        Debug.Log($"{artData.artName} stolen by force. Value: {artData.value}");
    }

    private void CompleteRecoverDummy(PlayerInteractor player)
    {
        if (placedDummyData == null)
        {
            player.ShowNotice("No dummy to recover");
            return;
        }

        bool success = player.Inventory.AddItem(placedDummyData, true);

        if (!success)
        {
            return;
        }

        placedDummyData = null;
        state = ArtPieceState.Empty;
        UpdateVisual();

        if (AlertManager.Instance != null && alertWhenRecoverDummy > 0f)
        {
            float alertAmount = GetModifiedAlertAmount(player, alertWhenRecoverDummy, false);
            AlertManager.Instance.AddAlert(alertAmount);
        }

        Debug.Log("Dummy recovered");
    }

    private void CompletePlaceDummy(PlayerInteractor player)
    {
        InventoryEntry dummy = player.Inventory.FindMatchingDummy(artData);

        if (dummy == null)
        {
            player.ShowNotice("No matching dummy to place");
            return;
        }

        placedDummyData = dummy.data;

        bool success = player.Inventory.RemoveItem(dummy);

        if (!success)
        {
            placedDummyData = null;
            return;
        }

        state = ArtPieceState.Dummy;
        UpdateVisual();

        if (AlertManager.Instance != null && alertWhenPlaceDummy > 0f)
        {
            float alertAmount = GetModifiedAlertAmount(player, alertWhenPlaceDummy, false);
            AlertManager.Instance.AddAlert(alertAmount);
        }

        Debug.Log("Dummy placed");
    }

    private float GetModifiedAlertAmount(PlayerInteractor player, float baseAlertAmount, bool forceSteal)
    {
        PlayerRoomTracker roomTracker = player.GetComponent<PlayerRoomTracker>();

        if (roomTracker == null)
        {
            return baseAlertAmount;
        }

        return roomTracker.GetModifiedAlertAmount(artData, baseAlertAmount, forceSteal);
    }

    private void UpdateVisual()
    {
        if (realVisual != null)
        {
            realVisual.SetActive(state == ArtPieceState.Real);
        }

        if (dummyVisual != null)
        {
            dummyVisual.SetActive(state == ArtPieceState.Dummy);
        }

        if (emptyPedestalVisual != null)
        {
            emptyPedestalVisual.SetActive(state == ArtPieceState.Empty);
        }
    }

    public string GetPrompt(PlayerInteractor player)
    {
        if (artData == null)
        {
            return "Unknown art";
        }

        if (state == ArtPieceState.Real)
        {
            bool hasMatchingDummy = player.Inventory.FindMatchingDummy(artData) != null;
            string dummyText = hasMatchingDummy ? "Dummy: OK" : "Dummy: NONE";

            return $"{artData.artName} | Value: {artData.value} | {artData.size} {artData.category} | {dummyText}\nE: Swap {swapDuration:0.0}s / F: Steal {stealDuration:0.0}s";
        }

        if (state == ArtPieceState.Dummy)
        {
            return $"{artData.artName} has dummy\nF: Recover Dummy {recoverDummyDuration:0.0}s";
        }

        if (state == ArtPieceState.Empty)
        {
            bool hasMatchingDummy = player.Inventory.FindMatchingDummy(artData) != null;
            string dummyText = hasMatchingDummy ? "Dummy: OK" : "Dummy: NONE";

            return $"Empty pedestal for {artData.size} {artData.category} | {dummyText}\nE: Place Dummy {placeDummyDuration:0.0}s";
        }

        return "";
    }
}