using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Настройки движения")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float mouseSensitivity = 15f; // Чувствительность мыши/тачпада
    [SerializeField] private Transform playerCameraTransform; // Изменили имя, чтобы избежать конфликтов

    private CharacterController _characterController;
    private Controls _inputActions;
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private float _verticalRotation = 0f;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _inputActions = new Controls();

        // Подписка на ходьбу (WASD)
        _inputActions.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _inputActions.Player.Move.canceled += ctx => _moveInput = Vector2.zero;

        // Подписка на поворот (Мышь/Тачпад)
        _inputActions.Player.Look.performed += ctx => _lookInput = ctx.ReadValue<Vector2>();
        _inputActions.Player.Look.canceled += ctx => _lookInput = Vector2.zero;
    }

    private void OnEnable()
    {
        _inputActions.Player.Enable();
        // Прячем курсор и блокируем его в центре экрана
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        _inputActions.Player.Disable();
        // Возвращаем курсор обратно при отключении скрипта
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
        Vector3 move = transform.forward * _moveInput.y + transform.right * _moveInput.x;
        _characterController.Move(move * moveSpeed * Time.deltaTime);
    }

    private void HandleRotation()
    {
        // Поворот игрока влево-вправо мышкой (ось Y)
        float mouseX = _lookInput.x * mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);

        // Наклон головы вверх-вниз мышкой (ось X)
        if (playerCameraTransform != null)
        {
            float mouseY = _lookInput.y * mouseSensitivity * Time.deltaTime;
            _verticalRotation -= mouseY;
            _verticalRotation = Mathf.Clamp(_verticalRotation, -60f, 60f); // Ограничение взгляда
            playerCameraTransform.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);
        }
    }

    // Метод выключения управления (для интеграции с глазком)
    public void ToggleControls(bool isEnabled)
    {
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
