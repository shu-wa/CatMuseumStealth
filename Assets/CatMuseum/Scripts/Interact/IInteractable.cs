public enum InteractionMode
{
    SwapIfPossible,
    ForceSteal
}

public interface IInteractable
{
    bool CanInteract { get; }

    bool CanStartInteraction(PlayerInteractor player, InteractionMode mode);
    float GetInteractionDuration(PlayerInteractor player, InteractionMode mode);
    string GetInteractionText(PlayerInteractor player, InteractionMode mode);

    void CompleteInteraction(PlayerInteractor player, InteractionMode mode);
    string GetPrompt(PlayerInteractor player);
}