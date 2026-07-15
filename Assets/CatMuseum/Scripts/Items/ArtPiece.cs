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

    [Header("guard check")]
    [SerializeField] private float lookHeight = 0.8f;


    private ArtPieceState state = ArtPieceState.Real;
    private ArtData placedDummyData;
    private bool hasBeenReportedEmpty = false;

    public bool CanInteract => true;

    public bool IsReal => state == ArtPieceState.Real;
    public bool HasDummy => state == ArtPieceState.Dummy;
    public bool IsEmpty => state == ArtPieceState.Empty;

    public bool CanReportEmpty => IsEmpty && !hasBeenReportedEmpty;

    public string ArtDisplayName
    {
        get
        {
            if (artData == null)
            {
                return gameObject.name;
            }

            return artData.artName;
        }
    }

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

                if (!CanPackArtIntoBackpack(player))
                {
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

        if (PlayerProfile.Instance == null)
        {
            player.ShowNotice("PlayerProfile is not found");
            return;
        }

        if (!PlayerProfile.Instance.TryConsumePackedDummyForArt(artData, out PackedBackpackItem consumedPackedDummy))
        {
            player.ShowNotice("Packed dummy is not found");
            return;
        }

        if (!TryPackArtIntoBackpack(player))
        {
            PlayerProfile.Instance.RestorePackedItem(consumedPackedDummy);
            return;
        }

        placedDummyData = dummy.data;

        bool success = player.Inventory.ReplaceItem(dummy, artData, false);

        if (!success)
        {
            placedDummyData = null;
            PlayerProfile.Instance.RemoveLatestLootForArt(artData);
            PlayerProfile.Instance.RestorePackedItem(consumedPackedDummy);
            return;
        }

        state = ArtPieceState.Dummy;
        UpdateVisual();

        Debug.Log($"{artData.artName} stolen with dummy. Value: {artData.value}");
    }

    private void CompleteSteal(PlayerInteractor player)
    {
        if (!TryPackArtIntoBackpack(player))
        {
            return;
        }

        bool success = player.Inventory.AddItem(artData, false);

        if (!success)
        {
            if (PlayerProfile.Instance != null)
            {
                PlayerProfile.Instance.RemoveLatestLootForArt(artData);
            }

            return;
        }

        placedDummyData = null;
        state = ArtPieceState.Empty;
        UpdateVisual();

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

        Debug.Log("Dummy placed");
    }

    public Vector3 GetLookPosition()
    {
        return transform.position + Vector3.up * lookHeight;
    }

    public float GetEmptyAlertAmount()
    {
        if (artData == null)
        {
            return 20.0f;
        }

        return artData.suspicionWhenStolen;
    }

    public void ReportEmpty()
    {
        if (!CanReportEmpty)
        {
            return;
        }

        hasBeenReportedEmpty = true;
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

    private bool CanPackArtIntoBackpack(PlayerInteractor player)
    {
        if (artData == null)
        {
            return false;
        }

        if (PlayerProfile.Instance == null)
        {
            player.ShowNotice("PlayerProfile is not found");
            return false;
        }

        if (!PlayerProfile.Instance.BackpackEquipped)
        {
            player.ShowNotice("Backpack is not equipped");
            return false;
        }

        if (!PlayerProfile.Instance.CanAutoPackArt(artData, true))
        {
            player.ShowNotice("Backpack is full");
            return false;
        }

        return true;
    }

    private bool TryPackArtIntoBackpack(PlayerInteractor player)
    {
        if (artData == null)
        {
            return false;
        }

        if (PlayerProfile.Instance == null)
        {
            player.ShowNotice("PlayerProfile is not found");
            return false;
        }

        bool success = PlayerProfile.Instance.TryAutoPackArt(
            artData,
            out int placedX,
            out int placedY,
            true
        );

        if (!success)
        {
            player.ShowNotice("Backpack is full");
            return false;
        }

        player.ShowNotice($"Packed: {artData.artName}");
        return true;
    }
}
