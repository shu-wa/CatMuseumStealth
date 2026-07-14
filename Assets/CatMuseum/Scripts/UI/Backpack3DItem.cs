using UnityEngine;

public class Backpack3DItem : MonoBehaviour
{
    public PackedBackpackItem PackedItem { get; private set; }

    public void Setup(PackedBackpackItem packedItem)
    {
        PackedItem = packedItem;
    }
}