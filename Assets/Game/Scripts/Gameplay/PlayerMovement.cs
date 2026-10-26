using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Настройки движения")]
    [Header("Гравитация")]

    [SerializeField] private float gravity = -9.81f; 
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float mouseSensitivity = 15f; 
    [SerializeField] private Transform playerCameraTransform; 

    private CharacterController _characterController;
    private Controls _inputActions;
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private float _verticalRotation = 0f;
    private float _verticalVelocity = 0f;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _inputActions = new Controls();

        _inputActions.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _inputActions.Player.Move.canceled += ctx => _moveInput = Vector2.zero;

        _inputActions.Player.Look.performed += ctx => _lookInput = ctx.ReadValue<Vector2>();
        _inputActions.Player.Look.canceled += ctx => _lookInput = Vector2.zero;
    }

    private void OnEnable()
    {
        _inputActions.Player.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        _inputActions.Player.Disable();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    
    private void HandleMovement()
    {
        if (_characterController.isGrounded)
        {
            _verticalVelocity = -2f;
        }
        else
        {
            _verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 move = transform.forward * _moveInput.y + transform.right * _moveInput.x;

        Vector3 finalVelocity = move * moveSpeed;

        finalVelocity.y = _verticalVelocity;

        _characterController.Move(finalVelocity * Time.deltaTime);
    }


    private void HandleRotation()
    {
        float mouseX = _lookInput.x * mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);

        if (playerCameraTransform != null)
        {
            float mouseY = _lookInput.y * mouseSensitivity * Time.deltaTime;
            _verticalRotation -= mouseY;
            _verticalRotation = Mathf.Clamp(_verticalRotation, -60f, 60f); 
            playerCameraTransform.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);
        }
    }

    public void ToggleControls(bool isEnabled)
    {
        this.enabled = isEnabled; 
        if (isEnabled)
            _inputActions.Player.Enable(); 
        else
            _inputActions.Player.Disable(); 

        if (isEnabled)
        {
            _inputActions.Player.Enable();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            _inputActions.Player.Disable();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        _moveInput = Vector2.zero;
        _lookInput = Vector2.zero;
    }

}
