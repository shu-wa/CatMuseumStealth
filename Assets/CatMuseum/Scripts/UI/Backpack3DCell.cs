using UnityEngine;

public class Backpack3DCell : MonoBehaviour
{
    public int GridX { get; private set; }
    public int GridY { get; private set; }

    public void Setup(int gridX, int gridY)
    {
        GridX = gridX;
        GridY = gridY;
    }
}