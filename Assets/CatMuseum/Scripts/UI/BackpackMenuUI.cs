using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BackpackMenuUI : MonoBehaviour
{
    [Header("root")]
    [SerializeField] private GameObject panelRoot;

    [Header("item data")]
    [SerializeField] private BackpackItemData[] allItems;

    [Header("lists")]
    [SerializeField] private RectTransform supportListRoot;
    [SerializeField] private RectTransform dummyListRoot;
    [SerializeField] private BackpackItemListEntryUI listEntryPrefab;

    [Header("grid")]
    [SerializeField] private RectTransform gridCellsRoot;
    [SerializeField] private RectTransform packedItemsRoot;
    [SerializeField] private BackpackGridCellUI gridCellPrefab;
    [SerializeField] private BackpackPackedItemUI packedItemPrefab;
    [SerializeField] private float cellSize = 64f;
    [SerializeField] private float cellGap = 4f;

    [Header("texts")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("buttons")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button backpackButton;
    [SerializeField] private TextMeshProUGUI backpackButtonText;

    [Header("dragIcon")]
    [SerializeField] private RectTransform dragIconRect;
    [SerializeField] private RectTransform dragPreviewRoot;
    [SerializeField] private Color dragPreviewCellColor = new Color(1f, 1f, 1f, 0.35f);
    [SerializeField] private Color dragPreviewBorderColor = new Color(0f, 0f, 0f, 0.35f);
    [SerializeField] private RectTransform dragPreviewIconRect;
    [SerializeField] private Image dragPreviewIconImage;
    [SerializeField] private Color dragPreviewIconColor = new Color(1f, 1f, 1f, 0.75f);


    [Header("placement visual")]
    [SerializeField] private Color dragPreviewValidColor = new Color(0.2f, 1f, 0.35f, 0.45f);
    [SerializeField] private Color dragPreviewInvalidColor = new Color(1f, 0.2f, 0.2f, 0.45f);




    private readonly List<Image> dragPreviewImages = new List<Image>();
    private readonly List<GameObject> dragPreviewCells = new List<GameObject>();

    private PackedBackpackItem draggingPackedItem;
    private bool isMovingPackedItem = false;
    public static bool IsAnyBackpackOpen { get; private set; }

    private Canvas rootCanvas;
    private RectTransform canvasRect;

    private bool isOpen = false;

    private BackpackItemData draggingItem;
    private bool isDragging = false;
    private bool draggingRotated = false;
    private BackpackGridCellUI hoveredCell;

    private int generatedGridWidth = -1;
    private int generatedGridHeight = -1;


    private Image dragIconImage;
    private TextMeshProUGUI dragIconText;
    private CanvasGroup dragIconCanvasGroup;

    private float CellPitch => cellSize + cellGap;

    private void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();

        if (rootCanvas != null)
        {
            canvasRect = rootCanvas.transform as RectTransform;
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
        }

        if (backpackButton != null)
        {
            backpackButton.onClick.AddListener(ToggleBackpack);
        }

        GenerateGrid();

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
        if (dragIconImage != null)
        {
            dragIconImage.raycastTarget = false;
        }

        if (dragIconRect != null)
        {
            CanvasGroup canvasGroup = dragIconRect.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                canvasGroup = dragIconRect.gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            dragIconRect.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isOpen)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
            return;
        }

        if (isDragging)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                ToggleDraggingRotation();
            }

            UpdateDragIconPosition();
            UpdateDragPreviewColor();
        }
    }

    public void Open()
    {
        isOpen = true;
        IsAnyBackpackOpen = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        EnsureGridGenerated();
        RefreshAll();
    }

    public void Close()
    {
        isOpen = false;
        IsAnyBackpackOpen = false;

        CancelDrag();

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void UpdateDragPreviewIcon()
    {
        if (draggingItem == null || dragPreviewIconRect == null || dragPreviewIconImage == null)
        {
            return;
        }

        int width = draggingItem.GetWidth(draggingRotated);
        int height = draggingItem.GetHeight(draggingRotated);

        float previewCellSize = 48f;
        float previewCellGap = 4f;

        float iconWidth = width * previewCellSize + (width - 1) * previewCellGap;
        float iconHeight = height * previewCellSize + (height - 1) * previewCellGap;

        dragPreviewIconRect.sizeDelta = new Vector2(iconWidth, iconHeight);
        dragPreviewIconRect.anchoredPosition = Vector2.zero;

        dragPreviewIconImage.sprite = draggingItem.icon;
        dragPreviewIconImage.color = draggingItem.icon != null
            ? dragPreviewIconColor
            : new Color(0f, 0f, 0f, 0f);

        dragPreviewIconImage.raycastTarget = false;
        dragPreviewIconImage.enabled = draggingItem.icon != null;
    }

    public void ToggleBackpack()
    {
        if (PlayerProfile.Instance == null)
        {
            ShowNotice("PlayerProfile is not found");
            return;
        }

        PlayerProfile.Instance.ToggleBackpack();
        RefreshAll();

        if (PlayerProfile.Instance.BackpackEquipped)
        {
            ShowNotice("Backpack equipped!");
        }
        else
        {
            ShowNotice("Backpack removed");
        }
        UpdateDragPreviewIcon();
    }

    public void BuyItem(BackpackItemData itemData)
    {
        if (PlayerProfile.Instance == null)
        {
            ShowNotice("PlayerProfile is not found");
            return;
        }

        if (itemData == null)
        {
            return;
        }

        bool success = PlayerProfile.Instance.TryBuyItem(itemData);

        if (success)
        {
            ShowNotice("Bought: " + itemData.itemName);
        }
        else
        {
            ShowNotice("Not enough money");
        }

        RefreshAll();
    }
    private void BuildDragPreview()
    {
        ClearDragPreview();

        if (draggingItem == null || dragPreviewRoot == null)
        {
            return;
        }

        int width = draggingItem.GetWidth(draggingRotated);
        int height = draggingItem.GetHeight(draggingRotated);

        float previewCellSize = 48f;
        float previewCellGap = 4f;
        float pitch = previewCellSize + previewCellGap;

        dragPreviewRoot.sizeDelta = new Vector2(
            width * previewCellSize + (width - 1) * previewCellGap,
            height * previewCellSize + (height - 1) * previewCellGap
        );

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject cellObject = new GameObject("DragPreviewCell", typeof(RectTransform), typeof(Image));
                cellObject.transform.SetParent(dragPreviewRoot, false);

                RectTransform cellRect = cellObject.GetComponent<RectTransform>();
                cellRect.anchorMin = new Vector2(0f, 1f);
                cellRect.anchorMax = new Vector2(0f, 1f);
                cellRect.pivot = new Vector2(0f, 1f);
                cellRect.sizeDelta = new Vector2(previewCellSize, previewCellSize);
                cellRect.anchoredPosition = new Vector2(x * pitch, -y * pitch);

                Image image = cellObject.GetComponent<Image>();
                image.color = dragPreviewCellColor;
                image.raycastTarget = false;
                dragPreviewImages.Add(image);

                Outline outline = cellObject.AddComponent<Outline>();
                outline.effectColor = dragPreviewBorderColor;
                outline.effectDistance = new Vector2(1f, -1f);

                dragPreviewCells.Add(cellObject);
            }
        }
        UpdateDragPreviewIcon();
    }

    private void UpdateDragPreviewColor()
    {
        if (!isDragging || draggingItem == null)
        {
            SetDragPreviewColor(dragPreviewCellColor);
            return;
        }

        if (!TryGetGridPositionFromMouse(out int gridX, out int gridY))
        {
            SetDragPreviewColor(dragPreviewCellColor);
            return;
        }

        bool canPlace;

        if (isMovingPackedItem && PlayerProfile.Instance != null)
        {
            canPlace = PlayerProfile.Instance.CanMovePackedItem(
                draggingPackedItem,
                gridX,
                gridY,
                draggingRotated
            );
        }
        else if (PlayerProfile.Instance != null)
        {
            canPlace = PlayerProfile.Instance.CanPlaceItem(
                draggingItem,
                gridX,
                gridY,
                draggingRotated
            );
        }
        else
        {
            canPlace = false;
        }

        SetDragPreviewColor(canPlace ? dragPreviewValidColor : dragPreviewInvalidColor);
    }

    private void SetDragPreviewColor(Color color)
    {
        for (int i = 0; i < dragPreviewImages.Count; i++)
        {
            if (dragPreviewImages[i] != null)
            {
                dragPreviewImages[i].color = color;
            }
        }
    }

    private void ClearDragPreview()
    {
        for (int i = 0; i < dragPreviewCells.Count; i++)
        {
            if (dragPreviewCells[i] != null)
            {
                Destroy(dragPreviewCells[i]);
            }
        }

        dragPreviewCells.Clear();
    }

    public void BeginMovePackedItem(PackedBackpackItem packedItem)
    {
        if (packedItem == null || packedItem.itemData == null)
        {
            return;
        }

        draggingPackedItem = packedItem;
        draggingItem = packedItem.itemData;
        draggingRotated = packedItem.rotated;
        isDragging = true;
        isMovingPackedItem = true;

        BuildDragPreview();

        if (dragIconImage != null)
        {
            dragIconImage.sprite = draggingItem.icon;
            dragIconImage.enabled = draggingItem.icon != null;
        }

        if (dragIconRect != null)
        {
            dragIconRect.gameObject.SetActive(true);

            float iconWidth = draggingItem.GetWidth(draggingRotated) * 48f;
            float iconHeight = draggingItem.GetHeight(draggingRotated) * 48f;
            dragIconRect.sizeDelta = new Vector2(iconWidth, iconHeight);
        }

        ShowNotice("Move item / R: Rotate");
        UpdateDragIconPosition();
    }

    public bool BeginDragItem(BackpackItemData itemData)
    {
        if (PlayerProfile.Instance == null)
        {
            ShowNotice("PlayerProfile is not found");
            return false;
        }

        if (itemData == null)
        {
            return false;
        }

        if (PlayerProfile.Instance.GetOwnedCount(itemData) <= 0)
        {
            ShowNotice("Buy this item first");
            return false;
        }

        draggingItem = itemData;
        isDragging = true;
        draggingRotated = false;

        BuildDragPreview();

        if (dragIconRect != null)
        {
            dragIconRect.gameObject.SetActive(true);

            float iconWidth = itemData.GetWidth(draggingRotated) * 48f;
            float iconHeight = itemData.GetHeight(draggingRotated) * 48f;
            dragIconRect.sizeDelta = new Vector2(iconWidth, iconHeight);
        }

        if (dragIconImage != null)
        {
            dragIconImage.sprite = itemData.icon;
            dragIconImage.color = itemData.icon != null
                ? Color.white
                : new Color(0.2f, 0.2f, 0.2f, 0.85f);
        }

        if (dragIconText != null)
        {
            dragIconText.text = itemData.itemName;
        }

        ShowNotice("Drag to grid / R: Rotate");
        UpdateDragIconPosition();
        return true;
    }

    public void EndDragItem()
    {
        if (!isDragging)
        {
            return;
        }

        if (draggingItem == null)
        {
            CancelDrag();
            return;
        }

        if (PlayerProfile.Instance == null)
        {
            CancelDrag();
            return;
        }

        if (!TryGetGridPositionFromMouse(out int gridX, out int gridY))
        {
            ShowNotice("Drop item on backpack grid");
            CancelDrag();
            return;
        }

        bool success;

        if (isMovingPackedItem)
        {
            success = PlayerProfile.Instance.MovePackedItem(
                draggingPackedItem,
                gridX,
                gridY,
                draggingRotated
            );
        }
        else
        {
            success = PlayerProfile.Instance.PlaceItemFromOwned(
                draggingItem,
                gridX,
                gridY,
                draggingRotated
            );
        }

        if (success)
        {
            ShowNotice(isMovingPackedItem ? "Moved item" : "Packed: " + draggingItem.itemName);
        }
        else
        {
            ShowNotice("Cannot place item there");
        }

        CancelDrag();
        RefreshAll();
    }

    public void CancelDrag()
    {
        draggingItem = null;
        draggingPackedItem = null;
        isDragging = false;
        isMovingPackedItem = false;
        draggingRotated = false;
        hoveredCell = null;

        ClearDragPreview();

        if (dragIconRect != null)
        {
            dragIconRect.gameObject.SetActive(false);
            dragPreviewImages.Clear();
        }
        if (dragPreviewIconImage != null)
        {
            dragPreviewIconImage.enabled = false;
        }
    }

    public void SetHoveredCell(BackpackGridCellUI cell)
    {
        hoveredCell = cell;
    }

    public void ClearHoveredCell(BackpackGridCellUI cell)
    {
        if (hoveredCell == cell)
        {
            hoveredCell = null;
        }
    }

    public void RemovePackedItem(PackedBackpackItem packedItem)
    {
        if (PlayerProfile.Instance == null)
        {
            return;
        }

        int index = PlayerProfile.Instance.GetPackedItemIndex(packedItem);

        if (index < 0)
        {
            return;
        }

        PlayerProfile.Instance.RemovePackedItemAt(index);
        ShowNotice("Removed from backpack");
        RefreshAll();
    }

    public void TryRemovePackedItemAtCell(int gridX, int gridY)
    {
        if (PlayerProfile.Instance == null)
        {
            return;
        }

        PackedBackpackItem packedItem = PlayerProfile.Instance.GetPackedItemAtCell(gridX, gridY);

        if (packedItem == null)
        {
            return;
        }

        RemovePackedItem(packedItem);
    }

    public void RefreshAll()
    {
        EnsureGridGenerated();

        RefreshStatus();
        RefreshLists();
        RefreshPackedItems();
    }

    private void RefreshStatus()
    {
        if (PlayerProfile.Instance == null)
        {
            if (statusText != null)
            {
                statusText.text = "Profile: None";
            }

            return;
        }

        int usedCells = PlayerProfile.Instance.GetUsedCellCount();
        int totalCells = PlayerProfile.Instance.GetTotalCellCount();

        string backpackText = PlayerProfile.Instance.BackpackEquipped
            ? "Equipped"
            : "Not Equipped";

        if (statusText != null)
        {
            statusText.text =
                $"Money: {PlayerProfile.Instance.Money}\n" +
                $"Backpack: {backpackText}\n" +
                $"Capacity: {usedCells} / {totalCells}";
        }

        if (backpackButtonText != null)
        {
            backpackButtonText.text = PlayerProfile.Instance.BackpackEquipped
                ? "Remove Backpack"
                : "Equip Backpack";
        }

        if (titleText != null)
        {
            titleText.text = "Backpack Setup";
        }
    }

    private void RefreshLists()
    {
        ClearChildren(supportListRoot);
        ClearChildren(dummyListRoot);

        if (allItems == null || listEntryPrefab == null)
        {
            return;
        }

        foreach (BackpackItemData item in allItems)
        {
            if (item == null)
            {
                continue;
            }

            RectTransform parentRoot = item.itemType == BackpackItemType.Support
                ? supportListRoot
                : dummyListRoot;

            if (parentRoot == null)
            {
                continue;
            }

            BackpackItemListEntryUI entry = Instantiate(listEntryPrefab, parentRoot);
            entry.Setup(this, item);
        }
    }

    private void RefreshPackedItems()
    {
        ClearChildren(packedItemsRoot);

        if (PlayerProfile.Instance == null || packedItemPrefab == null || packedItemsRoot == null)
        {
            return;
        }

        foreach (PackedBackpackItem packedItem in PlayerProfile.Instance.PackedItems)
        {
            if (packedItem == null || packedItem.itemData == null)
            {
                continue;
            }

            BackpackPackedItemUI itemUI = Instantiate(packedItemPrefab, packedItemsRoot);
            itemUI.Setup(this, packedItem, cellSize, cellGap);
        }
    }

    private void EnsureGridGenerated()
    {
        if (PlayerProfile.Instance == null)
        {
            return;
        }

        if (gridCellsRoot == null || gridCellPrefab == null)
        {
            return;
        }

        int width = PlayerProfile.Instance.BackpackWidth;
        int height = PlayerProfile.Instance.BackpackHeight;
        int expectedCellCount = width * height;

        bool needsGenerate =
            gridCellsRoot.childCount != expectedCellCount ||
            generatedGridWidth != width ||
            generatedGridHeight != height;

        if (needsGenerate)
        {
            GenerateGrid();
        }
    }

    private bool TryGetGridPositionFromMouse(out int gridX, out int gridY)
    {
        gridX = -1;
        gridY = -1;

        if (rootCanvas == null || gridCellsRoot == null)
        {
            return false;
        }

        if (PlayerProfile.Instance == null)
        {
            return false;
        }

        Camera uiCamera = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : rootCanvas.worldCamera;

        bool insideRect = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gridCellsRoot,
            Input.mousePosition,
            uiCamera,
            out Vector2 localPoint
        );

        if (!insideRect)
        {
            return false;
        }

        Rect rect = gridCellsRoot.rect;

        if (!rect.Contains(localPoint))
        {
            return false;
        }

        float fromLeft = localPoint.x - rect.xMin;
        float fromTop = rect.yMax - localPoint.y;

        if (draggingItem != null)
        {
            int itemWidth = draggingItem.GetWidth(draggingRotated);
            int itemHeight = draggingItem.GetHeight(draggingRotated);

            float itemPixelWidth = itemWidth * cellSize + (itemWidth - 1) * cellGap;
            float itemPixelHeight = itemHeight * cellSize + (itemHeight - 1) * cellGap;

            fromLeft -= itemPixelWidth * 0.5f;
            fromTop -= itemPixelHeight * 0.5f;
        }

        gridX = Mathf.RoundToInt(fromLeft / CellPitch);
        gridY = Mathf.RoundToInt(fromTop / CellPitch);

        if (gridX < 0 || gridY < 0)
        {
            return false;
        }

        if (gridX >= PlayerProfile.Instance.BackpackWidth)
        {
            return false;
        }

        if (gridY >= PlayerProfile.Instance.BackpackHeight)
        {
            return false;
        }

        float xInCell = fromLeft - gridX * CellPitch;
        float yInCell = fromTop - gridY * CellPitch;

        if (xInCell > cellSize || yInCell > cellSize)
        {
            return false;
        }

        return true;
    }

    private void GenerateGrid()
    {
        if (PlayerProfile.Instance == null)
        {
            return;
        }

        if (gridCellsRoot == null || packedItemsRoot == null || gridCellPrefab == null)
        {
            return;
        }

        ClearChildren(gridCellsRoot);

        int width = PlayerProfile.Instance.BackpackWidth;
        int height = PlayerProfile.Instance.BackpackHeight;

        float gridWidth = width * cellSize + (width - 1) * cellGap;
        float gridHeight = height * cellSize + (height - 1) * cellGap;

        gridCellsRoot.sizeDelta = new Vector2(gridWidth, gridHeight);
        packedItemsRoot.sizeDelta = new Vector2(gridWidth, gridHeight);

        GridLayoutGroup layout = gridCellsRoot.GetComponent<GridLayoutGroup>();

        if (layout == null)
        {
            layout = gridCellsRoot.gameObject.AddComponent<GridLayoutGroup>();
        }

        layout.cellSize = new Vector2(cellSize, cellSize);
        layout.spacing = new Vector2(cellGap, cellGap);
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = width;
        layout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        layout.startAxis = GridLayoutGroup.Axis.Horizontal;
        layout.childAlignment = TextAnchor.UpperLeft;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                BackpackGridCellUI cell = Instantiate(gridCellPrefab, gridCellsRoot);
                cell.Setup(this, x, y);
            }
        }

        generatedGridWidth = width;
        generatedGridHeight = height;
    }

    private void ToggleDraggingRotation()
    {
        if (draggingItem == null)
        {
            return;
        }

        if (!draggingItem.canRotate)
        {
            ShowNotice("This item cannot rotate");
            return;
        }

        if (draggingItem.width == draggingItem.height)
        {
            ShowNotice("Rotation does not change this item");
            return;
        }

        draggingRotated = !draggingRotated;

        if (dragIconRect != null)
        {
            float iconWidth = draggingItem.GetWidth(draggingRotated) * 48f;
            float iconHeight = draggingItem.GetHeight(draggingRotated) * 48f;
            dragIconRect.sizeDelta = new Vector2(iconWidth, iconHeight);
        }

        ShowNotice(draggingRotated ? "Rotated" : "Rotation reset");
        BuildDragPreview();
    }

    private void UpdateDragIconPosition()
    {
        if (dragIconRect == null)
        {
            return;
        }

        RectTransform parentRect = dragIconRect.parent as RectTransform;

        if (parentRect == null)
        {
            return;
        }

        Camera uiCamera = null;

        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = rootCanvas.worldCamera;
        }

        bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            Input.mousePosition,
            uiCamera,
            out Vector2 localPoint
        );

        if (!success)
        {
            return;
        }

        dragIconRect.anchoredPosition = localPoint;
    }

    private void ShowNotice(string message)
    {
        MenuNoticeUI.Instance?.ShowNotice(message);
    }

    private void ClearChildren(RectTransform root)
    {
        if (root == null)
        {
            return;
        }

        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Destroy(root.GetChild(i).gameObject);
        }
    }
}