using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimplePlayerController : MonoBehaviour
{
    [Header("move")]
    [SerializeField] private float walkSpeed = 4.0f;
    [SerializeField] private float dashSpeed = 7.0f;
    [SerializeField] private float rotationSpeed = 12.0f;
    [SerializeField] private float gravity = -20.0f;

    [Header("camera")]
    [SerializeField] private Transform cameraTransform;

    [Header("interaction lock")]
    [SerializeField] private bool stopMovementWhileInteracting = true;

    [Header("animation info")]
    [SerializeField] private bool isMoving;
    [SerializeField] private bool isSprinting;
    [SerializeField] private float moveAmount;
    [SerializeField] private float currentSpeed;

    private CharacterController controller;
    private PlayerInteractor interactor;
    private Vector3 velocity;

    public bool IsMoving => isMoving;
    public bool IsSprinting => isSprinting;
    public float MoveAmount => moveAmount;
    public float CurrentSpeed => currentSpeed;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        interactor = GetComponent<PlayerInteractor>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        Move();
        ApplyGravity();
    }

    private void Move()
    {
        if (stopMovementWhileInteracting && interactor != null && interactor.IsInteracting)
        {
            ResetMoveAnimationInfo();
            return;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(horizontal, 0f, vertical).normalized;

        moveAmount = input.magnitude;
        isMoving = moveAmount > 0.1f;

        if (!isMoving)
        {
            isSprinting = false;
            currentSpeed = 0f;
            return;
        }

        bool shiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        isSprinting = shiftPressed;

        currentSpeed = isSprinting ? dashSpeed : walkSpeed;

        Vector3 moveDirection;

        if (cameraTransform != null)
        {
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;

            cameraForward.Normalize();
            cameraRight.Normalize();

            moveDirection = cameraForward * input.z + cameraRight * input.x;
        }
        else
        {
            moveDirection = input;
        }

        controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void ResetMoveAnimationInfo()
    {
        isMoving = false;
        isSprinting = false;
        moveAmount = 0f;
        currentSpeed = 0f;
    }
}