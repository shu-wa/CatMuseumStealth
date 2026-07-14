using UnityEngine;

public enum ArtCategory
{
    Painting,
    Sculpture
}

public enum ArtSize
{
    Small,
    Medium,
    Large
}

[CreateAssetMenu(fileName = "NewArtData", menuName = "CatMuseum/Art Data")]
public class ArtData : ScriptableObject
{
    [Header("information")]
    public string artName;
    public ArtCategory category;
    public ArtSize size;

    [Header("game's")]
    public int value = 100;
    public float suspicionWhenStolen = 30f;
    public float suspicionWhenSwapped = 5f;

    [Header("backpack loot")]
    [Min(1)] public int backpackWidth = 1;
    [Min(1)] public int backpackHeight = 1;
    public bool backpackCanRotate = true;

    [Header("backpack loot visual")]
    public string backpackDisplayName;
    [TextArea(2, 5)] public string backpackInfoText;
    public Sprite backpackIcon;
    public GameObject backpackModelPrefab;
    public Vector3 backpackModelLocalPosition = Vector3.zero;
    public Vector3 backpackModelLocalRotationEuler = Vector3.zero;
    public Vector3 backpackModelLocalScale = Vector3.one;

    [Header("backpack loot spin")]
    public Vector3 backpackSpinAxis = Vector3.up;
    public float backpackSpinSpeed = 30f;
    public bool backpackUseLocalSpinAxis = true;

    public int CapacityCost
    {
        get
        {
            switch (size)
            {
                case ArtSize.Small:
                    return 1;
                case ArtSize.Medium:
                    return 2;
                case ArtSize.Large:
                    return 3;
                default:
                    return 1;
            }
        }
    }
}
