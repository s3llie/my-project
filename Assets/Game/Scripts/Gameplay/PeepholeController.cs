using UnityEngine;
using UnityEngine.InputSystem;
using Zenject; // Не забываем юзинг!

public class PeepholeController : MonoBehaviour
{
    [Header("Ссылки на объекты UI")]
    [SerializeField] private Camera playerMainCamera;
    [SerializeField] private GameObject peepholeCanvas;

    private Controls _inputActions;
    private PlayerMovement _playerMovement;

    // Сюда Zenject автоматически прокинет наш сервис эффектов!
    private VisualEffectsService _fxService;

    // Конструктор внедрения зависимостей Zenject
    [Inject]
    public void Construct(VisualEffectsService fxService)
    {
        _fxService = fxService;
    }

    private void Awake()
    {
        _inputActions = new Controls();
        _playerMovement = FindObjectOfType<PlayerMovement>();

        _inputActions.Player.Interact.performed += OnInteractPressed;
        _inputActions.Peephole.Exit.performed += OnExitPeephole;
    }

    private void OnEnable()
    {
        _inputActions.Player.Enable();
        _inputActions.Peephole.Disable();
        if (peepholeCanvas != null) peepholeCanvas.SetActive(false);
    }

    private void OnDisable()
    {
        _inputActions.Player.Disable();
        _inputActions.Peephole.Disable();
    }

    private void OnInteractPressed(InputAction.CallbackContext context)
    {
        Ray ray = playerMainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 3f) && hit.collider.CompareTag("Door"))
        {
            EnterPeepholeMode();
        }
    }

    private void OnExitPeephole(InputAction.CallbackContext context)
    {
        ExitPeepholeMode();
    }

    private void EnterPeepholeMode()
    {
        if (_playerMovement != null) _playerMovement.ToggleControls(false);
        if (peepholeCanvas != null) peepholeCanvas.SetActive(true); // Просто выводим чистую 2D-картинку

        // Временно оставляем триггер переключения часа для теста
        if (_fxService != null) _fxService.AdvanceStoryHorror();

        _inputActions.Player.Disable();
        _inputActions.Peephole.Enable();
    }

    private void ExitPeepholeMode()
    {
        if (peepholeCanvas != null) peepholeCanvas.SetActive(false);
        if (_playerMovement != null) _playerMovement.ToggleControls(true);

        // Ничего не сбрасываем, мир просто плавно продолжает меняться по сюжету!
        _inputActions.Peephole.Disable();
        _inputActions.Player.Enable();
    }


}
