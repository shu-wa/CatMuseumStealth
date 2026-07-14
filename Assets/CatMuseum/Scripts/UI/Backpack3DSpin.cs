using UnityEngine;

public class Backpack3DSpin : MonoBehaviour
{
    [Header("spin")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] private float rotateSpeed = 30f;
    [SerializeField] private bool useLocalAxis = true;

    public void Setup(Vector3 axis, float speed, bool useLocal)
    {
        rotationAxis = axis;
        rotateSpeed = speed;
        useLocalAxis = useLocal;
    }

    private void Update()
    {
        if (rotationAxis.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Space rotateSpace = useLocalAxis ? Space.Self : Space.World;

        transform.Rotate(
            rotationAxis.normalized,
            rotateSpeed * Time.deltaTime,
            rotateSpace
        );
    }
}