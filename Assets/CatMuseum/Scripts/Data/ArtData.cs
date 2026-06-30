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