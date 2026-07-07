using UnityEngine;

public enum BackpackItemType
{
    Dummy,
    Support
}

public enum SupportItemType
{
    None,
    MouseToy,
    SmokeBomb
}

[CreateAssetMenu(fileName = "NewBackpackItemData", menuName = "CatMuseum/Backpack Item Data")]
public class BackpackItemData : ScriptableObject
{
    [Header("information")]
    public string itemName;
    public BackpackItemType itemType;

    [Header("grid size")]
    [Min(1)] public int width = 1;
    [Min(1)] public int height = 1;

    [Header("shop")]
    [Min(0)] public int price = 50;

    [Header("visual")]
    public Sprite icon;

    [Header("dummy item")]
    public ArtData linkedArtData;

    [Header("support item")]
    public SupportItemType supportType = SupportItemType.None;

    public int Area => width * height;
}