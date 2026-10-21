using UnityEngine;
using UnityEngine.Rendering.PostProcessing; // Если URP, смени на: using UnityEngine.Rendering;

public class VisualEffectsService : MonoBehaviour
{
    [System.Serializable]
    public struct HorrorPreset
    {
        public float grainIntensity;
        public float vignetteIntensity;
        public Color colorFilter;
    }

    [Header("Настройки эффектов по главам/часам")]
    [SerializeField] private HorrorPreset[] storySteps;
    [SerializeField] private float changeSpeed = 0.5f; // Скорость смены атмосферы

    private PostProcessVolume _volume;
    private Grain _grainLayer;
    private Vignette _vignetteLayer;
    private ColorGrading _colorGradingLayer; // Для URP: ColorAdjustments

    private int _currentStep = 0;

    // Целевые значения, к которым мы плавно идем
    private float _targetGrain;
    private float _targetVignette;
    private Color _targetColor;

    private void Awake()
    {
        _volume = GetComponent<PostProcessVolume>();

        if (_volume != null)
        {
            _volume.profile.TryGetSettings(out _grainLayer);
            _volume.profile.TryGetSettings(out _vignetteLayer);
            _volume.profile.TryGetSettings(out _colorGradingLayer);
        }

        // Задаем стартовую точку
        if (storySteps.Length > 0)
        {
            _targetGrain = storySteps[0].grainIntensity;
            _targetVignette = storySteps[0].vignetteIntensity;
            _targetColor = storySteps[0].colorFilter;
            ApplyInstantSettings(0);
        }
    }

    private void Update()
    {
        // Плавно двигаем зернистость
        if (_grainLayer != null)
            _grainLayer.intensity.value = Mathf.MoveTowards(_grainLayer.intensity.value, _targetGrain, Time.deltaTime * changeSpeed);

        // Плавно двигаем виньетку
        if (_vignetteLayer != null)
            _vignetteLayer.intensity.value = Mathf.MoveTowards(_vignetteLayer.intensity.value, _targetVignette, Time.deltaTime * changeSpeed);

        // ПЛАВНО переливаем цвета (исправляет застревание зеленого цвета!)
        if (_colorGradingLayer != null)
        {
            _colorGradingLayer.colorFilter.value = Color.Lerp(_colorGradingLayer.colorFilter.value, _targetColor, Time.deltaTime * changeSpeed * 2f);
        }
    }

    // Главный метод продвижения сюжета (переключения часов)
    public void AdvanceStoryHorror()
    {
        if (_currentStep + 1 < storySteps.Length)
        {
            _currentStep++;

            // Задаем новые цели для плавного перехода в Update
            _targetGrain = storySteps[_currentStep].grainIntensity;
            _targetVignette = storySteps[_currentStep].vignetteIntensity;
            _targetColor = storySteps[_currentStep].colorFilter;

            Debug.Log($"<Color=Cyan>Атмосфера меняется!</Color> Включается Element {_currentStep}");
        }
        else
        {
            Debug.LogWarning("Сюжет дошел до финального пресета!");
        }
    }

    private void ApplyInstantSettings(int stepIndex)
    {
        if (_grainLayer != null) _grainLayer.intensity.value = storySteps[stepIndex].grainIntensity;
        if (_vignetteLayer != null) _vignetteLayer.intensity.value = storySteps[stepIndex].vignetteIntensity;
        if (_colorGradingLayer != null) _colorGradingLayer.colorFilter.value = storySteps[stepIndex].colorFilter;
    }
}
