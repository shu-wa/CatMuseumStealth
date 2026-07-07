using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BackpackItemListEntryUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("ui")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Image iconImage;

    private BackpackMenuUI menuUI;
    private BackpackItemData itemData;
    private bool draggingStarted = false;

    public void Setup(BackpackMenuUI menuUI, BackpackItemData itemData)
    {
        this.menuUI = menuUI;
        this.itemData = itemData;

        Refresh();
    }

    public void Refresh()
    {
        if (itemData == null)
        {
            return;
        }

        if (nameText != null)
        {
            nameText.text = itemData.itemName;
        }

        if (iconImage != null)
        {
            iconImage.sprite = itemData.icon;
            iconImage.color = itemData.icon != null ? Color.white : Color.gray;
        }

        int ownedCount = 0;

        if (PlayerProfile.Instance != null)
        {
            ownedCount = PlayerProfile.Instance.GetOwnedCount(itemData);
        }

        if (statusText != null)
        {
            statusText.text = ownedCount <= 0 ? "+" : $"x{ownedCount}";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (menuUI == null || itemData == null)
        {
            return;
        }

        if (PlayerProfile.Instance == null)
        {
            return;
        }

        int ownedCount = PlayerProfile.Instance.GetOwnedCount(itemData);

        if (ownedCount <= 0)
        {
            menuUI.BuyItem(itemData);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        draggingStarted = false;

        if (menuUI == null || itemData == null)
        {
            return;
        }

        if (PlayerProfile.Instance == null)
        {
            return;
        }

        if (PlayerProfile.Instance.GetOwnedCount(itemData) <= 0)
        {
            return;
        }

        draggingStarted = menuUI.BeginDragItem(itemData);
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!draggingStarted)
        {
            return;
        }

        menuUI.EndDragItem();
        draggingStarted = false;
    }
}