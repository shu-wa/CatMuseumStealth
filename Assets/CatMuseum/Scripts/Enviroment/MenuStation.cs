using UnityEngine;

public enum MenuStationType
{
    Closet,
    Mirror,
    Backpack,
    Whiteboard
}

public class MenuStation : MonoBehaviour, IMenuInteractable
{
    [Header("station")]
    [SerializeField] private MenuStationType stationType;

    [Header("map")]
    [SerializeField] private string mapSceneName = "Map_01_Museum";

    public void Interact(MenuInteractor interactor)
    {
        switch (stationType)
        {
            case MenuStationType.Closet:
                OpenCloset();
                break;

            case MenuStationType.Mirror:
                OpenMirror();
                break;

            case MenuStationType.Backpack:
                OpenBackpack();
                break;

            case MenuStationType.Whiteboard:
                OpenWhiteboard();
                break;
        }
    }

    public void SecondaryInteract(MenuInteractor interactor)
    {
    }

    public void HandleFocusedInput(MenuInteractor interactor)
    {
    }

    public string GetPrompt()
    {
        switch (stationType)
        {
            case MenuStationType.Closet:
                return "E: Open Closet";

            case MenuStationType.Mirror:
                return "E: Use Mirror";

            case MenuStationType.Backpack:
                if (PlayerProfile.Instance != null && PlayerProfile.Instance.BackpackEquipped)
                {
                    return "E: Take off Backpack";
                }

                return "E: Equip Backpack";

            case MenuStationType.Whiteboard:
                return "E: Start Mission";

            default:
                return "E: Interact";
        }
    }

    private void OpenCloset()
    {
        MenuNoticeUI.Instance?.ShowNotice("Closet: accessories will be added later");
    }

    private void OpenMirror()
    {
        MenuNoticeUI.Instance?.ShowNotice("Mirror: cat patterns will be added later");
    }

    private void OpenBackpack()
    {
        if (PlayerProfile.Instance == null)
        {
            MenuNoticeUI.Instance?.ShowNotice("PlayerProfile is not found");
            return;
        }

        PlayerProfile.Instance.ToggleBackpack();

        if (PlayerProfile.Instance.BackpackEquipped)
        {
            MenuNoticeUI.Instance?.ShowNotice("Backpack equipped!");
        }
        else
        {
            MenuNoticeUI.Instance?.ShowNotice("Backpack removed");
        }
    }

    private void OpenWhiteboard()
    {
        if (PlayerProfile.Instance == null)
        {
            MenuNoticeUI.Instance?.ShowNotice("PlayerProfile is not found");
            return;
        }

        PlayerProfile.Instance.SetSelectedMap(mapSceneName);

        if (!PlayerProfile.Instance.BackpackEquipped)
        {
            MenuNoticeUI.Instance?.ShowNotice("You need to equip a backpack first");
            return;
        }

        if (GameManager.Instance == null)
        {
            MenuNoticeUI.Instance?.ShowNotice("GameManager is not found");
            return;
        }

        GameManager.Instance.StartSelectedMission();
    }
}