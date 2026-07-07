using UnityEngine;

public class BackpackStation : MonoBehaviour, IMenuInteractable
{
    [Header("ui")]
    [SerializeField] private BackpackMenuUI backpackMenuUI;

    public void Interact(MenuInteractor interactor)
    {
        if (backpackMenuUI == null)
        {
            MenuNoticeUI.Instance?.ShowNotice("Backpack UI is not set");
            return;
        }

        backpackMenuUI.Open();
    }

    public void SecondaryInteract(MenuInteractor interactor)
    {
    }

    public void HandleFocusedInput(MenuInteractor interactor)
    {
    }

    public string GetPrompt()
    {
        return "E: Open Backpack";
    }
}