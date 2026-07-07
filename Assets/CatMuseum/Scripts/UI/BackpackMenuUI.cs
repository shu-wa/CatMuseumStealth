using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    public static bool IsAnyBackpackOpen { get; private set; }

    private Canvas rootCanvas;
    private RectTransform canvasRect;

    private bool isOpen = false;

    private BackpackItemData draggingItem;
    private bool isDragging = false;
    private BackpackGridCellUI hoveredCell;

    private int generatedGridWidth = -1;
    private int generatedGridHeight = -1;

    private RectTransform dragIconRect;
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

        CreateDragIcon();

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
            UpdateDragIconPosition();
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

        if (dragIconRect != null)
        {
            dragIconRect.gameObject.SetActive(true);
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

        bool success = PlayerProfile.Instance.PlaceItemFromOwned(
            draggingItem,
            gridX,
            gridY
        );

        if (success)
        {
            ShowNotice("Packed: " + draggingItem.itemName);
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
        isDragging = false;
        hoveredCell = null;

        if (dragIconRect != null)
        {
            dragIconRect.gameObject.SetActive(false);
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

        gridX = Mathf.FloorToInt(fromLeft / CellPitch);
        gridY = Mathf.FloorToInt(fromTop / CellPitch);

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

    private void CreateDragIcon()
    {
        if (rootCanvas == null)
        {
            return;
        }

        GameObject iconObject = new GameObject("BackpackDragIcon");
        iconObject.transform.SetParent(rootCanvas.transform, false);

        dragIconRect = iconObject.AddComponent<RectTransform>();
        dragIconRect.sizeDelta = new Vector2(120f, 60f);

        dragIconImage = iconObject.AddComponent<Image>();
        dragIconImage.color = new Color(0.2f, 0.2f, 0.2f, 0.85f);

        dragIconCanvasGroup = iconObject.AddComponent<CanvasGroup>();
        dragIconCanvasGroup.blocksRaycasts = false;

        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(iconObject.transform, false);

        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        dragIconText = textObject.AddComponent<TextMeshProUGUI>();
        dragIconText.alignment = TextAlignmentOptions.Center;
        dragIconText.fontSize = 18f;
        dragIconText.color = Color.white;

        iconObject.SetActive(false);
    }

    private void UpdateDragIconPosition()
    {
        if (dragIconRect == null || canvasRect == null)
        {
            return;
        }

        Camera uiCamera = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : rootCanvas.worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Input.mousePosition,
            uiCamera,
            out Vector2 localPoint
        );

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