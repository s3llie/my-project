using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
using System.Collections;

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

        if (Physics.Raycast(ray, out hit, 3f)) // Дистанция 3 метра
        {
            // Если посмотрели на дверь — включаем 2D глазок
            if (hit.collider.CompareTag("Door"))
            {
                EnterPeepholeMode();
            }
            // Если посмотрели на радио — включаем режим радиоприемника!
            else if (hit.collider.CompareTag("Radio"))
            {
                RadioController radio = hit.collider.GetComponent<RadioController>();
                if (radio != null) radio.InteractWithRadio();
            }
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

        _inputActions.Player.Disable();
        _inputActions.Peephole.Enable();
    }


private void ExitPeepholeMode()
{
    if (peepholeCanvas != null) peepholeCanvas.SetActive(false);
    if (_playerMovement != null) _playerMovement.ToggleControls(true);

    _inputActions.Peephole.Disable();
    _inputActions.Player.Enable();

    StartCoroutine(LockCursorNextFrame());
    Debug.Log("Вернулись в 3D-комнату.");
}
     
private IEnumerator LockCursorNextFrame()
 {
    yield return null; 
    
     Cursor.lockState = CursorLockMode.Locked;
     Cursor.visible = false;
 }
}
