using UnityEngine;

public enum BackpackItemType
{
    Dummy,
    Support,
    Loot
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

    [Header("rotation")]
    public bool canRotate = true;

    [Header("shop")]
    [Min(0)] public int price = 50;

    [Header("visual")]
    public Sprite icon;

    [Header("3d model")]
    public GameObject modelPrefab;
    public Vector3 modelLocalPosition = Vector3.zero;
    public Vector3 modelLocalRotationEuler = Vector3.zero;
    public Vector3 modelLocalScale = Vector3.one;

    [Header("3d spin")]
    public Vector3 spinAxis = Vector3.up;
    public float spinSpeed = 30f;
    public bool useLocalSpinAxis = true;

    [Header("dummy item")]
    public ArtData linkedArtData;

    [Header("support item")]
    public SupportItemType supportType = SupportItemType.None;



    public int Area => width * height;

    public int GetWidth(bool rotated)
    {
        return rotated ? height : width;
    }

    public int GetHeight(bool rotated)
    {
        return rotated ? width : height;
    }

    public static BackpackItemData CreateRuntimeLootFromArt(ArtData artData)
    {
        if (artData == null)
        {
            return null;
        }

        BackpackItemData itemData = ScriptableObject.CreateInstance<BackpackItemData>();

        itemData.itemName = "Stolen " + artData.artName;
        itemData.itemType = BackpackItemType.Loot;

        itemData.width = artData.backpackWidth;
        itemData.height = artData.backpackHeight;
        itemData.canRotate = artData.backpackCanRotate;

        itemData.price = 0;

        itemData.icon = artData.backpackIcon;
        itemData.modelPrefab = artData.backpackModelPrefab;
        itemData.modelLocalPosition = artData.backpackModelLocalPosition;
        itemData.modelLocalRotationEuler = artData.backpackModelLocalRotationEuler;
        itemData.modelLocalScale = artData.backpackModelLocalScale;

        itemData.spinAxis = artData.backpackSpinAxis;
        itemData.spinSpeed = artData.backpackSpinSpeed;
        itemData.useLocalSpinAxis = artData.backpackUseLocalSpinAxis;

        return itemData;
    }
}