using TMPro;
using UnityEngine;

public class MenuStatusUI : MonoBehaviour
{
    [Header("text")]
    [SerializeField] private TextMeshProUGUI statusText;

    private void Update()
    {
        if (statusText == null)
        {
            return;
        }

        if (PlayerProfile.Instance == null)
        {
            statusText.text = "Profile: None";
            return;
        }

        string backpackText = PlayerProfile.Instance.BackpackEquipped
            ? "Equipped"
            : "Not Equipped";

        int usedCells = PlayerProfile.Instance.GetUsedCellCount();
        int totalCells = PlayerProfile.Instance.GetTotalCellCount();

        statusText.text =
            $"Money: {PlayerProfile.Instance.Money}\n" +
            $"Backpack: {backpackText}\n" +
            $"Backpack Grid: {usedCells} / {totalCells}";
    }
}