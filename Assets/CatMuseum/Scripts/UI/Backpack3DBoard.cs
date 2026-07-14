using System.Collections.Generic;
using UnityEngine;

public class Backpack3DBoard : MonoBehaviour
{
    [Header("camera")]
    [SerializeField] private Camera boardCamera;

    [Header("roots")]
    [SerializeField] private Transform cellRoot;
    [SerializeField] private Transform itemRoot;
    [SerializeField] private Transform previewRoot;

    [Header("raycast")]
    [SerializeField] private LayerMask boardLayer;

    [Header("size")]
    [SerializeField] private float cellSize = 0.45f;
    [SerializeField] private float cellGap = 0.04f;
    [SerializeField] private float cellHeight = 0.04f;
    [SerializeField] private float itemHeight = 0.28f;

    [Header("materials")]
    [SerializeField] private Material cellMaterial;
    [SerializeField] private Material packedItemMaterial;
    [SerializeField] private Material previewValidMaterial;
    [SerializeField] private Material previewInvalidMaterial;
    [SerializeField] private Material previewNeutralMaterial;

    [Header("colors")]
    [SerializeField] private Color cellColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    [SerializeField] private Color packedItemColor = new Color(0.3f, 0.25f, 0.2f, 1f);
    [SerializeField] private Color previewValidColor = new Color(0.2f, 1f, 0.35f, 0.55f);
    [SerializeField] private Color previewInvalidColor = new Color(1f, 0.2f, 0.2f, 0.55f);
    [SerializeField] private Color previewNeutralColor = new Color(1f, 1f, 1f, 0.35f);
    
    [Header("chess board colors")]
    [SerializeField] private Color lightCellColor = new Color(0.85f, 0.85f, 0.78f, 1f);
    [SerializeField] private Color darkCellColor = new Color(0.45f, 0.45f, 0.42f, 1f);

    [Header("item visual adjustment")]
    [SerializeField] private float itemBaseYOffset = 0.005f;
    [SerializeField] private float itemVisualYOffset = -0.04f;

    [Header("item icon marker")]
    [SerializeField] private bool showIconMarkers = true;
    [SerializeField] private float iconMarkerYOffset = 0.16f;
    [SerializeField] private float iconMarkerScale = 0.75f;
    [SerializeField] private Vector3 iconMarkerRotationEuler = new Vector3(90f, 0f, 0f);
    [SerializeField] private Color iconMarkerColor = new Color(1f, 1f, 1f, 0.9f);

    private readonly List<GameObject> cellObjects = new List<GameObject>();
    private readonly List<GameObject> itemObjects = new List<GameObject>();
    private readonly List<GameObject> previewObjects = new List<GameObject>();

    private int generatedWidth = -1;
    private int generatedHeight = -1;

    private float CellPitch => cellSize + cellGap;

    private void Awake()
    {
        if (boardCamera == null)
        {
            boardCamera = Camera.main;
        }

        if (cellRoot == null)
        {
            GameObject root = new GameObject("CellRoot");
            root.transform.SetParent(transform, false);
            cellRoot = root.transform;
        }

        if (itemRoot == null)
        {
            GameObject root = new GameObject("ItemRoot");
            root.transform.SetParent(transform, false);
            itemRoot = root.transform;
        }

        if (previewRoot == null)
        {
            GameObject root = new GameObject("PreviewRoot");
            root.transform.SetParent(transform, false);
            previewRoot = root.transform;
        }
    }
    public void RefreshFromProfile()
    {
        EnsureBoardGenerated();
        RefreshPackedItems();
    }

    public void SetBoardVisible(bool visible)
    {
        gameObject.SetActive(visible);

        if (!visible)
        {
            ClearPreview();
        }
    }

    public bool TryGetGridPositionFromMouse(out int gridX, out int gridY)
    {
        gridX = -1;
        gridY = -1;

        if (boardCamera == null)
        {
            return false;
        }

        Ray ray = boardCamera.ScreenPointToRay(Input.mousePosition);

        int mask = boardLayer.value == 0 ? ~0 : boardLayer.value;

        RaycastHit[] hits = Physics.RaycastAll(ray, 100f, mask);

        if (hits == null || hits.Length == 0)
        {
            return false;
        }

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            Backpack3DCell cell = hit.collider.GetComponentInParent<Backpack3DCell>();

            if (cell == null)
            {
                continue;
            }

            gridX = cell.GridX;
            gridY = cell.GridY;
            return true;
        }

        return false;
    }

    public void UpdateDragPreview(BackpackItemData itemData, bool rotated, bool isMovingPackedItem, PackedBackpackItem movingItem)
    {
        ClearPreview();

        if (itemData == null || PlayerProfile.Instance == null)
        {
            return;
        }

        if (!TryGetGridPositionFromMouse(itemData, rotated, out int gridX, out int gridY))
        {
            return;
        }

        bool canPlace;

        if (isMovingPackedItem)
        {
            canPlace = PlayerProfile.Instance.CanMovePackedItem(
                movingItem,
                gridX,
                gridY,
                rotated
            );
        }
        else
        {
            canPlace = PlayerProfile.Instance.CanPlaceItem(
                itemData,
                gridX,
                gridY,
                rotated
            );
        }

        Material material = canPlace ? previewValidMaterial : previewInvalidMaterial;
        Color color = canPlace ? previewValidColor : previewInvalidColor;

        CreatePreviewItem(itemData, gridX, gridY, rotated, material, color);
    }

    public void ClearPreview()
    {
        for (int i = previewObjects.Count - 1; i >= 0; i--)
        {
            if (previewObjects[i] != null)
            {
                Destroy(previewObjects[i]);
            }
        }

        previewObjects.Clear();
    }

    private void EnsureBoardGenerated()
    {
        if (PlayerProfile.Instance == null)
        {
            return;
        }

        int width = PlayerProfile.Instance.BackpackWidth;
        int height = PlayerProfile.Instance.BackpackHeight;

        bool needsGenerate =
            generatedWidth != width ||
            generatedHeight != height ||
            cellObjects.Count != width * height;

        if (!needsGenerate)
        {
            return;
        }

        GenerateBoard(width, height);
    }

    private void GenerateBoard(int width, int height)
    {
        ClearObjects(cellObjects);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject cellObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cellObject.name = $"Cell_{x}_{y}";
                cellObject.transform.SetParent(cellRoot, false);

                cellObject.transform.localPosition = GetCellCenter(x, y);
                cellObject.transform.localScale = new Vector3(cellSize, cellHeight, cellSize);

                cellObject.layer = gameObject.layer;

                Backpack3DCell cell = cellObject.AddComponent<Backpack3DCell>();
                cell.Setup(x, y);

                Renderer renderer = cellObject.GetComponent<Renderer>();

                bool isLightCell = (x + y) % 2 == 0;
                Color currentCellColor = isLightCell ? lightCellColor : darkCellColor;

                ApplyMaterial(renderer, cellMaterial, currentCellColor);

                cellObjects.Add(cellObject);
            }
        }

        generatedWidth = width;
        generatedHeight = height;
    }

    private void RefreshPackedItems()
    {
        ClearObjects(itemObjects);

        if (PlayerProfile.Instance == null)
        {
            return;
        }

        foreach (PackedBackpackItem packedItem in PlayerProfile.Instance.PackedItems)
        {
            if (packedItem == null || packedItem.itemData == null)
            {
                continue;
            }

            GameObject itemObject = CreatePackedItemObject(packedItem);

            if (itemObject != null)
            {
                itemObjects.Add(itemObject);
            }
        }
    }

    private GameObject CreatePackedItemObject(PackedBackpackItem packedItem)
    {
        if (packedItem == null || packedItem.itemData == null)
        {
            return null;
        }

        BackpackItemData itemData = packedItem.itemData;

        int itemGridWidth = itemData.GetWidth(packedItem.rotated);
        int itemGridHeight = itemData.GetHeight(packedItem.rotated);

        float footprintWidth = itemGridWidth * cellSize + (itemGridWidth - 1) * cellGap;
        float footprintDepth = itemGridHeight * cellSize + (itemGridHeight - 1) * cellGap;
        float visualHeight = itemHeight;

        GameObject itemObject = new GameObject(itemData.itemName);
        itemObject.transform.SetParent(itemRoot, false);

        itemObject.transform.localPosition = GetItemCenter(
            packedItem.gridX,
            packedItem.gridY,
            itemGridWidth,
            itemGridHeight
        );

        itemObject.transform.localPosition += Vector3.up * (cellHeight * 0.5f + itemBaseYOffset);
        itemObject.layer = gameObject.layer;

        Backpack3DItem backpack3DItem = itemObject.AddComponent<Backpack3DItem>();
        backpack3DItem.Setup(packedItem);

        GameObject visualRootObject = new GameObject("VisualRoot");
        visualRootObject.transform.SetParent(itemObject.transform, false);
        visualRootObject.transform.localPosition = new Vector3(0f, itemVisualYOffset, 0f);
        visualRootObject.transform.localRotation = Quaternion.identity;
        visualRootObject.transform.localScale = Vector3.one;

        CreateSimpleItemVisual(
            visualRootObject.transform,
            itemData,
            footprintWidth,
            footprintDepth,
            visualHeight
        );

        if (showIconMarkers)
        {
            CreateItemIconMarker(
                itemObject.transform,
                itemData,
                packedItem,
                footprintWidth,
                footprintDepth
            );
        }

        Backpack3DSpin spin = visualRootObject.AddComponent<Backpack3DSpin>();
        spin.Setup(
            itemData.spinAxis,
            itemData.spinSpeed,
            itemData.useLocalSpinAxis
        );

        return itemObject;
    }

    private void CreateSimpleItemVisual(
    Transform visualRoot,
    BackpackItemData itemData,
    float footprintWidth,
    float footprintDepth,
    float visualHeight

    )
    {
        if (itemData == null)
    {
        return;
    }
        if (itemData.modelPrefab != null)
        {
            CreatePrefabItemVisual(visualRoot, itemData, footprintWidth, footprintDepth, visualHeight);
            return;
        }

        if (itemData.itemType == BackpackItemType.Support)
    {
        if (itemData.supportType == SupportItemType.MouseToy)
        {
            CreateMouseToyVisual(visualRoot, footprintWidth, footprintDepth, visualHeight);
            return;
        }

        if (itemData.supportType == SupportItemType.SmokeBomb)
        {
            CreateSmokeBombVisual(visualRoot, footprintWidth, footprintDepth, visualHeight);
            return;
        }
    }

    string lowerName = itemData.itemName != null ? itemData.itemName.ToLowerInvariant() : "";

    if (lowerName.Contains("sculpture"))
    {
        CreateSculptureDummyVisual(visualRoot, footprintWidth, footprintDepth, visualHeight);
        return;
    }

    if (lowerName.Contains("painting"))
    {
        CreatePaintingDummyVisual(visualRoot, footprintWidth, footprintDepth, visualHeight);
        return;
    }

    CreatePackageVisual(visualRoot, footprintWidth, footprintDepth, visualHeight);
}

private void CreatePaintingDummyVisual(
    Transform visualRoot,
    float footprintWidth,
    float footprintDepth,
    float visualHeight
)
{
    CreatePrimitiveChild(
        visualRoot,
        PrimitiveType.Cube,
        "Frame",
        new Vector3(0f, visualHeight * 0.45f, 0f),
        new Vector3(footprintWidth * 0.85f, visualHeight * 0.25f, footprintDepth * 0.85f),
        new Color(0.35f, 0.22f, 0.12f, 1f)
    );

    CreatePrimitiveChild(
        visualRoot,
        PrimitiveType.Cube,
        "Canvas",
        new Vector3(0f, visualHeight * 0.62f, 0f),
        new Vector3(footprintWidth * 0.65f, visualHeight * 0.08f, footprintDepth * 0.65f),
        new Color(0.9f, 0.85f, 0.65f, 1f)
    );
}

private void CreateSculptureDummyVisual(
    Transform visualRoot,
    float footprintWidth,
    float footprintDepth,
    float visualHeight
)
{
    float size = Mathf.Min(footprintWidth, footprintDepth);

    CreatePrimitiveChild(
        visualRoot,
        PrimitiveType.Cube,
        "Pedestal",
        new Vector3(0f, visualHeight * 0.25f, 0f),
        new Vector3(size * 0.55f, visualHeight * 0.35f, size * 0.55f),
        new Color(0.55f, 0.55f, 0.52f, 1f)
    );

    CreatePrimitiveChild(
        visualRoot,
        PrimitiveType.Sphere,
        "Statue",
        new Vector3(0f, visualHeight * 0.85f, 0f),
        new Vector3(size * 0.45f, size * 0.45f, size * 0.45f),
        new Color(0.82f, 0.82f, 0.78f, 1f)
    );
}

private void CreateMouseToyVisual(
    Transform visualRoot,
    float footprintWidth,
    float footprintDepth,
    float visualHeight
)
{
    float size = Mathf.Min(footprintWidth, footprintDepth);

    CreatePrimitiveChild(
        visualRoot,
        PrimitiveType.Sphere,
        "MouseBody",
        new Vector3(0f, visualHeight * 0.55f, 0f),
        new Vector3(size * 0.55f, size * 0.35f, size * 0.75f),
        new Color(0.55f, 0.55f, 0.55f, 1f)
    );

    CreatePrimitiveChild(
        visualRoot,
        PrimitiveType.Sphere,
        "MouseHead",
        new Vector3(0f, visualHeight * 0.58f, footprintDepth * 0.25f),
        new Vector3(size * 0.35f, size * 0.30f, size * 0.35f),
        new Color(0.62f, 0.62f, 0.62f, 1f)
    );

    CreatePrimitiveChild(
        visualRoot,
        PrimitiveType.Cube,
        "MouseTail",
        new Vector3(0f, visualHeight * 0.58f, -footprintDepth * 0.35f),
        new Vector3(size * 0.08f, size * 0.08f, footprintDepth * 0.45f),
        new Color(0.9f, 0.55f, 0.65f, 1f)
    );
}

private void CreateSmokeBombVisual(
    Transform visualRoot,
    float footprintWidth,
    float footprintDepth,
    float visualHeight
)
{
    float size = Mathf.Min(footprintWidth, footprintDepth);

    CreatePrimitiveChild(
        visualRoot,
        PrimitiveType.Sphere,
        "SmokeBombBody",
        new Vector3(0f, visualHeight * 0.65f, 0f),
        new Vector3(size * 0.65f, size * 0.65f, size * 0.65f),
        new Color(0.08f, 0.08f, 0.08f, 1f)
    );

    CreatePrimitiveChild(
        visualRoot,
        PrimitiveType.Cube,
        "Fuse",
        new Vector3(0f, visualHeight * 1.05f, 0f),
        new Vector3(size * 0.12f, visualHeight * 0.35f, size * 0.12f),
        new Color(0.85f, 0.65f, 0.25f, 1f)
    );
}

private void CreatePackageVisual(
    Transform visualRoot,
    float footprintWidth,
    float footprintDepth,
    float visualHeight
)
{
    CreatePrimitiveChild(
        visualRoot,
        PrimitiveType.Cube,
        "Package",
        new Vector3(0f, visualHeight * 0.5f, 0f),
        new Vector3(footprintWidth * 0.75f, visualHeight * 0.75f, footprintDepth * 0.75f),
        packedItemColor
    );
}

private GameObject CreatePrimitiveChild(
    Transform parent,
    PrimitiveType primitiveType,
    string objectName,
    Vector3 localPosition,
    Vector3 localScale,
    Color color
)
{
    GameObject childObject = GameObject.CreatePrimitive(primitiveType);
    childObject.name = objectName;
    childObject.transform.SetParent(parent, false);
    childObject.transform.localPosition = localPosition;
    childObject.transform.localRotation = Quaternion.identity;
    childObject.transform.localScale = localScale;
    childObject.layer = gameObject.layer;

    Collider childCollider = childObject.GetComponent<Collider>();
    if (childCollider != null)
    {
        Destroy(childCollider);
    }

    Renderer renderer = childObject.GetComponent<Renderer>();
    ApplyMaterial(renderer, null, color);

    return childObject;
}

    private void CreatePreviewItem(BackpackItemData itemData, int gridX, int gridY, bool rotated, Material material, Color color)
    {
        int itemWidth = itemData.GetWidth(rotated);
        int itemHeight = itemData.GetHeight(rotated);

        GameObject previewObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        previewObject.name = "DragPreview3D";
        previewObject.transform.SetParent(previewRoot, false);

        previewObject.transform.localPosition = GetItemCenter(gridX, gridY, itemWidth, itemHeight);
        previewObject.transform.localScale = new Vector3(
            itemWidth * cellSize + (itemWidth - 1) * cellGap,
            itemHeight * 0.35f,
            itemHeight * cellSize + (itemHeight - 1) * cellGap
        );

        previewObject.transform.localPosition += Vector3.up * (cellHeight + 0.08f);

        Renderer renderer = previewObject.GetComponent<Renderer>();
        ApplyMaterial(renderer, material, color);

        Collider collider = previewObject.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        previewObjects.Add(previewObject);
    }

    private Vector3 GetCellCenter(int x, int y)
    {
        if (PlayerProfile.Instance == null)
        {
            return Vector3.zero;
        }

        float boardWidth = PlayerProfile.Instance.BackpackWidth;
        float boardHeight = PlayerProfile.Instance.BackpackHeight;

        float localX = (x - (boardWidth - 1f) * 0.5f) * CellPitch;
        float localZ = ((boardHeight - 1f) * 0.5f - y) * CellPitch;

        return new Vector3(localX, 0f, localZ);
    }

    private Vector3 GetItemCenter(int gridX, int gridY, int itemWidth, int itemHeight)
    {
        Vector3 topLeft = GetCellCenter(gridX, gridY);

        float offsetX = (itemWidth - 1f) * CellPitch * 0.5f;
        float offsetZ = -(itemHeight - 1f) * CellPitch * 0.5f;

        return topLeft + new Vector3(offsetX, 0f, offsetZ);
    }

    public bool TryGetGridPositionFromMouse(
    BackpackItemData itemData,
    bool rotated,
    out int gridX,
    out int gridY
)
    {
        gridX = -1;
        gridY = -1;

        if (!TryGetGridPositionFromMouse(out int centerX, out int centerY))
        {
            return false;
        }

        if (itemData == null)
        {
            gridX = centerX;
            gridY = centerY;
            return true;
        }

        int itemWidth = itemData.GetWidth(rotated);
        int itemHeight = itemData.GetHeight(rotated);

        gridX = centerX - Mathf.FloorToInt(itemWidth * 0.5f);
        gridY = centerY - Mathf.FloorToInt(itemHeight * 0.5f);

        return true;
    }

    private void ApplyMaterial(Renderer renderer, Material material, Color fallbackColor)
    {
        if (renderer == null)
        {
            return;
        }

        if (material != null)
        {
            renderer.material = material;
            return;
        }

        renderer.material.color = fallbackColor;
    }

    private void ClearObjects(List<GameObject> objects)
    {
        for (int i = objects.Count - 1; i >= 0; i--)
        {
            if (objects[i] != null)
            {
                Destroy(objects[i]);
            }
        }

        objects.Clear();
    }

    public bool TryGetPackedItemFromMouse(out PackedBackpackItem packedItem)
    {
        packedItem = null;

        if (boardCamera == null)
        {
            return false;
        }

        Ray ray = boardCamera.ScreenPointToRay(Input.mousePosition);

        int mask = boardLayer.value == 0 ? ~0 : boardLayer.value;

        RaycastHit[] hits = Physics.RaycastAll(ray, 100f, mask);

        if (hits == null || hits.Length == 0)
        {
            return false;
        }

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            Backpack3DItem item = hit.collider.GetComponentInParent<Backpack3DItem>();

            if (item == null)
            {
                continue;
            }

            if (item.PackedItem == null)
            {
                continue;
            }

            packedItem = item.PackedItem;
            return true;
        }

        return false;
    }

    private void CreatePrefabItemVisual(
    Transform visualRoot,
    BackpackItemData itemData,
    float footprintWidth,
    float footprintDepth,
    float visualHeight
)
    {
        GameObject modelObject = Instantiate(itemData.modelPrefab, visualRoot);
        modelObject.name = itemData.modelPrefab.name;

        modelObject.transform.localPosition = itemData.modelLocalPosition;
        modelObject.transform.localRotation = Quaternion.Euler(itemData.modelLocalRotationEuler);
        modelObject.transform.localScale = itemData.modelLocalScale;

        DisableCollidersRecursively(modelObject);
        SetLayerRecursively(modelObject, gameObject.layer);
    }

    private void DisableCollidersRecursively(GameObject targetObject)
    {
        if (targetObject == null)
        {
            return;
        }

        Collider[] colliders = targetObject.GetComponentsInChildren<Collider>(true);

        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }
    }

    private void SetLayerRecursively(GameObject targetObject, int layer)
    {
        if (targetObject == null)
        {
            return;
        }

        targetObject.layer = layer;

        foreach (Transform child in targetObject.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    private void CreateItemIconMarker(
    Transform parent,
    BackpackItemData itemData,
    PackedBackpackItem packedItem,
    float footprintWidth,
    float footprintDepth
)
    {
        if (itemData == null || itemData.icon == null)
        {
            return;
        }

        GameObject iconObject = new GameObject("IconMarker");
        iconObject.transform.SetParent(parent, false);

        iconObject.transform.localPosition = new Vector3(0f, iconMarkerYOffset, 0f);
        iconObject.transform.localRotation = Quaternion.Euler(iconMarkerRotationEuler);
        iconObject.layer = gameObject.layer;

        Backpack3DItem backpack3DItem = iconObject.AddComponent<Backpack3DItem>();
        backpack3DItem.Setup(packedItem);

        SpriteRenderer spriteRenderer = iconObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = itemData.icon;
        spriteRenderer.color = iconMarkerColor;

        Vector2 spriteSize = itemData.icon.bounds.size;

        if (spriteSize.x <= 0.0001f || spriteSize.y <= 0.0001f)
        {
            iconObject.transform.localScale = Vector3.one;
            return;
        }

        float targetWidth = footprintWidth * iconMarkerScale;
        float targetHeight = footprintDepth * iconMarkerScale;

        iconObject.transform.localScale = new Vector3(
            targetWidth / spriteSize.x,
            targetHeight / spriteSize.y,
            1f
        );

        BoxCollider iconCollider = iconObject.AddComponent<BoxCollider>();
        iconCollider.size = new Vector3(spriteSize.x, spriteSize.y, 0.04f);
        iconCollider.center = Vector3.zero;
    }

    public void SetPackedItemVisible(PackedBackpackItem packedItem, bool visible)
    {
        if (packedItem == null)
        {
            return;
        }

        for (int i = 0; i < itemObjects.Count; i++)
        {
            GameObject itemObject = itemObjects[i];

            if (itemObject == null)
            {
                continue;
            }

            Backpack3DItem backpack3DItem = itemObject.GetComponent<Backpack3DItem>();

            if (backpack3DItem == null)
            {
                continue;
            }

            if (backpack3DItem.PackedItem == packedItem)
            {
                itemObject.SetActive(visible);
                return;
            }
        }
    }
}
