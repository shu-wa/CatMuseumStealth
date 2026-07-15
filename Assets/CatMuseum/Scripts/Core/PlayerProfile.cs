using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OwnedBackpackItem
{
    public BackpackItemData itemData;
    [Min(0)] public int count = 0;
}

[System.Serializable]
public class PackedBackpackItem
{
    public BackpackItemData itemData;
    public int gridX;
    public int gridY;
    public bool rotated;
}

public class PlayerProfile : MonoBehaviour
{
    public static PlayerProfile Instance { get; private set; }

    [Header("money")]
    [SerializeField] private int money = 0;

    [Header("backpack")]
    [SerializeField] private bool backpackEquipped = false;
    [SerializeField] private int backpackWidth = 8;
    [SerializeField] private int backpackHeight = 5;

    [Header("items")]
    [SerializeField] private List<OwnedBackpackItem> ownedItems = new List<OwnedBackpackItem>();
    [SerializeField] private List<PackedBackpackItem> packedItems = new List<PackedBackpackItem>();

    [Header("map")]
    [SerializeField] private string selectedMapSceneName = "Map_01_Museum";

    public int Money => money;
    public bool BackpackEquipped => backpackEquipped;
    public int BackpackWidth => backpackWidth;
    public int BackpackHeight => backpackHeight;
    public string SelectedMapSceneName => selectedMapSceneName;
    public IReadOnlyList<OwnedBackpackItem> OwnedItems => ownedItems;
    public IReadOnlyList<PackedBackpackItem> PackedItems => packedItems;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        money += amount;
        Debug.Log("Money: " + money);
    }

    public bool TrySpendMoney(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (money < amount)
        {
            return false;
        }

        money -= amount;
        Debug.Log("Money: " + money);
        return true;
    }

    public void SetBackpackEquipped(bool equipped)
    {
        backpackEquipped = equipped;
        Debug.Log("Backpack Equipped: " + backpackEquipped);
    }

    public void ToggleBackpack()
    {
        SetBackpackEquipped(!backpackEquipped);
    }

    public void SetSelectedMap(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            return;
        }

        selectedMapSceneName = sceneName;
        Debug.Log("Selected Map: " + selectedMapSceneName);
    }

    public int GetOwnedCount(BackpackItemData itemData)
    {
        OwnedBackpackItem item = FindOwnedItem(itemData);

        if (item == null)
        {
            return 0;
        }

        return item.count;
    }

    public int GetPackedCount(BackpackItemData itemData)
    {
        int total = 0;

        foreach (PackedBackpackItem packedItem in packedItems)
        {
            if (packedItem == null)
            {
                continue;
            }

            if (packedItem.itemData == itemData)
            {
                total++;
            }
        }

        return total;
    }

    public int GetUsedCellCount()
    {
        int total = 0;

        foreach (PackedBackpackItem packedItem in packedItems)
        {
            if (packedItem == null || packedItem.itemData == null)
            {
                continue;
            }

            total += packedItem.itemData.Area;
        }

        return total;
    }

    public int GetTotalCellCount()
    {
        return backpackWidth * backpackHeight;
    }

    public bool TryBuyItem(BackpackItemData itemData)
    {
        if (itemData == null)
        {
            return false;
        }

        if (!TrySpendMoney(itemData.price))
        {
            Debug.Log("Not enough money");
            return false;
        }

        AddOwnedItem(itemData, 1);

        Debug.Log("Bought item: " + itemData.itemName);
        return true;
    }

    public void AddOwnedItem(BackpackItemData itemData, int amount)
    {
        if (itemData == null || amount <= 0)
        {
            return;
        }

        OwnedBackpackItem ownedItem = FindOwnedItem(itemData);

        if (ownedItem == null)
        {
            ownedItem = new OwnedBackpackItem
            {
                itemData = itemData,
                count = 0
            };

            ownedItems.Add(ownedItem);
        }

        ownedItem.count += amount;

        Debug.Log($"{itemData.itemName} owned: {ownedItem.count}");
    }

    public bool PlaceItemFromOwned(BackpackItemData itemData, int gridX, int gridY, bool rotated)
    {
        if (itemData == null)
        {
            return false;
        }

        if (rotated && !itemData.canRotate)
        {
            rotated = false;
        }

        if (GetOwnedCount(itemData) <= 0)
        {
            Debug.Log("No owned item to place: " + itemData.itemName);
            return false;
        }

        if (!CanPlaceItem(itemData, gridX, gridY, rotated))
        {
            Debug.Log("Cannot place item: " + itemData.itemName);
            return false;
        }

        RemoveOwnedItem(itemData, 1);

        PackedBackpackItem packedItem = new PackedBackpackItem
        {
            itemData = itemData,
            gridX = gridX,
            gridY = gridY,
            rotated = rotated
        };

        packedItems.Add(packedItem);

        Debug.Log($"Placed item: {itemData.itemName} at ({gridX}, {gridY}) rotated: {rotated}");
        return true;
    }

    public bool RemovePackedItemAt(int packedIndex)
    {
        if (packedIndex < 0 || packedIndex >= packedItems.Count)
        {
            return false;
        }

        PackedBackpackItem packedItem = packedItems[packedIndex];

        if (packedItem == null || packedItem.itemData == null)
        {
            packedItems.RemoveAt(packedIndex);
            return false;
        }

        AddOwnedItem(packedItem.itemData, 1);
        Debug.Log("Removed from backpack: " + packedItem.itemData.itemName);

        packedItems.RemoveAt(packedIndex);
        return true;
    }

    public void RestorePackedItem(PackedBackpackItem packedItem)
    {
        if (packedItem == null || packedItem.itemData == null)
        {
            return;
        }

        if (packedItems.Contains(packedItem))
        {
            return;
        }

        packedItems.Add(packedItem);
    }

    public bool TryConsumePackedDummyForArt(ArtData targetArt, out PackedBackpackItem consumedItem)
    {
        consumedItem = null;

        if (targetArt == null)
        {
            return false;
        }

        for (int i = 0; i < packedItems.Count; i++)
        {
            PackedBackpackItem packedItem = packedItems[i];

            if (packedItem == null || packedItem.itemData == null)
            {
                continue;
            }

            BackpackItemData itemData = packedItem.itemData;

            if (itemData.itemType != BackpackItemType.Dummy)
            {
                continue;
            }

            if (!IsDummyMatchingArt(itemData, targetArt))
            {
                continue;
            }

            consumedItem = packedItem;
            packedItems.RemoveAt(i);
            Debug.Log("Consumed packed dummy: " + itemData.itemName);
            return true;
        }

        return false;
    }

    public bool RemoveLatestLootForArt(ArtData artData)
    {
        if (artData == null)
        {
            return false;
        }

        for (int i = packedItems.Count - 1; i >= 0; i--)
        {
            PackedBackpackItem packedItem = packedItems[i];

            if (packedItem == null || packedItem.itemData == null)
            {
                continue;
            }

            BackpackItemData itemData = packedItem.itemData;

            if (itemData.itemType != BackpackItemType.Loot)
            {
                continue;
            }

            if (itemData.linkedArtData != artData)
            {
                continue;
            }

            packedItems.RemoveAt(i);
            Debug.Log("Removed loot from backpack: " + itemData.itemName);
            return true;
        }

        return false;
    }

    public int RemoveSoldLootItemsFromBackpack()
    {
        int removedCount = 0;

        for (int i = packedItems.Count - 1; i >= 0; i--)
        {
            PackedBackpackItem packedItem = packedItems[i];

            if (packedItem == null || packedItem.itemData == null)
            {
                continue;
            }

            if (packedItem.itemData.itemType != BackpackItemType.Loot)
            {
                continue;
            }

            Debug.Log("Sold loot removed from backpack: " + packedItem.itemData.itemName);
            packedItems.RemoveAt(i);
            removedCount++;
        }

        return removedCount;
    }

    public bool CanPlaceItem(BackpackItemData itemData, int gridX, int gridY, bool rotated)
    {
        if (itemData == null)
        {
            return false;
        }

        int itemWidth = itemData.GetWidth(rotated);
        int itemHeight = itemData.GetHeight(rotated);

        if (!IsAreaInsideGrid(gridX, gridY, itemWidth, itemHeight))
        {
            return false;
        }

        return IsAreaFree(gridX, gridY, itemWidth, itemHeight);
    }

    public PackedBackpackItem GetPackedItemAtCell(int cellX, int cellY)
    {
        foreach (PackedBackpackItem packedItem in packedItems)
        {
            if (packedItem == null || packedItem.itemData == null)
            {
                continue;
            }

            int itemWidth = packedItem.itemData.GetWidth(packedItem.rotated);
            int itemHeight = packedItem.itemData.GetHeight(packedItem.rotated);

            bool insideX =
                cellX >= packedItem.gridX &&
                cellX < packedItem.gridX + itemWidth;

            bool insideY =
                cellY >= packedItem.gridY &&
                cellY < packedItem.gridY + itemHeight;

            if (insideX && insideY)
            {
                return packedItem;
            }
        }

        return null;
    }

    public int GetPackedItemIndex(PackedBackpackItem targetItem)
    {
        return packedItems.IndexOf(targetItem);
    }

    public void ClearBackpack()
    {
        for (int i = packedItems.Count - 1; i >= 0; i--)
        {
            RemovePackedItemAt(i);
        }
    }

    private bool IsAreaInsideGrid(int startX, int startY, int width, int height)
    {
        if (startX < 0 || startY < 0)
        {
            return false;
        }

        if (startX + width > backpackWidth)
        {
            return false;
        }

        if (startY + height > backpackHeight)
        {
            return false;
        }

        return true;
    }

    private bool IsAreaFree(int startX, int startY, int width, int height)
    {
        for (int y = startY; y < startY + height; y++)
        {
            for (int x = startX; x < startX + width; x++)
            {
                if (GetPackedItemAtCell(x, y) != null)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private bool RemoveOwnedItem(BackpackItemData itemData, int amount)
    {
        if (itemData == null || amount <= 0)
        {
            return false;
        }

        OwnedBackpackItem ownedItem = FindOwnedItem(itemData);

        if (ownedItem == null)
        {
            return false;
        }

        if (ownedItem.count < amount)
        {
            return false;
        }

        ownedItem.count -= amount;

        if (ownedItem.count <= 0)
        {
            ownedItems.Remove(ownedItem);
        }

        return true;
    }

    private OwnedBackpackItem FindOwnedItem(BackpackItemData itemData)
    {
        foreach (OwnedBackpackItem item in ownedItems)
        {
            if (item == null)
            {
                continue;
            }

            if (item.itemData == itemData)
            {
                return item;
            }
        }

        return null;
    }

    private bool IsDummyMatchingArt(BackpackItemData dummyItem, ArtData targetArt)
    {
        if (dummyItem == null || dummyItem.linkedArtData == null || targetArt == null)
        {
            return false;
        }

        ArtData dummyArtData = dummyItem.linkedArtData;

        return
            dummyArtData.category == targetArt.category &&
            dummyArtData.size == targetArt.size;
    }

    public bool CanMovePackedItem(PackedBackpackItem packedItem, int newGridX, int newGridY, bool rotated)
    {
        if (packedItem == null || packedItem.itemData == null)
        {
            return false;
        }

        int oldX = packedItem.gridX;
        int oldY = packedItem.gridY;
        bool oldRotated = packedItem.rotated;

        packedItem.gridX = -999;
        packedItem.gridY = -999;

        bool canPlace = CanPlaceItem(packedItem.itemData, newGridX, newGridY, rotated);

        packedItem.gridX = oldX;
        packedItem.gridY = oldY;
        packedItem.rotated = oldRotated;

        return canPlace;
    }
    public bool MovePackedItem(PackedBackpackItem packedItem, int newGridX, int newGridY, bool rotated)
    {
        if (!CanMovePackedItem(packedItem, newGridX, newGridY, rotated))
        {
            return false;
        }

        packedItem.gridX = newGridX;
        packedItem.gridY = newGridY;
        packedItem.rotated = rotated;

        return true;
    }

    public bool CanAutoPackItem(BackpackItemData itemData, bool allowRotate = true)
    {
        if (itemData == null)
        {
            return false;
        }

        bool[] rotationOptions;

        if (allowRotate && itemData.canRotate && itemData.width != itemData.height)
        {
            rotationOptions = new bool[] { false, true };
        }
        else
        {
            rotationOptions = new bool[] { false };
        }

        foreach (bool rotated in rotationOptions)
        {
            int itemWidth = itemData.GetWidth(rotated);
            int itemHeight = itemData.GetHeight(rotated);

            for (int y = 0; y <= backpackHeight - itemHeight; y++)
            {
                for (int x = 0; x <= backpackWidth - itemWidth; x++)
                {
                    if (CanPlaceItem(itemData, x, y, rotated))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public bool TryAutoPackItem(BackpackItemData itemData, out int placedX, out int placedY, bool allowRotate = true)
    {
        placedX = -1;
        placedY = -1;

        if (itemData == null)
        {
            return false;
        }

        bool[] rotationOptions;

        if (allowRotate && itemData.canRotate && itemData.width != itemData.height)
        {
            rotationOptions = new bool[] { false, true };
        }
        else
        {
            rotationOptions = new bool[] { false };
        }

        foreach (bool rotated in rotationOptions)
        {
            int itemWidth = itemData.GetWidth(rotated);
            int itemHeight = itemData.GetHeight(rotated);

            for (int y = 0; y <= backpackHeight - itemHeight; y++)
            {
                for (int x = 0; x <= backpackWidth - itemWidth; x++)
                {
                    if (!CanPlaceItem(itemData, x, y, rotated))
                    {
                        continue;
                    }

                    packedItems.Add(new PackedBackpackItem
                    {
                        itemData = itemData,
                        gridX = x,
                        gridY = y,
                        rotated = rotated
                    });

                    placedX = x;
                    placedY = y;
                    return true;
                }
            }
        }

        return false;
    }

    public bool CanAutoPackArt(ArtData artData, bool allowRotate = true)
    {
        BackpackItemData runtimeLootItem = BackpackItemData.CreateRuntimeLootFromArt(artData);

        if (runtimeLootItem == null)
        {
            return false;
        }

        return CanAutoPackItem(runtimeLootItem, allowRotate);
    }

    public bool TryAutoPackArt(ArtData artData, out int placedX, out int placedY, bool allowRotate = true)
    {
        placedX = -1;
        placedY = -1;

        BackpackItemData runtimeLootItem = BackpackItemData.CreateRuntimeLootFromArt(artData);

        if (runtimeLootItem == null)
        {
            return false;
        }

        return TryAutoPackItem(runtimeLootItem, out placedX, out placedY, allowRotate);
    }
}
