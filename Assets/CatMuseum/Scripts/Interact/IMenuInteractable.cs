public interface IMenuInteractable
{
    void Interact(MenuInteractor interactor);
    void SecondaryInteract(MenuInteractor interactor);
    void HandleFocusedInput(MenuInteractor interactor);
    string GetPrompt();
}