using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("follow target")]
    [SerializeField] private Transform target;

    [Header("camera position")]
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private float distance = 5.0f;
    [SerializeField] private float minDistance = 1.0f;
    [SerializeField] private float smoothTime = 0.05f;

    [Header("view position")]
    [SerializeField] private float mouseSensitivity = 3.0f;
    [SerializeField] private float minPitch = -25.0f;
    [SerializeField] private float maxPitch = 60.0f;

    [Header("camera collision")]
    [SerializeField] private LayerMask cameraCollisionLayer;
    [SerializeField] private float collisionRadius = 0.25f;
    [SerializeField] private float collisionBuffer = 0.15f;

    private float yaw;
    private float pitch = 20.0f;
    private Vector3 currentVelocity;

    private void Start()
    {
        if (target != null)
        {
            yaw = target.eulerAngles.y;
        }

        LockCursor();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        HandleCursor();
        HandleMouseInput();
        UpdateCameraPosition();
    }

    private void HandleCursor()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnlockCursor();
        }

        if (Input.GetMouseButtonDown(0))
        {
            LockCursor();
        }
    }

    private void HandleMouseInput()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            return;
        }

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        yaw += mouseX * mouseSensitivity;
        pitch -= mouseY * mouseSensitivity;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    private void UpdateCameraPosition()
    {
        Vector3 targetPoint = target.position + targetOffset;

        Quaternion cameraRotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 cameraDirection = cameraRotation * Vector3.back;

        Vector3 desiredCameraPosition = targetPoint + cameraDirection * distance;

        float adjustedDistance = distance;

        if (Physics.SphereCast(
            targetPoint,
            collisionRadius,
            cameraDirection,
            out RaycastHit hit,
            distance,
            cameraCollisionLayer,
            QueryTriggerInteraction.Ignore))
        {
            adjustedDistance = Mathf.Clamp(
                hit.distance - collisionBuffer,
                minDistance,
                distance
            );
        }

        Vector3 finalCameraPosition = targetPoint + cameraDirection * adjustedDistance;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            finalCameraPosition,
            ref currentVelocity,
            smoothTime
        );

        transform.LookAt(targetPoint);
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (target == null)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        Vector3 targetPoint = target.position + targetOffset;
        Gizmos.DrawWireSphere(targetPoint, 0.2f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(targetPoint, transform.position);
        Gizmos.DrawWireSphere(transform.position, collisionRadius);
    }
}