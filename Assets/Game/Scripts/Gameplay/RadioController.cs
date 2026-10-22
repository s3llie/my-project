using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Zenject;
using System.Collections; 

public class RadioController : MonoBehaviour
{
    [Header("Аудио Источники")]
    [SerializeField] private AudioSource staticNoiseSource;
    [SerializeField] private AudioSource storyVoiceSource;

    [Header("Настройки Частоты")]
    [SerializeField] private float targetFrequency = 91.5f;
    [SerializeField] private float frequencyRange = 4.0f;
    [SerializeField] private float tuneSpeed = 4f;

    [Header("Камера (Zoom)")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float zoomFOV = 35f;
    [SerializeField] private float zoomDuration = 0.5f;    

    [Header("Интерфейс (UI)")]
    [SerializeField] private CanvasGroup radioUiCanvasGroup;
    [SerializeField] private TextMeshProUGUI frequencyText;

    private Controls _inputActions;
    private PlayerMovement _playerMovement;
    private VisualEffectsService _fxService;

    private float _currentFrequency = 85.0f;
    private float _tuneInput;
    private bool _isInteracting = false;
    private bool _hasTriggeredStory = false; 

    private float _targetVoiceVolume = 0f;
    private float _targetStaticVolume = 1f;
    private Coroutine _zoomCoroutine;

    [Inject]
    public void Construct(VisualEffectsService fxService, PlayerMovement playerMovement)
    {
        _fxService = fxService;
        _playerMovement = playerMovement;
    }

    private void Awake()
    {
        _inputActions = new Controls();
        if (radioUiCanvasGroup != null) radioUiCanvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        _inputActions.Radio.Tune.performed += OnTunePerformed;
        _inputActions.Radio.Tune.canceled += OnTuneCanceled;
        _inputActions.Radio.ExitRadio.performed += OnExitRadioPerformed;
    }

    private void OnDisable()
    {
        _inputActions.Radio.Tune.performed -= OnTunePerformed;
        _inputActions.Radio.Tune.canceled -= OnTuneCanceled;
        _inputActions.Radio.ExitRadio.performed -= OnExitRadioPerformed;
        _inputActions.Radio.Disable();
    }

    private void OnTunePerformed(InputAction.CallbackContext ctx) => _tuneInput = ctx.ReadValue<float>();
    private void OnTuneCanceled(InputAction.CallbackContext ctx) => _tuneInput = 0f;
    private void OnExitRadioPerformed(InputAction.CallbackContext ctx) => ExitRadioMode();

    private void Update()
    {
        if (!_isInteracting) return;

        _currentFrequency += _tuneInput * tuneSpeed * Time.deltaTime;
        _currentFrequency = Mathf.Clamp(_currentFrequency, 80f, 100f);

        if (frequencyText != null)
        {
            frequencyText.text = _currentFrequency.ToString("F1") + " MHz";
        }

        UpdateRadioAudioLogic();
    }

    private void UpdateRadioAudioLogic()
    {
        float distance = Mathf.Abs(_currentFrequency - targetFrequency);

        if (distance < frequencyRange)
        {
            float proximity = 1f - (distance / frequencyRange);

            _targetVoiceVolume = proximity;
            _targetStaticVolume = 1f - proximity;

            if (proximity > 0.92f)
            {
                _hasTriggeredStory = true;
            }
        }
        else
        {
            _targetVoiceVolume = 0f;
            _targetStaticVolume = 1f;
        }

        storyVoiceSource.volume = Mathf.MoveTowards(storyVoiceSource.volume, _targetVoiceVolume, Time.deltaTime * 2f);
        staticNoiseSource.volume = Mathf.MoveTowards(staticNoiseSource.volume, _targetStaticVolume, Time.deltaTime * 2f);
    }

    public void InteractWithRadio()
    {
        if (_isInteracting) return;

        _isInteracting = true;
        if (_playerMovement != null) _playerMovement.ToggleControls(false);

        _inputActions.Radio.Enable();

        staticNoiseSource.volume = 1f;
        storyVoiceSource.volume = 0f;
        staticNoiseSource.Play();
        storyVoiceSource.Play();

        if (_zoomCoroutine != null) StopCoroutine(_zoomCoroutine);
        _zoomCoroutine = StartCoroutine(TransitionRadioMode(true));
    }

    private void ExitRadioMode()
    {
        if (!_isInteracting) return;

        _isInteracting = false;
        _inputActions.Radio.Disable();

        staticNoiseSource.Stop();
        storyVoiceSource.Stop();

        if (_hasTriggeredStory)
        {
            Debug.Log("<color=red>Игрок отошел от радио. Мир начинает меняться...</color>");
            if (_fxService != null)
            {
                _fxService.AdvanceStoryHorror();
            }
        }

        if (_playerMovement != null) _playerMovement.ToggleControls(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (_zoomCoroutine != null) StopCoroutine(_zoomCoroutine);
        _zoomCoroutine = StartCoroutine(TransitionRadioMode(false));
    }

    private IEnumerator TransitionRadioMode(bool entering)
    {
        float elapsed = 0f;
        float startFOV = playerCamera != null ? playerCamera.fieldOfView : normalFOV;
        float targetFOV = entering ? zoomFOV : normalFOV;

        float startAlpha = radioUiCanvasGroup != null ? radioUiCanvasGroup.alpha : 0f;
        float targetAlpha = entering ? 1f : 0f;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / zoomDuration;

            float easeT = Mathf.SmoothStep(0f, 1f, t);

            if (playerCamera != null)
            {
                playerCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, easeT);
            }

            if (radioUiCanvasGroup != null)
            {
                radioUiCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, easeT);
            }

            yield return null;
        }

        if (playerCamera != null) playerCamera.fieldOfView = targetFOV;
        if (radioUiCanvasGroup != null) radioUiCanvasGroup.alpha = targetAlpha;
    }
}
