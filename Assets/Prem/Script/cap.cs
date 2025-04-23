using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform playerModel;

    private Vector3 movementInput;
    private Camera mainCamera;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        // Get input axes (both keyboard and controller friendly)
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Store input in a Vector3
        movementInput = new Vector3(horizontal, 0f, vertical).normalized;
    }

    private void FixedUpdate()
    {
        MovePlayer();
        RotatePlayer();
    }

    private void MovePlayer()
    {
        // Calculate camera-relative movement direction
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Create movement vector relative to camera orientation
        Vector3 moveDirection = cameraForward * movementInput.z + cameraRight * movementInput.x;

        // Apply movement to rigidbody
        Vector3 movement = moveDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);
    }

    private void RotatePlayer()
    {
        // Only rotate if there's movement input
        if (movementInput.magnitude > 0.1f)
        {
            // Calculate camera-relative rotation
            Vector3 cameraForward = mainCamera.transform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            Vector3 relativeDirection = cameraForward * movementInput.z + mainCamera.transform.right * movementInput.x;

            // Create target rotation
            Quaternion targetRotation = Quaternion.LookRotation(relativeDirection);

            // Smoothly rotate towards the target rotation
            playerModel.rotation = Quaternion.Slerp(playerModel.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }
}