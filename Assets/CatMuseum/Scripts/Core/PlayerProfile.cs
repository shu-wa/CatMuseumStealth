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

    public bool PlaceItemFromOwned(BackpackItemData itemData, int gridX, int gridY)
    {
        if (itemData == null)
        {
            return false;
        }

        if (GetOwnedCount(itemData) <= 0)
        {
            Debug.Log("No owned item to place: " + itemData.itemName);
            return false;
        }

        if (!CanPlaceItem(itemData, gridX, gridY))
        {
            Debug.Log("Cannot place item: " + itemData.itemName);
            return false;
        }

        RemoveOwnedItem(itemData, 1);

        PackedBackpackItem packedItem = new PackedBackpackItem
        {
            itemData = itemData,
            gridX = gridX,
            gridY = gridY
        };

        packedItems.Add(packedItem);

        Debug.Log($"Placed item: {itemData.itemName} at ({gridX}, {gridY})");
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

    public bool CanPlaceItem(BackpackItemData itemData, int gridX, int gridY)
    {
        if (itemData == null)
        {
            return false;
        }

        if (!IsAreaInsideGrid(gridX, gridY, itemData.width, itemData.height))
        {
            return false;
        }

        return IsAreaFree(gridX, gridY, itemData.width, itemData.height);
    }

    public PackedBackpackItem GetPackedItemAtCell(int cellX, int cellY)
    {
        foreach (PackedBackpackItem packedItem in packedItems)
        {
            if (packedItem == null || packedItem.itemData == null)
            {
                continue;
            }

            bool insideX =
                cellX >= packedItem.gridX &&
                cellX < packedItem.gridX + packedItem.itemData.width;

            bool insideY =
                cellY >= packedItem.gridY &&
                cellY < packedItem.gridY + packedItem.itemData.height;

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
}