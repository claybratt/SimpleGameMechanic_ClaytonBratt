using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    public float WalkSpeed = 5f;
    public float SprintMultiplier = 2f;
    public float JumpForce = 5f;
    public float GroundCheckDistance = 1.5f;
    public float LookSensitivityX = 1f;
    public float LookSensitivityY = 1f;
    public float MinYLookAngle = -90f;
    public float MaxYLookAngle = 90f;
    public float MinFov = 10f;
    public float MaxFov = 100f;
    public Camera PlayerCamera;
    public float Gravity = -9.81f;
    public float ScrollSensitivity = 12.5f;

    private Vector3 velocity;
    private float verticalRotation = 0;
    private CharacterController characterController;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        //Debug.Log(Input.GetAxisRaw("Horizontal"));
        //Debug.Log(Input.GetAxisRaw("Vertical"));
        HandleMovement();
        HandleCameraRotation();
        HandleCameraFOV();
    }

    private void HandleMovement()
    {
        float horizontalMovement = Input.GetAxis("Horizontal");
        float verticalMovement = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.forward * verticalMovement + transform.right * horizontalMovement;
        moveDirection.Normalize();

        float speed = WalkSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed *= SprintMultiplier;
        }

        // Ground check and jumping
        if (IsGrounded() && velocity.y < 0)
        {
            velocity.y = -2f; // Prevent floating above the ground
        }

        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            velocity.y = Mathf.Sqrt(JumpForce * -2f * Gravity);
        }

        if (Input.GetMouseButtonDown(1))
        {
            velocity.y = Mathf.Sqrt(JumpForce * -2f * Gravity);
        }

        // Apply gravity
        velocity.y += Gravity * Time.deltaTime;

        // Move the character
        characterController.Move((moveDirection * speed + velocity) * Time.deltaTime);
    }

    private void HandleCameraRotation()
    {
        if (PlayerCamera != null)
        {
            float mouseX = Input.GetAxis("Mouse X") * LookSensitivityX;
            float mouseY = Input.GetAxis("Mouse Y") * LookSensitivityY;

            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, MinYLookAngle, MaxYLookAngle);

            PlayerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
            transform.Rotate(Vector3.up * mouseX);
        }
    }

    private void HandleCameraFOV()
    {
        if (PlayerCamera != null)
        {
            float fov = PlayerCamera.fieldOfView;

            fov -= Input.GetAxis("Mouse ScrollWheel") * ScrollSensitivity;
            fov = Mathf.Clamp(fov, MinFov, MaxFov);

            PlayerCamera.fieldOfView = fov;
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, GroundCheckDistance);
    }
}