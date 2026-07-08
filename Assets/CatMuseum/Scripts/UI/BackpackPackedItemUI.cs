using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BackpackPackedItemUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("ui")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image iconImage;

    private BackpackMenuUI menuUI;
    private PackedBackpackItem packedItem;

    public void Setup(BackpackMenuUI menuUI, PackedBackpackItem packedItem, float cellSize, float cellGap)
    {
        this.menuUI = menuUI;
        this.packedItem = packedItem;

        if (packedItem == null || packedItem.itemData == null)
        {
            return;
        }

        BackpackItemData itemData = packedItem.itemData;

        RectTransform rectTransform = transform as RectTransform;

        if (rectTransform != null)
        {
            float pitch = cellSize + cellGap;

            int itemWidth = itemData.GetWidth(packedItem.rotated);
            int itemHeight = itemData.GetHeight(packedItem.rotated);

            float width = itemWidth * cellSize + (itemWidth - 1) * cellGap;
            float height = itemHeight * cellSize + (itemHeight - 1) * cellGap;

            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);

            rectTransform.sizeDelta = new Vector2(width, height);
            rectTransform.anchoredPosition = new Vector2(
                packedItem.gridX * pitch,
                -packedItem.gridY * pitch
            );
        }

        if (nameText != null)
        {
            nameText.text = itemData.itemName;
        }

        if (iconImage != null)
        {
            iconImage.sprite = itemData.icon;
            iconImage.color = itemData.icon != null ? Color.white : new Color(0.3f, 0.3f, 0.3f, 0.85f);
            iconImage.raycastTarget = true;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            menuUI?.RemovePackedItem(packedItem);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        if (menuUI == null || packedItem == null)
        {
            return;
        }

        menuUI.BeginMovePackedItem(packedItem);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Unityにドラッグ対象として認識させるために必要。
        // 実際の追従処理はBackpackMenuUI側で行う。
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        menuUI?.EndDragItem();
    }
}