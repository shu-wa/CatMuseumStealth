using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BackpackGridCellUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("visual")]
    [SerializeField] private Image backgroundImage;

    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.15f);
    [SerializeField] private Color hoverColor = new Color(1f, 1f, 1f, 0.35f);

    private BackpackMenuUI menuUI;

    public int GridX { get; private set; }
    public int GridY { get; private set; }

    public void Setup(BackpackMenuUI menuUI, int gridX, int gridY)
    {
        this.menuUI = menuUI;
        GridX = gridX;
        GridY = gridY;

        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }

        SetColor(normalColor);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        menuUI?.SetHoveredCell(this);
        SetColor(hoverColor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        menuUI?.ClearHoveredCell(this);
        SetColor(normalColor);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            menuUI?.TryRemovePackedItemAtCell(GridX, GridY);
        }
    }

    private void SetColor(Color color)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = color;
        }
    }
}