using UnityEngine;

public class RuntimeBackpackInput : MonoBehaviour
{
    [Header("ui")]
    [SerializeField] private BackpackMenuUI backpackMenuUI;

    [Header("input")]
    [SerializeField] private KeyCode openKey = KeyCode.Tab;

    [Header("rules")]
    [SerializeField] private bool requireBackpackEquipped = true;

    private void Update()
    {
        if (Input.GetKeyDown(openKey))
        {
            ToggleBackpackUI();
        }
    }

    private void ToggleBackpackUI()
    {
        if (backpackMenuUI == null)
        {
            return;
        }

        if (requireBackpackEquipped)
        {
            if (PlayerProfile.Instance == null || !PlayerProfile.Instance.BackpackEquipped)
            {
                MenuNoticeUI.Instance?.ShowNotice("Backpack is not equipped");
                return;
            }
        }

        backpackMenuUI.ToggleOpen();
    }
}