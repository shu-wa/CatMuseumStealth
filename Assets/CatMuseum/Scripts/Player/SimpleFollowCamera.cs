using UnityEngine;

public class SimpleFollowCamera : MonoBehaviour
{
    [Header("follow target")]
    [SerializeField] private Transform target;

    [Header("camera position")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 4f, -6f);
    [SerializeField] private float smoothTime = 0.15f;

    [Header("view position")]
    [SerializeField] private float lookHeight = 1.2f;

    private Vector3 currentVelocity;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 targetPosition = target.position + offset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref currentVelocity,
            smoothTime
        );

        Vector3 lookTarget = target.position + Vector3.up * lookHeight;
        transform.LookAt(lookTarget);
    }
}