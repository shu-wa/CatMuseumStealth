using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventoryEntry
{
    public ArtData data;
    public bool isDummy;

    public InventoryEntry(ArtData data, bool isDummy)
    {
        this.data = data;
        this.isDummy = isDummy;
    }
}

[Serializable]
public class StartingInventoryItem
{
    public ArtData data;
    public bool isDummy = true;
    [Min(1)] public int count = 1;
}

public class PlayerInventory : MonoBehaviour
{
    [Header("Capacity")]
    [SerializeField] private int maxCapacity = 6;

    [Header("startingItem")]
    [SerializeField] private List<StartingInventoryItem> startingItems = new List<StartingInventoryItem>();

    private readonly List<InventoryEntry> items = new List<InventoryEntry>();

    public int MaxCapacity => maxCapacity;

    public int UsedCapacity => GetUsedCapacity();

    public int TotalStolenValue
    {
        get
        {
            int total = 0;

            foreach (InventoryEntry item in items)
            {
                if (item.data != null && !item.isDummy)
                {
                    total += item.data.value;
                }
            }

            return total;
        }
    }

    private void Awake()
    {
        foreach (StartingInventoryItem startItem in startingItems)
        {
            if (startItem.data == null) continue;

            for (int i = 0; i < startItem.count; i++)
            {
                AddItem(startItem.data, startItem.isDummy);
            }
        }

        PrintInventory();
    }

    public int GetUsedCapacity()
    {
        int total = 0;

        foreach (InventoryEntry item in items)
        {
            if (item.data != null)
            {
                total += item.data.CapacityCost;
            }
        }

        return total;
    }

    public bool CanAdd(ArtData data)
    {
        if (data == null) return false;
        return GetUsedCapacity() + data.CapacityCost <= maxCapacity;
    }

    public bool AddItem(ArtData data, bool isDummy)
    {
        if (!CanAdd(data))
        {
            Debug.Log("no capacity");
            return false;
        }

        items.Add(new InventoryEntry(data, isDummy));
        PrintInventory();
        return true;
    }

    public InventoryEntry FindMatchingDummy(ArtData targetArt)
    {
        foreach (InventoryEntry item in items)
        {
            if (!item.isDummy) continue;
            if (item.data == null) continue;

            bool sameCategory = item.data.category == targetArt.category;
            bool sameSize = item.data.size == targetArt.size;

            if (sameCategory && sameSize)
            {
                return item;
            }
        }

        return null;
    }

    public bool ReplaceItem(InventoryEntry oldItem, ArtData newData, bool newIsDummy)
    {
        int index = items.IndexOf(oldItem);
        if (index < 0) return false;

        int currentCapacity = GetUsedCapacity();
        int futureCapacity = currentCapacity - oldItem.data.CapacityCost + newData.CapacityCost;

        if (futureCapacity > maxCapacity)
        {
            Debug.Log("excahnge no capacity");
            return false;
        }

        items[index] = new InventoryEntry(newData, newIsDummy);
        PrintInventory();
        return true;
    }

    public bool RemoveItem(InventoryEntry targetItem)
    {
        if (targetItem == null)
        {
            return false;
        }

        bool removed = items.Remove(targetItem);

        if (removed)
        {
            PrintInventory();
        }

        return removed;
    }

    public void PrintInventory()
    {
        Debug.Log($"capacity: {GetUsedCapacity()} / {maxCapacity}");

        foreach (InventoryEntry item in items)
        {
            string type = item.isDummy ? "Dummy" : "Stolen goods";
            Debug.Log($"{type}: {item.data.artName} / {item.data.category} / {item.data.size}");
        }
    }
}